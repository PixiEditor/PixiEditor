using System.Drawing;
using System.Reflection;
using FFMpegCore;
using FFMpegCore.Arguments;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using PixiEditor.AnimationRenderer.Core;
using PixiEditor.DrawingApi.Core.Surfaces.ImageData;
using PixiEditor.Numerics;

namespace PixiEditor.AnimationRenderer.FFmpeg;

public class FFMpegRenderer : IAnimationRenderer
{
    public int FrameRate { get; set; } = 60;
    public string OutputFormat { get; set; } = "mp4";
    public VecI Size { get; set; }

    public async Task<bool> RenderAsync(List<Image> rawFrames, string outputPath, CancellationToken cancellationToken, Action<double>? progressCallback = null)
    {
        string path = "ThirdParty/{0}/ffmpeg";
#if WINDOWS
        path = string.Format(path, "Windows");
#elif MACOS
        path = string.Format(path, "MacOS");
#elif LINUX
        path = string.Format(path, "Linux");
#endif

        GlobalFFOptions.Configure(new FFOptions()
        {
            BinaryFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), path),
        });

        try
        {
            List<ImgFrame> frames = new();
            
            foreach (var frame in rawFrames)
            {
                frames.Add(new ImgFrame(frame));
            }

            RawVideoPipeSource streamPipeSource = new(frames) { FrameRate = FrameRate, };
            
            string paletteTempPath = Path.Combine(Path.GetDirectoryName(outputPath), "RenderTemp", "palette.png");
            
            if (!Directory.Exists(Path.GetDirectoryName(paletteTempPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(paletteTempPath));
            }
            
            if (RequiresPaletteGeneration())
            {
                GeneratePalette(streamPipeSource, paletteTempPath);
            }
            
            streamPipeSource = new(frames) { FrameRate = FrameRate, };

            var args = FFMpegArguments
                .FromPipeInput(streamPipeSource, options =>
                {
                    options.WithFramerate(FrameRate);
                });

            var outputArgs = GetProcessorForFormat(args, outputPath, paletteTempPath);
            TimeSpan totalTimeSpan = TimeSpan.FromSeconds(frames.Count / (float)FrameRate);
            var result = await outputArgs.CancellableThrough(cancellationToken)
                .NotifyOnProgress(progressCallback, totalTimeSpan).ProcessAsynchronously();
            
            if (RequiresPaletteGeneration())
            {
                File.Delete(paletteTempPath);
                Directory.Delete(Path.GetDirectoryName(paletteTempPath));
            }
            
            DisposeStream(frames);
            
            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    private void DisposeStream(List<ImgFrame> frames)
    {
        foreach (var frame in frames)
        {
            frame.Dispose();
        }
    }

    private FFMpegArgumentProcessor GetProcessorForFormat(FFMpegArguments args, string outputPath,
        string paletteTempPath)
    {
        return OutputFormat switch
        {
            "gif" => GetGifArguments(args, outputPath, paletteTempPath),
            "mp4" => GetMp4Arguments(args, outputPath),
            _ => throw new NotSupportedException($"Output format {OutputFormat} is not supported")
        };
    }

    private FFMpegArgumentProcessor GetGifArguments(FFMpegArguments args, string outputPath, string paletteTempPath)
    {
        return args
            .AddFileInput(paletteTempPath)
            .OutputToFile(outputPath, true, options =>
            {
                options.WithCustomArgument(
                        $"-filter_complex \"[0:v]fps={FrameRate},scale={Size.X}:{Size.Y}:flags=lanczos[x];[x][1:v]paletteuse\"") // Apply the palette
                    .WithCustomArgument($"-vsync 0"); // Ensure each input frame gets displayed exactly once
            });
    }

    private FFMpegArgumentProcessor GetMp4Arguments(FFMpegArguments args, string outputPath)
    {
        return args
            .OutputToFile(outputPath, true, options =>
            {
                options.WithFramerate(FrameRate)
                    .WithConstantRateFactor(18)
                    .WithVideoBitrate(1800)
                    .WithVideoCodec("mpeg4")
                    .ForcePixelFormat("yuv420p");
            });
    }

    private bool RequiresPaletteGeneration()
    {
        return OutputFormat == "gif";
    }

    private void GeneratePalette(IPipeSource imageStream, string path)
    {
        FFMpegArguments
            .FromPipeInput(imageStream, options =>
            {
                options.WithFramerate(FrameRate);
            })
            .OutputToFile(path, true, options =>
            {
                options
                    .WithCustomArgument($"-vf \"palettegen\"");
            })
            .ProcessSynchronously();
    }
}
