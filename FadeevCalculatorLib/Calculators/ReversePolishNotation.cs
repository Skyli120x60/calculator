using System.Globalization;

namespace FadeevCalculatorLib.Calculators;

public static class ReversePolishNotation
{
    private enum OperatorKind
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        Power,
        UnaryMinus
    }

    private enum OperatorAssociativity
    {
        Left,
        Right
    }

    private abstract record Token;
    private sealed record NumberToken(decimal Value) : Token;
    private sealed record OperatorToken(OperatorKind Kind) : Token;
    private sealed record LeftParenToken : Token;
    private sealed record RightParenToken : Token;

    public static decimal Evaluate(string expression)
    {
        if (expression is null)
            throw new ArgumentNullException(nameof(expression));

        var tokens = Tokenize(expression);
        var rpn = ToRpn(tokens);
        return EvaluateRpn(rpn);
    }

    private static List<Token> Tokenize(string expression)
    {
        var tokens = new List<Token>();

        int i = 0;
        Token? previousToken = null;

        while (i < expression.Length)
        {
            var c = expression[i];

            if (char.IsWhiteSpace(c))
            {
                i++;
                continue;
            }

            if (c == '(')
            {
                tokens.Add(new LeftParenToken());
                previousToken = tokens[^1];
                i++;
                continue;
            }

            if (c == ')')
            {
                tokens.Add(new RightParenToken());
                previousToken = tokens[^1];
                i++;
                continue;
            }

            if (IsBinaryOperatorChar(c) || c == '-')
            {
                if (c == '-')
                {
                    bool unary = previousToken is null
                                 || previousToken is OperatorToken
                                 || previousToken is LeftParenToken;

                    tokens.Add(new OperatorToken(unary ? OperatorKind.UnaryMinus : OperatorKind.Subtract));
                }
                else
                {
                    tokens.Add(new OperatorToken(ToOperatorKind(c)));
                }

                previousToken = tokens[^1];
                i++;
                continue;
            }

            if (char.IsDigit(c) || c == '.' || c == ',')
            {
                int start = i;
                bool seenSeparator = false;

                i++; // consume first char
                while (i < expression.Length)
                {
                    char ch = expression[i];
                    if (char.IsDigit(ch))
                    {
                        i++;
                        continue;
                    }

                    if ((ch == '.' || ch == ',') && !seenSeparator)
                    {
                        seenSeparator = true;
                        i++;
                        continue;
                    }

                    break;
                }

                var numberText = expression.Substring(start, i - start).Replace(',', '.');
                if (numberText == "." || numberText == "-." || numberText == "-,")
                    throw new FormatException("Invalid number format.");

                if (!decimal.TryParse(numberText, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
                    throw new FormatException($"Invalid number: '{numberText}'.");

                tokens.Add(new NumberToken(value));
                previousToken = tokens[^1];
                continue;
            }

            throw new FormatException($"Unexpected character '{c}' in expression.");
        }

        if (tokens.Count == 0)
            throw new FormatException("Expression is empty.");

        return tokens;
    }

    // Примечание: унарный минус после ^ должен быть в скобках: 2^(-2), а не 2^-2.
    // Shunting-yard выталкивает ^ из стека до того, как правый операнд попадает в него.
    private static IReadOnlyList<Token> ToRpn(IReadOnlyList<Token> tokens)
    {
        var output = new List<Token>(tokens.Count);
        var opStack = new Stack<Token>();

        foreach (var token in tokens)
        {
            switch (token)
            {
                case NumberToken:
                    output.Add(token);
                    break;

                case OperatorToken opToken:
                    while (opStack.Count > 0)
                    {
                        var top = opStack.Peek();
                        if (top is not OperatorToken topOp)
                            break;

                        var (prec, assoc) = GetOperatorInfo(opToken.Kind);
                        var (topPrec, _) = GetOperatorInfo(topOp.Kind);

                        bool shouldPop =
                            assoc == OperatorAssociativity.Left
                                ? prec <= topPrec
                                : prec < topPrec;

                        if (!shouldPop)
                            break;

                        output.Add(opStack.Pop());
                    }

                    opStack.Push(opToken);
                    break;

                case LeftParenToken:
                    opStack.Push(token);
                    break;

                case RightParenToken:
                    while (opStack.Count > 0 && opStack.Peek() is not LeftParenToken)
                        output.Add(opStack.Pop());

                    if (opStack.Count == 0)
                        throw new FormatException("Mismatched parentheses in expression.");

                    opStack.Pop(); // remove '('
                    break;

                default:
                    throw new NotSupportedException("Unknown token.");
            }
        }

        while (opStack.Count > 0)
        {
            var token = opStack.Pop();
            if (token is LeftParenToken or RightParenToken)
                throw new FormatException("Mismatched parentheses in expression.");

            output.Add(token);
        }

        return output;
    }

    private static decimal EvaluateRpn(IReadOnlyList<Token> rpn)
    {
        var stack = new Stack<decimal>();

        foreach (var token in rpn)
        {
            switch (token)
            {
                case NumberToken num:
                    stack.Push(num.Value);
                    break;

                case OperatorToken op:
                    ApplyOperator(stack, op.Kind);
                    break;

                default:
                    throw new FormatException("Invalid RPN token stream.");
            }
        }

        if (stack.Count != 1)
            throw new InvalidOperationException("Expression has too many operands.");

        return stack.Pop();
    }

    private static void ApplyOperator(Stack<decimal> stack, OperatorKind kind)
    {
        switch (kind)
        {
            case OperatorKind.UnaryMinus:
            {
                if (stack.Count < 1)
                    throw new FormatException("Unary operator missing operand.");

                var a = stack.Pop();
                stack.Push(checked(-a));
                return;
            }

            case OperatorKind.Add:
                RequireOperands(stack, 2, "Addition");
                {
                    var b = stack.Pop();
                    var a = stack.Pop();
                    stack.Push(checked(a + b));
                }
                return;
            case OperatorKind.Subtract:
                RequireOperands(stack, 2, "Subtraction");
                {
                    var b = stack.Pop();
                    var a = stack.Pop();
                    stack.Push(checked(a - b));
                }
                return;
            case OperatorKind.Multiply:
                RequireOperands(stack, 2, "Multiplication");
                {
                    var b = stack.Pop();
                    var a = stack.Pop();
                    stack.Push(checked(a * b));
                }
                return;
            case OperatorKind.Divide:
                RequireOperands(stack, 2, "Division");
                {
                    var b = stack.Pop();
                    var a = stack.Pop();
                    if (b == 0m)
                        throw new DivideByZeroException("Division by zero.");
                    stack.Push(checked(a / b));
                }
                return;
            case OperatorKind.Power:
                RequireOperands(stack, 2, "Power");
                {
                    var exponent = stack.Pop();
                    var value = stack.Pop();
                    int exponentInt = ConvertExponentToInt(exponent);
                    stack.Push(PowDecimal(value, exponentInt));
                }
                return;
            default:
                throw new NotSupportedException($"Unknown operator: {kind}");
        }
    }

    private static void RequireOperands(Stack<decimal> stack, int count, string operationName)
    {
        if (stack.Count < count)
            throw new FormatException($"{operationName}: missing operand.");
    }

    private static int ConvertExponentToInt(decimal exponent)
    {
        var truncated = decimal.Truncate(exponent);
        if (exponent != truncated)
            throw new NotSupportedException("Power operator supports only integer exponents.");

        if (truncated < int.MinValue || truncated > int.MaxValue)
            throw new OverflowException("Exponent is out of supported range.");

        return (int)truncated;
    }

    private static decimal PowDecimal(decimal value, int exponent)
    {
        if (exponent == 0)
            return 1m;

        if (exponent < 0)
        {
            if (value == 0m)
                throw new DivideByZeroException("Division by zero in negative power.");

            long positiveExp = -(long)exponent; // safe even for int.MinValue
            decimal denom = PowDecimalPositive(value, positiveExp);
            return 1m / denom;
        }

        return PowDecimalPositive(value, exponent);
    }

    private static decimal PowDecimalPositive(decimal value, long exponent)
    {
        // Exponentiation by squaring with decimal overflow checks.
        checked
        {
            decimal result = 1m;
            decimal factor = value;
            long e = exponent;

            while (e > 0)
            {
                if ((e & 1L) == 1L)
                    result *= factor;

                e >>= 1;
                if (e > 0)
                    factor *= factor;
            }

            return result;
        }
    }

    private static bool IsBinaryOperatorChar(char c)
        => c is '+' or '*' or '/' or '^';

    private static OperatorKind ToOperatorKind(char c)
        => c switch
        {
            '+' => OperatorKind.Add,
            '*' => OperatorKind.Multiply,
            '/' => OperatorKind.Divide,
            '^' => OperatorKind.Power,
            _ => throw new NotSupportedException($"Unsupported operator char '{c}'.")
        };

    private static (int Precedence, OperatorAssociativity Associativity) GetOperatorInfo(OperatorKind kind)
        => kind switch
        {
            OperatorKind.Add => (1, OperatorAssociativity.Left),
            OperatorKind.Subtract => (1, OperatorAssociativity.Left),
            OperatorKind.Multiply => (2, OperatorAssociativity.Left),
            OperatorKind.Divide => (2, OperatorAssociativity.Left),
            OperatorKind.UnaryMinus => (3, OperatorAssociativity.Right),
            OperatorKind.Power => (4, OperatorAssociativity.Right),
            _ => throw new NotSupportedException($"Unsupported operator kind '{kind}'.")
        };
}

