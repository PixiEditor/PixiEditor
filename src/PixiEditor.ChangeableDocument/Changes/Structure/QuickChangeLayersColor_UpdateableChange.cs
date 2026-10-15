using Drawie.Backend.Core.ColorsImpl;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.ChangeInfos.Vectors;
using PixiEditor.ChangeableDocument.Helpers;

namespace PixiEditor.ChangeableDocument.Changes.Structure;

internal class QuickChangeLayersColor_UpdateableChange : InterruptableUpdateableChange
{
    public Guid[] LayerGuids { get; }
    public Color Color { get; set; }
    private Dictionary<Guid, Color?[]> originalColors;
    private Dictionary<Guid, IReadOnlyShapeVectorData> originalShapes;
    private int frame;

    [GenerateUpdateableChangeActions]
    public QuickChangeLayersColor_UpdateableChange(int frame, IReadOnlyCollection<Guid> layerGuids, Color color)
    {
        this.frame = frame;
        LayerGuids = layerGuids.ToArray();
        Color = color;
    }

    public override bool InitializeAndValidate(Document target)
    {
        int quickColorChangeable = 0;
        foreach (Guid guid in LayerGuids)
        {
            if (!target.HasMember(guid))
                return false;

            var member = target.FindMember(guid);
            if (member is IQuickColorChangeable changeable)
            {
                quickColorChangeable++;
                originalColors ??= new Dictionary<Guid, Color?[]>();
                originalColors[guid] = changeable.GetColors();
                if (member is VectorLayerNode vectorNode)
                {
                    originalShapes ??= new Dictionary<Guid, IReadOnlyShapeVectorData>();
                    originalShapes[guid] = vectorNode.EmbeddedShapeData.Clone() as IReadOnlyShapeVectorData;
                }
            }
        }

        return true;
    }

    [UpdateChangeMethod]
    public void Update(Color color)
    {
        Color = color;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        List<IChangeInfo> changeInfos = new List<IChangeInfo>();
        ignoreInUndo = false;

        foreach (Guid guid in LayerGuids)
        {
            if (target.FindMember(guid) is IQuickColorChangeable quickColorChangeable)
            {
                var changes = quickColorChangeable.ChangeColor(Color);
                if (changes != null)
                {
                    changeInfos.AddRange(changes);
                }
            }
        }

        return changeInfos;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> ApplyTemporarily(Document target)
    {
        List<IChangeInfo> changeInfos = new List<IChangeInfo>();
        foreach (Guid guid in LayerGuids)
        {
            if (target.FindMember(guid) is IQuickColorChangeable quickColorChangeable)
            {
                var changes = quickColorChangeable.ChangeColor(Color);
                if (changes != null)
                {
                    changeInfos.AddRange(changes);
                }
            }
        }

        return changeInfos;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        List<IChangeInfo> changeInfos = new List<IChangeInfo>();
        foreach (Guid guid in LayerGuids)
        {
            var member = target.FindMember(guid);
            if (member is VectorLayerNode vectorNode)
            {
                vectorNode.EmbeddedShapeData = originalShapes[guid].Clone();
                changeInfos.Add(new VectorShape_ChangeInfo(guid, AffectedAreasUtility.GetTightLayerArea(member, frame)));
            }
            else if (member is IQuickColorChangeable quickColorChangeable && originalColors.TryGetValue(guid, out Color?[] colors))
            {
                var changes = quickColorChangeable.ChangeColor(colors);
                if (changes != null)
                {
                    changeInfos.AddRange(changes);
                }
            }
        }

        return changeInfos;
    }
}
