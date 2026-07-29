namespace LogScope.Models;

public sealed record MessageFrequency(string Message, int Count);

public sealed record LogReport(
    int TotalEntries,
    DateTime? FirstTimestamp,
    DateTime? LastTimestamp,
    IReadOnlyDictionary<string, int> EntriesByLevel,
    IReadOnlyList<MessageFrequency> MostFrequentMessages);
