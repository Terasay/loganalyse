using System.Text;
using System.Text.Json;
using LogScope.Models;

namespace LogScope.Services;

public sealed class ExportService
{
    public void Export(
        IReadOnlyCollection<LogEntry> entries,
        string outputPath,
        string format)
    {
        var fullPath = Path.GetFullPath(outputPath);
        var directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        switch (format.ToLowerInvariant())
        {
            case "csv":
                ExportCsv(entries, fullPath);
                break;

            case "json":
                ExportJson(entries, fullPath);
                break;

            default:
                throw new ArgumentException($"Unsupported export format: {format}");
        }
    }

    private static void ExportCsv(
        IEnumerable<LogEntry> entries,
        string outputPath)
    {
        using var writer = new StreamWriter(
            outputPath,
            append: false,
            encoding: new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        writer.WriteLine("timestamp,level,message,line_number");

        foreach (var entry in entries)
        {
            writer.WriteLine(string.Join(",",
                EscapeCsv(entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff")),
                EscapeCsv(entry.Level),
                EscapeCsv(entry.Message),
                entry.LineNumber));
        }
    }

    private static void ExportJson(
        IEnumerable<LogEntry> entries,
        string outputPath)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        File.WriteAllText(
            outputPath,
            JsonSerializer.Serialize(entries, options),
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
