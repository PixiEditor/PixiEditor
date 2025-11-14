using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using ChunkyImageLib.DataHolders;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Position;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Rendering.ContextData;
using PixiEditor.Models.Rendering;
using PixiEditor.ViewModels;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Views.Main.ViewportControls;

internal partial class FixedViewport : UserControl, INotifyPropertyChanged
{
    public static readonly StyledProperty<DocumentViewModel> DocumentProperty =
        AvaloniaProperty.Register<FixedViewport, DocumentViewModel>(nameof(Document), null);

    public static readonly StyledProperty<bool> DelayedProperty =
        AvaloniaProperty.Register<FixedViewport, bool>(nameof(Delayed), false);

    public static readonly StyledProperty<bool> RenderInDocSizeProperty =
        AvaloniaProperty.Register<FixedViewport, bool>(
            nameof(RenderInDocSize));

    public static readonly StyledProperty<VecI> CustomRenderSizeProperty =
        AvaloniaProperty.Register<FixedViewport, VecI>(
            nameof(CustomRenderSize));

    public static readonly StyledProperty<int> MaxBilinearSampleSizeProperty = AvaloniaProperty.Register<FixedViewport, int>(
        nameof(MaxBilinearSampleSize));

    public int MaxBilinearSampleSize
    {
        get => GetValue(MaxBilinearSampleSizeProperty);
        set => SetValue(MaxBilinearSampleSizeProperty, value);
    }

    public VecI CustomRenderSize
    {
        get => GetValue(CustomRenderSizeProperty);
        set => SetValue(CustomRenderSizeProperty, value);
    }

    public bool RenderInDocSize
    {
        get => GetValue(RenderInDocSizeProperty);
        set => SetValue(RenderInDocSizeProperty, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool Delayed
    {
        get => GetValue(DelayedProperty);
        set => SetValue(DelayedProperty, value);
    }

    public DocumentViewModel? Document
    {
        get => GetValue(DocumentProperty);
        set => SetValue(DocumentProperty, value);
    }

    public Texture? SceneTexture => Document?.SceneTextures.TryGetValue(GuidValue, out var tex) == true ? tex : null;

    public Guid GuidValue { get; } = Guid.NewGuid();

    static FixedViewport()
    {
        DocumentProperty.Changed.Subscribe(OnDocumentChange);
        RenderInDocSizeProperty.Changed.Subscribe(OnRenderInDocSizeChanged);
    }

    public FixedViewport()
    {
        InitializeComponent();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double aspectRatio = Document?.Width / (double)Document?.Height ?? 1;
        double width = availableSize.Width;
        double height = width / aspectRatio;
        if (height > availableSize.Height)
        {
            height = availableSize.Height;
            width = height * aspectRatio;
        }

        return new Size(width, height);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        Document?.Operations.AddOrUpdateViewport(GetLocation());
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Document?.Operations.RemoveViewport(GuidValue);
    }

    private ChunkResolution CalculateResolution()
    {
        if (Document is null)
            return ChunkResolution.Full;
        double density = Document.Width / mainImage.Bounds.Width;
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
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SceneTexture)));
        mainImage.InvalidateVisual();
    }

    private ViewportInfo GetLocation()
    {
        VecD docSize = new VecD(1);
        if (Document is not null)
            docSize = Document.SizeBindable;

        Matrix3X3 scaling = Matrix3X3.CreateScale((float)Bounds.Width / (float)docSize.X,
            (float)Bounds.Height / (float)docSize.Y);

        return new ViewportInfo(
            0,
            docSize / 2,
            new VecD(Bounds.Width, Bounds.Height),
            new ViewportData(scaling, VecD.Zero, 1, false, false),
            new PointerInfo(),
            new KeyboardInfo(),
            ViewModelMain.Current.GetEditorData(), // TODO: Remove singleton
            null,
            "DEFAULT",
            CalculateSampling(),
            docSize,
            CalculateResolution(),
            GuidValue,
            Delayed,
            false,
            ForceRefreshFinalImage);
    }

    internal SamplingOptions CalculateSampling()
    {
        if (Document == null)
            return SamplingOptions.Default;

        if (Document.SizeBindable.LongestAxis > MaxBilinearSampleSize || SceneTexture == null)
        {
            return SamplingOptions.Default;
        }

        VecD densityVec = ((VecD)Document.SizeBindable).Divide(new VecD(Bounds.Width, Bounds.Height));
        double density = Math.Min(densityVec.X, densityVec.Y);
        return density > 1
            ? SamplingOptions.Bilinear
            : SamplingOptions.Default;
    }

    private static void OnDocumentChange(AvaloniaPropertyChangedEventArgs<DocumentViewModel> args)
    {
        DocumentViewModel? oldDoc = args.OldValue.Value;
        DocumentViewModel? newDoc = args.NewValue.Value;
        FixedViewport? viewport = (FixedViewport)args.Sender;
        oldDoc?.Operations.RemoveViewport(viewport.GuidValue);
        newDoc?.Operations.AddOrUpdateViewport(viewport.GetLocation());
        viewport.InvalidateMeasure();

        if (oldDoc != null)
        {
            oldDoc.SizeChanged -= viewport.DocSizeChanged;
        }

        if (newDoc != null)
        {
            newDoc.SizeChanged += viewport.DocSizeChanged;
        }

        viewport.ForceRefreshFinalImage();
    }

    private static void OnRenderInDocSizeChanged(AvaloniaPropertyChangedEventArgs<bool> args)
    {
        FixedViewport? viewport = (FixedViewport)args.Sender;
        viewport.CustomRenderSize = args.NewValue.Value ? viewport.Document?.SizeBindable ?? VecI.Zero : VecI.Zero;
        viewport.InvalidateMeasure();
        viewport.ForceRefreshFinalImage();
    }

    private void DocSizeChanged(object? sender, DocumentSizeChangedEventArgs e)
    {
        Document?.Operations.AddOrUpdateViewport(GetLocation());
        InvalidateMeasure();
    }

    private void OnImageSizeChanged(object sender, SizeChangedEventArgs e)
    {
        Document?.Operations.AddOrUpdateViewport(GetLocation());
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        Document?.Operations.AddOrUpdateViewport(GetLocation());
    }
}
