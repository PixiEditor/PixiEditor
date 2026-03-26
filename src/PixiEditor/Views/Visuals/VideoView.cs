using System.Globalization;
using System.Reflection;
using Avalonia.Input;
using Avalonia.Platform;
using PixiEditor.Models.IO;
using PixiEditor.OperatingSystem;
using PixiEditor.UI.Common.Fonts;

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

    public static readonly StyledProperty<bool> AutoPlayProperty = AvaloniaProperty.Register<VideoView, bool>(
        nameof(AutoPlay), false);

    public static readonly StyledProperty<bool> IsPlayingProperty =
        AvaloniaProperty.Register<VideoView, bool>("IsPlaying");


    public bool AutoPlay
    {
        get => GetValue(AutoPlayProperty);
        set => SetValue(AutoPlayProperty, value);
    }

    public string? Source { get => GetValue(SourceProperty); set => SetValue(SourceProperty, value); }
    public int VideoWidth { get => GetValue(VideoWidthProperty); set => SetValue(VideoWidthProperty, value); }
    public int VideoHeight { get => GetValue(VideoHeightProperty); set => SetValue(VideoHeightProperty, value); }

    public static readonly StyledProperty<bool> IsDownloadingProperty = AvaloniaProperty.Register<VideoView, bool>(
        nameof(IsDownloading));

    public bool IsDownloading
    {
        get => GetValue(IsDownloadingProperty);
        private set => SetValue(IsDownloadingProperty, value);
    }

    public bool IsPlaying
    {
        get { return (bool)GetValue(IsPlayingProperty); }
        set { SetValue(IsPlayingProperty, value); }
    }


    private Stopwatch? _playbackClock;
    private double currentTime;
    private readonly double fps = 60.0;
    private bool isPaused;
    private string? downloadedTempFile;
    private CancellationTokenSource? _downloadCts;
    private bool ignorePlayChange = false;
    private bool isPlayQueued = false;

    static VideoView()
    {
        IsPlayingProperty.Changed.AddClassHandler<VideoView>((x, e) =>
        {
            if (x.ignorePlayChange)
                return;

            if (e.NewValue is true)
            {
                x.Play();
            }
            else if (e.NewValue is false)
            {
                x.Pause();
            }
        });
    }

    public VideoView()
    {
        this.GetObservable(SourceProperty).Subscribe(async path =>
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                bool autoPlay = AutoPlay;
                Task.Run(async () =>
                {
                    try
                    {
                        _downloadCts?.Cancel();
                        _downloadCts = new CancellationTokenSource();
                        downloadedTempFile = await DownloadToTempFile(path, _downloadCts.Token);
                        if (autoPlay || isPlayQueued)
                        {
                            Dispatcher.UIThread.Post(Play);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to load video: {ex.Message}");
                    }
                });
            }
        });
    }

    public void Stop()
    {
        ignorePlayChange = true;
        _cts?.Dispose();
        _cts = null;
        _process?.Kill(true);
        _process?.Dispose();
        _process = null;
        _playbackClock?.Stop();
        currentTime = 0;
        IsPlaying = false;
        ignorePlayChange = false;
    }

    public void Pause()
    {
        if (isPaused)
            return;

        ignorePlayChange = true;
        isPaused = true;
        IsPlaying = false;
        ignorePlayChange = false;

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
        Stop();
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
        if (Source == null)
            return;

        if (downloadedTempFile == null)
        {
            isPlayQueued = true;
            return;
        }

        ignorePlayChange = true;
        _cts?.Dispose();
        _cts = null;
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
            {
                ignorePlayChange = false;
                return;
            }

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

            IsPlaying = true;
            isPlayQueued = false;
            ignorePlayChange = false;
        }
        catch (OperationCanceledException)
        {
        }
    }

    private async Task<string> DownloadToTempFile(string url, CancellationToken ct)
    {
        var vidoesPath = Path.Combine(Paths.TempSessionFilesPath, "videos");
        if (!Directory.Exists(vidoesPath))
        {
            Directory.CreateDirectory(vidoesPath);
        }

        var tempPath = Path.Combine(vidoesPath, $"videoview_{Guid.NewGuid()}.mp4");

        Dispatcher.UIThread.Post(() => IsDownloading = true);
        using var http = new HttpClient();
        using var response = await http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        await using var input = await response.Content.ReadAsStreamAsync(ct);
        await using var output = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.Read);

        await input.CopyToAsync(output, ct);

        Dispatcher.UIThread.Post(() => IsDownloading = false);

        return tempPath;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (_bitmap != null)
        {
            context.DrawImage(_bitmap, new Rect(0, 0, VideoWidth, VideoHeight), new Rect(Bounds.Size));
        }

        if (isPaused)
        {
            context.DrawText(new FormattedText(PixiPerfectIcons.Pause, CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight, new Typeface(PixiPerfectIconExtensions.PixiPerfectFontFamily), 48, Brushes.White), new Point(Bounds.Width / 2 - 12, Bounds.Height / 2 - 12));
        }
    }

    private void EndOfStreamReached()
    {
        Dispatcher.UIThread.Post(() =>
        {
            if ((_cts == null || !_cts.IsCancellationRequested) && !isPaused)
            {
                Stop();
                Play();
            }
            else
            {
                _cts?.Dispose();
                _cts = null;
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
