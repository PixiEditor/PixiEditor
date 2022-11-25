using System.Windows.Markup;

namespace PixiEditor.ViewModels;
internal class MainVM : MarkupExtension
{
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return ViewModelMain.Current;
    }
}
