using System.ComponentModel;
using Avalonia;
using Avalonia.Media;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.Models.Structures;
using Drawie.Numerics;
using PixiEditor.GraphNavigation;
using PixiEditor.ViewModels.Document;
using PixiEditor.ViewModels.Nodes;
using Traverse = PixiEditor.GraphNavigation.Traverse;

namespace PixiEditor.Models.Handlers;

public interface INodeHandler : INavigableNode<INodeHandler, INodePropertyHandler, INodePropertyHandler>, INotifyPropertyChanged, IDisposable
{
    public Guid Id { get; }
    public string NodeNameBindable { get; set; }
    public IBrush CategoryBackgroundBrush { get; }
    public string InternalName { get; }
    public NodeMetadata Metadata { get; set; }
    public ObservableRangeCollection<INodePropertyHandler> Inputs { get; }
    public ObservableRangeCollection<INodePropertyHandler> Outputs { get; }
    public TexturePreview? Preview { get; set; }
    public VecD PositionBindable { get; set; }
    public Rect UiSize { get; set; }
    public bool IsNodeSelected { get; set; }
    public string Icon { get; }
    public HashSet<NodeFrameViewModelBase> Frames { get; }
    public IReadOnlyDictionary<string, INodePropertyHandler> InputPropertyMap { get; }
    public IReadOnlyDictionary<string, INodePropertyHandler> OutputPropertyMap { get; }
    public bool IsSocketConverterNode { get; }

    IEnumerable<INodePropertyHandler> INavigableNode<INodeHandler, INodePropertyHandler, INodePropertyHandler>.Inputs => Inputs;

    IEnumerable<INodePropertyHandler> INavigableNode<INodeHandler, INodePropertyHandler, INodePropertyHandler>.Outputs => Outputs;
}
