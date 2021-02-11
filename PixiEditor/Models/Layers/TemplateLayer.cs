using System;
using System.IO;
using System.Security.Permissions;
using System.Timers;
using System.Windows.Media;
using System.Windows.Threading;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.IO;

namespace PixiEditor.Models.Layers
{
    internal class TemplateLayer : Layer
    {
        private static readonly Brush ActiveBrush = new SolidColorBrush(Color.FromRgb(130, 35, 35));
        private static readonly Brush InactiveBrush = new SolidColorBrush(Color.FromArgb(70, 130, 35, 35));

        private readonly Timer updateTimer = new Timer(300);

        private string path;
        private FileSystemWatcher watcher = new FileSystemWatcher();
        private bool timerRunning;
        private DateTime lastEditTime;

        public string Path
        {
            get => path;
            set
            {
                path = value;
                RaisePropertyChanged(nameof(Path));
            }
        }

        public TemplateLayer(string path, int width, int height)
            : base("Template Layer")
        {
            Path = path;
            WatcherInit();

            UpdateBitmap();
            Width = width;
            Height = height;

            UpdateLayerColor();
            updateTimer.Elapsed += UpdateTimer_Elapsed;
            updateTimer.AutoReset = true;
        }

        public void DocumentSizeChanged(object sender, DocumentSizeChangedEventArgs e)
        {
            Width = e.NewWidth;
            Height = e.NewHeight;
        }

        protected override void UpdateLayerColor()
        {
            if (IsActive)
            {
                LayerColor = ActiveBrush;
            }
            else
            {
                LayerColor = InactiveBrush;
            }
        }

        private void UpdateBitmap()
        {
            App.Current.Dispatcher.Invoke(() => LayerBitmap = Importer.ImportImage(Path));
        }

        private void WatcherInit()
        {
            watcher.Path = System.IO.Path.GetDirectoryName(path);
            watcher.Filter = System.IO.Path.GetFileName(path);

            watcher.NotifyFilter = NotifyFilters.LastWrite |
                NotifyFilters.DirectoryName |
                NotifyFilters.FileName;

            watcher.EnableRaisingEvents = true;

            watcher.Changed += TemplateChanged;
            watcher.Deleted += TemplateDeleted;
            watcher.Renamed += TemplateRenamed;
        }

        private void TemplateRenamed(object sender, RenamedEventArgs e)
        {
            path = e.FullPath;
            watcher.Path = System.IO.Path.GetDirectoryName(path);
            watcher.Filter = System.IO.Path.GetFileName(path);
        }

        private void TemplateDeleted(object sender, FileSystemEventArgs e)
        {
            path = null;
            watcher.Dispose();
            watcher = null;
        }

        private void TemplateChanged(object sender, FileSystemEventArgs e)
        {
            lastEditTime = DateTime.Now;

            if (!timerRunning)
            {
                updateTimer.Start();
                timerRunning = true;
            }
        }

        private void UpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if ((DateTime.Now - lastEditTime).TotalMilliseconds > 500)
            {
                UpdateBitmap();
                updateTimer.Stop();
                timerRunning = false;
            }
        }

        ~TemplateLayer()
        {
            watcher.Dispose();
            updateTimer.Dispose();
        }
    }
}