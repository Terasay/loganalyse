# LogScope

**LogScope** is a lightweight command-line log analyzer written in C# and .NET 8. It reads structured application logs, filters entries, calculates summary statistics, finds repeated messages, and exports results to CSV or JSON.

The project is deliberately dependency-free and demonstrates practical work with file I/O, regular expressions, LINQ, CLI argument parsing, error handling, serialization, and a modular project structure.

## Features

- Parses logs in the format `[timestamp] [level] message`
- Supports `TRACE`, `DEBUG`, `INFO`, `WARN`, `WARNING`, `ERROR`, and `FATAL`
- Filters by one or several log levels
- Filters by message text and date range
- Displays counts by level and the most frequent messages
- Shows a preview of matching entries
- Exports filtered entries to CSV or JSON
- Skips malformed lines without crashing
- Uses no third-party NuGet packages

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Run

```bash
dotnet run --project src/LogScope -- --file sample/sample.log
```

Filter only warnings and errors:

```bash
dotnet run --project src/LogScope -- \
  --file sample/sample.log \
  --level WARN,ERROR,FATAL
```

Search messages and export the result:

```bash
dotnet run --project src/LogScope -- \
  --file sample/sample.log \
  --contains database \
  --export output/database-errors.json \
  --format json
```

Filter by date range:

```bash
dotnet run --project src/LogScope -- \
  --file sample/sample.log \
  --from 2026-07-29T18:42:00 \
  --to 2026-07-29T18:43:00
```

## CLI options

| Option | Description |
|---|---|
| `-f`, `--file <path>` | Path to the input log file |
| `-l`, `--level <levels>` | Comma-separated log levels |
| `-c`, `--contains <text>` | Message substring filter |
| `--from <date>` | Start date or date-time |
| `--to <date>` | End date or date-time |
| `--top <number>` | Number of frequent messages to display |
| `-e`, `--export <path>` | Export destination |
| `--format <csv\|json>` | Export format |
| `-h`, `--help` | Show help |

## Project structure

```text
LogScope/
├── sample/
│   └── sample.log
├── src/LogScope/
│   ├── Cli/
│   │   └── CliOptions.cs
│   ├── Models/
│   │   ├── LogEntry.cs
│   │   ├── LogParseResult.cs
│   │   └── LogReport.cs
│   ├── Services/
│   │   ├── ExportService.cs
│   │   ├── LogAnalyzer.cs
│   │   └── LogParser.cs
│   ├── LogScope.csproj
│   └── Program.cs
├── .gitignore
├── LICENSE
└── README.md
```

## Possible improvements

- Add unit tests with xUnit
- Support custom log formats through configuration
- Add streaming analysis for very large files
- Build a desktop UI with Avalonia or WPF
- Add charts and HTML reports

## License

MIT
