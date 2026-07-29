using LogScope.Cli;
using LogScope.Models;

namespace LogScope.Services;

public sealed class LogAnalyzer
{
    public IReadOnlyList<LogEntry> ApplyFilters(
        IEnumerable<LogEntry> entries,
        CliOptions options)
    {
        var query = entries;

        if (options.Levels.Count > 0)
        {
            query = query.Where(entry => options.Levels.Contains(entry.Level));
        }

        if (!string.IsNullOrWhiteSpace(options.ContainsText))
        {
            query = query.Where(entry =>
                entry.Message.Contains(
                    options.ContainsText,
                    StringComparison.OrdinalIgnoreCase));
        }

        if (options.From.HasValue)
        {
            query = query.Where(entry => entry.Timestamp >= options.From.Value);
        }

        if (options.To.HasValue)
        {
            query = query.Where(entry => entry.Timestamp <= options.To.Value);
        }

        return query
            .OrderBy(entry => entry.Timestamp)
            .ToList();
    }

    public LogReport BuildReport(
        IReadOnlyCollection<LogEntry> entries,
        int topCount)
    {
        var entriesByLevel = entries
            .GroupBy(entry => entry.Level)
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Count(),
                StringComparer.OrdinalIgnoreCase);

        var frequentMessages = entries
            .Where(entry => !string.IsNullOrWhiteSpace(entry.Message))
            .GroupBy(entry => entry.Message, StringComparer.OrdinalIgnoreCase)
            .Select(group => new MessageFrequency(group.First().Message, group.Count()))
            .OrderByDescending(item => item.Count)
            .ThenBy(item => item.Message, StringComparer.OrdinalIgnoreCase)
            .Take(topCount)
            .ToList();

        return new LogReport(
            entries.Count,
            entries.Count == 0 ? null : entries.Min(entry => entry.Timestamp),
            entries.Count == 0 ? null : entries.Max(entry => entry.Timestamp),
            entriesByLevel,
            frequentMessages);
    }
}
