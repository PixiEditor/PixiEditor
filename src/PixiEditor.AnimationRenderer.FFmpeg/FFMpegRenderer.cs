using System.Drawing;
using FFMpegCore;
using FFMpegCore.Arguments;
using FFMpegCore.Enums;
using PixiEditor.AnimationRenderer.Core;
using PixiEditor.Numerics;

namespace PixiEditor.AnimationRenderer.FFmpeg;

public class FFMpegRenderer : IAnimationRenderer
{
    public int FrameRate { get; set; } = 60;
    public string OutputFormat { get; set; } = "mp4";
    public VecI Size { get; set; }

    public async Task<bool> RenderAsync(string framesPath, string outputPath)
    {
        string[] frames = Directory.GetFiles(framesPath, "*.png");
        if (frames.Length == 0)
        {
            return false;
        }

        string[] finalFrames = new string[frames.Length];

        for (int i = 0; i < frames.Length; i++)
        {
            if (int.TryParse(Path.GetFileNameWithoutExtension(frames[i]), out int frameNumber))
            {
                finalFrames[frameNumber - 1] = frames[i];
            }
        }

        GlobalFFOptions.Configure(new FFOptions()
        {
            BinaryFolder = @"C:\ProgramData\chocolatey\lib\ffmpeg\tools\ffmpeg\bin",
        });

        try
        {
            if (RequiresPaletteGeneration())
            {
                GeneratePalette(finalFrames, framesPath);
            }

            var args = FFMpegArguments
                .FromConcatInput(finalFrames, options =>
                {
                    options.WithFramerate(FrameRate);
                });

            var outputArgs = GetProcessorForFormat(args, framesPath, outputPath);
            return await outputArgs.ProcessAsynchronously();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    private FFMpegArgumentProcessor GetProcessorForFormat(FFMpegArguments args, string framesPath, string outputPath)
    {
        return OutputFormat switch
        {
            "gif" => GetGifArguments(args, framesPath, outputPath),
            "mp4" => GetMp4Arguments(args, outputPath),
            _ => throw new NotSupportedException($"Output format {OutputFormat} is not supported")
        };
    }

    private FFMpegArgumentProcessor GetGifArguments(FFMpegArguments args, string framesPath, string outputPath)
    {
        return args
            .AddFileInput(Path.Combine(framesPath, "palette.png"))
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
                    .WithVideoCodec(VideoCodec.LibX264)
                    .WithConstantRateFactor(21)
                    .ForcePixelFormat("yuv420p");
            });
    }

    private bool RequiresPaletteGeneration()
    {
        return OutputFormat == "gif";
    }

    private void GeneratePalette(string[] frames, string path)
    {
        string palettePath = Path.Combine(path, "palette.png");
        FFMpegArguments
            .FromConcatInput(frames, options =>
            {
                options.WithFramerate(FrameRate);
            })
            .OutputToFile(palettePath, true, options =>
            {
                options
                    .WithCustomArgument($"-vf \"palettegen\"");
            })
            .ProcessSynchronously();
    }
}
