using System.Text.Json;
using FadeevCalculatorLib.Infrastructure;
using FadeevCalculatorLib.Models;

namespace FadeevCalculatorLib.Services;

public sealed class SettingsService
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

    public SettingsService(string? filePath = null)
    {
        _filePath = filePath ?? AppPaths.GetFilePath("settings.json");
    }

    public ThemeSettings Load()
    {
        try
        {
            if (!File.Exists(_filePath))
                return ThemeSettings.Default;

            using var stream = File.OpenRead(_filePath);
            var settings = JsonSerializer.Deserialize<ThemeSettings>(stream, ReadOptions);
            return settings ?? ThemeSettings.Default;
        }
        catch
        {
            return ThemeSettings.Default;
        }
    }

    public void Save(ThemeSettings settings)
    {
        EnsureDirectory();
        var json = JsonSerializer.Serialize(settings, WriteOptions);
        File.WriteAllText(_filePath, json);
    }

    private void EnsureDirectory()
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);
    }
}

