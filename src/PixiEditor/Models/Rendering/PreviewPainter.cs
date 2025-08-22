using Avalonia;
using Avalonia.Threading;
using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.Models.Rendering;

public class PreviewPainter : IDisposable
{
    public string ElementToRenderName { get; set; }
    public IPreviewRenderable PreviewRenderable { get; set; }
    public ColorSpace ProcessingColorSpace { get; set; }
    public KeyFrameTime FrameTime { get; set; }
    public VecI DocumentSize { get; set; }
    public DocumentRenderer Renderer { get; set; }

    public bool AllowPartialResolutions { get; set; } = true;

    public bool CanRender => canRender;

    public event Action<bool>? CanRenderChanged;

    private Dictionary<int, Texture> renderTextures = new();
    private Dictionary<int, PainterInstance> painterInstances = new();

    private HashSet<int> dirtyTextures = new HashSet<int>();
    private HashSet<int> repaintingTextures = new HashSet<int>();

    private Dictionary<int, VecI> pendingResizes = new();
    private HashSet<int> pendingRemovals = new();

    private bool canRender;

    private int lastRequestId = 0;

    public PreviewPainter(DocumentRenderer renderer, IPreviewRenderable previewRenderable, KeyFrameTime frameTime,
        VecI documentSize, ColorSpace processingColorSpace, string elementToRenderName = "")
    {
        PreviewRenderable = previewRenderable;
        ElementToRenderName = elementToRenderName;
        ProcessingColorSpace = processingColorSpace;
        FrameTime = frameTime;
        DocumentSize = documentSize;
        Renderer = renderer;
    }

    public void Paint(DrawingSurface renderOn, int painterId)
    {
        if (!renderTextures.TryGetValue(painterId, out Texture? renderTexture))
        {
            return;
        }

        if (renderTexture == null || renderTexture.IsDisposed)
        {
            return;
        }

        renderOn.Canvas.DrawSurface(renderTexture.DrawingSurface, 0, 0);
    }

    public PainterInstance AttachPainterInstance()
    {
        int requestId = lastRequestId++;

        PainterInstance painterInstance = new() { RequestId = requestId };

        painterInstances[requestId] = painterInstance;

        return painterInstance;
    }

    public void ChangeRenderTextureSize(int requestId, VecI size)
    {
        if (size.X <= 0 || size.Y <= 0)
        {
            return;
        }

        if (repaintingTextures.Contains(requestId))
        {
            pendingResizes[requestId] = size;
            return;
        }

        if (renderTextures.ContainsKey(requestId))
        {
            renderTextures[requestId].Dispose();
        }

        renderTextures[requestId] = Texture.ForProcessing(size, ProcessingColorSpace);
    }

    public void RemovePainterInstance(int requestId)
    {
        painterInstances.Remove(requestId);
        dirtyTextures.Remove(requestId);

        if (repaintingTextures.Contains(requestId))
        {
            pendingRemovals.Add(requestId);
            return;
        }

        if (renderTextures.TryGetValue(requestId, out var renderTexture))
        {
            renderTexture?.Dispose();
            renderTextures.Remove(requestId);
        }
    }

    public void Repaint()
    {
        foreach (var texture in renderTextures)
        {
            dirtyTextures.Add(texture.Key);
        }

        RepaintDirty();
    }

    public void RepaintFor(int requestId)
    {
        dirtyTextures.Add(requestId);
        RepaintDirty();
    }

    private void RepaintDirty()
    {
        var dirtyArray = dirtyTextures.ToArray();
        bool couldRender = canRender;
        canRender = PreviewRenderable?.GetPreviewBounds(FrameTime.Frame, ElementToRenderName) != null &&
                    painterInstances.Count > 0;
        if (couldRender != canRender)
        {
            CanRenderChanged?.Invoke(canRender);
        }

        if (!CanRender)
        {
            return;
        }

        foreach (var texture in dirtyArray)
        {
            if (!renderTextures.TryGetValue(texture, out var renderTexture))
            {
                continue;
            }

            if (!painterInstances.TryGetValue(texture, out var painterInstance))
            {
                repaintingTextures.Remove(texture);
                dirtyTextures.Remove(texture);
                continue;
            }

            repaintingTextures.Add(texture);

            renderTexture.DrawingSurface.Canvas.Clear();
            renderTexture.DrawingSurface.Canvas.Save();

            Matrix3X3? matrix = painterInstance.RequestMatrix?.Invoke();
            VecI bounds = painterInstance.RequestRenderBounds?.Invoke() ?? VecI.Zero;

            ChunkResolution finalResolution = FindResolution(bounds);
            SamplingOptions samplingOptions = FindSamplingOptions(bounds);

            renderTexture.DrawingSurface.Canvas.SetMatrix(matrix ?? Matrix3X3.Identity);
            renderTexture.DrawingSurface.Canvas.Scale((float)finalResolution.InvertedMultiplier());

            RenderContext context = new(renderTexture.DrawingSurface, FrameTime, finalResolution,
                DocumentSize,
                DocumentSize,
                ProcessingColorSpace, samplingOptions);

            dirtyTextures.Remove(texture);
            Renderer.RenderNodePreview(PreviewRenderable, renderTexture.DrawingSurface, context, ElementToRenderName)
                .ContinueWith(_ =>
                {
                    Dispatcher.UIThread.Invoke(() =>
                    {
                        if (pendingRemovals.Contains(texture))
                        {
                            if (!renderTexture.IsDisposed)
                            {
                                try
                                {
                                    renderTexture.Dispose();
                                }
                                catch (Exception) { }
                            }

                            renderTextures.Remove(texture);
                            pendingRemovals.Remove(texture);
                            pendingResizes.Remove(texture);
                            dirtyTextures.Remove(texture);
                            return;
                        }

                        if (renderTexture is { IsDisposed: false })
                        {
                            try
                            {
                                renderTexture.DrawingSurface.Canvas.Restore();
                            }
                            catch (Exception)
                            {
                                repaintingTextures.Remove(texture);
                                dirtyTextures.Remove(texture);
                                pendingResizes.Remove(texture);
                                return;
                            }
                        }

                        painterInstance.RequestRepaint?.Invoke();
                        repaintingTextures.Remove(texture);

                        if (pendingResizes.Remove(texture, out VecI size))
                        {
                            ChangeRenderTextureSize(texture, size);
                            dirtyTextures.Add(texture);
                        }

                        if (repaintingTextures.Count == 0 && dirtyTextures.Count > 0)
                        {
                            RepaintDirty();
                        }
                    });
                });
        }
    }

    private ChunkResolution FindResolution(VecI bounds)
    {
        if (bounds.X <= 0 || bounds.Y <= 0 || !AllowPartialResolutions)
        {
            return ChunkResolution.Full;
        }

        double density = DocumentSize.X / (double)bounds.X;
        if (density > 8.01)
            return ChunkResolution.Eighth;
        if (density > 4.01)
            return ChunkResolution.Quarter;
        if (density > 2.01)
            return ChunkResolution.Half;
        return ChunkResolution.Full;
    }

    private SamplingOptions FindSamplingOptions(VecI bounds)
    {
        var density = DocumentSize.X / (double)bounds.X;
        return density > 1
            ? SamplingOptions.Bilinear
            : SamplingOptions.Default;
    }

    public void Dispose()
    {
        foreach (var texture in renderTextures)
        {
            texture.Value.Dispose();
        }
    }
}

public class PainterInstance
{
    public int RequestId { get; set; }
    public Func<VecI> RequestRenderBounds;

    public Func<Matrix3X3?>? RequestMatrix;
    public Action RequestRepaint;
}
