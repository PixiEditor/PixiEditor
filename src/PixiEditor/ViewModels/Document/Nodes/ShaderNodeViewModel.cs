using System.Collections.Specialized;
using Drawie.Backend.Core.Bridge;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.ViewModels.Nodes;
using PixiEditor.ViewModels.Nodes.Properties;

namespace PixiEditor.ViewModels.Document.Nodes;

[NodeViewModel("SHADER_NODE", "EFFECTS", PixiPerfectIcons.Terminal)]
internal class ShaderNodeViewModel : NodeViewModel<ShaderNode>
{
    public ShaderNodeViewModel()
    {
        Inputs.CollectionChanged += InputsOnCollectionChanged;
    }

    private void InputsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if(e.NewItems == null) return;

        foreach (var newItem in e.NewItems)
        {
            if (newItem is StringPropertyViewModel stringPropertyViewModel)
            {
                stringPropertyViewModel.Kind = DrawingBackendApi.Current.ShaderImplementation.ShaderLanguageExtension;
            }
        }
    }
}
