using System.Diagnostics;
using ChunkyImageLib;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using PixiEditor.AnimationRenderer.Core;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Helpers;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Parser;
using PixiEditor.UI.Common.Localization;
using PixiEditor.Views.Animations;

namespace PixiEditor.Models.IO.CustomDocumentFormats;

internal class AnimationDocumentBuilder : IDocumentBuilder
{
    public IAnimationRenderer Renderer { get; }
    public IReadOnlyCollection<string> Extensions { get; } = [".gif", ".png"];

    public AnimationDocumentBuilder(IAnimationRenderer renderer)
    {
        Renderer = renderer;
    }

    public void Build(DocumentViewModelBuilder builder, string path)
    {
        if (!Extensions.Any(ext => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException(
                $"Unsupported file format. Supported formats are: {string.Join(", ", Extensions)}");

        List<Frame> frames = null;
        double playbackFps = 60;
        try
        {
            frames = Renderer.GetFrames(path, out playbackFps);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load frames from {path}: {ex.Message}");
        }

        if (frames == null || frames.Count == 0)
        {
            var surface = Surface.Load(path);
            builder
                .WithSize(surface.Size)
                .WithGraph(x => x
                    .WithImageLayerNode(
                        new LocalizedString("IMAGE"),
                        surface,
                        ColorSpace.CreateSrgbLinear(),
                        out int id)
                    .WithOutputNode(id, "Output")
                );

            return;
        }

        if (frames.Count == 1)
        {
            var surface = new Surface(frames[0].ImageData.Size);
            surface.DrawingSurface.Canvas.DrawBitmap(frames[0].ImageData, 0, 0);
            frames[0].ImageData.Dispose();
            builder
                .WithSize(surface.Size)
                .WithGraph(x => x
                    .WithImageLayerNode(
                        new LocalizedString("IMAGE"),
                        surface,
                        ColorSpace.CreateSrgbLinear(),
                        out int id)
                    .WithOutputNode(id, "Output")
                );

            return;
        }

        VecI size = frames[0].ImageData.Size;

        AnimationData animationData = new AnimationData();
        animationData.FrameRate = (int)playbackFps;
        KeyFrameGroup layerGroup = new KeyFrameGroup();
        string layerName = new LocalizedString("BASE_LAYER_NAME");
        Surface firstFrameSurface = new Surface(size);
        firstFrameSurface.DrawingSurface.Canvas.DrawBitmap(frames[0].ImageData, 0, 0);

        builder.WithSize(size)
            .WithGraph(graph =>
                graph.WithImageLayerNode(layerName, firstFrameSurface, ColorSpace.CreateSrgbLinear(),
                        out int nodeId)
                    .WithOutputNode(nodeId, RenderNode.OutputPropertyName));

        var layerNode = builder.Graph.AllNodes.FirstOrDefault(x => x.Name == layerName);
        if (layerNode == null)
            throw new InvalidOperationException("Failed to find the created layer node.");

        layerGroup.NodeId = layerNode.Id;

        List<KeyFrameData> keyFrames = new List<KeyFrameData>();
        keyFrames.AddRange(layerNode.KeyFrames);

        int currentFrame = 1;
        for (int i = 0; i < frames.Count; i++)
        {
            var frame = frames[i];
            Surface surface = new Surface(frame.ImageData.Size);
            surface.DrawingSurface.Canvas.DrawBitmap(frame.ImageData, 0, 0);
            frame.ImageData.Dispose();

            KeyFrameData keyFrameData = new KeyFrameData
            {
                Id = keyFrames.Count,
                AffectedElement = ImageLayerNode.ImageLayerKey,
                Data = new ChunkyImage(surface, ColorSpace.CreateSrgbLinear()),
                StartFrame = currentFrame,
                Duration = frame.DurationTicks,
                IsVisible = true
            };

            keyFrames.Add(keyFrameData);
            layerGroup.Children.Add(new ElementKeyFrame { NodeId = layerGroup.NodeId, KeyFrameId = keyFrameData.Id });
            currentFrame += frame.DurationTicks;
        }

        layerNode.KeyFrames = keyFrames.ToArray();

        animationData.KeyFrameGroups = new List<KeyFrameGroup> { layerGroup };
        builder.WithAnimationData(animationData, null);
    }
}
