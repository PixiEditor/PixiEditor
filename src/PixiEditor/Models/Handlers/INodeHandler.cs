using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Media;
using ChunkyImageLib;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.DrawingApi.Core;
using PixiEditor.Models.Rendering;
using PixiEditor.Models.Structures;
using PixiEditor.Numerics;

namespace PixiEditor.Models.Handlers;

public interface INodeHandler : INotifyPropertyChanged
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
    public bool IsSelected { get; set; }
    public void TraverseBackwards(Func<INodeHandler, bool> func);
    public void TraverseBackwards(Func<INodeHandler, INodeHandler, bool> func);
    public void TraverseBackwards(Func<INodeHandler, INodeHandler, INodePropertyHandler, bool> func);
    public void TraverseForwards(Func<INodeHandler, bool> func);
    public void TraverseForwards(Func<INodeHandler, INodeHandler, bool> func);
    public void TraverseForwards(Func<INodeHandler, INodeHandler, INodePropertyHandler, bool> func);
}
