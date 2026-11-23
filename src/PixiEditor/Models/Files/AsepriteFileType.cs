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
    public override FileTypeDialogDataSet.SetKind SetKind { get; } = FileTypeDialogDataSet.SetKind.Image | FileTypeDialogDataSet.SetKind.Video;
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

            if (config.ExportAsSpriteSheet)
            {
                Surface spriteSheet = GenerateSpriteSheet(document, config, job);
                if (spriteSheet == null)
                    return new SaveResult(SaveResultType.CustomError, "ERR_FAILED_GENERATE_SPRITE_SHEET");
                
                ushort swidth = (ushort)spriteSheet.Size.X;
                ushort sheight = (ushort)spriteSheet.Size.Y;
                
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
                            Width = swidth,
                            Height = sheight,
                            PixelData = ReadSurfacePixels(spriteSheet, swidth, sheight)
                        }
                    }
                });
                
                spriteSheet.Dispose();
                
                job?.Report(0.8, new LocalizedString("RENDERING_IMAGE"));
                var aseFile = AsepriteExporter.CreateFromLayers(swidth, sheight, layers, frames);
                AsepriteExporter.Write(pathWithExtension, aseFile);
            }
            else
            {
                bool forceRasterizeAll = config.AnimationRenderer != null;
                
                if (hasAnimation || forceRasterizeAll)
                {
                    int firstFrame = 1;
                    int lastFrame = document.AnimationDataViewModel.GetLastVisibleFrame();
                    if (lastFrame <= firstFrame && forceRasterizeAll) {
                        lastFrame = firstFrame + 1; // Ensure at least one frame if something is weird
                    }

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
            }

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

    private Surface? GenerateSpriteSheet(DocumentViewModel document, ExportConfig config, ExportJob? job)
    {
        if (document is null)
            return null;

        var (rows, columns) = (config.SpriteSheetRows, config.SpriteSheetColumns);

        rows = Math.Max(1, rows);
        columns = Math.Max(1, columns);

        Surface surface = new Surface(new VecI(config.ExportSize.X * columns, config.ExportSize.Y * rows));

        job?.Report(0, new LocalizedString("RENDERING_FRAME", 0, document.AnimationDataViewModel.FramesCount));

        document.RenderFramesProgressive(
            (frame, index) =>
            {
                job?.CancellationTokenSource?.Token.ThrowIfCancellationRequested();

                job?.Report(index / (double)document.AnimationDataViewModel.FramesCount,
                    new LocalizedString("RENDERING_FRAME", index, document.AnimationDataViewModel.FramesCount));
                int x = index % columns;
                int y = index / columns;
                Surface target = frame;
                if (config.ExportSize != frame.Size)
                {
                    target =
                        frame.ResizeNearestNeighbor(new VecI(config.ExportSize.X, config.ExportSize.Y));
                }

                surface!.DrawingSurface.Canvas.DrawSurface(target.DrawingSurface, x * config.ExportSize.X,
                    y * config.ExportSize.Y);
                target.Dispose();
            }, job?.CancellationTokenSource?.Token ?? CancellationToken.None, config.ExportOutput);

        return surface;
    }
}
