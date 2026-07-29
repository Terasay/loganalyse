namespace LogScope.Models;

public sealed record LogEntry(
    DateTime Timestamp,
    string Level,
    string Message,
    int LineNumber);
