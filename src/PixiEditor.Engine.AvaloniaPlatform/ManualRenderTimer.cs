using Avalonia.Rendering;

namespace PixiEditor.Engine.AvaloniaPlatform;

/// <summary>A <see cref="IRenderTimer"/> implementation that is only triggered manually.</summary>
internal sealed class ManualRenderTimer : IRenderTimer
{
    public event Action<TimeSpan>? Tick;

    bool IRenderTimer.RunsInBackground
        => false;

    public void TriggerTick(TimeSpan elapsed)
        => Tick?.Invoke(elapsed);
}
