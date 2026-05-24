using FadeevCalculatorLib.Calculators;
using FadeevCalculatorLib.Models;

namespace FadeevCalculatorTests;

public sealed class CalculatorEngineTests
{
    private static CalculatorEngine CreateEngine()
    {
        var root = Path.Combine(Path.GetTempPath(), "FadeevCalculatorTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return new CalculatorEngine(
            historyFilePath: Path.Combine(root, "history.json"),
            settingsFilePath: Path.Combine(root, "settings.json"));
    }

   
    [Theory]
    [InlineData("2+3", 5)]
    [InlineData("10-4", 6)]
    [InlineData("7*8", 56)]
    [InlineData("20/5", 4)]
    [InlineData("2^10", 1024)]
    public void Calculate_BasicOperations_ReturnCorrectResult(string expression, decimal expected)
    {
        var engine = CreateEngine();
        Assert.Equal(expected, engine.Calculate(expression));
    }

    [Theory]
    [InlineData("1.5+2.5", 4)]
    [InlineData("0.1+0.2", 0.3)]      
    [InlineData("10.0/4.0", 2.5)]
    [InlineData("1,5+2,5", 4)]        // запятая как разделитель
    [InlineData("3,14*2", 6.28)]
    public void Calculate_DecimalNumbers_ReturnCorrectResult(string expression, decimal expected)
    {
        var engine = CreateEngine();
        Assert.Equal(expected, engine.Calculate(expression));
    }

    
    [Theory]
    [InlineData("2+3*4", 14)]   // * перед +
    [InlineData("(2+3)*4", 20)]   // скобки меняют порядок
    [InlineData("2^3^2", 512)]  // ^ правоассоциативен: 2^(3^2) = 2^9
    [InlineData("10-2-3", 5)]    // - левоассоциативен: (10-2)-3
    [InlineData("2*3+4*5", 26)]
    public void Calculate_PrecedenceAndAssociativity_IsCorrect(string expression, decimal expected)
    {
        var engine = CreateEngine();
        Assert.Equal(expected, engine.Calculate(expression));
    }

   

    [Theory]
    [InlineData("-5+2", -3)]
    [InlineData("3*-2", -6)]
    [InlineData("(-5)*(-2)", 10)]
    [InlineData("-2^2", -4)]   // -(2^2), а не (-2)^2
    public void Calculate_UnaryMinus_IsHandledCorrectly(string expression, decimal expected)
    {
        var engine = CreateEngine();
        Assert.Equal(expected, engine.Calculate(expression));
    }

    

    [Theory]
    [InlineData("5^0", 1)]
    [InlineData("5^1", 5)]
    [InlineData("2^(-2)", 0.25)]    // отрицательный показатель
    public void Calculate_Power_EdgeCases(string expression, decimal expected)
    {
        var engine = CreateEngine();
        Assert.Equal(expected, engine.Calculate(expression));
    }

    [Fact]
    public void Calculate_Power_FractionalExponent_ThrowsNotSupported()
    {
        var engine = CreateEngine();
        Assert.Throws<NotSupportedException>(() => engine.Calculate("2^0.5"));
    }

   

    [Fact]
    public void Calculate_DivisionByZero_Throws()
    {
        var engine = CreateEngine();
        Assert.Throws<DivideByZeroException>(() => engine.Calculate("8/0"));
    }

    [Fact]
    public void Calculate_Overflow_Throws()
    {
        var engine = CreateEngine();
        Assert.Throws<OverflowException>(() => engine.Calculate("79228162514264337593543950335*2"));
    }

    [Theory]
    [InlineData("(2+3")]        // незакрытая скобка
    [InlineData("2+3)")]        // лишняя закрывающая
    [InlineData("")]            // пустое выражение
    [InlineData("   ")]         // только пробелы
    [InlineData("2@3")]         // недопустимый символ
    [InlineData("2++3")]        // два оператора подряд
    [InlineData("+")]           // только оператор
    public void Calculate_InvalidExpression_ThrowsFormatException(string expression)
    {
        var engine = CreateEngine();
        Assert.Throws<FormatException>(() => engine.Calculate(expression));
    }

    

    [Fact]
    public void Calculate_AddsToHistory()
    {
        var engine = CreateEngine();
        engine.Calculate("2+3");
        Assert.Single(engine.GetHistory());
    }

    [Fact]
    public void Evaluate_DoesNotAddToHistory()
    {
        var engine = CreateEngine();
        engine.Evaluate("2+3");
        Assert.Empty(engine.GetHistory());
    }

    [Fact]
    public void Calculate_HistoryCapAt200_OldestItemsDropped()
    {
        var engine = CreateEngine();
        for (int i = 0; i < 205; i++)
            engine.Calculate("1+1");

        Assert.Equal(200, engine.GetHistory().Count);
    }

    [Fact]
    public void History_IsPersisted_AcrossEngineInstances()
    {
        var root = Path.Combine(Path.GetTempPath(), "FadeevCalculatorTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var historyPath = Path.Combine(root, "history.json");
        var settingsPath = Path.Combine(root, "settings.json");

        var engine1 = new CalculatorEngine(historyPath, settingsPath);
        engine1.Calculate("2+2");
        engine1.SaveAll();

        var engine2 = new CalculatorEngine(historyPath, settingsPath);
        Assert.Single(engine2.GetHistory());
        Assert.Equal("2+2", engine2.GetHistory()[0].Expression);
    }

    // ──────────────────────────────────────────────
    // ClearHistory
    // ──────────────────────────────────────────────

    [Fact]
    public void ClearHistory_RemovesAllItems()
    {
        var engine = CreateEngine();
        engine.Calculate("2+2");
        engine.Calculate("3+3");

        engine.ClearHistory();

        Assert.Empty(engine.GetHistory());
    }

    [Fact]
    public void ClearHistory_IsPersisted_AcrossEngineInstances()
    {
        var root = Path.Combine(Path.GetTempPath(), "FadeevCalculatorTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var historyPath = Path.Combine(root, "history.json");
        var settingsPath = Path.Combine(root, "settings.json");

        var engine1 = new CalculatorEngine(historyPath, settingsPath);
        engine1.Calculate("2+2");
        engine1.ClearHistory(); // SaveAll вызывается внутри

        var engine2 = new CalculatorEngine(historyPath, settingsPath);
        Assert.Empty(engine2.GetHistory());
    }

    // ──────────────────────────────────────────────
    // ThemeSettings
    // ──────────────────────────────────────────────

    [Fact]
    public void GetThemeSettings_ReturnsDefault_OnFreshEngine()
    {
        var engine = CreateEngine();
        var settings = engine.GetThemeSettings();

        Assert.True(settings.IsDarkTheme);
        Assert.Equal("#1E90FF", settings.AccentColor);
    }

    [Fact]
    public void UpdateThemeSettings_ChangesAreReflected()
    {
        var engine = CreateEngine();
        var updated = new ThemeSettings(isDarkTheme: false, accentColor: "#FF0000");

        engine.UpdateThemeSettings(updated);

        var result = engine.GetThemeSettings();
        Assert.False(result.IsDarkTheme);
        Assert.Equal("#FF0000", result.AccentColor);
    }

    [Fact]
    public void UpdateThemeSettings_IsPersisted_AcrossEngineInstances()
    {
        var root = Path.Combine(Path.GetTempPath(), "FadeevCalculatorTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var historyPath = Path.Combine(root, "history.json");
        var settingsPath = Path.Combine(root, "settings.json");

        var engine1 = new CalculatorEngine(historyPath, settingsPath);
        engine1.UpdateThemeSettings(new ThemeSettings(isDarkTheme: false, accentColor: "#AABBCC"));

        var engine2 = new CalculatorEngine(historyPath, settingsPath);
        var settings = engine2.GetThemeSettings();

        Assert.False(settings.IsDarkTheme);
        Assert.Equal("#AABBCC", settings.AccentColor);
    }

    [Fact]
    public void UpdateThemeSettings_NullArgument_ThrowsArgumentNullException()
    {
        var engine = CreateEngine();
        Assert.Throws<ArgumentNullException>(() => engine.UpdateThemeSettings(null!));
    }
}

