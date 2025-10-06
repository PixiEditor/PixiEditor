using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using Drawie.Backend.Core.Surfaces;
using PixiEditor.Models.DocumentModels;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.Models.Rendering;

internal class AnimationKeyFramePreviewRenderer(DocumentInternalParts internals)
{
    public RectD? GetPreviewBounds(int frame, string elementToRenderName = "")
    {
        if (internals.Tracker.Document.AnimationData.TryFindKeyFrame(
                Guid.Parse(elementToRenderName),
                out KeyFrame keyFrame))
        {
            return new RectD(VecD.Zero, internals.Tracker.Document.Size); 
        }
        
        return null;
    }

    /*public bool RenderPreview(DrawingSurface renderOn, RenderContext context, string elementToRenderName)
    {
        if (internals.Tracker.Document.AnimationData.TryFindKeyFrame(
                Guid.Parse(elementToRenderName),
                out KeyFrame keyFrame))
        {
            var nodeId = keyFrame.NodeId;
            var node = internals.Tracker.Document.NodeGraph.AllNodes.FirstOrDefault(x => x.Id == nodeId);
            
            if (node is IPreviewRenderable previewRenderable)
            {
                return previewRenderable.RenderPreview(renderOn, context, elementToRenderName);
            }
        }
        
        return false;
    }*/
}
