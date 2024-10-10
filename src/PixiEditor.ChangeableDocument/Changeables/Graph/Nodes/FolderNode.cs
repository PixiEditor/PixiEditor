using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("Folder")]
public class FolderNode : StructureNode, IReadOnlyFolderNode
{
    public RenderInputProperty Content { get; }

    public FolderNode()
    {
        Content = CreateRenderInput("Content", "CONTENT", ctx =>
        {
            RectD? bounds = new RectD(VecI.Zero, ctx.DocumentSize);
            
            // Folder doesn't need to do anything if no operations are present
            if (bounds == null || !HasOperations()) 
            {
                return Output.GetFirstRenderTarget(ctx);
            }

            VecI size = (VecI)bounds.Value.Size;
            var outputWorkingSurface = RequestTexture(0, size, false);
            return outputWorkingSurface.DrawingSurface;
        });
    }

    public override Node CreateCopy() => new FolderNode { MemberName = MemberName };


    protected override void OnExecute(RenderContext context)
    {
        base.OnExecute(context);

        /*if(Background.Value == null && Content.Value == null)
        {
            Output.Value = null;
            return;
        }

        if (!IsVisible.Value || Opacity.Value <= 0 || IsEmptyMask())
        {
            Output.Value = Background.Value;
            return;
        }

        blendPaint.Color = new Color(255, 255, 255, 255);
        blendPaint.BlendMode = DrawingApi.Core.Surfaces.BlendMode.Src;

        if (Content.Value == null)
        {
            return;
        }

        VecI size = Content.Value?.Size ?? VecI.Zero;

        var outputWorkingSurface = RequestTexture(0, size);
        var filterlessWorkingSurface = RequestTexture(1, size);

        if (Background.Value != null)
        {
            DrawBackground(filterlessWorkingSurface.DrawingSurface, context);
            blendPaint.BlendMode = RenderingContext.GetDrawingBlendMode(BlendMode.Value);
        }

        if (Content.Value != null)
        {
            blendPaint.Color = blendPaint.Color.WithAlpha((byte)Math.Round(Opacity.Value * 255));
            DrawSurface(filterlessWorkingSurface.DrawingSurface, Content.Value, context, null);
        }

        FilterlessOutput.Value = filterlessWorkingSurface;

        if (!HasOperations())
        {
            if (Background.Value != null)
            {
                blendPaint.Color = new Color(255, 255, 255, 255);
                blendPaint.BlendMode = DrawingApi.Core.Surfaces.BlendMode.Src;
                DrawBackground(outputWorkingSurface.DrawingSurface, context);
                blendPaint.BlendMode = RenderingContext.GetDrawingBlendMode(BlendMode.Value);
            }

            if (Content.Value != null)
            {
                blendPaint.Color = blendPaint.Color.WithAlpha((byte)Math.Round(Opacity.Value * 255));
                DrawSurface(outputWorkingSurface.DrawingSurface, Content.Value.DrawingSurface, context, Filters.Value);
            }

            Output.Value = outputWorkingSurface.DrawingSurface;
        }

        if (Content.Value != null)
        {
            DrawSurface(outputWorkingSurface.DrawingSurface, Content.Value.DrawingSurface, context, Filters.Value);

            ApplyMaskIfPresent(outputWorkingSurface, context);
        }

        if (Background.Value != null)
        {
            Texture tempSurface = RequestTexture(2, outputWorkingSurface.Size);
            DrawBackground(tempSurface.DrawingSurface, context);

            ApplyRasterClip(outputWorkingSurface, tempSurface);

            blendPaint.Color = blendPaint.Color.WithAlpha((byte)Math.Round(Opacity.Value * 255));
            blendPaint.BlendMode = RenderingContext.GetDrawingBlendMode(BlendMode.Value);
            tempSurface.DrawingSurface.Canvas.DrawSurface(outputWorkingSurface.DrawingSurface, 0, 0, blendPaint);

            Output.Value = tempSurface.DrawingSurface;
            return;
        }

        Output.Value = outputWorkingSurface.DrawingSurface;*/
    }

    public override VecD ScenePosition => Content.Value?.DeviceClipBounds.Size / 2f ?? VecD.Zero;
    public override VecD SceneSize => Content.Value?.DeviceClipBounds.Size ?? VecD.Zero;

    public override void Render(SceneObjectRenderContext sceneContext)
    {
        RectD bounds = RectD.Create(VecI.Zero, sceneContext.DocumentSize); 
        
        if(Content.Connection == null || !HasOperations())
        {
            return;
        }
        
        VecI size = (VecI)bounds.Size;
        var outputWorkingSurface = RequestTexture(0, size, false);
        
        sceneContext.RenderSurface.Canvas.DrawSurface(outputWorkingSurface.DrawingSurface, 0, 0, blendPaint);
        outputWorkingSurface.DrawingSurface.Canvas.Clear();
    }

    public override RectD? GetTightBounds(KeyFrameTime frameTime)
    {
        RectI bounds = new RectI();
        if (Content.Connection != null)
        {
            Content.Connection.Node.TraverseBackwards((n) =>
            {
                if (n is ImageLayerNode imageLayerNode)
                {
                    RectI? imageBounds = (RectI?)imageLayerNode.GetTightBounds(frameTime);
                    if (imageBounds != null)
                    {
                        bounds = bounds.Union(imageBounds.Value);
                    }
                }

                return true;
            });

            return (RectD)bounds;
        }

        return null;
    }

    public HashSet<Guid> GetLayerNodeGuids()
    {
        HashSet<Guid> guids = new();
        Content.Connection?.Node.TraverseBackwards((n) =>
        {
            if (n is ImageLayerNode imageLayerNode)
            {
                guids.Add(imageLayerNode.Id);
            }

            return true;
        });

        return guids;
    }

    /// <summary>
    /// Creates a clone of the folder, its mask and all of its children
    /// </summary>
    /*internal override Folder Clone()
    {
        var builder = ImmutableList<StructureMember>.Empty.ToBuilder();
        for (var i = 0; i < Children.Count; i++)
        {
            var child = Children[i];
            builder.Add(child.Clone());
        }

        return new Folder
        {
            GuidValue = GuidValue,
            IsVisible = IsVisible,
            Name = Name,
            Opacity = Opacity,
            Children = builder.ToImmutable(),
            Mask = Mask?.CloneFromCommitted(),
            BlendMode = BlendMode,
            ClipToMemberBelow = ClipToMemberBelow,
            MaskIsVisible = MaskIsVisible
        };
    }*/
}
