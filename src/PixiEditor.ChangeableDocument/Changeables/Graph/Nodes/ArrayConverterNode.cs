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
    private List<SyncedTypeInputProperty> syncedInputs = new();

    public ArrayConverterNode()
    {
        First = CreateSyncedTypeInput("First", "FIRST", null)
            .AllowGenericFallback(true);
        syncedInputs.Add(First);
        First.ConnectionChanged += OnConnectionChanged;

        Output = CreateSyncedTypeOutput("Output", "OUTPUT", First)
            .AllowGenericFallback().WithTypeAdjuster(t =>
            {
                if (t.IsArray)
                {
                    return t;
                }

                return t.MakeArrayType();
            });
    }

    private void OnConnectionChanged(SyncedTypeInputProperty syncedTypeInputProperty)
    {
        if (syncedTypeInputProperty.InternalProperty.Connection != null)
        {
            if (syncedTypeInputProperty == First && InputProperties.Count > 1) return;

            var previousSyncedInput = syncedInputs[^1];
            var input = CreateSyncedTypeInput($"Input {inputsCount}", $"INPUT_{inputsCount}", previousSyncedInput)
                .AllowGenericFallback(true);
            syncedInputs.Add(input);
            input.ConnectionChanged += OnConnectionChanged;
            inputsCount++;
        }
        else
        {
            if (syncedTypeInputProperty == First)
            {
                return;
            }

            RemoveInputProperty(syncedTypeInputProperty.InternalProperty);
            syncedInputs.Remove(syncedTypeInputProperty);
            syncedTypeInputProperty.ConnectionChanged -= OnConnectionChanged;
            syncedTypeInputProperty.StopListeningToConnectionChanges();
            if (InputProperties.FirstOrDefault() != First.InternalProperty)
            {
                MoveInputProperty(First.InternalProperty, 0);
            }

            ResyncInputs();
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
            if (value == null) continue;

            if (value is Delegate func && !array.GetType().GetElementType().IsAssignableTo(typeof(Delegate)))
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

    private void ResyncInputs()
    {
        First.Other = null;
        for (int i = 1; i < syncedInputs.Count; i++)
        {
            var current = syncedInputs[i];
            var previous = syncedInputs[i - 1];

            if (current.Other != previous)
            {
                current.Other = previous;
                current.ForceUpdateType();
            }

            if(i == syncedInputs.Count - 1)
            {
                First.Other = current;
                First.ForceUpdateType();
            }
        }
    }
}
