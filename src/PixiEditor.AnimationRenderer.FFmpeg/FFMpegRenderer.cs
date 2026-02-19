using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Drawie.Backend.Core.Surfaces;
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
    public QualityPreset QualityPreset { get; set; } = QualityPreset.VeryHigh;

    public Regex FramerateRegex { get; } = new Regex(@"(\d+(?:\.\d+)?) fps", RegexOptions.Compiled);

    public async Task<bool> RenderAsync(List<Image> rawFrames, string outputPath, CancellationToken cancellationToken,
        Action<double>? progressCallback = null)
    {
        PrepareFFMpeg();

        string tempPath = Path.Combine(Path.GetTempPath(), "PixiEditor", "Rendering");
        Directory.CreateDirectory(tempPath);

        string paletteTempPath = Path.Combine(tempPath, "palette.png");

        try
        {
            List<ImgFrame> frames = new();

            foreach (var frame in rawFrames)
            {
                frames.Add(new ImgFrame(frame));
            }

            RawVideoPipeSource streamPipeSource = new(frames) { FrameRate = FrameRate, };

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
        PrepareFFMpeg();

        string tempPath = Path.Combine(Path.GetTempPath(), "PixiEditor", "Rendering");
        Directory.CreateDirectory(tempPath);

        string paletteTempPath = Path.Combine(tempPath, "palette.png");

        try
        {
            List<ImgFrame> frames = new();

            foreach (var frame in rawFrames)
            {
                frames.Add(new ImgFrame(frame));
            }

            RawVideoPipeSource streamPipeSource = new(frames) { FrameRate = FrameRate, };

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

    public List<Frame> GetFrames(string inputPath, out double playbackFps)
    {
        PrepareFFMpeg();

        using var ms = new MemoryStream();
        if (!FFMpegArguments.FromFileInput(inputPath)
                .OutputToPipe(new StreamPipeSink(ms),
                    options =>
                        options.WithCustomArgument("-vsync 0")
                            .WithCustomArgument("-vcodec png")
                            .ForceFormat("image2pipe"))
                .ProcessSynchronously())
        {
            throw new InvalidOperationException("Failed to extract frames from video");
        }

        ms.Seek(0, SeekOrigin.Begin);
        List<Bitmap> frames = PipeUtil.ReadFramesFromPipe(ms);

        playbackFps = ExtractFramerateInfo(inputPath);

        return frames.Select(f => new Frame(f, 1)).ToList();
    }

    private double ExtractFramerateInfo(string inputPath)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName =
                Path.Combine(Path.GetDirectoryName(Environment.ProcessPath),
                    $"ThirdParty/{IOperatingSystem.Current.Name}/ffmpeg/ffmpeg"),
            Arguments = $"-i \"{inputPath}\"",
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using Process process = new() { StartInfo = startInfo };
        process.Start();
        process.WaitForExit();
        string info = process.StandardError.ReadToEnd();

        Match match = FramerateRegex.Match(info);
        if (match.Success && double.TryParse(match.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture,
                out double fps))
        {
            return fps;
        }

        return 24;
    }

    private static void PrepareFFMpeg()
    {
        string path = $"ThirdParty/{IOperatingSystem.Current.Name}/ffmpeg";

        string binaryPath = Path.Combine(Path.GetDirectoryName(Environment.ProcessPath), path);

        GlobalFFOptions.Configure(new FFOptions() { BinaryFolder = binaryPath });

        if (IOperatingSystem.Current.IsUnix)
        {
            MakeExecutableIfNeeded(binaryPath);
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
            "png" => GetApngArguments(args, outputPath),
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
        int qscale = QualityPreset switch
        {
            QualityPreset.VeryLow => 31,
            QualityPreset.Low => 25,
            QualityPreset.Medium => 19,
            QualityPreset.High => 10,
            QualityPreset.VeryHigh => 1,
            _ => 2
        };
        return args
            .OutputToFile(outputPath, true, options =>
            {
                options.WithFramerate(FrameRate)
                    .WithCustomArgument($"-qscale:v {qscale}")
                    .WithVideoCodec("mpeg4")
                    .ForcePixelFormat("yuv420p");
            });
    }

    private FFMpegArgumentProcessor GetApngArguments(FFMpegArguments args, string outputPath)
    {
        return args
            .OutputToFile(outputPath, true, options =>
            {
                options.WithFramerate(FrameRate)
                    .WithVideoCodec("apng")
                    .ForceFormat("apng");
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
