namespace ESearchtxt.Models;

public sealed class LogsIndexViewModel
{
    public required LogSearchParameters Search { get; init; }

    public required IReadOnlyList<LogEntry> Results { get; init; }

    public required int TotalEntries { get; init; }

    public required int FilteredEntries { get; init; }
}
