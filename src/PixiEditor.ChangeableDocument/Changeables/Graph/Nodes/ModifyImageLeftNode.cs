using System.Collections.Concurrent;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("ModifyImageLeft")]
[PairNode(typeof(ModifyImageRightNode), "ModifyImageZone", true)]
public class ModifyImageLeftNode : Node
{
    public InputProperty<Texture?> Image { get; }
    
    public FuncOutputProperty<VecD> Coordinate { get; }
    
    public FuncOutputProperty<Color> Color { get; }

    public override string DisplayName { get; set; } = "MODIFY_IMAGE_LEFT_NODE";
    
    private ConcurrentDictionary<RenderingContext, Pixmap> pixmapCache = new();

    public ModifyImageLeftNode()
    {
        Image = CreateInput<Texture>(nameof(Surface), "IMAGE", null);
        Coordinate = CreateFuncOutput(nameof(Coordinate), "UV", ctx => ctx.Position);
        Color = CreateFuncOutput(nameof(Color), "COLOR", GetColor);
    }

    private Color GetColor(FuncContext context)
    {
        context.ThrowOnMissingContext();

        var targetPixmap = pixmapCache[context.RenderingContext];
        
        if (targetPixmap == null)
            return new Color();
        
        var x = context.Position.X * context.Size.X;
        var y = context.Position.Y * context.Size.Y;
        
        return targetPixmap.GetPixelColor((int)x, (int)y);
    }

    internal void PreparePixmap(RenderingContext forContext)
    {
        pixmapCache[forContext] = Image.Value?.Surface.Snapshot().PeekPixels();
    }
    
    internal void DisposePixmap(RenderingContext forContext)
    {
        if (pixmapCache.TryRemove(forContext, out var targetPixmap))
        {
            targetPixmap?.Dispose();
        }
    }

    protected override Texture? OnExecute(RenderingContext context)
    {
        return Image.Value;
    }

    public override Node CreateCopy() => new ModifyImageLeftNode();
}
