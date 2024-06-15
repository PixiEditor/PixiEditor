using FFMpegCore;
using FFMpegCore.Enums;
using PixiEditor.AnimationRenderer.Core;

namespace PixiEditor.AnimationRenderer.FFmpeg;

public class FFMpegRenderer : IAnimationRenderer
{
    public async Task<bool> RenderAsync(string framesPath, int frameRate = 60)
    {
        string[] frames = Directory.GetFiles(framesPath, "*.png");
        if (frames.Length == 0)
        {
            return false;
        }
        
        GlobalFFOptions.Configure(new FFOptions() { BinaryFolder = @"C:\ProgramData\chocolatey\lib\ffmpeg\tools\ffmpeg\bin" });
        
        return await FFMpegArguments
            .FromConcatInput(frames)
            .OutputToFile($"{framesPath}/output.mp4", true, options =>
            {
                options.WithVideoCodec(VideoCodec.LibX264)
                    .WithFramerate(frameRate);
            })
            .ProcessAsynchronously();
    }
}
