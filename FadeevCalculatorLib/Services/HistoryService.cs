using System.Text.Json;
using FadeevCalculatorLib.Infrastructure;
using FadeevCalculatorLib.Models;

namespace FadeevCalculatorLib.Services;

public sealed class HistoryService
{
    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly JsonSerializerOptions WriteOptions = new(ReadOptions)
    {
        WriteIndented = true
    };

    private readonly string _filePath;

    public HistoryService(string? filePath = null)
    {
        _filePath = filePath ?? AppPaths.GetFilePath("history.json");
    }

    public IReadOnlyList<CalculationHistoryItem> Load()
    {
        try
        {
            if (!File.Exists(_filePath))
                return Array.Empty<CalculationHistoryItem>();

            using var stream = File.OpenRead(_filePath);
            var items = JsonSerializer.Deserialize<List<CalculationHistoryItem>>(stream, ReadOptions);
            return items ?? new List<CalculationHistoryItem>();
        }
        catch
        {
            return Array.Empty<CalculationHistoryItem>();
        }
    }

    public void Save(IEnumerable<CalculationHistoryItem> items)
    {
        EnsureDirectory();
        var json = JsonSerializer.Serialize(items, WriteOptions);
        File.WriteAllText(_filePath, json);
    }

    private void EnsureDirectory()
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);
    }
}
