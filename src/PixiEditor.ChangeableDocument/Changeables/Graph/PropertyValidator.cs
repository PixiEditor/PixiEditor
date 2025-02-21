using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public delegate ValidatorResult ValidateProperty(object? value);

public class PropertyValidator
{
    public List<ValidateProperty> Rules { get; } = new();

    public PropertyValidator Min(VecI min)
    {
       return Min(min, v => new VecI(Math.Max(v.X, min.X), Math.Max(v.Y, min.Y))); 
    }
    
    public PropertyValidator Min(VecD min)
    {
        return Min(min, v => new VecD(Math.Max(v.X, min.X), Math.Max(v.Y, min.Y))); 
    }

    public PropertyValidator Min<T>(T min, Func<T, T>? adjust = null) where T : IComparable<T>
    {
        Rules.Add((value) =>
        {
            if (value is T val)
            {
                bool isValid = val.CompareTo(min) >= 0;
                return new (isValid, isValid ? val : GetReturnValue(val, min, adjust));
            }

            return new (false, GetReturnValue(min, min, adjust));
        });

        return this;
    }

    public PropertyValidator Custom(ValidateProperty rule)
    {
        Rules.Add(rule);
        return this;
    }

    private object? GetReturnValue<T>(T original, T min, Func<T, T>? fallback) where T : IComparable<T>
    {
        if (fallback != null)
        {
            return fallback(original);
        }

        return min;
    }

    public bool Validate(object? value, out string? errors)
    {
        object lastValue = value;

        foreach (var rule in Rules)
        {
            var result = rule(lastValue);
            lastValue = result.ClosestValidValue;
            if (!result.IsValid)
            {
                errors = result.ErrorMessage;
                return false;
            }
        }

        errors = null;
        return true;
    }

    public object? GetClosestValidValue(object? o)
    {
        return Rules.Aggregate(o, (current, rule) => rule(current).ClosestValidValue);
    }
}

public record ValidatorResult
{
    public bool IsValid { get; }
    public object? ClosestValidValue { get; }
    public string? ErrorMessage { get; }

    public ValidatorResult(bool isValid, string? errorMessage)
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    public ValidatorResult(bool isValid, object? closestValidValue)
    {
        IsValid = isValid;
        ClosestValidValue = closestValidValue;
    }
}
