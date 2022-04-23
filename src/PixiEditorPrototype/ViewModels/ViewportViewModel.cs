using System;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using ChunkyImageLib.DataHolders;

namespace PixiEditorPrototype.ViewModels;
internal class ViewportViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private ViewModelMain mainVM;

    public ViewportViewModel(ViewModelMain mainVM, Guid targetDocumentGuid)
    {
        this.mainVM = mainVM;
        TargetDocumentGuid = targetDocumentGuid;
    }

    private double angle = 0;
    public double Angle
    {
        get => angle;
        set
        {
            angle = value;
            PropertyChanged?.Invoke(this, new(nameof(Angle)));
            mainVM.GetDocumentByGuid(TargetDocumentGuid)?.RefreshViewport(GuidValue);
        }
    }

    private Vector2d center = new(32, 32);
    public Vector2d Center
    {
        get => center;
        set
        {
            center = value;
            PropertyChanged?.Invoke(this, new(nameof(Center)));
            mainVM.GetDocumentByGuid(TargetDocumentGuid)?.RefreshViewport(GuidValue);
        }
    }

    private Vector2d realDimensions = new(double.MaxValue, double.MaxValue);
    public Vector2d RealDimensions
    {
        get => realDimensions;
        set
        {
            realDimensions = value;
            PropertyChanged?.Invoke(this, new(nameof(RealDimensions)));
            mainVM.GetDocumentByGuid(TargetDocumentGuid)?.RefreshViewport(GuidValue);
        }
    }

    private Vector2d dimensions = new(64, 64);
    public Vector2d Dimensions
    {
        get => dimensions;
        set
        {
            dimensions = value;
            PropertyChanged?.Invoke(this, new(nameof(Dimensions)));
            mainVM.GetDocumentByGuid(TargetDocumentGuid)?.RefreshViewport(GuidValue);
        }
    }

    public Guid GuidValue { get; } = Guid.NewGuid();

    public Guid TargetDocumentGuid { get; }

    private ChunkResolution resolution = ChunkResolution.Full;
    public ChunkResolution Resolution
    {
        get => resolution;
        set
        {
            if (value == resolution)
                return;
            resolution = value;
            PropertyChanged?.Invoke(this, new(nameof(Resolution)));
            PropertyChanged?.Invoke(this, new(nameof(TargetBitmap)));
        }
    }

    public WriteableBitmap? TargetBitmap
    {
        get
        {
            var doc = mainVM.GetDocumentByGuid(TargetDocumentGuid);
            if (doc is null)
                return null;
            return doc.Bitmaps.TryGetValue(Resolution, out var value) ? value : null;
        }
    }
}
