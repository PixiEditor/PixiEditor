using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("Folder")]
public class FolderNode : StructureNode, IReadOnlyFolderNode, IClipSource, IPreviewRenderable
{
    public const string ContentInternalName = "Content";
    private VecI documentSize;
    public RenderInputProperty Content { get; }

    public FolderNode()
    {
        Content = CreateRenderInput(ContentInternalName, "CONTENT");
        AllowHighDpiRendering = true;
    }

    public override Node CreateCopy() => new FolderNode { MemberName = MemberName, ClipToPreviousMember = this.ClipToPreviousMember };

    public override VecD GetScenePosition(KeyFrameTime time) =>
        documentSize / 2f; 

    public override VecD GetSceneSize(KeyFrameTime time) =>
        documentSize; 

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

            if (sceneContext.TargetPropertyOutput == Output)
            {
                paint.ColorFilter = Filters.Value?.ColorFilter;
                paint.ImageFilter = Filters.Value?.ImageFilter;
            }

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

            RenderFolderContent(sceneContext, bounds, true);
        }
        else if (sceneContext.TargetPropertyOutput == FilterlessOutput ||
                 sceneContext.TargetPropertyOutput == RawOutput)
        {
            RenderFolderContent(sceneContext, bounds, false);
        }
    }

    private void RenderFolderContent(SceneObjectRenderContext sceneContext, RectD bounds, bool useFilters)
    {
        VecI size = (VecI)bounds.Size;
        var outputWorkingSurface = RequestTexture(0, size, sceneContext.ProcessingColorSpace, true);

        blendPaint.ImageFilter = null;
        blendPaint.ColorFilter = null;

        Content.Value?.Paint(sceneContext, outputWorkingSurface.DrawingSurface);

        ApplyMaskIfPresent(outputWorkingSurface.DrawingSurface, sceneContext);

        if (Background.Value != null && sceneContext.TargetPropertyOutput != RawOutput)
        {
            Texture tempSurface = RequestTexture(1, outputWorkingSurface.Size, sceneContext.ProcessingColorSpace);
            if (Background.Connection.Node is IClipSource clipSource && ClipToPreviousMember)
            {
                DrawClipSource(tempSurface.DrawingSurface, clipSource, sceneContext);
            }

            ApplyRasterClip(outputWorkingSurface.DrawingSurface, tempSurface.DrawingSurface);
        }

        AdjustPaint(useFilters);

        blendPaint.BlendMode = RenderContext.GetDrawingBlendMode(BlendMode.Value);
        sceneContext.RenderSurface.Canvas.DrawSurface(outputWorkingSurface.DrawingSurface, 0, 0, blendPaint);
    }

    private void AdjustPaint(bool useFilters)
    {
        blendPaint.Color = Colors.White.WithAlpha((byte)Math.Round(Opacity.Value * 255f));
        if (useFilters)
        {
            blendPaint.ColorFilter = Filters.Value?.ColorFilter;
            blendPaint.ImageFilter = Filters.Value?.ImageFilter;
        }
        else
        {
            blendPaint.ColorFilter = null;
            blendPaint.ImageFilter = null;
        }
    }

    public override RectD? GetTightBounds(KeyFrameTime frameTime)
    {
        RectI? bounds = null;
        if (Content.Connection != null)
        {
            Content.Connection.Node.TraverseBackwards((n) =>
            {
                if (n is StructureNode structureNode)
                {
                    RectI? imageBounds = (RectI?)structureNode.GetTightBounds(frameTime);
                    if (imageBounds != null)
                    {
                        if (bounds == null)
                        {
                            bounds = imageBounds;
                        }
                        else
                        {
                            bounds = bounds.Value.Union(imageBounds.Value);
                        }
                    }
                }

                return true;
            });

            return (RectD?)bounds ?? RectD.Empty;
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

    public override RectD? GetPreviewBounds(int frame, string elementFor = "")
    {
        if (elementFor == nameof(EmbeddedMask))
        {
            return base.GetPreviewBounds(frame, elementFor);
        }

        return GetTightBounds(frame);
    }

    public override bool RenderPreview(DrawingSurface renderOn, RenderContext context,
        string elementToRenderName)
    {
        if (elementToRenderName == nameof(EmbeddedMask))
        {
            return base.RenderPreview(renderOn, context, elementToRenderName);
        }

        if (Content.Connection != null)
        {
            var executionQueue = GraphUtils.CalculateExecutionQueue(Content.Connection.Node);
            while (executionQueue.Count > 0)
            {
                IReadOnlyNode node = executionQueue.Dequeue();
                if (node is IPreviewRenderable previewRenderable)
                {
                    previewRenderable.RenderPreview(renderOn, context, elementToRenderName);
                }
            }
        }

        return true;
    }

    void IClipSource.DrawClipSource(SceneObjectRenderContext context, DrawingSurface drawOnto)
    {
        if (Content.Connection != null)
        {
            var executionQueue = GraphUtils.CalculateExecutionQueue(Content.Connection.Node);

            while (executionQueue.Count > 0)
            {
                IReadOnlyNode node = executionQueue.Dequeue();
                if (node is IClipSource clipSource)
                {
                    clipSource.DrawClipSource(context, drawOnto);
                }
            }
        }
    }
}
