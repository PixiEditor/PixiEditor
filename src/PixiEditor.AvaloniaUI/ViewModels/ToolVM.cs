using System.Linq;
using Avalonia.Markup.Xaml;

namespace PixiEditor.AvaloniaUI.ViewModels;

internal class ToolVM : MarkupExtension
{
    public string TypeName { get; set; }

    public ToolVM(string typeName) => TypeName = typeName;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return ViewModelMain.Current?.ToolsSubViewModel.ToolSet?.Where(tool => tool.GetType().Name == TypeName).FirstOrDefault();
    }
}
