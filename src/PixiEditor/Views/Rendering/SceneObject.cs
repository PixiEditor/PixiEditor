using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.Numerics;

namespace PixiEditor.Views.Rendering;

public interface ISceneObject
{
    public VecD Position { get; set; }
    public VecD Size { get; set; }
    
    public RectD GlobalBounds => new RectD(Position, Size);
    
    public RenderGraph Graph { get; set; }

    public void RenderInScene(DrawingSurface surface)
    {
        RectD localBounds = new RectD(0, 0, Size.X, Size.Y);
        
        int savedNum = surface.Canvas.Save();
        surface.Canvas.ClipRect(RectD.Create((VecI)Position.Floor(), (VecI)Size.Ceiling()));
        surface.Canvas.Translate((float)Position.X, (float)Position.Y);
        
        RenderContext context = new RenderContext(surface, localBounds);
        
        Graph.RenderInLocalSpace(context);
        
        surface.Canvas.RestoreToCount(savedNum);
    }
}
