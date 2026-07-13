using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Utility;

[NodeInfo("Random")]
public class RandomNode : Node
{
    public const string InputTriggerPropertyName = "InputTrigger";
    public const string TriggerPropertyName = "Trigger";
    public InputProperty<float> Min { get; }
    public InputProperty<float> Max { get; }
    public InputProperty<int> Seed { get; }

    public InputProperty<RandomTrigger> Trigger { get; }
    public InputProperty<float> InputTrigger { get; }
    public OutputProperty<float> Result { get; }

    protected override bool ExecuteOnlyOnCacheChange => Trigger.Value == RandomTrigger.OnInputChanged;
    protected override CacheTriggerFlags CacheTrigger => CacheTriggerFlags.Inputs;

    private Random random;
    private int lastSeed;

    private float lastInputTriggerValue;

    public RandomNode()
    {
        Min = CreateInput("Min", "MIN", 0f);
        Max = CreateInput("Max", "MAX", 1f);
        Seed = CreateInput("Seed", "SEED", 0);

        Trigger = CreateInput(TriggerPropertyName, "RANDOM_TRIGGER_MODE", RandomTrigger.OnExecute);
        InputTrigger = CreateInput(InputTriggerPropertyName, "RANDOM_INPUT_TRIGGER_VALUE", 0f);

        Result = CreateOutput("Result", "RESULT", 0f);

        random = new Random(Seed.Value);
    }

    protected override void OnExecute(RenderContext context)
    {
        if (Seed.Value != lastSeed)
        {
            random = new Random(Seed.Value);
            lastSeed = Seed.Value;
        }

        if (Trigger.Value == RandomTrigger.OnInputChanged && Math.Abs(InputTrigger.Value - lastInputTriggerValue) < float.Epsilon)
        {
            return;
        }

        Result.Value = (float)(Min.Value + random.NextDouble() * (Max.Value - Min.Value));
        lastInputTriggerValue = InputTrigger.Value;
    }

    public override Node CreateCopy()
    {
        return new RandomNode();
    }
}

public enum RandomTrigger
{
    OnExecute,
    OnInputChanged
}
