using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using Microsoft.Win32;
using PixiEditor.ChangeableDocument.Actions.Undo;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.Parser;
using PixiEditorPrototype.CustomControls.SymmetryOverlay;
using PixiEditorPrototype.Models;
using SkiaSharp;

namespace PixiEditorPrototype.ViewModels;

internal class DocumentViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public RelayCommand? UndoCommand { get; }
    public RelayCommand? RedoCommand { get; }
    public RelayCommand? ClearSelectionCommand { get; }
    public RelayCommand? CreateNewLayerCommand { get; }
    public RelayCommand? CreateNewFolderCommand { get; }
    public RelayCommand? DeleteStructureMemberCommand { get; }
    public RelayCommand? ResizeCanvasCommand { get; }
    public RelayCommand? CombineCommand { get; }
    public RelayCommand? ClearHistoryCommand { get; }
    public RelayCommand? CreateMaskCommand { get; }
    public RelayCommand? DeleteMaskCommand { get; }
    public RelayCommand? ApplyMaskCommand { get; }
    public RelayCommand? ToggleLockTransparencyCommand { get; }
    public RelayCommand? ApplyTransformCommand { get; }
    public RelayCommand? PasteImageCommand { get; }
    public RelayCommand? DragSymmetryCommand { get; }
    public RelayCommand? EndDragSymmetryCommand { get; }
    public RelayCommand? ClipToMemberBelowCommand { get; }
    public RelayCommand? TransformSelectionPathCommand { get; }
    public RelayCommand? TransformSelectedAreaCommand { get; }

    private VecI size = new VecI(64, 64);

    public void SetSize(VecI size)
    {
        this.size = size;
        RaisePropertyChanged(nameof(SizeBindable));
        RaisePropertyChanged(nameof(Width));
        RaisePropertyChanged(nameof(Height));
    }

    public VecI SizeBindable => size;

    private SKPath selectionPath = new SKPath();

    public void SetSelectionPath(SKPath selectionPath)
    {
        (var toDispose, this.selectionPath) = (this.selectionPath, selectionPath);
        toDispose.Dispose();
        RaisePropertyChanged(nameof(SelectionPathBindable));
    }

    public SKPath SelectionPathBindable => selectionPath;

    private int horizontalSymmetryAxisY;

    public void SetHorizontalSymmetryAxisY(int horizontalSymmetryAxisY)
    {
        this.horizontalSymmetryAxisY = horizontalSymmetryAxisY;
        RaisePropertyChanged(nameof(HorizontalSymmetryAxisYBindable));
    }

    public int HorizontalSymmetryAxisYBindable => horizontalSymmetryAxisY;

    private int verticalSymmetryAxisX;

    public void SetVerticalSymmetryAxisX(int verticalSymmetryAxisX)
    {
        this.verticalSymmetryAxisX = verticalSymmetryAxisX;
        RaisePropertyChanged(nameof(VerticalSymmetryAxisXBindable));
    }

    public int VerticalSymmetryAxisXBindable => verticalSymmetryAxisX;

    private bool horizontalSymmetryAxisEnabled;

    public void SetHorizontalSymmetryAxisEnabled(bool horizontalSymmetryAxisEnabled)
    {
        this.horizontalSymmetryAxisEnabled = horizontalSymmetryAxisEnabled;
        RaisePropertyChanged(nameof(HorizontalSymmetryAxisEnabledBindable));
    }

    public bool HorizontalSymmetryAxisEnabledBindable
    {
        get => horizontalSymmetryAxisEnabled;
        set => Helpers.ActionAccumulator.AddFinishedActions(new SymmetryAxisState_Action(SymmetryAxisDirection.Horizontal, value));
    }

    private bool verticalSymmetryAxisEnabled;

    public void SetVerticalSymmetryAxisEnabled(bool verticalSymmetryAxisEnabled)
    {
        this.verticalSymmetryAxisEnabled = verticalSymmetryAxisEnabled;
        RaisePropertyChanged(nameof(VerticalSymmetryAxisEnabledBindable));
    }

    public bool VerticalSymmetryAxisEnabledBindable
    {
        get => verticalSymmetryAxisEnabled;
        set => Helpers.ActionAccumulator.AddFinishedActions(new SymmetryAxisState_Action(SymmetryAxisDirection.Vertical, value));
    }

    private string name = string.Empty;

    public string Name
    {
        get => name;
        set
        {
            name = value;
            RaisePropertyChanged(nameof(Name));
        }
    }

    private bool busy = false;

    public bool Busy
    {
        get => busy;
        set
        {
            busy = value;
            RaisePropertyChanged(nameof(Busy));
        }
    }

    public StructureMemberViewModel? SelectedStructureMember => FindFirstSelectedMember();

    public Guid GuidValue { get; } = Guid.NewGuid();
    public int Width => size.X;
    public int Height => size.Y;

    public Dictionary<ChunkResolution, WriteableBitmap> Bitmaps { get; set; } = new()
    {
        [ChunkResolution.Full] = new WriteableBitmap(64, 64, 96, 96, PixelFormats.Pbgra32, null), [ChunkResolution.Half] = new WriteableBitmap(32, 32, 96, 96, PixelFormats.Pbgra32, null), [ChunkResolution.Quarter] = new WriteableBitmap(16, 16, 96, 96, PixelFormats.Pbgra32, null), [ChunkResolution.Eighth] = new WriteableBitmap(8, 8, 96, 96, PixelFormats.Pbgra32, null),
    };

    public WriteableBitmap PreviewBitmap { get; set; }
    public SKSurface PreviewSurface { get; set; }

    public Dictionary<ChunkResolution, SKSurface> Surfaces { get; set; } = new();
    public FolderViewModel StructureRoot { get; }
    public DocumentTransformViewModel TransformViewModel { get; }
    public int ResizeWidth { get; set; } = 1024;
    public int ResizeHeight { get; set; } = 1024;


    private DocumentHelpers Helpers { get; }

    private readonly ViewModelMain owner;


    private bool updateableChangeActive = false;

    private bool selectingRect = false;
    private bool selectingEllipse = false;
    private bool selectingLasso = false;
    private bool drawingRectangle = false;
    private bool drawingEllipse = false;
    private bool drawingLine = false;
    private bool drawingPathBasedPen = false;
    private bool drawingLineBasedPen = false;
    private bool drawingPixelPerfectPen = false;
    private bool transformingRectangle = false;
    private bool transformingEllipse = false;
    private bool shiftingLayer = false;

    private bool transformingSelectionPath = false;
    ShapeCorners initialSelectionCorners = new();

    private bool pastingImage = false;
    private Surface? pastedImage;

    private ShapeCorners lastShape = new ShapeCorners();
    private ShapeData lastShapeData = new();

    private SKColor lastEllipseStrokeColor = SKColors.Empty;
    private SKColor lastEllipseFillColor = SKColors.Empty;
    private int lastEllipseStrokeWidth = 0;
    private RectI lastEllipseLocation = RectI.Empty;

    public DocumentViewModel(ViewModelMain owner, string name)
    {
        this.owner = owner;
        Name = name;
        TransformViewModel = new();
        TransformViewModel.TransformMoved += OnTransformUpdate;

        Helpers = new DocumentHelpers(this);
        StructureRoot = new FolderViewModel(this, Helpers, Helpers.Tracker.Document.StructureRoot.GuidValue);

        UndoCommand = new RelayCommand(Undo);
        RedoCommand = new RelayCommand(Redo);
        ClearSelectionCommand = new RelayCommand(ClearSelection);
        CreateNewLayerCommand = new RelayCommand(_ => Helpers.StructureHelper.CreateNewStructureMember(StructureMemberType.Layer));
        CreateNewFolderCommand = new RelayCommand(_ => Helpers.StructureHelper.CreateNewStructureMember(StructureMemberType.Folder));
        DeleteStructureMemberCommand = new RelayCommand(DeleteStructureMember);
        ResizeCanvasCommand = new RelayCommand(ResizeCanvas);
        CombineCommand = new RelayCommand(Combine);
        ClearHistoryCommand = new RelayCommand(ClearHistory);
        CreateMaskCommand = new RelayCommand(CreateMask);
        DeleteMaskCommand = new RelayCommand(DeleteMask);
        ToggleLockTransparencyCommand = new RelayCommand(ToggleLockTransparency);
        PasteImageCommand = new RelayCommand(PasteImage);
        ApplyTransformCommand = new RelayCommand(ApplyTransform);
        DragSymmetryCommand = new RelayCommand(DragSymmetry);
        EndDragSymmetryCommand = new RelayCommand(EndDragSymmetry);
        ClipToMemberBelowCommand = new RelayCommand(ClipToMemberBelow);
        ApplyMaskCommand = new RelayCommand(ApplyMask);
        TransformSelectionPathCommand = new RelayCommand(TransformSelectionPath);
        TransformSelectedAreaCommand = new RelayCommand(TransformSelectedArea);

        foreach (var bitmap in Bitmaps)
        {
            var surface = SKSurface.Create(
                new SKImageInfo(bitmap.Value.PixelWidth, bitmap.Value.PixelHeight, SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb()),
                bitmap.Value.BackBuffer, bitmap.Value.BackBufferStride);
            Surfaces[bitmap.Key] = surface;
        }

        var previewSize = StructureMemberViewModel.CalculatePreviewSize(SizeBindable);
        PreviewBitmap = new WriteableBitmap(previewSize.X, previewSize.Y, 96, 96, PixelFormats.Pbgra32, null);
        PreviewSurface = SKSurface.Create(new SKImageInfo(previewSize.X, previewSize.Y, SKColorType.Bgra8888), PreviewBitmap.BackBuffer, PreviewBitmap.BackBufferStride);
    }

    public static DocumentViewModel FromSerializableDocument(ViewModelMain owner, SerializableDocument serDocument, string name)
    {
        DocumentViewModel document = new DocumentViewModel(owner, name);
        var acc = document.Helpers.ActionAccumulator;
        acc.AddActions(new ResizeCanvas_Action(new(serDocument.Width, serDocument.Height), ResizeAnchor.TopLeft));
        int index = 0;
        foreach (var layer in serDocument.Layers.Reverse())
        {
            var guid = Guid.NewGuid();
            var png = SKBitmap.Decode(layer.PngBytes);
            Surface surface = new(new VecI(layer.Width, layer.Height));
            surface.SkiaSurface.Canvas.DrawBitmap(png, 0, 0);
            acc.AddFinishedActions(
                new CreateStructureMember_Action(document.StructureRoot.GuidValue, guid, index, StructureMemberType.Layer),
                new StructureMemberName_Action(guid, layer.Name),
                new PasteImage_Action(surface, new(new RectD(new VecD(layer.OffsetX, layer.OffsetY), new(layer.Width, layer.Height))), guid, true, false),
                new EndPasteImage_Action()
            );
            if (layer.Opacity != 1)
                acc.AddFinishedActions(
                    new StructureMemberOpacity_Action(guid, layer.Opacity),
                    new EndStructureMemberOpacity_Action());
            if (!layer.IsVisible)
                acc.AddFinishedActions(new StructureMemberIsVisible_Action(layer.IsVisible, guid));
        }

        acc.AddActions(new DeleteRecordedChanges_Action());
        return document;
    }

    public StructureMemberViewModel? FindFirstSelectedMember() => Helpers.StructureHelper.FindFirstWhere(member => member.IsSelected);

    private bool CanStartUpdate()
    {
        var member = FindFirstSelectedMember();
        if (member is null)
            return false;
        bool drawOnMask = member.ShouldDrawOnMask;
        if (!drawOnMask)
        {
            if (member is FolderViewModel)
                return false;
            if (member is LayerViewModel)
                return true;
        }

        if (!member.HasMaskBindable)
            return false;
        return true;
    }

    private void TransformSelectedArea(object? obj)
    {
        if (updateableChangeActive || FindFirstSelectedMember() is not LayerViewModel layer || SelectionPathBindable.IsEmpty)
            return;
        IReadOnlyChunkyImage? layerImage = (Helpers.Tracker.Document.FindMember(layer.GuidValue) as IReadOnlyLayer)?.LayerImage;
        if (layerImage is null)
            return;

        // find area location and size
        using SKPath path = new(SelectionPathBindable);
        var bounds = (RectD)path.TightBounds;
        bounds = bounds.Intersect(new RectD(VecD.Zero, SizeBindable));
        var intBounds = (RectI)bounds.RoundOutwards();

        // extract surface to be transformed
        path.Transform(SKMatrix.CreateTranslation(-intBounds.X, -intBounds.Y));
        Surface surface = new(intBounds.Size);
        surface.SkiaSurface.Canvas.Save();
        surface.SkiaSurface.Canvas.ClipPath(path);
        layerImage.DrawMostUpToDateRegionOn(intBounds, ChunkResolution.Full, surface.SkiaSurface, VecI.Zero);
        surface.SkiaSurface.Canvas.Restore();

        // clear area
        if (!owner.KeepOriginalImageOnTransform)
        {
            Helpers.ActionAccumulator.AddActions(new ClearSelectedArea_Action(layer.GuidValue, false));
        }

        Helpers.ActionAccumulator.AddActions(new ClearSelection_Action());

        // initiate transform using paste image logic
        pastedImage = surface;
        pastingImage = true;
        ShapeCorners corners = new(intBounds);
        Helpers.ActionAccumulator.AddActions(new PasteImage_Action(pastedImage, corners, layer.GuidValue, true, false));
        TransformViewModel.ShowFreeTransform(corners);
    }

    private void ApplyMask(object? obj)
    {
        if (updateableChangeActive || FindFirstSelectedMember() is not LayerViewModel layer || !layer.HasMaskBindable)
            return;
        Helpers.ActionAccumulator.AddFinishedActions(new ApplyLayerMask_Action(layer.GuidValue));
    }

    private void ClipToMemberBelow(object? obj)
    {
        if (updateableChangeActive || FindFirstSelectedMember() is not { } member)
            return;
        member.ClipToMemberBelowEnabledBindable = !member.ClipToMemberBelowEnabledBindable;
    }

    private void DragSymmetry(object? obj)
    {
        if (obj is null)
            return;
        var info = (SymmetryAxisDragInfo)obj;
        Helpers.ActionAccumulator.AddActions(new SymmetryAxisPosition_Action(info.Direction, info.NewPosition));
    }

    private void EndDragSymmetry(object? obj)
    {
        if (obj is null)
            return;
        Helpers.ActionAccumulator.AddFinishedActions(new EndSymmetryAxisPosition_Action());
    }

    public void StartUpdatePathBasedPen(VecD pos)
    {
        if (!CanStartUpdate())
            return;
        updateableChangeActive = true;
        drawingPathBasedPen = true;
        var member = FindFirstSelectedMember();
        Helpers.ActionAccumulator.AddActions(new PathBasedPen_Action(
            member!.GuidValue,
            pos,
            new SKColor(owner.SelectedColor.R, owner.SelectedColor.G, owner.SelectedColor.B, owner.SelectedColor.A),
            owner.StrokeWidth,
            member.ShouldDrawOnMask));
    }

    public void EndPathBasedPen()
    {
        if (!drawingPathBasedPen)
            return;
        drawingPathBasedPen = false;
        updateableChangeActive = false;
        Helpers.ActionAccumulator.AddFinishedActions(new EndPathBasedPen_Action());
    }

    public void StartUpdateLineBasedPen(VecI pos, SKColor color, bool replacing = false)
    {
        if (!CanStartUpdate())
            return;
        updateableChangeActive = true;
        drawingLineBasedPen = true;
        var member = FindFirstSelectedMember();
        Helpers.ActionAccumulator.AddActions(new LineBasedPen_Action(
            member!.GuidValue,
            color,
            pos,
            (int)owner.StrokeWidth,
            replacing,
            member.ShouldDrawOnMask));
    }

    public void EndLineBasedPen()
    {
        if (!drawingLineBasedPen)
            return;
        drawingLineBasedPen = false;
        updateableChangeActive = false;
        Helpers.ActionAccumulator.AddFinishedActions(new EndLineBasedPen_Action());
    }

    public void StartUpdatePixelPerfectPen(VecI pos, SKColor color)
    {
        if (!CanStartUpdate())
            return;
        updateableChangeActive = true;
        drawingPixelPerfectPen = true;
        var member = FindFirstSelectedMember();
        Helpers.ActionAccumulator.AddActions(new PixelPerfectPen_Action(member!.GuidValue, pos, color, member.ShouldDrawOnMask));
    }

    public void EndUPixelPerfectPen()
    {
        if (!drawingPixelPerfectPen)
            return;
        drawingPixelPerfectPen = false;
        updateableChangeActive = false;
        Helpers.ActionAccumulator.AddFinishedActions(new EndPixelPerfectPen_Action());
    }

    public void StartUpdateLine(VecI from, VecI to, SKColor color, SKStrokeCap cap, int strokeWidth)
    {
        if (!CanStartUpdate())
            return;
        drawingLine = true;
        updateableChangeActive = true;
        var member = FindFirstSelectedMember();
        Helpers.ActionAccumulator.AddActions(
            new DrawLine_Action(member!.GuidValue, from, to, strokeWidth, color, cap, member.ShouldDrawOnMask));
    }

    public void EndLine()
    {
        if (!drawingLine)
            return;
        drawingLine = false;
        updateableChangeActive = false;
        Helpers.ActionAccumulator.AddFinishedActions(new EndDrawLine_Action());
    }

    public void StartUpdateEllipse(RectI location, SKColor strokeColor, SKColor fillColor, int strokeWidth)
    {
        if (!CanStartUpdate())
            return;
        drawingEllipse = true;
        updateableChangeActive = true;
        lastEllipseFillColor = fillColor;
        lastEllipseStrokeWidth = strokeWidth;
        lastEllipseStrokeColor = strokeColor;
        lastEllipseLocation = location;
        var member = FindFirstSelectedMember();
        Helpers.ActionAccumulator.AddActions(new DrawEllipse_Action(
            member!.GuidValue,
            location,
            strokeColor,
            fillColor,
            strokeWidth,
            member.ShouldDrawOnMask));
    }

    public void EndEllipse()
    {
        if (!drawingEllipse)
            return;
        drawingEllipse = false;
        TransformViewModel.ShowShapeTransform(new ShapeCorners(lastEllipseLocation));
        transformingEllipse = true;
    }

    public void StartUpdateRectangle(ShapeData data)
    {
        if (!CanStartUpdate())
            return;
        updateableChangeActive = true;
        drawingRectangle = true;
        var member = FindFirstSelectedMember();
        Helpers.ActionAccumulator.AddActions(new DrawRectangle_Action(member!.GuidValue, data, member.ShouldDrawOnMask));
        lastShape = new ShapeCorners(data.Center, data.Size);
        lastShapeData = data;
    }

    public void EndRectangleDrawing()
    {
        if (!drawingRectangle)
            return;
        drawingRectangle = false;

        TransformViewModel.ShowShapeTransform(lastShape);
        transformingRectangle = true;
    }

    public void StartUpdateShiftLayer(VecI delta)
    {
        if (FindFirstSelectedMember() is not LayerViewModel layer)
            return;
        updateableChangeActive = true;
        shiftingLayer = true;
        Helpers.ActionAccumulator.AddActions(new ShiftLayer_Action(layer.GuidValue, delta));
    }

    public void EndShiftLayer()
    {
        if (!shiftingLayer)
            return;
        updateableChangeActive = false;
        shiftingLayer = false;
        Helpers.ActionAccumulator.AddFinishedActions(new EndShiftLayer_Action());
    }

    private void TransformSelectionPath(object? arg)
    {
        var path = SelectionPathBindable;
        if (path.IsEmpty)
            return;
        updateableChangeActive = true;
        transformingSelectionPath = true;
        var bounds = path.TightBounds;
        initialSelectionCorners = new ShapeCorners(bounds);
        TransformViewModel.ShowShapeTransform(initialSelectionCorners);
        Helpers.ActionAccumulator.AddActions(new TransformSelectionPath_Action(initialSelectionCorners));
    }

    public void StartUpdateLassoSelection(VecI startPos, SelectionMode mode)
    {
        updateableChangeActive = true;
        selectingLasso = true;
        Helpers.ActionAccumulator.AddActions(new SelectLasso_Action(startPos, mode));
    }

    public void EndLassoSelection()
    {
        if (!selectingLasso)
            return;
        selectingLasso = false;
        updateableChangeActive = false;
        Helpers.ActionAccumulator.AddFinishedActions(new EndSelectLasso_Action());
    }

    public SKColor PickColor(VecI pos, bool fromAllLayers)
    {
        // there is a tiny chance that the image might get disposed by another thread
        try
        {
            // it might've been a better idea to implement this function
            // via a passthrough action to avoid all the try catches
            if (fromAllLayers)
            {
                VecI chunkPos = OperationHelper.GetChunkPos(pos, ChunkyImage.FullChunkSize);
                return ChunkRenderer.MergeWholeStructure(chunkPos, ChunkResolution.Full, Helpers.Tracker.Document.StructureRoot)
                    .Match<SKColor>(
                        (Chunk chunk) =>
                        {
                            VecI posOnChunk = pos - chunkPos * ChunkyImage.FullChunkSize;
                            var color = chunk.Surface.GetSRGBPixel(posOnChunk);
                            chunk.Dispose();
                            return color;
                        },
                        _ => SKColors.Transparent
                    );
            }

            if (SelectedStructureMember is not LayerViewModel layerVm)
                return SKColors.Transparent;
            var maybeMember = Helpers.Tracker.Document.FindMember(layerVm.GuidValue);
            if (maybeMember is not IReadOnlyLayer layer)
                return SKColors.Transparent;
            return layer.LayerImage.GetMostUpToDatePixel(pos);
        }
        catch (ObjectDisposedException)
        {
            return SKColors.Transparent;
        }
    }

    private void ApplyTransform(object? param)
    {
        if (!transformingRectangle && !pastingImage && !transformingSelectionPath && !transformingEllipse)
            return;

        if (transformingRectangle)
        {
            transformingRectangle = false;
            TransformViewModel.HideTransform();
            Helpers.ActionAccumulator.AddFinishedActions(new EndDrawRectangle_Action());
        }
        else if (transformingEllipse)
        {
            transformingEllipse = false;
            TransformViewModel.HideTransform();
            Helpers.ActionAccumulator.AddFinishedActions(new EndDrawEllipse_Action());
        }
        else if (pastingImage)
        {
            pastingImage = false;
            TransformViewModel.HideTransform();
            Helpers.ActionAccumulator.AddFinishedActions(new EndPasteImage_Action());
            pastedImage?.Dispose();
            pastedImage = null;
        }
        else if (transformingSelectionPath)
        {
            transformingSelectionPath = false;
            TransformViewModel.HideTransform();
            Helpers.ActionAccumulator.AddFinishedActions(new EndTransformSelectionPath_Action());
        }

        updateableChangeActive = false;
    }

    public void StartUpdateRectSelection(RectI rect, SelectionMode mode)
    {
        selectingRect = true;
        updateableChangeActive = true;
        Helpers.ActionAccumulator.AddActions(new SelectRectangle_Action(rect, mode));
    }

    public void EndRectSelection()
    {
        if (!selectingRect)
            return;
        selectingRect = false;
        updateableChangeActive = false;
        Helpers.ActionAccumulator.AddFinishedActions(new EndSelectRectangle_Action());
    }

    public void StartUpdateEllipseSelection(RectI borders, SelectionMode mode)
    {
        selectingEllipse = true;
        updateableChangeActive = true;
        Helpers.ActionAccumulator.AddActions(new SelectEllipse_Action(borders, mode));
    }

    public void EndEllipseSelection()
    {
        if (!selectingEllipse)
            return;
        selectingEllipse = false;
        updateableChangeActive = false;
        Helpers.ActionAccumulator.AddFinishedActions(new EndSelectEllipse_Action());
    }

    private void OnTransformUpdate(object? sender, ShapeCorners newCorners)
    {
        if (transformingRectangle)
        {
            StartUpdateRectangle(new ShapeData(
                newCorners.RectCenter,
                newCorners.RectSize,
                newCorners.RectRotation,
                lastShapeData.StrokeWidth,
                lastShapeData.StrokeColor,
                lastShapeData.FillColor,
                lastShapeData.BlendMode));
        }
        else if (transformingEllipse)
        {
            StartUpdateEllipse(RectI.FromTwoPoints((VecI)newCorners.TopLeft, (VecI)newCorners.BottomRight), lastEllipseStrokeColor, lastEllipseFillColor, lastEllipseStrokeWidth);
        }
        else if (pastingImage)
        {
            var member = FindFirstSelectedMember();
            if (member is null || pastedImage is null)
                return;
            Helpers.ActionAccumulator.AddActions(new PasteImage_Action(pastedImage, newCorners, member.GuidValue, false, false));
        }
        else if (transformingSelectionPath)
        {
            Helpers.ActionAccumulator.AddActions(new TransformSelectionPath_Action(newCorners));
        }
    }

    public void FloodFill(VecI pos, SKColor color, bool referenceAllLayers)
    {
        var member = FindFirstSelectedMember();
        if (updateableChangeActive || member is null)
            return;
        Helpers.ActionAccumulator.AddFinishedActions(new FloodFill_Action(member.GuidValue, pos, color, referenceAllLayers, member.ShouldDrawOnMask));
    }

    public void AddOrUpdateViewport(ViewportInfo info)
    {
        Helpers.ActionAccumulator.AddActions(new RefreshViewport_PassthroughAction(info));
    }

    public void RemoveViewport(Guid viewportGuid)
    {
        Helpers.ActionAccumulator.AddActions(new RemoveViewport_PassthroughAction(viewportGuid));
    }

    private void PasteImage(object? args)
    {
        if (FindFirstSelectedMember() is not LayerViewModel layer)
            return;
        OpenFileDialog dialog = new();
        if (dialog.ShowDialog() != true)
            return;

        pastedImage = Surface.Load(dialog.FileName);
        pastingImage = true;
        ShapeCorners corners = new ShapeCorners(new RectD(VecD.Zero, pastedImage.Size));
        Helpers.ActionAccumulator.AddActions(new PasteImage_Action(pastedImage, corners, layer.GuidValue, false, false));
        TransformViewModel.ShowFreeTransform(corners);
    }

    private void ClearSelection(object? param)
    {
        if (updateableChangeActive)
            return;
        Helpers.ActionAccumulator.AddFinishedActions(new ClearSelection_Action());
    }

    private void DeleteStructureMember(object? param)
    {
        if (updateableChangeActive || FindFirstSelectedMember() is not { } member)
            return;
        Helpers.ActionAccumulator.AddFinishedActions(new DeleteStructureMember_Action(member.GuidValue));
    }

    private void Undo(object? param)
    {
        if (updateableChangeActive)
            return;
        Helpers.ActionAccumulator.AddActions(new Undo_Action());
    }

    private void Redo(object? param)
    {
        if (updateableChangeActive)
            return;
        Helpers.ActionAccumulator.AddActions(new Redo_Action());
    }

    private void ToggleLockTransparency(object? param)
    {
        if (updateableChangeActive || FindFirstSelectedMember() is not LayerViewModel layer)
            return;
        layer.LockTransparencyBindable = !layer.LockTransparencyBindable;
    }

    private void ResizeCanvas(object? param)
    {
        if (updateableChangeActive)
            return;
        Helpers.ActionAccumulator.AddFinishedActions(new ResizeCanvas_Action(new(ResizeWidth, ResizeHeight), owner.ResizeAnchor));
    }

    private void CreateMask(object? param)
    {
        var member = FindFirstSelectedMember();
        if (updateableChangeActive || member is null || member.HasMaskBindable)
            return;
        Helpers.ActionAccumulator.AddFinishedActions(new CreateStructureMemberMask_Action(member.GuidValue));
    }

    private void DeleteMask(object? param)
    {
        var member = FindFirstSelectedMember();
        if (updateableChangeActive || member is null || !member.HasMaskBindable)
            return;
        Helpers.ActionAccumulator.AddFinishedActions(new DeleteStructureMemberMask_Action(member.GuidValue));
    }

    private void Combine(object? param)
    {
        if (updateableChangeActive)
            return;
        List<Guid> selected = new();
        AddSelectedMembers(StructureRoot, selected);
        if (selected.Count < 2)
            return;

        var (child, parent) = Helpers.StructureHelper.FindChildAndParentOrThrow(selected[0]);
        int index = parent.Children.IndexOf(child);
        Guid newGuid = Guid.NewGuid();

        //make a new layer, put combined image onto it, delete layers that were merged
        Helpers.ActionAccumulator.AddActions(
            new CreateStructureMember_Action(parent.GuidValue, newGuid, index, StructureMemberType.Layer),
            new StructureMemberName_Action(newGuid, child.NameBindable + "-comb"),
            new CombineStructureMembersOnto_Action(selected.ToHashSet(), newGuid));
        foreach (var member in selected)
            Helpers.ActionAccumulator.AddActions(new DeleteStructureMember_Action(member));
        Helpers.ActionAccumulator.AddActions(new ChangeBoundary_Action());
    }

    private void ClearHistory(object? param)
    {
        if (updateableChangeActive)
            return;
        Helpers.ActionAccumulator.AddActions(new DeleteRecordedChanges_Action());
    }

    private void AddSelectedMembers(FolderViewModel folder, List<Guid> collection)
    {
        foreach (var child in folder.Children)
        {
            if (child.IsSelected)
                collection.Add(child.GuidValue);
            if (child is FolderViewModel innerFolder)
                AddSelectedMembers(innerFolder, collection);
        }
    }

    public void RaisePropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
