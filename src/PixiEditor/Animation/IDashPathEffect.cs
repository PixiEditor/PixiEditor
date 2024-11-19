using Drawie.Backend.Core.Vector;

namespace PixiEditor.Animation;

public interface IDashPathEffect
{
    public IReadOnlyList<float> Dashes { get; set; }
    public float Phase { get; set; }
    public PathEffect ToPathEffect();
}

public class DashPathEffect : IDashPathEffect
{
    public IReadOnlyList<float> Dashes { get; set; }
    public float Phase { get; set; }

    public DashPathEffect(IReadOnlyList<float> dashes, float phase)
    {
        Dashes = dashes;
        Phase = phase;
    }
    public PathEffect ToPathEffect()
    {
        return PathEffect.CreateDash(Dashes.ToArray(), Phase);
    }
}
