using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.Models.Rendering;

public class PreviewRenderer
{
    private Queue<RenderRequest> renderRequests = new();

    private bool isExecuting = false;

    public IReadOnlyDocument Document { get; }

    public PreviewRenderer(IReadOnlyDocument document)
    {
        Document = document;
    }

    public async Task RenderPreviews(KeyFrameTime frameTime)
    {
        await ExecuteRenderRequests(frameTime);
    }

    public async Task<bool> QueueRenderNodePreview(IPreviewRenderable previewRenderable, DrawingSurface renderOn,
        RenderContext context,
        string elementToRenderName)
    {
        if (previewRenderable is Node { IsDisposed: true }) return false;
        TaskCompletionSource<bool> tcs = new();
        RenderRequest request = new(tcs, context, renderOn, previewRenderable, elementToRenderName);

        renderRequests.Enqueue(request);

        return await tcs.Task;
    }

    private async Task ExecuteRenderRequests(KeyFrameTime frameTime)
    {
        isExecuting = true;
        using var ctx = DrawingBackendApi.Current?.RenderingDispatcher.EnsureContext();

        ChunkResolution highestResolution = renderRequests.MaxBy(x => 8 - (int)x.Context.ChunkResolution).Context.ChunkResolution;
        using Texture docSizeTex = Texture.ForDisplay(Document.Size);
        RenderContext context = new RenderContext(docSizeTex.DrawingSurface, frameTime, highestResolution,
            Document.Size,
            Document.Size, Document.ProcessingColorSpace, SamplingOptions.Default);

        Document.NodeGraph.Execute(context);

        while (renderRequests.Count > 0)
        {
            RenderRequest request = renderRequests.Dequeue();

            /*if (frameTime.Frame != lastExecutedGraphFrame && request.PreviewRenderable != this)
            {
                using Texture executeSurface = Texture.ForDisplay(new VecI(1));
                RenderDocument(executeSurface.DrawingSurface, frameTime, VecI.One);
            }*/

            try
            {
                bool result = true;
                if (request.PreviewRenderable != null)
                {
                    if (request.PreviewRenderable.GetType() == typeof(DocumentRenderer))
                    {
                        var renderOn = request.RenderOn;
                        int saved = renderOn.Canvas.Save();
                        renderOn.Canvas.Scale((float)request.Context.ChunkResolution.Multiplier());
                        renderOn.Canvas.Clear();
                        renderOn.Canvas.DrawSurface(docSizeTex.DrawingSurface, 0, 0);
                        renderOn.Canvas.RestoreToCount(saved);
                        result = true;
                    }
                    else
                    {
                        result = request.PreviewRenderable.RenderPreview(request.RenderOn, request.Context,
                            request.ElementToRenderName);
                    }
                }

                request.TaskCompletionSource.SetResult(result);
            }
            catch (Exception e)
            {
                request.TaskCompletionSource.SetException(e);
            }
        }

        isExecuting = false;
    }
}
