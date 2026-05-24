using System.Windows;
using System.Windows.Input;
using FadeevCalculatorApp.ViewModels;

namespace FadeevCalculatorApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
        Closing += (_, _) => ((MainViewModel)DataContext!).Persist();
    }

    private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (DataContext is not MainViewModel vm || string.IsNullOrEmpty(e.Text))
            return;

        char ch = e.Text[0];
        if (char.IsDigit(ch) || ch is '.' or ',' or '+' or '-' or '*' or '/' or '^' or '(' or ')')
        {
            vm.AppendToken(e.Text);
            e.Handled = true;
        }
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (DataContext is not MainViewModel vm)
            return;

        if (e.Key is Key.Enter or Key.Return)
        {
            if (vm.EvaluateCommand.CanExecute(null))
                vm.EvaluateCommand.Execute(null);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Back)
        {
            vm.Backspace();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Escape)
        {
            if (vm.ClearAllCommand.CanExecute(null))
                vm.ClearAllCommand.Execute(null);
            e.Handled = true;
        }
    }
}