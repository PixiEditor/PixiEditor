using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PixiEditor.Models.Commands.XAML;

internal class CommandExists : MarkupExtension
{
    public string Name { get; set; }

    public CommandExists() { }
    public CommandExists(string name) => Name = name;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (Design.IsDesignMode)
        {
            return true;
        }

        if (CommandController.Current.Commands.ContainsKey(Name))
        {
            return true;
        }

        return false;
    }
}
