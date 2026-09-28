using Drawie.Backend.Core.Shaders.Generation;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Arrays;

[NodeInfo("ArrayConverter")]
public class ArrayConverterNode : Node
{
    public SyncedTypeInputProperty First { get; }
    public SyncedTypeOutputProperty Output { get; }

    private int inputsCount = 1;
    private List<SyncedTypeInputProperty> syncedInputs = new();

    private SyncGroup syncGroup = new();

    public ArrayConverterNode()
    {
        syncGroup = new();
        First = CreateSyncedTypeInput("Input 0", "Input 0", syncGroup)
            .AllowGenericFallback(true);
        syncedInputs.Add(First);
        First.ConnectionChanged += OnConnectionChanged;

        Output = CreateSyncedTypeOutput("Output", "OUTPUT", syncGroup)
            .AllowGenericFallback().WithTypeAdjuster(t => t.MakeArrayType());


        Output.ForceUpdateType(typeof(object));
    }

    internal override void SerializeAdditionalDataInternal(IReadOnlyDocument target, Dictionary<string, object> additionalData)
    {
        string[] inputNames = syncedInputs.Select((input, index) => $"Input {index}").ToArray();
        additionalData["inputNames"] = inputNames;
    }

    internal override void DeserializeAdditionalDataInternal(IReadOnlyDocument target, IReadOnlyDictionary<string, object> data, List<IChangeInfo> infos)
    {
        if (data.TryGetValue("inputNames", out object inputNamesObj) && inputNamesObj is string[] inputNames)
        {
            for (int i = 0; i < inputNames.Length; i++)
            {
                string inputName = inputNames[i];
                if(InputProperties.Any(x => x.InternalPropertyName == inputName))
                {
                    continue;
                }

                var input = CreateSyncedTypeInput(inputNames[i], inputName, syncGroup)
                    .AllowGenericFallback(true);
                syncedInputs.Add(input);
                input.ConnectionChanged += OnConnectionChanged;
                inputsCount++;
            }
        }
    }

    private void OnConnectionChanged(SyncedTypeInputProperty syncedTypeInputProperty)
    {
        if (syncedTypeInputProperty.InternalProperty.Connection != null)
        {
            if (InputProperties.Any(x => x.Connection == null)) return;

            var input = CreateSyncedTypeInput($"Input {inputsCount}", $"INPUT_{inputsCount}", syncGroup)
                .AllowGenericFallback(true);

            syncedInputs.Add(input);
            input.ConnectionChanged += OnConnectionChanged;
            inputsCount++;
        }
        else
        {
            if (syncedTypeInputProperty == First) return;

            RemoveInputProperty(syncedTypeInputProperty.InternalProperty);
            syncedInputs.Remove(syncedTypeInputProperty);
            syncGroup.RemoveInput(syncedTypeInputProperty);
            syncedTypeInputProperty.ConnectionChanged -= OnConnectionChanged;
            syncedTypeInputProperty.StopListeningToConnectionChanges();
        }
    }

    protected override void OnExecute(RenderContext context)
    {
        Type targetType = Output.InternalProperty.ValueType.GetElementType() ?? typeof(object);

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
}
