using PixiEditor.Avalonia.ViewModels;
using PixiEditor.Models.Containers;

namespace PixiEditor.ViewModels.SubViewModels;

internal class ToolsViewModel : SubViewModel<MainViewModel>, IToolsHandler
{
    public ToolsViewModel(MainViewModel owner) : base(owner)
    {
    }

    public void SetTool(object parameter)
    {
        throw new NotImplementedException();
    }
}
