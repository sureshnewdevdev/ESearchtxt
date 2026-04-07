using ESearchtxt.Models;

namespace ESearchtxt.Services;

public interface ILogParserService
{
    IReadOnlyList<LogEntry> ReadAllEntries();

    IReadOnlyList<LogEntry> Search(LogSearchParameters searchParameters);

    LogSearchParameters BuildPromptOptions(LogSearchParameters searchParameters);
}
