using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core.Surfaces;
using PixiEditor.Models.Handlers;

namespace PixiEditor.Models.Rendering;

internal class SceneRenderer
{
    public IReadOnlyDocument Document { get; }
    public IDocument DocumentViewModel { get; }
    public ChunkResolution Resolution { get; set; }

    public SceneRenderer(IReadOnlyDocument trackerDocument, IDocument documentViewModel)
    {
        Document = trackerDocument;
        DocumentViewModel = documentViewModel;
        Resolution = ChunkResolution.Full;
    }

    public void RenderScene(DrawingSurface target)
    {
        RenderOnionSkin(target);
        //RenderContext ctx = new(target, DocumentViewModel.AnimationHandler.ActiveFrameTime, Resolution, Document.Size); 
        //Document.NodeGraph.Execute(ctx);
    }

    private void RenderOnionSkin(DrawingSurface target)
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
            
            RenderContext onionContext = new(target, frame, Resolution, Document.Size, finalOpacity);
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
            RenderContext onionContext = new(target, frame, Resolution, Document.Size, finalOpacity);
            Document.NodeGraph.Execute(onionContext);
        }
    }
}
