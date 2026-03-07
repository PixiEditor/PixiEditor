using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Arrays;

[NodeInfo("ArrayElement")]
public class ArrayElementNode : Node
{
    public SyncedTypeInputProperty Array { get; }
    public InputProperty<int> Index { get; }

    public SyncedTypeOutputProperty Output { get; }

    public ArrayElementNode()
    {
        Index = CreateInput<int>("Index", "INDEX", 0)
            .WithRules(x => x.Min(0));
        Array = CreateSyncedTypeInput("Array", "ARRAY", null, typeof(object[]))
            .AddTypeHandler<Array>(true);
        Output = CreateSyncedTypeOutput("Output", "OUTPUT", Array)
            .AllowGenericFallback().WithTypeAdjuster(t =>
            {
                if (t.IsArray)
                {
                    return t.GetElementType()!;
                }

                return t;
            });
    }

    protected override void OnExecute(RenderContext context)
    {
        if (Array.Value == null) return;

        var array = Array.Value as Array;
        if (array == null || array.Length == 0) return;

        int index = Index.Value;
        index = Math.Clamp(index, 0, array.Length - 1);

        Output.Value = array.GetValue(index);
    }

    public override Node CreateCopy()
    {
        return new ArrayElementNode();
    }
}
