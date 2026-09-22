using System.Windows.Input;

namespace PixiEditor.Models.Handlers;

public class ContextualOption
{
    public string Name { get; set; }
    public ICommand ExecuteCommand { get; set; }
    public string Icon { get; set; }
}
