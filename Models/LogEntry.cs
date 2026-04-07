namespace ESearchtxt.Models;

public sealed class LogEntry
{
    public DateTimeOffset Timestamp { get; init; }

    public string Level { get; init; } = string.Empty;

    public string Application { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public string RawEntry { get; init; } = string.Empty;

    public string? UserId { get; init; }

    public string? EventName { get; init; }

    public bool HasException =>
        Message.Contains("Exception", StringComparison.OrdinalIgnoreCase)
        || Message.Contains("SqlException", StringComparison.OrdinalIgnoreCase)
        || Message.Contains("Unhandled", StringComparison.OrdinalIgnoreCase);
}
