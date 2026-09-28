using Drawie.Backend.Core.ColorsImpl;
using Drawie.Numerics;
using PixiEditor.Helpers.Extensions;

namespace PixiEditor.ViewModels.Nodes.Properties;

internal class Vec4DPropertyViewModel : NodePropertyViewModel<Vec4D>
{
    public new Avalonia.Media.Color Value
    {
        get => Color.FromVec4D(base.Value).ToColor();
        set => base.Value = value.ToColor().ToVec4D();
    }
    
    public Vec4DPropertyViewModel(NodeViewModel node, Type valueType) : base(node, valueType)
    {
    }
}
