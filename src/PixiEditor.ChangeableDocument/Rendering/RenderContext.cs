using PixiEditor.ChangeableDocument.Changeables.Animations;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using BlendMode = PixiEditor.ChangeableDocument.Enums.BlendMode;
using DrawingApiBlendMode = Drawie.Backend.Core.Surfaces.BlendMode;

namespace PixiEditor.ChangeableDocument.Rendering;

public class RenderContext
{
    public double Opacity { get; set; }

    public KeyFrameTime FrameTime { get; }
    public ChunkResolution ChunkResolution { get; set; }
    public SamplingOptions DesiredSamplingOptions { get; set; } = SamplingOptions.Default;
    public VecI RenderOutputSize { get; set; }

    public VecI DocumentSize { get; set; }
    public DrawingSurface RenderSurface { get; set; }
    public bool FullRerender { get; set; } = false;
    
    public ColorSpace ProcessingColorSpace { get; set; }
    public string? TargetOutput { get; set; }   

    private List<Guid> virtualGraphSessions = new List<Guid>();
    private Dictionary<Guid, List<InputProperty>> recordedVirtualInputs = new();
    private Dictionary<Guid, List<OutputProperty>> recordedVirtualOutputs = new();

    public RenderContext(DrawingSurface renderSurface, KeyFrameTime frameTime, ChunkResolution chunkResolution,
        VecI renderOutputSize, VecI documentSize, ColorSpace processingColorSpace, SamplingOptions desiredSampling, double opacity = 1)
    {
        RenderSurface = renderSurface;
        FrameTime = frameTime;
        ChunkResolution = chunkResolution;
        RenderOutputSize = renderOutputSize;
        Opacity = opacity;
        ProcessingColorSpace = processingColorSpace;
        DocumentSize = documentSize;
        DesiredSamplingOptions = desiredSampling;
    }

    public static DrawingApiBlendMode GetDrawingBlendMode(BlendMode blendMode)
    {
        return blendMode switch
        {
            BlendMode.Normal => DrawingApiBlendMode.SrcOver,
            BlendMode.Erase => DrawingApiBlendMode.DstOut,
            BlendMode.Darken => DrawingApiBlendMode.Darken,
            BlendMode.Multiply => DrawingApiBlendMode.Multiply,
            BlendMode.ColorBurn => DrawingApiBlendMode.ColorBurn,
            BlendMode.Lighten => DrawingApiBlendMode.Lighten,
            BlendMode.Screen => DrawingApiBlendMode.Screen,
            BlendMode.ColorDodge => DrawingApiBlendMode.ColorDodge,
            BlendMode.LinearDodge => DrawingApiBlendMode.Plus,
            BlendMode.Overlay => DrawingApiBlendMode.Overlay,
            BlendMode.SoftLight => DrawingApiBlendMode.SoftLight,
            BlendMode.HardLight => DrawingApiBlendMode.HardLight,
            BlendMode.Difference => DrawingApiBlendMode.Difference,
            BlendMode.Exclusion => DrawingApiBlendMode.Exclusion,
            BlendMode.Hue => DrawingApiBlendMode.Hue,
            BlendMode.Saturation => DrawingApiBlendMode.Saturation,
            BlendMode.Luminosity => DrawingApiBlendMode.Luminosity,
            BlendMode.Color => DrawingApiBlendMode.Color,
            _ => DrawingApiBlendMode.SrcOver,
        };
    }

    public RenderContext Clone()
    {
        return new RenderContext(RenderSurface, FrameTime, ChunkResolution, RenderOutputSize, DocumentSize, ProcessingColorSpace, DesiredSamplingOptions, Opacity)
        {
            FullRerender = FullRerender,
            TargetOutput = TargetOutput,
        };
    }

    public void BeginVirtualConnectionScope(Guid virtualSessionId)
    {
        if (virtualGraphSessions.Contains(virtualSessionId))
            return;

        virtualGraphSessions.Add(virtualSessionId);
    }

    public void EndVirtualConnectionScope(Guid virtualSessionId)
    {
        if (!virtualGraphSessions.Contains(virtualSessionId))
            return;

        virtualGraphSessions.Remove(virtualSessionId);

        foreach (var inputProperty in recordedVirtualInputs)
        {
            foreach (var input in inputProperty.Value)
            {
                input.RemoveVirtualConnection(virtualSessionId);
                input.ActiveVirtualSession = null;
            }
        }

        foreach (var outputProperty in recordedVirtualOutputs)
        {
            foreach (var output in outputProperty.Value)
            {
                output.RemoveAllVirtualConnections(virtualSessionId);
            }
        }
    }

    public void RecordVirtualConnection(InputProperty inputProperty, Guid virtualSessionId)
    {
        if (virtualGraphSessions.Count == 0)
            return;

        if (!recordedVirtualInputs.TryGetValue(virtualSessionId, out var inputs))
        {
            inputs = new List<InputProperty>();
            recordedVirtualInputs[virtualSessionId] = inputs;
        }

        inputs.Add(inputProperty);
        inputProperty.ActiveVirtualSession = virtualSessionId;
    }

    public void RecordVirtualConnection(OutputProperty outputProperty, Guid virtualConnectionId)
    {
        if (virtualGraphSessions.Count == 0)
            return;

        if (!recordedVirtualOutputs.TryGetValue(virtualConnectionId, out var outputs))
        {
            outputs = new List<OutputProperty>();
            recordedVirtualOutputs[virtualConnectionId] = outputs;
        }

        outputs.Add(outputProperty);
    }

    public void CleanupVirtualConnectionScopes()
    {
        foreach (var virtualSessionId in virtualGraphSessions.ToArray())
        {
            EndVirtualConnectionScope(virtualSessionId);
        }
    }
}
