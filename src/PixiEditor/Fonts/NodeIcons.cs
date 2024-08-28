using System.Collections.ObjectModel;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Animable;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.CombineSeparate;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.FilterNodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Points;

namespace PixiEditor.Fonts;

public static class NodeIcons
{
    public static ReadOnlyDictionary<Type, string> IconMap { get; } = new ReadOnlyDictionary<Type, string>(
        new Dictionary<Type, string>
        {
            { typeof(TimeNode), "\uE900" },
            { typeof(FolderNode), "\ue901" },
            { typeof(CreateImageNode), "\ue902" },
            { typeof(MergeNode), "\ue903" },
            { typeof(ModifyImageLeftNode), "\ue904" },
            { typeof(ImageLayerNode), "\ue905" },
            { typeof(RasterizePointsNode), "\ue906" },
            { typeof(SampleImageNode), "\ue907" },
            { typeof(CombineColorNode), "\ue908" },
            { typeof(ApplyFilterNode), "\ue909" },
            { typeof(DistributePointsNode), "\ue90a" },
            { typeof(LerpColorNode), "\ue90b" },
            { typeof(NoiseNode), "\ue90c" },
            { typeof(EllipseNode), "\ue90d" },
            { typeof(MathNode), "\ue90e" },
            { typeof(KernelFilterNode), "\ue90f" },
            { typeof(CombineChannelsNode), "\ue915" },
            { typeof(ColorMatrixFilterNode), "\ue911" },
            { typeof(GrayscaleNode), "\ue912" },
            { typeof(SeparateColorNode), "\ue913" },
            { typeof(RemoveClosePointsNode), "\ue914" },
            { typeof(SeparateChannelsNode), "\ue910" },
            { typeof(CombineVecDNode), "\ue916" },
            { typeof(CombineVecINode), "\ue917" },
            { typeof(SeparateVecDNode), "\ue918" },
            { typeof(SeparateVecINode), "\ue919" }
        });
}
