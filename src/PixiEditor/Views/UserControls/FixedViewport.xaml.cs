using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Models.Position;
using PixiEditor.Numerics;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.Views.UserControls;
#nullable enable
internal partial class FixedViewport : UserControl, INotifyPropertyChanged
{
    public static readonly DependencyProperty DocumentProperty =
        DependencyProperty.Register(nameof(Document), typeof(DocumentViewModel), typeof(FixedViewport), new(null, OnDocumentChange));

    private static readonly DependencyProperty BitmapsProperty =
        DependencyProperty.Register(nameof(Bitmaps), typeof(Dictionary<ChunkResolution, WriteableBitmap>), typeof(FixedViewport), new(null, OnBitmapsChange));

    public static readonly DependencyProperty DelayedProperty = DependencyProperty.Register(
        nameof(Delayed), typeof(bool), typeof(FixedViewport), new PropertyMetadata(false));

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool Delayed
    {
        get => (bool)GetValue(DelayedProperty);
        set => SetValue(DelayedProperty, value);
    }

    public Dictionary<ChunkResolution, WriteableBitmap>? Bitmaps
    {
        get => (Dictionary<ChunkResolution, WriteableBitmap>?)GetValue(BitmapsProperty);
        set => SetValue(BitmapsProperty, value);
    }

    public DocumentViewModel? Document
    {
        get => (DocumentViewModel)GetValue(DocumentProperty);
        set => SetValue(DocumentProperty, value);
    }

    public WriteableBitmap? TargetBitmap
    {
        get => Document?.LazyBitmaps.TryGetValue(CalculateResolution(), out WriteableBitmap? value) == true ? value : null;
    }

    public Guid GuidValue { get; } = Guid.NewGuid();

    public FixedViewport()
    {
        InitializeComponent();
        Binding binding = new Binding { Source = this, Path = new PropertyPath($"{nameof(Document)}.{nameof(Document.LazyBitmaps)}") };
        SetBinding(BitmapsProperty, binding);

        Loaded += OnLoad;
        Unloaded += OnUnload;
    }

    private void OnUnload(object sender, RoutedEventArgs e)
    {
        Document?.Operations.RemoveViewport(GuidValue);
    }

    private void OnLoad(object sender, RoutedEventArgs e)
    {
        Document?.Operations.AddOrUpdateViewport(GetLocation());
    }

    private ChunkResolution CalculateResolution()
    {
        if (Document is null)
            return ChunkResolution.Full;
        double density = Document.Width / mainImage.ActualWidth;
        if (density > 8.01)
            return ChunkResolution.Eighth;
        else if (density > 4.01)
            return ChunkResolution.Quarter;
        else if (density > 2.01)
            return ChunkResolution.Half;
        return ChunkResolution.Full;
    }

    private void ForceRefreshFinalImage()
    {
        mainImage.InvalidateVisual();
    }

    private ViewportInfo GetLocation()
    {
        VecD docSize = new VecD(1);
        if (Document is not null)
            docSize = Document.SizeBindable;

        return new ViewportInfo(
            0,
            docSize / 2,
            new VecD(mainImage.ActualWidth, mainImage.ActualHeight),
            docSize,
            CalculateResolution(),
            GuidValue,
            Delayed,
            ForceRefreshFinalImage);
    }

    private static void OnDocumentChange(DependencyObject viewportObj, DependencyPropertyChangedEventArgs args)
    {
        DocumentViewModel? oldDoc = (DocumentViewModel?)args.OldValue;
        DocumentViewModel? newDoc = (DocumentViewModel?)args.NewValue;
        FixedViewport? viewport = (FixedViewport)viewportObj;
        oldDoc?.Operations.RemoveViewport(viewport.GuidValue);
        newDoc?.Operations.AddOrUpdateViewport(viewport.GetLocation());
    }

    private static void OnBitmapsChange(DependencyObject viewportObj, DependencyPropertyChangedEventArgs args)
    {
        FixedViewport? viewport = ((FixedViewport)viewportObj);
        viewport.PropertyChanged?.Invoke(viewportObj, new(nameof(TargetBitmap)));
        viewport.Document?.Operations.AddOrUpdateViewport(viewport.GetLocation());
    }

    private void OnImageSizeChanged(object sender, SizeChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, new(nameof(TargetBitmap)));
        Document?.Operations.AddOrUpdateViewport(GetLocation());
    }
}
