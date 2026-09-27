using System.Globalization;

namespace PluginExample;

/// <summary>
/// Exact arithmetic on decimal numbers: + - * / % ^, parentheses, unary
/// minus, the constants pi and e, and sqrt, abs, round(x[, digits]), floor,
/// ceil, min(a, b...), max(a, b...). Decimal (28-29 significant digits) keeps
/// money and measurements exact where binary floating point would not
/// (0.1 + 0.2 = 0.3). A recursive-descent parser: expression -> terms ->
/// factors -> powers -> unary -> primary.
/// </summary>
public static class Calculator
{
    public static decimal Evaluate(string expression)
    {
        var p = new Parser(expression);
        decimal value = p.Expression();
        p.SkipSpaces();
        if (!p.AtEnd) throw new FormatException($"Unexpected '{p.Current}' at position {p.Position + 1}.");
        return value;
    }

    /// <summary>The value written back to the user or the model: no trailing zeros.</summary>
    public static string Format(decimal value) =>
        value.ToString("0.############################", CultureInfo.InvariantCulture);

    private sealed class Parser(string text)
    {
        private int _pos;
        public bool AtEnd => _pos >= text.Length;
        public char Current => text[_pos];
        public int Position => _pos;

        public void SkipSpaces()
        {
            while (!AtEnd && char.IsWhiteSpace(Current)) _pos++;
        }

        private bool Accept(char c)
        {
            SkipSpaces();
            if (!AtEnd && Current == c) { _pos++; return true; }
            return false;
        }

        private void Expect(char c)
        {
            if (!Accept(c)) throw new FormatException($"'{c}' expected at position {_pos + 1}.");
        }

        public decimal Expression()
        {
            decimal v = Term();
            while (true)
            {
                if (Accept('+')) v += Term();
                else if (Accept('-')) v -= Term();
                else return v;
            }
        }

        private decimal Term()
        {
            decimal v = Power();
            while (true)
            {
                if (Accept('*') || Accept('×')) v *= Power();
                else if (Accept('/') || Accept('÷'))
                {
                    decimal d = Power();
                    if (d == 0) throw new DivideByZeroException("Division by zero.");
                    v /= d;
                }
                else if (Accept('%'))
                {
                    decimal d = Power();
                    if (d == 0) throw new DivideByZeroException("Division by zero.");
                    v %= d;
                }
                else return v;
            }
        }

        private decimal Power()
        {
            decimal b = Unary();
            if (!Accept('^')) return b;
            decimal e = Power();                      // right-associative: 2^3^2 = 2^9
            return Pow(b, e);
        }

        private decimal Unary()
        {
            if (Accept('-')) return -Unary();
            if (Accept('+')) return Unary();
            return Primary();
        }

        private decimal Primary()
        {
            SkipSpaces();
            if (AtEnd) throw new FormatException("The expression ends too early.");
            if (Accept('('))
            {
                decimal v = Expression();
                Expect(')');
                return v;
            }
            if (char.IsDigit(Current) || Current == '.') return Number();
            if (char.IsLetter(Current)) return NameOrCall();
            throw new FormatException($"Unexpected '{Current}' at position {_pos + 1}.");
        }

        private decimal Number()
        {
            int start = _pos;
            while (!AtEnd && (char.IsDigit(Current) || Current == '.')) _pos++;
            string s = text[start.._pos];
            if (!decimal.TryParse(s, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var v))
                throw new FormatException($"'{s}' is not a number.");
            return v;
        }

        private decimal NameOrCall()
        {
            int start = _pos;
            while (!AtEnd && char.IsLetter(Current)) _pos++;
            string name = text[start.._pos].ToLowerInvariant();
            if (name == "pi") return 3.1415926535897932384626433833m;
            if (name == "e") return 2.7182818284590452353602874714m;

            Expect('(');
            var args = new List<decimal> { Expression() };
            while (Accept(',')) args.Add(Expression());
            Expect(')');

            decimal One() => args.Count == 1 ? args[0] : throw new FormatException($"{name} takes one argument.");
            return name switch
            {
                "sqrt" => One() < 0 ? throw new ArgumentException("sqrt of a negative number.") : Sqrt(One()),
                "abs" => Math.Abs(One()),
                "floor" => Math.Floor(One()),
                "ceil" => Math.Ceiling(One()),
                "round" => args.Count switch
                {
                    1 => Math.Round(args[0], MidpointRounding.AwayFromZero),
                    2 => Math.Round(args[0], (int)args[1], MidpointRounding.AwayFromZero),
                    _ => throw new FormatException("round takes one or two arguments."),
                },
                "min" => args.Min(),
                "max" => args.Max(),
                _ => throw new FormatException($"Unknown function '{name}'."),
            };
        }
    }

    private static decimal Pow(decimal b, decimal e)
    {
        if (e == Math.Floor(e) && Math.Abs(e) <= 1000)
        {
            decimal result = 1;
            for (int i = 0; i < (int)Math.Abs(e); i++) result *= b;
            if (e < 0)
            {
                if (result == 0) throw new DivideByZeroException("Division by zero.");
                result = 1 / result;
            }
            return result;
        }
        return (decimal)Math.Pow((double)b, (double)e);   // fractional exponents: double precision
    }

    private static decimal Sqrt(decimal x)
    {
        if (x == 0) return 0;
        decimal guess = (decimal)Math.Sqrt((double)x);   // then Newton steps at decimal precision
        for (int i = 0; i < 6; i++) guess = (guess + x / guess) / 2;
        return guess;
    }
}
