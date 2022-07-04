using System;

namespace PixiEditor.Helpers.Converters;

/// <summary>
/// Use this if you want to share the same converter over the whole application. <para/> Do not use this if your converter has properties.
/// </summary>
public abstract class SingleInstanceConverter<TThis> : MarkupConverter
    where TThis : SingleInstanceConverter<TThis>
{
    private static SingleInstanceConverter<TThis> instance;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (instance is null)
        {
            instance = this;
        }

        return instance;
    }
}