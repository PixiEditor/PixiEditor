using Drawie.Backend.Core.Shaders.Generation;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("ArrayConverter")]
public class ArrayConverterNode : Node
{
    public SyncedTypeInputProperty First { get; }
    public SyncedTypeOutputProperty Output { get; }

    private int inputsCount = 1;

    public ArrayConverterNode()
    {
        First = CreateSyncedTypeInput("First", "FIRST", null)
            .AllowGenericFallback(true);
        First.ConnectionChanged += OnConnectionChanged;

        Output = CreateSyncedTypeOutput("Output", "OUTPUT", First)
            .AllowGenericFallback();
    }

    private void OnConnectionChanged(SyncedTypeInputProperty syncedTypeInputProperty)
    {
        if (syncedTypeInputProperty.InternalProperty.Connection != null)
        {
            if(syncedTypeInputProperty == First && InputProperties.Count > 1) return;

            var input = CreateSyncedTypeInput($"Input {inputsCount}", $"INPUT_{inputsCount}", First)
                .AllowGenericFallback(true);
            input.ConnectionChanged += OnConnectionChanged;
            inputsCount++;
        }
        else
        {
            if (syncedTypeInputProperty == First) return;

            RemoveInputProperty(syncedTypeInputProperty.InternalProperty);
            syncedTypeInputProperty.ConnectionChanged -= OnConnectionChanged;
            syncedTypeInputProperty.StopListeningToConnectionChanges();
        }
    }

    protected override void OnExecute(RenderContext context)
    {
        Type targetType = First.InternalProperty.ValueType;
        if (First.Value is Delegate del)
        {
            try
            {
                var val = del.DynamicInvoke(FuncContext.NoContext);
                if (val is ShaderExpressionVariable expr)
                {
                    val = expr.GetConstant();
                    targetType = val.GetType();
                }
            }
            catch
            {
                return;
            }
        }
        Array array = Array.CreateInstance(targetType, InputProperties.Count - 1);

        for (int i = 0; i < InputProperties.Count - 1; i++)
        {
            var input = InputProperties[i];
            object value = input.Value;
            if(value is Delegate func)
            {
                try
                {
                    value = func.DynamicInvoke(FuncContext.NoContext);
                    if (value is ShaderExpressionVariable expr)
                    {
                        value = expr.GetConstant();
                    }
                }
                catch
                {
                    value = null;
                }
            }

            array.SetValue(value, i);
        }

        Output.Value = array;
    }

    public override Node CreateCopy()
    {
        return new ArrayConverterNode();
    }
}
