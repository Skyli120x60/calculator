namespace FadeevCalculatorLib.Models;

public sealed class ThemeSettings
{
    public static readonly ThemeSettings Default = new(isDarkTheme: true, accentColor: "#1E90FF");

    public bool IsDarkTheme { get; set; } = true;
    public string AccentColor { get; set; } = "#1E90FF";

    public ThemeSettings() { }

    public ThemeSettings(bool isDarkTheme, string accentColor)
    {
        IsDarkTheme = isDarkTheme;
        AccentColor = accentColor;
    }
}

