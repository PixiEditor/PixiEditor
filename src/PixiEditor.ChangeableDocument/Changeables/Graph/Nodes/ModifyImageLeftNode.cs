using System.Collections.Concurrent;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Context;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Shaders;
using PixiEditor.DrawingApi.Core.Shaders.Generation;
using PixiEditor.DrawingApi.Core.Shaders.Generation.Expressions;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("ModifyImageLeft")]
[PairNode(typeof(ModifyImageRightNode), "ModifyImageZone", true)]
public class ModifyImageLeftNode : Node, IPairNode
{
    public InputProperty<Texture?> Image { get; }

    public FuncOutputProperty<Float2> Coordinate { get; }

    public FuncOutputProperty<Half4> Color { get; }

    public Guid OtherNode { get; set; }

    private ConcurrentDictionary<RenderingContext, Pixmap> pixmapCache = new();

    public ModifyImageLeftNode()
    {
        Image = CreateInput<Texture>("Surface", "IMAGE", null);
        Coordinate = CreateFuncOutput("Coordinate", "UV", ctx => ctx.OriginalPosition);
        Color = CreateFuncOutput("Color", "COLOR", GetColor);
    }

    private Half4 GetColor(FuncContext context)
    {
        context.ThrowOnMissingContext();

        return context.SampleTexture(Image.Value, context.SamplePosition);

        /*var targetPixmap = pixmapCache[context.RenderingContext];

        if (targetPixmap == null)
            return new Color();

        var x = context.Position.X * context.Size.X;
        var y = context.Position.Y * context.Size.Y;

        return targetPixmap.GetPixelColor((int)x, (int)y);*/
    }

    internal void PreparePixmap(RenderingContext forContext)
    {
        pixmapCache[forContext] = Image.Value?.PeekReadOnlyPixels();
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
