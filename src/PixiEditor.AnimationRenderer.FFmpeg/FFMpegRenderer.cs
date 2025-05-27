using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using FFMpegCore;
using FFMpegCore.Arguments;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using PixiEditor.AnimationRenderer.Core;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using PixiEditor.OperatingSystem;

namespace PixiEditor.AnimationRenderer.FFmpeg;

public class FFMpegRenderer : IAnimationRenderer
{
    public int FrameRate { get; set; } = 60;
    public string OutputFormat { get; set; } = "mp4";
    public VecI Size { get; set; }

    public async Task<bool> RenderAsync(List<Image> rawFrames, string outputPath, CancellationToken cancellationToken,
        Action<double>? progressCallback = null)
    {
        string path = $"ThirdParty/{IOperatingSystem.Current.Name}/ffmpeg";

        string binaryPath = Path.Combine(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory), path);

        GlobalFFOptions.Configure(new FFOptions() { BinaryFolder = binaryPath });

        if (IOperatingSystem.Current.IsUnix)
        {
            MakeExecutableIfNeeded(binaryPath);
        }

        string paletteTempPath = Path.Combine(Path.GetDirectoryName(outputPath), "RenderTemp", "palette.png");

        try
        {
            List<ImgFrame> frames = new();

            foreach (var frame in rawFrames)
            {
                frames.Add(new ImgFrame(frame));
            }

            RawVideoPipeSource streamPipeSource = new(frames) { FrameRate = FrameRate, };


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

            DisposeStream(frames);

            return result;
        }
        finally
        {
            if (RequiresPaletteGeneration() && File.Exists(paletteTempPath))
            {
                File.Delete(paletteTempPath);
                Directory.Delete(Path.GetDirectoryName(paletteTempPath));
            }
        }
    }

    public bool Render(List<Image> rawFrames, string outputPath, CancellationToken cancellationToken,
        Action<double>? progressCallback)
    {
        string path = $"ThirdParty/{IOperatingSystem.Current.Name}/ffmpeg";

        string binaryPath = Path.Combine(Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory), path);

        GlobalFFOptions.Configure(new FFOptions() { BinaryFolder = binaryPath });

        if (IOperatingSystem.Current.IsUnix)
        {
            MakeExecutableIfNeeded(binaryPath);
        }

        string paletteTempPath = Path.Combine(Path.GetDirectoryName(outputPath), "RenderTemp", "palette.png");

        try
        {
            List<ImgFrame> frames = new();

            foreach (var frame in rawFrames)
            {
                frames.Add(new ImgFrame(frame));
            }

            RawVideoPipeSource streamPipeSource = new(frames) { FrameRate = FrameRate, };


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
            var result = outputArgs.CancellableThrough(cancellationToken)
                .NotifyOnProgress(progressCallback, totalTimeSpan).ProcessSynchronously();

            DisposeStream(frames);

            return result;
        }
        finally
        {
            if (RequiresPaletteGeneration() && File.Exists(paletteTempPath))
            {
                File.Delete(paletteTempPath);
                Directory.Delete(Path.GetDirectoryName(paletteTempPath));
            }
        }
    }

    private static void MakeExecutableIfNeeded(string binaryPath)
    {
        string filePath = Path.Combine(binaryPath, "ffmpeg");

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("FFmpeg binary not found");
        }

        try
        {
            var process = IOperatingSystem.Current.ProcessUtility.Execute($"{filePath}", "-version");

            bool exited = process.WaitForExit(500);

            if (!exited)
            {
                throw new InvalidOperationException("Failed to perform FFmpeg check");
            }

            if (process.ExitCode == 0)
            {
                return;
            }

            IOperatingSystem.Current.ProcessUtility.Execute("chmod", $"+x {filePath}").WaitForExit(500);
        }
        catch (Exception e)
        {
            IOperatingSystem.Current.ProcessUtility.Execute("chmod", $"+x {filePath}")
                .WaitForExit(500);
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
