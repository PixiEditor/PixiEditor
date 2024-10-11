using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("Folder")]
public class FolderNode : StructureNode, IReadOnlyFolderNode, IClipSource
{
    public RenderInputProperty Content { get; }

    public FolderNode()
    {
        Content = CreateRenderInput("Content", "CONTENT", ctx =>
        {
            RectD? bounds = new RectD(VecI.Zero, ctx.DocumentSize);

            // Folder doesn't need to do anything if no operations are present
            if (bounds == null || (!HasOperations() && BlendMode.Value == Enums.BlendMode.Normal))
            {
                return Output.GetFirstRenderTarget(ctx);
            }

            VecI size = (VecI)bounds.Value.Size;
            var outputWorkingSurface = RequestTexture(0, size, false);
            return outputWorkingSurface.DrawingSurface;
        });
    }

    public override Node CreateCopy() => new FolderNode { MemberName = MemberName };

    public override VecD ScenePosition => Content.Value?.DeviceClipBounds.Size / 2f ?? VecD.Zero;
    public override VecD SceneSize => Content.Value?.DeviceClipBounds.Size ?? VecD.Zero;

    public override void Render(SceneObjectRenderContext sceneContext)
    {
        RectD bounds = RectD.Create(VecI.Zero, sceneContext.DocumentSize);
        if (!IsVisible.Value || Opacity.Value <= 0 || IsEmptyMask())
        {
            Output.Value = sceneContext.RenderSurface;
            return;
        }

        if (Content.Connection == null || (!HasOperations() && BlendMode.Value == Enums.BlendMode.Normal))
        {
            return;
        }

        VecI size = (VecI)bounds.Size;
        var outputWorkingSurface = RequestTexture(0, size, false);

        if (RenderTarget.Value != null)
        {
            blendPaint.BlendMode = RenderContext.GetDrawingBlendMode(BlendMode.Value);
        }

        ApplyMaskIfPresent(outputWorkingSurface.DrawingSurface, sceneContext);

        if (RenderTarget.Value != null)
        {
            Texture tempSurface = RequestTexture(1, outputWorkingSurface.Size);
            if (RenderTarget.Connection.Node is IClipSource clipSource)
            {
                DrawClipSource(tempSurface.DrawingSurface, clipSource, sceneContext);
            }

            ApplyRasterClip(outputWorkingSurface.DrawingSurface, tempSurface.DrawingSurface);
            blendPaint.BlendMode = RenderContext.GetDrawingBlendMode(BlendMode.Value);
            tempSurface.DrawingSurface.Canvas.DrawSurface(outputWorkingSurface.DrawingSurface, 0, 0, blendPaint);

            sceneContext.RenderSurface.Canvas.DrawSurface(tempSurface.DrawingSurface, 0, 0, blendPaint);
            outputWorkingSurface.DrawingSurface.Canvas.Clear();
            return;
        }

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
    public void DrawOnTexture(SceneObjectRenderContext context, DrawingSurface drawOnto)
    {
        if (Content.Connection != null)
        {
            var executionQueue = GraphUtils.CalculateExecutionQueue(Content.Connection.Node);
            
            while (executionQueue.Count > 0)
            {
                IReadOnlyNode node = executionQueue.Dequeue();
                if (node is IClipSource clipSource)
                {
                    clipSource.DrawOnTexture(context, drawOnto);
                }
            }
        }
    }
}
