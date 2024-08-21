using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public delegate (bool validationResult, object? closestValidValue) ValidateProperty(object? value);

public class PropertyValidator
{
    public List<ValidateProperty> Rules { get; } = new();

    public PropertyValidator Min<T>(T min, Func<T, T>? adjust = null) where T : IComparable<T>
    {
        Rules.Add((value) =>
        {
            if (value is T val)
            {
                bool isValid = val.CompareTo(min) >= 0;
                return (isValid, isValid ? val : GetReturnValue(val, min, adjust));
            }

            return (false, GetReturnValue(min, min, adjust));
        });

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

    /*public PropertyValidator Select<T>(Func<T, object> selector)
    {
        PropertyValidator newValidator = new();

        newValidator.Rules.Add(v =>
        {
            if (v is T val)
            {
                return (true, selector(val));
            }

            return (false, v);
        });

        return newValidator;
    }

    public void All(params PropertyValidator[] validators)
    {
        Rules.Add(v =>
        {
            foreach (var validator in validators)
            {
                if (!validator.Validate(v))
                {
                    return (false, validator.GetClosestValidValue(v));
                }
            }

            return (true, v);
        });
    }*/

    public bool Validate(object? value)
    {
        object lastValue = value;

        foreach (var rule in Rules)
        {
            var (isValid, toPass) = rule(lastValue);
            lastValue = toPass;
            if (!isValid)
            {
                return false;
            }
        }

        return true;
    }

    public object? GetClosestValidValue(object? o)
    {
        return Rules.Aggregate(o, (current, rule) => rule(current).closestValidValue);
    }
}
