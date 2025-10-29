using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[NodeInfo("Folder")]
public class FolderNode : StructureNode, IReadOnlyFolderNode, IClipSource
{
    public const string ContentInternalName = "Content";
    private VecI documentSize;
    public RenderInputProperty Content { get; }

    public FolderNode()
    {
        Content = CreateRenderInput(ContentInternalName, "CONTENT");
        AllowHighDpiRendering = true;
    }

    public override Node CreateCopy() => new FolderNode
    {
        MemberName = MemberName,
        ClipToPreviousMember = this.ClipToPreviousMember,
        EmbeddedMask = this.EmbeddedMask?.CloneFromCommitted()
    };

    public override VecD GetScenePosition(KeyFrameTime time) =>
        documentSize / 2f;

    public override VecD GetSceneSize(KeyFrameTime time) =>
        documentSize;

    protected override void OnExecute(RenderContext context)
    {
        base.OnExecute(context);
        documentSize = context.RenderOutputSize;
    }

    public override void Render(SceneObjectRenderContext sceneContext)
    {
        RenderPreviews(sceneContext);
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

            int saved = sceneContext.RenderSurface.SaveLayer(paint);
            Content.Value?.Paint(sceneContext, sceneContext.RenderSurface);

            sceneContext.RenderSurface.RestoreToCount(saved);
            return;
        }

        if (sceneContext.TargetPropertyOutput == Output)
        {
            if (Background.Value != null)
            {
                blendPaint.BlendMode = RenderContext.GetDrawingBlendMode(BlendMode.Value);
            }

            RenderFolderContent(sceneContext, true);
        }
        else if (sceneContext.TargetPropertyOutput == FilterlessOutput ||
                 sceneContext.TargetPropertyOutput == RawOutput)
        {
            RenderFolderContent(sceneContext, false);
        }
    }

    private void RenderFolderContent(SceneObjectRenderContext sceneContext, bool useFilters)
    {
        VecI size = sceneContext.RenderSurface.DeviceClipBounds.Size + sceneContext.RenderSurface.DeviceClipBounds.Pos;
        var outputWorkingSurface = RequestTexture(0, size, sceneContext.ProcessingColorSpace, true);
        outputWorkingSurface.DrawingSurface.Canvas.Save();
        outputWorkingSurface.DrawingSurface.Canvas.SetMatrix(sceneContext.RenderSurface.TotalMatrix);

        int saved = sceneContext.RenderSurface.Save();
        sceneContext.RenderSurface.SetMatrix(Matrix3X3.Identity);

        blendPaint.ImageFilter = null;
        blendPaint.ColorFilter = null;

        Content.Value?.Paint(sceneContext, outputWorkingSurface.DrawingSurface.Canvas);

        int saved2 = outputWorkingSurface.DrawingSurface.Canvas.Save();
        outputWorkingSurface.DrawingSurface.Canvas.Scale((float)sceneContext.ChunkResolution.InvertedMultiplier());

        ApplyMaskIfPresent(outputWorkingSurface.DrawingSurface.Canvas, sceneContext, sceneContext.ChunkResolution);

        outputWorkingSurface.DrawingSurface.Canvas.RestoreToCount(saved2);

        if (Background.Value != null && sceneContext.TargetPropertyOutput != RawOutput)
        {
            Texture tempSurface = RequestTexture(1, outputWorkingSurface.Size, sceneContext.ProcessingColorSpace);
            tempSurface.DrawingSurface.Canvas.Save();
            tempSurface.DrawingSurface.Canvas.SetMatrix(outputWorkingSurface.DrawingSurface.Canvas.TotalMatrix);

            outputWorkingSurface.DrawingSurface.Canvas.SetMatrix(Matrix3X3.Identity);
            if (Background.Connection.Node is IClipSource clipSource && ClipToPreviousMember)
            {
                DrawClipSource(tempSurface.DrawingSurface.Canvas, clipSource, sceneContext);
            }

            ApplyRasterClip(outputWorkingSurface.DrawingSurface, tempSurface.DrawingSurface);
        }

        AdjustPaint(useFilters);

        blendPaint.BlendMode = RenderContext.GetDrawingBlendMode(BlendMode.Value);
        sceneContext.RenderSurface.DrawSurface(outputWorkingSurface.DrawingSurface, 0, 0, blendPaint);

        sceneContext.RenderSurface.RestoreToCount(saved);
        outputWorkingSurface.DrawingSurface.Canvas.Restore();
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
        RectD? bounds = null;
        if (!IsVisible.Value)
            return null;

        if (Content.Connection != null)
        {
            Content.Connection.Node.TraverseBackwards(
                (n, input) =>
                {
                    if (n is StructureNode { IsVisible.Value: true } structureNode)
                    {
                        RectD? childBounds = structureNode.GetTightBounds(frameTime);
                        if (childBounds != null)
                        {
                            if (bounds == null)
                            {
                                bounds = childBounds;
                            }
                            else
                            {
                                bounds = bounds.Value.Union(childBounds.Value);
                            }
                        }
                    }

                    return true;
                }, FilterInvisibleFolders);

            return bounds ?? RectD.Empty;
        }

        return null;
    }

    public override RectD? GetApproxBounds(KeyFrameTime frameTime)
    {
        RectD? bounds = null;
        if (Content.Connection != null)
        {
            Content.Connection.Node.TraverseBackwards(
                (n, input) =>
                {
                    if (n is StructureNode { IsVisible.Value: true } structureNode)
                    {
                        RectD? childBounds = structureNode.GetApproxBounds(frameTime);
                        if (childBounds != null)
                        {
                            if (bounds == null)
                            {
                                bounds = childBounds;
                            }
                            else
                            {
                                bounds = bounds.Value.Union(childBounds.Value);
                            }
                        }
                    }

                    return true;
                }, FilterInvisibleFolders);

            return bounds ?? RectD.Empty;
        }

        return null;
    }

    private bool FilterInvisibleFolders(IInputProperty input)
    {
        if (input is
            {
                Node: IReadOnlyFolderNode folderNode, InternalPropertyName: FolderNode.ContentInternalName
            })
        {
            return folderNode.IsVisible.Value;
        }

        return true;
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

    protected override bool ShouldRenderPreview(string elementToRenderName)
    {
        if (elementToRenderName == nameof(EmbeddedMask))
        {
            return base.ShouldRenderPreview(elementToRenderName);
        }

        return Content.Connection != null;
    }

    public override RectD? GetPreviewBounds(RenderContext ctx, string elementToRenderName)
    {
        return GetApproxBounds(ctx.FrameTime);
    }

    public override void RenderPreview(DrawingSurface renderOn, RenderContext context,
        string elementToRenderName)
    {
        if (elementToRenderName == nameof(EmbeddedMask))
        {
            base.RenderPreview(renderOn, context, elementToRenderName);
            return;
        }

        if (Content.Connection != null)
        {
            if (context is SceneObjectRenderContext ctx)
            {
                RenderFolderContent(ctx, true);
            }
        }
    }

    void IClipSource.DrawClipSource(SceneObjectRenderContext context, Canvas drawOnto)
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

    public IReadOnlyStructureNode[] GetChildrenNodes()
    {
        List<IReadOnlyStructureNode> children = new();
        if (Content.Connection != null)
        {
            Content.Connection.Node.TraverseBackwards((n) =>
            {
                if (n is IReadOnlyStructureNode structureNode)
                {
                    children.Add(structureNode);
                }

                return true;
            });
        }

        return children.ToArray();
    }
}
