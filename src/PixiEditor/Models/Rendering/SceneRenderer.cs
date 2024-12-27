using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.ImageData;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Models.Handlers;

namespace PixiEditor.Models.Rendering;

internal class SceneRenderer
{
    public IReadOnlyDocument Document { get; }
    public IDocument DocumentViewModel { get; }
    public bool HighResRendering { get; set; } = true;

    public SceneRenderer(IReadOnlyDocument trackerDocument, IDocument documentViewModel)
    {
        Document = trackerDocument;
        DocumentViewModel = documentViewModel;
    }

    public void RenderScene(DrawingSurface target, ChunkResolution resolution)
    {
        if(Document.Renderer.IsBusy || DocumentViewModel.Busy) return;
        RenderOnionSkin(target, resolution);
        RenderGraph(target, resolution);
    }

    private void RenderGraph(DrawingSurface target, ChunkResolution resolution)
    {
        DrawingSurface renderTarget = target;
        Texture? texture = null;
        
        if (!HighResRendering || !HighDpiRenderNodePresent(Document.NodeGraph))
        {
            texture = Texture.ForProcessing(Document.Size, Document.ProcessingColorSpace);
            renderTarget = texture.DrawingSurface;
        }

        RenderContext context = new(renderTarget, DocumentViewModel.AnimationHandler.ActiveFrameTime,
            resolution, Document.Size, Document.ProcessingColorSpace);
        Document.NodeGraph.Execute(context);
        
        if(texture != null)
        {
            target.Canvas.DrawSurface(texture.DrawingSurface, 0, 0);
            texture.Dispose();
        }
    }

    private bool HighDpiRenderNodePresent(IReadOnlyNodeGraph documentNodeGraph)
    {
        bool highDpiRenderNodePresent = false;
        documentNodeGraph.TryTraverse(n =>
        {
            if (n is IHighDpiRenderNode { AllowHighDpiRendering: true })
            {
                highDpiRenderNodePresent = true;
            } 
        });
        
        return highDpiRenderNodePresent;
    }

    private void RenderOnionSkin(DrawingSurface target, ChunkResolution resolution)
    {
        var animationData = Document.AnimationData;
        if (!DocumentViewModel.AnimationHandler.OnionSkinningEnabledBindable)
        {
            return;
        }

        double onionOpacity = animationData.OnionOpacity / 100.0;
        double alphaFalloffMultiplier = 1.0 / animationData.OnionFrames;

        // Render previous frames'
        for (int i = 1; i <= animationData.OnionFrames; i++)
        {
            int frame = DocumentViewModel.AnimationHandler.ActiveFrameTime.Frame - i;
            if (frame < DocumentViewModel.AnimationHandler.FirstFrame)
            {
                break;
            }

            double finalOpacity = onionOpacity * alphaFalloffMultiplier * (animationData.OnionFrames - i + 1);

            RenderContext onionContext = new(target, frame, resolution, Document.Size, Document.ProcessingColorSpace, finalOpacity);
            Document.NodeGraph.Execute(onionContext);
        }

        // Render next frames
        for (int i = 1; i <= animationData.OnionFrames; i++)
        {
            int frame = DocumentViewModel.AnimationHandler.ActiveFrameTime.Frame + i;
            if (frame > DocumentViewModel.AnimationHandler.LastFrame)
            {
                break;
            }

            double finalOpacity = onionOpacity * alphaFalloffMultiplier * (animationData.OnionFrames - i + 1);
            RenderContext onionContext = new(target, frame, resolution, Document.Size, Document.ProcessingColorSpace, finalOpacity);
            Document.NodeGraph.Execute(onionContext);
        }
    }
}
