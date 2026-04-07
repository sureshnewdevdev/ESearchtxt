using System.Globalization;
using System.Text.RegularExpressions;
using ESearchtxt.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ESearchtxt.Services;

public sealed partial class LogParserService : ILogParserService
{
    private static readonly Regex HeaderRegex = BuildHeaderRegex();
    private static readonly Regex UserRegex = new("UserId \\\"(?<userId>[^\\\"]+)\\\"", RegexOptions.Compiled);
    private static readonly Regex EventRegex = new("Received (?<eventName>[A-Za-z0-9]+Event)", RegexOptions.Compiled);

    private readonly Lazy<IReadOnlyList<LogEntry>> _entries;
    private readonly string _logPath;

    public LogParserService(IWebHostEnvironment environment)
    {
        _logPath = Path.Combine(environment.ContentRootPath, "LogsDemo.txt");
        _entries = new Lazy<IReadOnlyList<LogEntry>>(ReadEntriesFromDisk);
    }

    public IReadOnlyList<LogEntry> ReadAllEntries() => _entries.Value;

    public IReadOnlyList<LogEntry> Search(LogSearchParameters searchParameters)
    {
        var query = ReadAllEntries().AsEnumerable();

        if (searchParameters.From.HasValue)
        {
            query = query.Where(x => x.Timestamp >= searchParameters.From.Value);
        }

        if (searchParameters.To.HasValue)
        {
            query = query.Where(x => x.Timestamp <= searchParameters.To.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchParameters.Level))
        {
            query = query.Where(x => x.Level.Equals(searchParameters.Level, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(searchParameters.Application))
        {
            query = query.Where(x => x.Application.Equals(searchParameters.Application, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(searchParameters.EventName))
        {
            query = query.Where(x => x.EventName != null && x.EventName.Equals(searchParameters.EventName, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(searchParameters.UserId))
        {
            query = query.Where(x => x.UserId != null && x.UserId.Equals(searchParameters.UserId, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(searchParameters.MessageContains))
        {
            query = query.Where(x => x.RawEntry.Contains(searchParameters.MessageContains, StringComparison.OrdinalIgnoreCase));
        }

        if (searchParameters.OnlyExceptions)
        {
            query = query.Where(x => x.HasException || x.Level.Equals("ERR", StringComparison.OrdinalIgnoreCase));
        }

        return query
            .OrderByDescending(x => x.Timestamp)
            .ToList();
    }

    public LogSearchParameters BuildPromptOptions(LogSearchParameters searchParameters)
    {
        var allEntries = ReadAllEntries();

        searchParameters.LevelOptions = BuildOptions(allEntries.Select(x => x.Level), searchParameters.Level);
        searchParameters.ApplicationOptions = BuildOptions(allEntries.Select(x => x.Application), searchParameters.Application);
        searchParameters.EventOptions = BuildOptions(allEntries.Select(x => x.EventName).Where(x => !string.IsNullOrWhiteSpace(x))!, searchParameters.EventName);
        searchParameters.UserOptions = BuildOptions(allEntries.Select(x => x.UserId).Where(x => !string.IsNullOrWhiteSpace(x))!, searchParameters.UserId);

        return searchParameters;
    }

    private static List<SelectListItem> BuildOptions(IEnumerable<string> values, string? selected)
    {
        var options = values
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .Select(x => new SelectListItem(x, x, string.Equals(x, selected, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        options.Insert(0, new SelectListItem("All", string.Empty, string.IsNullOrWhiteSpace(selected)));

        return options;
    }

    private IReadOnlyList<LogEntry> ReadEntriesFromDisk()
    {
        if (!File.Exists(_logPath))
        {
            return [];
        }

        var lines = File.ReadAllLines(_logPath);
        var entries = new List<LogEntry>();
        var current = new List<string>();

        foreach (var line in lines)
        {
            if (HeaderRegex.IsMatch(line) && current.Count > 0)
            {
                entries.Add(ParseEntry(current));
                current.Clear();
            }

            current.Add(line);
        }

        if (current.Count > 0)
        {
            entries.Add(ParseEntry(current));
        }

        return entries;
    }

    private static LogEntry ParseEntry(List<string> lines)
    {
        var firstLine = lines[0];
        var match = HeaderRegex.Match(firstLine);

        if (!match.Success)
        {
            return new LogEntry
            {
                Timestamp = DateTimeOffset.MinValue,
                Level = "UNK",
                Application = "Unknown",
                Message = string.Join(Environment.NewLine, lines),
                RawEntry = string.Join(Environment.NewLine, lines)
            };
        }

        var timestamp = DateTimeOffset.ParseExact(
            match.Groups["timestamp"].Value,
            "yyyy-MM-dd HH:mm:ss.fff zzz",
            CultureInfo.InvariantCulture);

        var message = match.Groups["message"].Value;
        if (lines.Count > 1)
        {
            message = string.Join(Environment.NewLine, new[] { message }.Concat(lines.Skip(1)));
        }

        var userMatch = UserRegex.Match(message);
        var eventMatch = EventRegex.Match(message);

        return new LogEntry
        {
            Timestamp = timestamp,
            Level = match.Groups["level"].Value,
            Application = match.Groups["application"].Value,
            Message = message,
            RawEntry = string.Join(Environment.NewLine, lines),
            UserId = userMatch.Success ? userMatch.Groups["userId"].Value : null,
            EventName = eventMatch.Success ? eventMatch.Groups["eventName"].Value : null
        };
    }

    [GeneratedRegex("^(?<timestamp>\\d{4}-\\d{2}-\\d{2} \\d{2}:\\d{2}:\\d{2}\\.\\d{3} [+-]\\d{2}:\\d{2}) \\[(?<level>[A-Z]{3})\\] \\[(?<application>[^\\]]+)\\] (?<message>.*)$", RegexOptions.Compiled)]
    private static partial Regex BuildHeaderRegex();
}
