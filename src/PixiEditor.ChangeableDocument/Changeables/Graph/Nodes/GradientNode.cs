using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("Gradient")]
public class GradientNode : Node
{
    public InputProperty<GradientType> Type { get; }
    public InputProperty<VecD> StartPoint { get; private set; }
    public InputProperty<VecD> EndPoint { get; private set; }
    public InputProperty<VecD> CenterPoint { get; private set; }
    public InputProperty<double> Radius { get; private set; }
    public InputProperty<double> Angle { get; private set; }
    public InputProperty<int> StopsCount { get; }
    public OutputProperty<GradientPaintable> Gradient { get; }

    public Dictionary<InputProperty<Color>, InputProperty<float>> ColorStops { get; } = new();

    public GradientNode()
    {
        Gradient = CreateOutput<GradientPaintable>("Gradient", "GRADIENT", null);
        Type = CreateInput<GradientType>("Type", "TYPE", GradientType.Linear)
            .NonOverridenChanged(UpdateType);
        StartPoint = CreateInput<VecD>("StartPoint", "START_POINT", new VecD(0, 0));
        EndPoint = CreateInput<VecD>("EndPoint", "END_POINT", new VecD(1, 0));
        StopsCount = CreateInput<int>("StopsCount", "STOPS_COUNT", 2)
            .NonOverridenChanged(_ => RegenerateStops());

        GenerateStops();
    }

    private void UpdateType(GradientType type)
    {
        if (type == GradientType.Linear)
        {
            RemoveInputProperty(CenterPoint);
            RemoveInputProperty(Radius);
            RemoveInputProperty(Angle);
            if (!HasInputProperty(StartPoint.InternalPropertyName))
            {
                StartPoint = CreateInput<VecD>("StartPoint", "START_POINT", new VecD(0, 0));
            }

            if (!HasInputProperty(EndPoint.InternalPropertyName))
            {
                EndPoint = CreateInput<VecD>("EndPoint", "END_POINT", new VecD(1, 0));
            }
        }
        else if (type == GradientType.Radial)
        {
            RemoveInputProperty(StartPoint);
            RemoveInputProperty(EndPoint);
            RemoveInputProperty(Angle);
            if (!HasInputProperty("CenterPoint"))
            {
                CenterPoint = CreateInput<VecD>("CenterPoint", "CENTER_POINT", new VecD(0.5, 0.5));
            }

            if (!HasInputProperty("Radius"))
            {
                Radius = CreateInput<double>("Radius", "RADIUS", 0.5).WithRules(x => x.Min(0d));
            }
        }
        else if (type == GradientType.Conical)
        {
            RemoveInputProperty(StartPoint);
            RemoveInputProperty(EndPoint);
            RemoveInputProperty(Radius);
            if (!HasInputProperty("CenterPoint"))
            {
                CenterPoint = CreateInput<VecD>("CenterPoint", "CENTER_POINT", new VecD(0.5, 0.5));
            }

            if (!HasInputProperty("Angle"))
            {
                Angle = CreateInput<double>("Angle", "ANGLE", 0);
            }
        }
    }

    private void RegenerateStops()
    {
        if (StopsCount.Value < ColorStops.Count)
        {
            int diff = ColorStops.Count - StopsCount.Value;
            var keysToRemove = ColorStops.Keys.TakeLast(diff).ToList();
            foreach (var key in keysToRemove)
            {
                RemoveInputProperty(key);
                RemoveInputProperty(ColorStops[key]);
                ColorStops.Remove(key);
            }
        }

        GenerateStops();
    }

    private void GenerateStops()
    {
        int startIndex = ColorStops.Count;
        for (int i = startIndex; i < StopsCount.Value; i++)
        {
            var colorInput = CreateInput<Color>($"ColorStop{i + 1}Color", $"COLOR_STOP_COLOR",
                Drawie.Backend.Core.ColorsImpl.Colors.White);
            var positionInput = CreateInput<float>($"ColorStop{i + 1}Position", $"COLOR_STOP_POSITION",
                i / (float)(StopsCount.Value - 1));
            ColorStops[colorInput] = positionInput;
        }
    }

    protected override void OnExecute(RenderContext context)
    {
        var stops = ColorStops.Select(kvp => new GradientStop(kvp.Key.Value, kvp.Value.Value)).ToList();
        Gradient.Value = GenerateGradient(Type.Value, stops);
    }

    private GradientPaintable GenerateGradient(GradientType type, List<GradientStop> stops)
    {
        return type switch
        {
            GradientType.Linear => new LinearGradientPaintable(StartPoint.Value, EndPoint.Value, stops),
            GradientType.Radial => new RadialGradientPaintable(CenterPoint.Value, Radius.Value, stops),
            GradientType.Conical => new SweepGradientPaintable(CenterPoint.Value, Angle.Value, stops),
            _ => throw new NotImplementedException("Unknown gradient type")
        };
    }

    public override Node CreateCopy()
    {
        return new GradientNode();
    }
}

public enum GradientType
{
    Linear,
    Radial,
    Conical
}
