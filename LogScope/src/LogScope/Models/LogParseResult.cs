namespace LogScope.Models;

public sealed record LogParseResult(
    IReadOnlyList<LogEntry> Entries,
    int SkippedLines);
