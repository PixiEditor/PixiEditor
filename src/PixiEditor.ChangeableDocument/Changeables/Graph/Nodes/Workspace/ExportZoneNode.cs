using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Workspace;

[NodeInfo("ExportZone")]
public class ExportZoneNode : Node
{
    public const string IsDefaultName = "IsDefault";
    public const string SizeName = "Size";
    public const string OffsetName = "Offset";
    public InputProperty<string> Name { get; }
    public InputProperty<VecI> Offset { get; }
    public InputProperty<VecI> Size { get; }
    public InputProperty<bool> IsDefault { get; }

    public ExportZoneNode()
    {
        Name = CreateInput<string>("Name", "NAME", string.Empty);
        Offset = CreateInput<VecI>(OffsetName, "POSITION", VecI.Zero);
        Size = CreateInput<VecI>(SizeName, "SIZE", VecI.One);
        IsDefault = CreateInput<bool>(IsDefaultName, "IS_DEFAULT", false);
    }

    protected override void OnExecute(RenderContext context)
    {
        // This node is used to define the export zone, so it doesn't need to do anything in the execution phase.
        // The export zone is defined by the position and size inputs.
    }

    public override Node CreateCopy()
    {
        return new ExportZoneNode();
    }
}
