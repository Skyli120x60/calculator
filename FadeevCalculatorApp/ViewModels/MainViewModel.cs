using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FadeevCalculatorLib.Calculators;
using FadeevCalculatorApp.Infrastructure;

namespace FadeevCalculatorApp.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly CalculatorEngine _engine;
    private readonly RelayCommand _evaluateCommand;

    private string _expression = string.Empty;
    private string _resultText = string.Empty;

    private string _pageBackground = "#000000";
    private string _panelBackground = "#000000";
    private string _keyBackground = "#000000";
    private string _keyForeground = "#000000";
    private string _accentKeyBackground = "#3ECFB5";
    private string _accentKeyForeground = "#0D1117";
    private string _secondaryText = "#888888";
    private string _displayBorder = "#2D3348";

    private string _windowTitle = string.Empty;
    private string _expressionLabelText = string.Empty;

    public MainViewModel()
    {
        _engine = new CalculatorEngine();

        AppendCommand = new RelayCommand(p => AppendToken(p as string ?? string.Empty));
        ClearAllCommand = new RelayCommand(_ => ClearAll());
        ClearEntryCommand = new RelayCommand(_ => ClearEntry());
        BackspaceCommand = new RelayCommand(_ => Backspace());

        _evaluateCommand = new RelayCommand(_ => EvaluateAndCommit(), _ => !string.IsNullOrWhiteSpace(Expression));
        EvaluateCommand = _evaluateCommand;

        WindowTitle = "Калькулятор Фадеева";
        ExpressionLabelText = "Выражение";

        _engine.ClearHistory();
        UpdateThemeColors();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Expression
    {
        get => _expression;
        set
        {
            if (SetProperty(ref _expression, value ?? string.Empty))
            {
                _evaluateCommand.RaiseCanExecuteChanged();
                UpdatePreview();
            }
        }
    }

    public string ResultText
    {
        get => _resultText;
        set => SetProperty(ref _resultText, value ?? string.Empty);
    }

    public string PageBackground { get => _pageBackground; private set => SetProperty(ref _pageBackground, value); }
    public string PanelBackground { get => _panelBackground; private set => SetProperty(ref _panelBackground, value); }
    public string KeyBackground { get => _keyBackground; private set => SetProperty(ref _keyBackground, value); }
    public string KeyForeground { get => _keyForeground; private set => SetProperty(ref _keyForeground, value); }
    public string AccentKeyBackground { get => _accentKeyBackground; private set => SetProperty(ref _accentKeyBackground, value); }
    public string AccentKeyForeground { get => _accentKeyForeground; private set => SetProperty(ref _accentKeyForeground, value); }
    public string SecondaryText { get => _secondaryText; private set => SetProperty(ref _secondaryText, value); }
    public string DisplayBorder { get => _displayBorder; private set => SetProperty(ref _displayBorder, value); }

    public string WindowTitle { get => _windowTitle; private set => SetProperty(ref _windowTitle, value); }
    public string ExpressionLabelText { get => _expressionLabelText; private set => SetProperty(ref _expressionLabelText, value); }

    public ICommand AppendCommand { get; }
    public ICommand ClearAllCommand { get; }
    public ICommand ClearEntryCommand { get; }
    public ICommand EvaluateCommand { get; }
    public ICommand BackspaceCommand { get; }

    public void Persist() => _engine.SaveAll();

    public void AppendToken(string token)
    {
        if (!string.IsNullOrEmpty(token))
            Expression += token;
    }

    public void Backspace()
    {
        if (!string.IsNullOrEmpty(Expression))
            Expression = Expression[..^1];
    }

    private void EvaluateAndCommit()
    {
        if (string.IsNullOrWhiteSpace(Expression))
            return;

        try
        {
            var result = _engine.Evaluate(Expression);
            ResultText = result.ToString(CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            ResultText = $"Ошибка: {ex.Message}";
        }
    }

    private void UpdatePreview()
    {
        if (string.IsNullOrWhiteSpace(Expression))
        {
            ResultText = string.Empty;
            return;
        }

        try
        {
            var result = _engine.Evaluate(Expression);
            ResultText = result.ToString(CultureInfo.InvariantCulture);
        }
        catch
        {
            // Выражение неполное — не показываем ошибку
        }
    }

    private void ClearAll()
    {
        Expression = string.Empty;
        ResultText = string.Empty;
    }

    private void ClearEntry()
    {
        if (string.IsNullOrWhiteSpace(Expression))
            return;

        var expr = Expression;
        int end = expr.Length - 1;

        while (end >= 0 && (char.IsDigit(expr[end]) || expr[end] == '.' || expr[end] == ','))
            end--;

        var removeStart = end + 1;

        if (removeStart > 0 && expr[removeStart - 1] == '-')
        {
            if (removeStart - 1 == 0)
                removeStart--;
            else
            {
                char prev = expr[removeStart - 2];
                if (!char.IsDigit(prev) && prev != '.' && prev != ',' && prev != ')')
                    removeStart--;
            }
        }

        Expression = expr.Remove(removeStart);
        ResultText = string.Empty;
    }

    private void UpdateThemeColors()
    {
        var settings = _engine.GetThemeSettings();

        if (settings.IsDarkTheme)
        {
            PageBackground = "#0D1117";
            PanelBackground = "#161B22";
            KeyBackground = "#21262D";
            KeyForeground = "#E6EDF3";
            SecondaryText = "#8B949E";
            DisplayBorder = "#30363D";
            AccentKeyBackground = "#3ECFB5";
            AccentKeyForeground = "#0D1117";
        }
        else
        {
            PageBackground = "#F0EDE8";
            PanelBackground = "#FFFFFF";
            KeyBackground = "#E8E4DD";
            KeyForeground = "#1C1917";
            SecondaryText = "#78716C";
            DisplayBorder = "#D6D3CD";
            AccentKeyBackground = "#0D9488";
            AccentKeyForeground = "#FFFFFF";
        }
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
