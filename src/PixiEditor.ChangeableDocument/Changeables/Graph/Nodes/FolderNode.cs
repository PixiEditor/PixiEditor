using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Surfaces;
using PixiEditor.DrawingApi.Core.Surfaces.PaintImpl;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("Folder")]
public class FolderNode : StructureNode, IReadOnlyFolderNode, IClipSource, IPreviewRenderable
{
    private VecI documentSize;
    public RenderInputProperty Content { get; }

    public FolderNode()
    {
        Content = CreateRenderInput("Content", "CONTENT" /*, ctx =>
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
        }*/);
    }

    public override Node CreateCopy() => new FolderNode { MemberName = MemberName };

    public override VecD GetScenePosition(KeyFrameTime time) => documentSize / 2f; //GetTightBounds(time).GetValueOrDefault().Center;
    public override VecD GetSceneSize(KeyFrameTime time) => documentSize; //GetTightBounds(time).GetValueOrDefault().Size;

    protected override void OnExecute(RenderContext context)
    {
        base.OnExecute(context);
        documentSize = context.DocumentSize;
    }

    public override void Render(SceneObjectRenderContext sceneContext)
    {
        if (!IsVisible.Value || Opacity.Value <= 0 || IsEmptyMask())
        {
            Output.Value = Background.Value;
            return;
        }

        if (Content.Connection == null || (!HasOperations() && BlendMode.Value == Enums.BlendMode.Normal))
        {
            using Paint paint = new();
            paint.Color = Colors.White.WithAlpha((byte)Math.Round(Opacity.Value * 255f));
            int saved = sceneContext.RenderSurface.Canvas.SaveLayer(paint);
            Content.Value?.Paint(sceneContext, sceneContext.RenderSurface);
            
            sceneContext.RenderSurface.Canvas.RestoreToCount(saved);
            return;
        }

        RectD bounds = RectD.Create(VecI.Zero, sceneContext.DocumentSize);

        if (sceneContext.TargetPropertyOutput == Output)
        {
            if (Background.Value != null)
            {
                blendPaint.BlendMode = RenderContext.GetDrawingBlendMode(BlendMode.Value);
            }

            RenderFolder(sceneContext, bounds, true);
        }
        else if (sceneContext.TargetPropertyOutput == FilterlessOutput ||
                 sceneContext.TargetPropertyOutput == RawOutput)
        {
            RenderFolder(sceneContext, bounds, false);
        }
    }

    private void RenderFolder(SceneObjectRenderContext sceneContext, RectD bounds, bool useFilters)
    {
        VecI size = (VecI)bounds.Size;
        var outputWorkingSurface = RequestTexture(0, size, true);
        
        blendPaint.Color = Colors.White.WithAlpha((byte)Math.Round(Opacity.Value * 255f));
        int saved = outputWorkingSurface.DrawingSurface.Canvas.SaveLayer(blendPaint);
        
        Content.Value?.Paint(sceneContext, outputWorkingSurface.DrawingSurface);
        
        outputWorkingSurface.DrawingSurface.Canvas.RestoreToCount(saved);

        ApplyMaskIfPresent(outputWorkingSurface.DrawingSurface, sceneContext);

        if (Background.Value != null)
        {
            Texture tempSurface = RequestTexture(1, outputWorkingSurface.Size);
            if (Background.Connection.Node is IClipSource clipSource)
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

    public override RectD? GetPreviewBounds(int frame, string elementFor = "")
    {
        if (elementFor == nameof(EmbeddedMask))
        {
            return base.GetPreviewBounds(frame, elementFor);
        }

        return GetTightBounds(frame);
    }

    public override bool RenderPreview(DrawingSurface renderOn, ChunkResolution resolution, int frame,
        string elementToRenderName)
    {
        if (elementToRenderName == nameof(EmbeddedMask))
        {
            return base.RenderPreview(renderOn, resolution, frame, elementToRenderName);
        }

        if (Content.Connection != null)
        {
            var executionQueue = GraphUtils.CalculateExecutionQueue(Content.Connection.Node);
            while (executionQueue.Count > 0)
            {
                IReadOnlyNode node = executionQueue.Dequeue();
                if (node is IPreviewRenderable previewRenderable)
                {
                    previewRenderable.RenderPreview(renderOn, resolution, frame, elementToRenderName);
                }
            }
        }

        return true;
    }
}
