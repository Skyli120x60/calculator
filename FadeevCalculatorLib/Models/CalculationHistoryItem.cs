using System.Text.Json.Serialization;

namespace FadeevCalculatorLib.Models;

public sealed class CalculationHistoryItem
{
    [JsonConstructor]
    public CalculationHistoryItem(string expression, decimal result, DateTime timestamp)
    {
        Expression = expression;
        Result = result;
        Timestamp = timestamp;
    }

    public string Expression { get; }
    public decimal Result { get; }
    public DateTime Timestamp { get; }
}
