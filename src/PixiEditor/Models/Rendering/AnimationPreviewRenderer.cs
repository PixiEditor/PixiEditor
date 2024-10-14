using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Numerics;

namespace PixiEditor.Models.Rendering;

internal class AnimationKeyFramePreviewRenderer(DocumentInternalParts internals) : IPreviewRenderable
{
    private readonly DocumentInternalParts internals = internals;
    
    public RectD? GetPreviewBounds(int frame, string elementToRenderName = "")
    {
        if (internals.Tracker.Document.AnimationData.TryFindKeyFrame(
                Guid.Parse(elementToRenderName),
                out KeyFrame keyFrame))
        {
            var nodeId = keyFrame.NodeId;
            var node = internals.Tracker.Document.NodeGraph.AllNodes.FirstOrDefault(x => x.Id == nodeId);
            
            if (node is IPreviewRenderable previewRenderable)
            {
                return previewRenderable.GetPreviewBounds(0, elementToRenderName); 
            }
        }
        
        return null;
    }

    public bool RenderPreview(DrawingSurface renderOn, ChunkResolution resolution, int frame, string elementToRenderName)
    {
        if (internals.Tracker.Document.AnimationData.TryFindKeyFrame(
                Guid.Parse(elementToRenderName),
                out KeyFrame keyFrame))
        {
            var nodeId = keyFrame.NodeId;
            var node = internals.Tracker.Document.NodeGraph.AllNodes.FirstOrDefault(x => x.Id == nodeId);
            
            if (node is IPreviewRenderable previewRenderable)
            {
                return previewRenderable.RenderPreview(renderOn, resolution, frame, elementToRenderName);
            }
        }
        
        return false;
    }
}
