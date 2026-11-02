using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.Models.Handlers;
using PixiEditor.ViewModels.Document;
using PixiEditor.ViewModels.Document.Nodes;

namespace PixiEditor.ViewModels.Layers;

internal partial class FiltersPopupViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial IStructureMemberHandler StructureMember { get; set; }

    [ObservableProperty]
    public partial DocumentViewModel Document { get; set; }

    public FiltersPopupViewModel()
    {

    }
}
