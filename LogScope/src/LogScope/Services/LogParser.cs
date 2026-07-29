using System.Globalization;
using System.Text.RegularExpressions;
using LogScope.Models;

namespace LogScope.Services;

public sealed class LogParser
{
    private static readonly Regex LogLinePattern = new(
        @"^\[(?<timestamp>[^\]]+)\]\s+\[(?<level>TRACE|DEBUG|INFO|WARN|WARNING|ERROR|FATAL)\]\s+(?<message>.*)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly string[] TimestampFormats =
    {
        "yyyy-MM-dd HH:mm:ss",
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-dd HH:mm:ss.fff",
        "yyyy-MM-ddTHH:mm:ss.fff"
    };

    public LogParseResult Parse(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Log file was not found.", filePath);
        }

        var entries = new List<LogEntry>();
        var skippedLines = 0;
        var lineNumber = 0;

        foreach (var line in File.ReadLines(filePath))
        {
            lineNumber++;

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var match = LogLinePattern.Match(line);
            if (!match.Success ||
                !DateTime.TryParseExact(
                    match.Groups["timestamp"].Value,
                    TimestampFormats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeLocal,
                    out var timestamp))
            {
                skippedLines++;
                continue;
            }

            var level = NormalizeLevel(match.Groups["level"].Value);
            var message = match.Groups["message"].Value.Trim();

            entries.Add(new LogEntry(timestamp, level, message, lineNumber));
        }

        return new LogParseResult(entries, skippedLines);
    }

    private static string NormalizeLevel(string level) =>
        level.Equals("WARNING", StringComparison.OrdinalIgnoreCase)
            ? "WARN"
            : level.ToUpperInvariant();
}
