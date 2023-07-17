using Avalonia.Data;
using Avalonia.Markup.Xaml;

namespace PixiEditor.Helpers;

public class LocalizationExtension : MarkupExtension
{
    private LocalizationExtensionToProvide toProvide;
    private static Binding flowDirectionBinding;

    public LocalizationExtension(LocalizationExtensionToProvide toProvide)
    {
        
    }
    
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        switch (toProvide)
        {
            case LocalizationExtensionToProvide.FlowDirection:
                return GetFlowDirectionBinding(serviceProvider);
        }

        throw new NotImplementedException();
    }

    private object GetFlowDirectionBinding(IServiceProvider serviceProvider)
    {
        /*flowDirectionBinding = new Binding("CurrentLanguage.FlowDirection");
        flowDirectionBinding.Source = ViewModelMain.Current.LocalizationProvider;
        flowDirectionBinding.Mode = BindingMode.OneWay;

        var expression = (BindingExpression)flowDirectionBinding.ProvideValue(serviceProvider);

        ViewModelMain.Current.LocalizationProvider.OnLanguageChanged += _ => expression.UpdateTarget();

        return expression;*/
        return null;
    }
}

public enum LocalizationExtensionToProvide
{
    FlowDirection
}
