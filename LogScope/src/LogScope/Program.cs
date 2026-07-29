using LogScope.Cli;
using LogScope.Models;
using LogScope.Services;

namespace LogScope;

internal static class Program
{
    private static int Main(string[] args)
    {
        try
        {
            var options = CliOptions.Parse(args);

            if (options.ShowHelp || args.Length == 0)
            {
                Console.WriteLine(CliOptions.HelpText);
                return 0;
            }

            if (string.IsNullOrWhiteSpace(options.FilePath))
            {
                Console.Error.WriteLine("Error: --file is required.\n");
                Console.Error.WriteLine(CliOptions.HelpText);
                return 2;
            }

            var parser = new LogParser();
            var analyzer = new LogAnalyzer();
            var exporter = new ExportService();

            var parseResult = parser.Parse(options.FilePath);
            var filteredEntries = analyzer.ApplyFilters(parseResult.Entries, options);
            var report = analyzer.BuildReport(filteredEntries, options.TopCount);

            PrintReport(report, parseResult.SkippedLines);
            PrintPreview(filteredEntries);

            if (!string.IsNullOrWhiteSpace(options.ExportPath))
            {
                exporter.Export(
                    filteredEntries,
                    options.ExportPath,
                    options.ExportFormat);

                Console.WriteLine();
                Console.WriteLine(
                    $"Exported {filteredEntries.Count} entries to " +
                    $"{Path.GetFullPath(options.ExportPath)}");
            }

            return 0;
        }
        catch (ArgumentException exception)
        {
            Console.Error.WriteLine($"Argument error: {exception.Message}");
            return 2;
        }
        catch (FileNotFoundException exception)
        {
            Console.Error.WriteLine(exception.Message);
            return 3;
        }
        catch (UnauthorizedAccessException exception)
        {
            Console.Error.WriteLine($"Access error: {exception.Message}");
            return 4;
        }
        catch (IOException exception)
        {
            Console.Error.WriteLine($"I/O error: {exception.Message}");
            return 5;
        }
    }

    private static void PrintReport(LogReport report, int skippedLines)
    {
        Console.WriteLine("LogScope report");
        Console.WriteLine(new string('=', 52));
        Console.WriteLine($"Matched entries : {report.TotalEntries}");
        Console.WriteLine($"Skipped lines   : {skippedLines}");
        Console.WriteLine($"First entry     : {FormatTimestamp(report.FirstTimestamp)}");
        Console.WriteLine($"Last entry      : {FormatTimestamp(report.LastTimestamp)}");

        Console.WriteLine();
        Console.WriteLine("Entries by level:");

        if (report.EntriesByLevel.Count == 0)
        {
            Console.WriteLine("  No matching entries.");
        }
        else
        {
            foreach (var pair in report.EntriesByLevel)
            {
                Console.WriteLine($"  {pair.Key,-7} {pair.Value,6}");
            }
        }

        Console.WriteLine();
        Console.WriteLine("Most frequent messages:");

        if (report.MostFrequentMessages.Count == 0)
        {
            Console.WriteLine("  No messages to display.");
        }
        else
        {
            foreach (var item in report.MostFrequentMessages)
            {
                Console.WriteLine($"  {item.Count,4}x  {item.Message}");
            }
        }
    }

    private static void PrintPreview(IReadOnlyList<LogEntry> entries)
    {
        const int previewCount = 10;

        Console.WriteLine();
        Console.WriteLine($"Preview (first {Math.Min(previewCount, entries.Count)} entries):");

        foreach (var entry in entries.Take(previewCount))
        {
            Console.WriteLine(
                $"  {entry.Timestamp:yyyy-MM-dd HH:mm:ss} " +
                $"[{entry.Level,-5}] {entry.Message}");
        }

        if (entries.Count > previewCount)
        {
            Console.WriteLine($"  ... and {entries.Count - previewCount} more.");
        }
    }

    private static string FormatTimestamp(DateTime? timestamp) =>
        timestamp?.ToString("yyyy-MM-dd HH:mm:ss") ?? "n/a";
}
