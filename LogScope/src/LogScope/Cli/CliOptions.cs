using System.Globalization;

namespace LogScope.Cli;

public sealed class CliOptions
{
    public string? FilePath { get; private set; }
    public HashSet<string> Levels { get; } = new(StringComparer.OrdinalIgnoreCase);
    public string? ContainsText { get; private set; }
    public DateTime? From { get; private set; }
    public DateTime? To { get; private set; }
    public int TopCount { get; private set; } = 5;
    public string? ExportPath { get; private set; }
    public string ExportFormat { get; private set; } = "csv";
    public bool ShowHelp { get; private set; }

    public static CliOptions Parse(string[] args)
    {
        var options = new CliOptions();

        for (var i = 0; i < args.Length; i++)
        {
            var argument = args[i];

            switch (argument)
            {
                case "-h":
                case "--help":
                    options.ShowHelp = true;
                    break;

                case "-f":
                case "--file":
                    options.FilePath = ReadValue(args, ref i, argument);
                    break;

                case "-l":
                case "--level":
                    foreach (var level in ReadValue(args, ref i, argument)
                                 .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    {
                        options.Levels.Add(NormalizeLevel(level));
                    }
                    break;

                case "-c":
                case "--contains":
                    options.ContainsText = ReadValue(args, ref i, argument);
                    break;

                case "--from":
                    options.From = ParseDate(ReadValue(args, ref i, argument), argument);
                    break;

                case "--to":
                    options.To = ParseDate(ReadValue(args, ref i, argument), argument);
                    break;

                case "--top":
                    var topValue = ReadValue(args, ref i, argument);
                    if (!int.TryParse(topValue, out var topCount) || topCount < 1 || topCount > 100)
                    {
                        throw new ArgumentException("--top must be an integer between 1 and 100.");
                    }

                    options.TopCount = topCount;
                    break;

                case "-e":
                case "--export":
                    options.ExportPath = ReadValue(args, ref i, argument);
                    break;

                case "--format":
                    options.ExportFormat = ReadValue(args, ref i, argument).ToLowerInvariant();
                    if (options.ExportFormat is not ("csv" or "json"))
                    {
                        throw new ArgumentException("--format must be either 'csv' or 'json'.");
                    }
                    break;

                default:
                    throw new ArgumentException($"Unknown argument: {argument}");
            }
        }

        if (options.From.HasValue && options.To.HasValue && options.From.Value > options.To.Value)
        {
            throw new ArgumentException("--from cannot be later than --to.");
        }

        return options;
    }

    public static string HelpText => """
LogScope - small command-line log analyzer

Usage:
  dotnet run --project src/LogScope -- --file <path> [options]

Options:
  -f, --file <path>        Path to a log file (required)
  -l, --level <levels>     Filter by level, comma-separated (INFO,ERROR)
  -c, --contains <text>    Keep entries whose message contains text
      --from <date>        Start date/time, e.g. 2026-07-29 or 2026-07-29T12:00:00
      --to <date>          End date/time
      --top <number>       Number of frequent messages to show (default: 5)
  -e, --export <path>      Export filtered entries
      --format <csv|json>  Export format (default: csv)
  -h, --help               Show this help

Supported log format:
  [2026-07-29 18:42:01] [ERROR] Database connection failed
""";

    private static string ReadValue(string[] args, ref int index, string argument)
    {
        if (index + 1 >= args.Length)
        {
            throw new ArgumentException($"Missing value for {argument}.");
        }

        index++;
        return args[index];
    }

    private static DateTime ParseDate(string value, string argument)
    {
        var formats = new[]
        {
            "yyyy-MM-dd",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-dd HH:mm:ss"
        };

        if (DateTime.TryParseExact(
                value,
                formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal,
                out var result))
        {
            return result;
        }

        throw new ArgumentException(
            $"Invalid date for {argument}. Use yyyy-MM-dd or yyyy-MM-ddTHH:mm:ss.");
    }

    private static string NormalizeLevel(string level) =>
        level.Equals("WARNING", StringComparison.OrdinalIgnoreCase)
            ? "WARN"
            : level.ToUpperInvariant();
}
