using PixiEditor.ChangeableDocument.Changeables.Animations;
using Avalonia.Media;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using PixiEditor.Models.IO;
using PixiEditor.Models.IO.CustomDocumentFormats.Aseprite;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Models.Files;

/// <summary>
/// IoFileType for Aseprite (.ase/.aseprite) files, enabling export from PixiEditor.
/// </summary>
internal class AsepriteFileType : IoFileType
{
    public override string[] Extensions => new[] { ".ase", ".aseprite" };
    public override string DisplayName => new LocalizedString("ASEPRITE_FILE");
    public override FileTypeDialogDataSet.SetKind SetKind { get; } = FileTypeDialogDataSet.SetKind.Image;
    public override SolidColorBrush EditorColor { get; } = new SolidColorBrush(Color.FromRgb(125, 137, 150));

    public override async Task<SaveResult> TrySaveAsync(string pathWithExtension, DocumentViewModel document,
        ExportConfig config, ExportJob? job)
    {
        return await Task.Run(() => TrySave(pathWithExtension, document, config, job));
    }

    public override SaveResult TrySave(string pathWithExtension, DocumentViewModel document,
        ExportConfig config, ExportJob? job)
    {
        try
        {
            job?.Report(0, new LocalizedString("RENDERING_IMAGE"));

            int frameCount = Math.Max(1, document.AnimationDataViewModel.FramesCount);
            bool hasAnimation = document.AnimationDataViewModel.KeyFrames.Count > 0;

            ushort width = (ushort)config.ExportSize.X;
            ushort height = (ushort)config.ExportSize.Y;

            if (width <= 0 || height <= 0)
                return new SaveResult(SaveResultType.CustomError, "ERR_EXPORT_SIZE_INVALID");

            var layers = new List<AsepriteLayerInfo>
            {
                new AsepriteLayerInfo
                {
                    Name = "Layer",
                    Opacity = 255,
                    BlendMode = 0,
                    IsVisible = true,
                    IsEditable = true,
                    IsGroup = false,
                    ChildLevel = 0
                }
            };

            var frames = new List<AsepriteFrameInfo>();

            if (hasAnimation)
            {
                int firstFrame = 1;
                int lastFrame = document.AnimationDataViewModel.GetLastVisibleFrame();

                for (int f = firstFrame; f < lastFrame; f++)
                {
                    job?.CancellationTokenSource?.Token.ThrowIfCancellationRequested();
                    double progress = (double)(f - firstFrame) / Math.Max(1, lastFrame - firstFrame);
                    job?.Report(progress, new LocalizedString("RENDERING_FRAME", f, lastFrame - 1));

                    var frameTime = new KeyFrameTime(f, progress);
                    var renderResult = document.TryRenderWholeImage(frameTime, config.ExportSize, config.ExportOutput);

                    if (renderResult.IsT0)
                        return new SaveResult(SaveResultType.ConcurrencyError);

                    Surface frameSurface = renderResult.AsT1;
                    byte[] rgba = ReadSurfacePixels(frameSurface, width, height);
                    frameSurface.Dispose();

                    frames.Add(new AsepriteFrameInfo
                    {
                        DurationMs = (ushort)Math.Max(1,
                            (int)Math.Round(1000.0 / Math.Max(1, document.AnimationDataViewModel.FrameRateBindable))),
                        Cels = new List<AsepriteCelInfo>
                        {
                            new AsepriteCelInfo
                            {
                                LayerIndex = 0,
                                X = 0,
                                Y = 0,
                                Opacity = 255,
                                Width = width,
                                Height = height,
                                PixelData = rgba
                            }
                        }
                    });
                }
            }
            else
            {
                // Single frame
                var renderResult = document.TryRenderWholeImage(0, config.ExportSize, config.ExportOutput);

                if (renderResult.IsT0)
                    return new SaveResult(SaveResultType.ConcurrencyError);

                Surface surface = renderResult.AsT1;
                byte[] rgba = ReadSurfacePixels(surface, width, height);
                surface.Dispose();

                frames.Add(new AsepriteFrameInfo
                {
                    DurationMs = 100,
                    Cels = new List<AsepriteCelInfo>
                    {
                        new AsepriteCelInfo
                        {
                            LayerIndex = 0,
                            X = 0,
                            Y = 0,
                            Opacity = 255,
                            Width = width,
                            Height = height,
                            PixelData = rgba
                        }
                    }
                });
            }

            job?.Report(0.8, new LocalizedString("RENDERING_IMAGE"));

            var aseFile = AsepriteExporter.CreateFromLayers(width, height, layers, frames);
            AsepriteExporter.Write(pathWithExtension, aseFile);

            job?.Report(1, new LocalizedString("FINISHED"));

            return new SaveResult(SaveResultType.Success);
        }
        catch (OperationCanceledException)
        {
            return new SaveResult(SaveResultType.Cancelled);
        }
        catch (Exception e)
        {
            return new SaveResult(SaveResultType.UnknownError, e.Message);
        }
    }

    /// <summary>
    /// Reads RGBA pixel data from a Surface.
    /// </summary>
    private static byte[] ReadSurfacePixels(Surface surface, int width, int height)
    {
        var imageInfo = new ImageInfo(width, height, ColorType.Rgba8888, AlphaType.Unpremul);
        byte[] pixels = new byte[width * height * 4];

        unsafe
        {
            fixed (byte* ptr = pixels)
            {
                surface.DrawingSurface.ReadPixels(imageInfo, (IntPtr)ptr, width * 4, 0, 0);
            }
        }

        return pixels;
    }
}
