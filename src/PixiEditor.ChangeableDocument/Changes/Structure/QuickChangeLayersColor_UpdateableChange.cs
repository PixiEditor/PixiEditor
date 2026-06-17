using Drawie.Backend.Core.ColorsImpl;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

namespace PixiEditor.ChangeableDocument.Changes.Structure;

internal class QuickChangeLayersColor_UpdateableChange : InterruptableUpdateableChange
{
    public Guid[] LayerGuids { get; }
    public Color Color { get; set; }
    private Dictionary<Guid, Color?[]> originalColors;

    [GenerateUpdateableChangeActions]
    public QuickChangeLayersColor_UpdateableChange(IReadOnlyCollection<Guid> layerGuids, Color color)
    {
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

            if (target.FindMember(guid) is IQuickColorChangeable changeable)
            {
                quickColorChangeable++;
                originalColors ??= new Dictionary<Guid, Color?[]>();
                originalColors[guid] = changeable.GetColors();
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
            if (target.FindMember(guid) is IQuickColorChangeable quickColorChangeable && originalColors.TryGetValue(guid, out Color?[] colors))
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
