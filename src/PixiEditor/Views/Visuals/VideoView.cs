using System.Reflection;
using Avalonia.Input;
using Avalonia.Platform;
using Drawie.Numerics;
using PixiEditor.Helpers;
using PixiEditor.Models.IO;
using PixiEditor.OperatingSystem;

namespace PixiEditor.Views.Visuals;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

public class VideoView : Control
{
    private WriteableBitmap? _bitmap;
    private byte[]? _buffer;
    private Process? _process;
    private CancellationTokenSource? _cts;

    public static readonly StyledProperty<string?> SourceProperty =
        AvaloniaProperty.Register<VideoView, string?>(nameof(Source));

    public static readonly StyledProperty<int> VideoWidthProperty =
        AvaloniaProperty.Register<VideoView, int>(nameof(VideoWidth), 640);

    public static readonly StyledProperty<int> VideoHeightProperty =
        AvaloniaProperty.Register<VideoView, int>(nameof(VideoHeight), 360);

    public string? Source { get => GetValue(SourceProperty); set => SetValue(SourceProperty, value); }
    public int VideoWidth { get => GetValue(VideoWidthProperty); set => SetValue(VideoWidthProperty, value); }
    public int VideoHeight { get => GetValue(VideoHeightProperty); set => SetValue(VideoHeightProperty, value); }

    private Stopwatch? _playbackClock;
    private double currentTime;
    private readonly double fps = 60.0;
    private bool isPaused;
    private string? downloadedTempFile;

    public VideoView()
    {
        this.GetObservable(SourceProperty).Subscribe(async path =>
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                Task.Run(async () =>
                {
                    try
                    {
                        downloadedTempFile = await DownloadToTempFile(path, CancellationToken.None);
                        Dispatcher.UIThread.Post(Play);
                    }
                    catch
                    {
                        return;
                    }
                });
            }
        });
    }

    public void Stop()
    {
        _cts?.Dispose();
        _process?.Kill(true);
        _process?.Dispose();
        _process = null;
        _playbackClock?.Stop();
        currentTime = 0;
    }

    public void Pause()
    {
        if (isPaused)
            return;

        isPaused = true;

        try
        {
            _cts?.Cancel();
            _process?.Kill(true);
        }
        catch { }

        _process?.Dispose();
        _process = null;

        if (_playbackClock != null)
        {
            currentTime += _playbackClock.Elapsed.TotalSeconds;
            _playbackClock.Stop();
            _playbackClock = null;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (downloadedTempFile != null)
        {
            Stop();
            try { File.Delete(downloadedTempFile); }
            catch { }

            downloadedTempFile = null;
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (isPaused)
        {
            isPaused = false;
            Play();
        }
        else
        {
            Pause();
        }
    }

    private Process StartFFmpeg(string path, double time)
    {
        string args = $"-ss {time.ToString(System.Globalization.CultureInfo.InvariantCulture)} "
                      + "-fflags +genpts "
                      + $"-i \"{path}\" " + $"-vf scale={VideoWidth}:{VideoHeight},fps={fps} "
                      + "-f rawvideo -pix_fmt rgba -";
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg" + IOperatingSystem.Current.ExecutableExtension,
                WorkingDirectory =
                    Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "ThirdParty",
                        IOperatingSystem.Current.Name, "ffmpeg"),
                Arguments = args,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        process.Start();
        return process;
    }

    private void RenderFrame()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (_bitmap == null) return;
            using var fb = _bitmap.Lock();
            unsafe
            {
                fixed (byte* src = _buffer)
                {
                    Buffer.MemoryCopy(src, (void*)fb.Address, _buffer!.Length, _buffer.Length);
                }
            }

            InvalidateVisual();
        });
    }

    private void DecodeLoop(CancellationToken ct, int videoWidth, int videoHeight)
    {
        int frameSize = videoWidth * videoHeight * 4;
        var stream = _process!.StandardOutput.BaseStream;
        long frameIndex = 0;

        while (!ct.IsCancellationRequested)
        {
            int read = 0;
            while (read < frameSize)
            {
                int r = stream.Read(_buffer!, read, frameSize - read);
                if (r == 0)
                {
                    return;
                }

                read += r;
            }

            RenderFrame();
            double targetTimeSec = frameIndex / fps;
            double elapsedSec = _playbackClock?.Elapsed.TotalSeconds ?? 0;
            int sleepMs = (int)((targetTimeSec - elapsedSec) * 1000);

            if (sleepMs > 0)
                Thread.Sleep(sleepMs);

            frameIndex++;
        }
    }

    public async void Play()
    {
        if (Source == null || downloadedTempFile == null)
            return;

        _cts?.Dispose();
        _process?.Kill(true);

        _cts = new CancellationTokenSource();
        var ct = _cts.Token;

        int frameSize = VideoWidth * VideoHeight * 4;
        if (_buffer == null || _buffer.Length != frameSize)
        {
            _buffer = new byte[frameSize];
        }

        if (_bitmap == null || _bitmap?.PixelSize != new PixelSize(VideoWidth, VideoHeight))
        {
            _bitmap?.Dispose();
            _bitmap = null;
            _bitmap = new WriteableBitmap(
                new PixelSize(VideoWidth, VideoHeight),
                new Vector(96, 96),
                PixelFormat.Rgba8888);
        }

        try
        {
            if (ct.IsCancellationRequested)
                return;

            _process = StartFFmpeg(downloadedTempFile, currentTime);

            var associatedProcess = _process;
            _process.WaitForExitAsync(ct).ContinueWith(x =>
            {
                if (_playbackClock?.ElapsedMilliseconds > 0 && associatedProcess.HasExited)
                {
                    EndOfStreamReached();
                }
            }, TaskScheduler.Default);
            _playbackClock = Stopwatch.StartNew();

            int videoWidth = VideoWidth;
            int videoHeight = VideoHeight;

            _ = Task.Run(() => DecodeLoop(ct, videoWidth, videoHeight), ct);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task<string> DownloadToTempFile(string url, CancellationToken ct)
    {
        var vidoesPath = Path.Combine(Paths.TempFilesPath, "videos");
        if (!Directory.Exists(vidoesPath))
        {
            Directory.CreateDirectory(vidoesPath);
        }

        var tempPath = Path.Combine(vidoesPath, $"videoview_{Guid.NewGuid()}.mp4");

        using var http = new HttpClient();
        using var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        await using var input = await response.Content.ReadAsStreamAsync(ct);
        await using var output = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.Read);

        await input.CopyToAsync(output, ct);

        return tempPath;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (_bitmap != null)
        {
            context.DrawImage(_bitmap, new Rect(0, 0, VideoWidth, VideoHeight), new Rect(Bounds.Size));
        }
    }

    private void EndOfStreamReached()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (!_cts.IsCancellationRequested && !isPaused)
            {
                Stop();
                Play();
            }
            else
            {
                _cts?.Dispose();
                _process?.Kill(true);
                _process?.Dispose();
                _process = null;
            }
        });
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var vidSize = new Size(VideoWidth, VideoHeight);
        if (double.IsFinite(availableSize.Width) || double.IsFinite(availableSize.Height))
        {
            double aspect = vidSize.Width / vidSize.Height;
            double width = double.IsFinite(availableSize.Width) ? availableSize.Width : vidSize.Width;
            double height = double.IsFinite(availableSize.Height) ? availableSize.Height : vidSize.Height;
            if (width / height > aspect)
            {
                width = height * aspect;
            }
            else
            {
                height = width / aspect;
            }

            return new Size(width, height);
        }

        return vidSize;
    }

    override protected Size ArrangeOverride(Size finalSize)
    {
        double aspect = (double)VideoWidth / VideoHeight;
        double width = finalSize.Width;
        double height = finalSize.Height;

        if (width / height > aspect)
        {
            width = height * aspect;
        }
        else
        {
            height = width / aspect;
        }

        return new Size(width, height);
    }
}
