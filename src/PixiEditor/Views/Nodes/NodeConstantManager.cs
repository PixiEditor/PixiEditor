using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls.Primitives;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Views.Nodes;

internal class NodeConstantManager : TemplatedControl
{
    
    public static readonly StyledProperty<ObservableCollection<NodeGraphConstantViewModel>> ConstantsProperty =
        AvaloniaProperty.Register<NodeConstantManager, ObservableCollection<NodeGraphConstantViewModel>>(nameof(Constants));

    public ObservableCollection<NodeGraphConstantViewModel> Constants
    {
        get => GetValue(ConstantsProperty);
        set => SetValue(ConstantsProperty, value);
    }
}
