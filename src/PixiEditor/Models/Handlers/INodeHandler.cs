using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia;
using Avalonia.Media;
using ChunkyImageLib;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using Drawie.Backend.Core;
using PixiEditor.Models.Rendering;
using PixiEditor.Models.Structures;
using Drawie.Numerics;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.Models.Handlers;

public interface INodeHandler : INotifyPropertyChanged, IDisposable
{
    public Guid Id { get; }
    public string NodeNameBindable { get; set; }
    public IBrush CategoryBackgroundBrush { get; }
    public string InternalName { get; }
    public NodeMetadata Metadata { get; set; }
    public ObservableRangeCollection<INodePropertyHandler> Inputs { get; }
    public ObservableRangeCollection<INodePropertyHandler> Outputs { get; }
    public PreviewPainter? ResultPainter { get; set; }
    public VecD PositionBindable { get; set; }
    public Rect UiSize { get; set; }
    public bool IsNodeSelected { get; set; }
    public string Icon { get; }
    public void TraverseBackwards(Func<INodeHandler, Traverse> func);
    public void TraverseBackwards(Func<INodeHandler, INodeHandler, Traverse> func);
    public void TraverseBackwards(Func<INodeHandler, INodeHandler, INodePropertyHandler, Traverse> func);
    public void TraverseForwards(Func<INodeHandler, Traverse> func);
    public void TraverseForwards(Func<INodeHandler, INodeHandler, Traverse> func);
    public void TraverseForwards(Func<INodeHandler, INodeHandler, INodePropertyHandler, Traverse> func);
    public void TraverseForwards(Func<INodeHandler, INodeHandler, INodePropertyHandler, INodePropertyHandler, Traverse> func);
    public HashSet<NodeFrameViewModelBase> Frames { get; }
    public IReadOnlyDictionary<string, INodePropertyHandler> InputPropertyMap { get; }
    public IReadOnlyDictionary<string, INodePropertyHandler> OutputPropertyMap { get; }
}
