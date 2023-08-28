using System.Linq;
using System.Windows.Markup;
using Avalonia.Markup.Xaml;
using PixiEditor.AvaloniaUI.ViewModels;

namespace PixiEditor.ViewModels;

internal class ToolVM : MarkupExtension
{
    public string TypeName { get; set; }

    public ToolVM(string typeName) => TypeName = typeName;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return ViewModelMain.Current?.ToolsSubViewModel.ToolSet?.Where(tool => tool.GetType().Name == TypeName).FirstOrDefault();
    }
}
