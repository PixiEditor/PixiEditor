using System.Linq;
using Avalonia.Markup.Xaml;

namespace PixiEditor.ViewModels;

internal class ToolVM : MarkupExtension
{
    public string TypeName { get; set; }

    public ToolVM(string typeName) => TypeName = typeName;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (ViewModelMain.Current?.ToolsSubViewModel?.ActiveToolSet?.Tools == null)
            return null;

        if (string.IsNullOrEmpty(TypeName))
        {
            return null;
        }

        return (ViewModelMain.Current?.ToolsSubViewModel.ActiveToolSet?.Tools).Where(x => x != null).FirstOrDefault(tool => tool?.GetType()?.Name == TypeName);
    }
}
