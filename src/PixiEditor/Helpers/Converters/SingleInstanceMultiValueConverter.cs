namespace PixiEditor.Helpers.Converters;

internal abstract class SingleInstanceMultiValueConverter<TThis> : MultiValueMarkupConverter
    where TThis : SingleInstanceMultiValueConverter<TThis>
{
    private static SingleInstanceMultiValueConverter<TThis> instance;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (instance is null)
        {
            instance = this;
        }

        return instance;
    }
}
