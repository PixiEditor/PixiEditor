using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using Microsoft.Win32;
using PixiEditor.ChangeableDocument.Actions.Undo;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditorPrototype.CustomControls.SymmetryOverlay;
using PixiEditorPrototype.Models;
using SkiaSharp;

namespace PixiEditorPrototype.ViewModels;

internal class DocumentViewModel : INotifyPropertyChanged
{
    private StructureMemberViewModel? selectedStructureMember;
    public StructureMemberViewModel? SelectedStructureMember
    {
        get => selectedStructureMember;
        private set
        {
            selectedStructureMember = value;
            PropertyChanged?.Invoke(this, new(nameof(SelectedStructureMember)));
        }
    }

    public Dictionary<ChunkResolution, WriteableBitmap> Bitmaps { get; set; } = new()
    {
        [ChunkResolution.Full] = new WriteableBitmap(64, 64, 96, 96, PixelFormats.Pbgra32, null),
        [ChunkResolution.Half] = new WriteableBitmap(32, 32, 96, 96, PixelFormats.Pbgra32, null),
        [ChunkResolution.Quarter] = new WriteableBitmap(16, 16, 96, 96, PixelFormats.Pbgra32, null),
        [ChunkResolution.Eighth] = new WriteableBitmap(8, 8, 96, 96, PixelFormats.Pbgra32, null),
    };

    public Dictionary<ChunkResolution, SKSurface> Surfaces { get; set; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    public void RaisePropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public FolderViewModel StructureRoot { get; }
    public DocumentTransformViewModel TransformViewModel { get; }
    public RelayCommand? UndoCommand { get; }
    public RelayCommand? RedoCommand { get; }
    public RelayCommand? ClearSelectionCommand { get; }
    public RelayCommand? CreateNewLayerCommand { get; }
    public RelayCommand? CreateNewFolderCommand { get; }
    public RelayCommand? DeleteStructureMemberCommand { get; }
    public RelayCommand? ChangeSelectedItemCommand { get; }
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

    public int Width => Helpers.Tracker.Document.Size.X;
    public int Height => Helpers.Tracker.Document.Size.Y;
    public SKPath SelectionPath => Helpers.Tracker.Document.Selection.SelectionPath;
    public Guid GuidValue { get; } = Guid.NewGuid();
    public int HorizontalSymmetryAxisY => Helpers.Tracker.Document.HorizontalSymmetryAxisY;
    public int VerticalSymmetryAxisX => Helpers.Tracker.Document.VerticalSymmetryAxisX;
    public bool HorizontalSymmetryAxisEnabled
    {
        get => Helpers.Tracker.Document.HorizontalSymmetryAxisEnabled;
        set => Helpers.ActionAccumulator.AddFinishedActions(new SymmetryAxisState_Action(SymmetryAxisDirection.Horizontal, value));
    }
    public bool VerticalSymmetryAxisEnabled
    {
        get => Helpers.Tracker.Document.VerticalSymmetryAxisEnabled;
        set => Helpers.ActionAccumulator.AddFinishedActions(new SymmetryAxisState_Action(SymmetryAxisDirection.Vertical, value));
    }

    public int ResizeWidth { get; set; }
    public int ResizeHeight { get; set; }

    private DocumentHelpers Helpers { get; }

    private ViewModelMain owner;

    public DocumentViewModel(ViewModelMain owner)
    {
        this.owner = owner;

        TransformViewModel = new();
        TransformViewModel.TransformMoved += OnTransformUpdate;

        Helpers = new DocumentHelpers(this);
        StructureRoot = new FolderViewModel(this, Helpers, Helpers.Tracker.Document.StructureRoot);

        UndoCommand = new RelayCommand(Undo);
        RedoCommand = new RelayCommand(Redo);
        ClearSelectionCommand = new RelayCommand(ClearSelection);
        CreateNewLayerCommand = new RelayCommand(_ => Helpers.StructureHelper.CreateNewStructureMember(StructureMemberType.Layer));
        CreateNewFolderCommand = new RelayCommand(_ => Helpers.StructureHelper.CreateNewStructureMember(StructureMemberType.Folder));
        DeleteStructureMemberCommand = new RelayCommand(DeleteStructureMember);
        ChangeSelectedItemCommand = new RelayCommand(ChangeSelectedItem);
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

        Helpers.ActionAccumulator.AddFinishedActions
            (new CreateStructureMember_Action(StructureRoot.GuidValue, Guid.NewGuid(), 0, StructureMemberType.Layer));
    }

    private void TransformSelectedArea(object? obj)
    {
        if (updateableChangeActive || SelectedStructureMember is not LayerViewModel layer || SelectionPath.IsEmpty)
            return;
        IReadOnlyChunkyImage? layerImage = (Helpers.Tracker.Document.FindMember(layer.GuidValue) as IReadOnlyLayer)?.LayerImage;
        if (layerImage is null)
            return;

        // find area location and size
        using SKPath path = SelectionPath;
        var bounds = path.TightBounds;
        bounds.Intersect(SKRect.Create(0, 0, Width, Height));
        VecI pixelTopLeft = (VecI)((VecD)bounds.Location).Floor();
        VecI pixelSize = (VecI)((VecD)bounds.Location + (VecD)bounds.Size - pixelTopLeft).Ceiling();

        // extract surface to be transformed
        path.Transform(SKMatrix.CreateTranslation(-pixelTopLeft.X, -pixelTopLeft.Y));
        Surface surface = new(pixelSize);
        surface.SkiaSurface.Canvas.Save();
        surface.SkiaSurface.Canvas.ClipPath(path);
        layerImage.DrawMostUpToDateRegionOn(SKRectI.Create(pixelTopLeft, pixelSize), ChunkResolution.Full, surface.SkiaSurface, VecI.Zero);
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
        ShapeCorners corners = new(pixelTopLeft, pixelSize);
        Helpers.ActionAccumulator.AddActions(new PasteImage_Action(pastedImage, corners, layer.GuidValue, false));
        TransformViewModel.ShowFreeTransform(corners);
    }

    private void ApplyMask(object? obj)
    {
        if (updateableChangeActive || SelectedStructureMember is not LayerViewModel layer || !layer.HasMask)
            return;
        Helpers.ActionAccumulator.AddFinishedActions(new ApplyLayerMask_Action(layer.GuidValue));
    }

    private void ClipToMemberBelow(object? obj)
    {
        if (updateableChangeActive || SelectedStructureMember is null)
            return;
        SelectedStructureMember.ClipToMemberBelowEnabled = !SelectedStructureMember.ClipToMemberBelowEnabled;
    }

    private bool updateableChangeActive = false;

    private bool selectingRect = false;
    private bool selectingLasso = false;
    private bool drawingRectangle = false;
    private bool transformingRectangle = false;
    private bool shiftingLayer = false;

    private bool transformingSelectionPath = false;
    ShapeCorners initialSelectionCorners = new();

    private bool pastingImage = false;
    private Surface? pastedImage;

    private ShapeCorners lastShape = new ShapeCorners();
    private ShapeData lastShapeData = new();

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

    public void StartUpdateRectangle(ShapeData data)
    {
        if (SelectedStructureMember is null)
            return;
        bool drawOnMask = SelectedStructureMember.HasMask && SelectedStructureMember.ShouldDrawOnMask;
        if (SelectedStructureMember is not LayerViewModel && !drawOnMask)
            return;
        updateableChangeActive = true;
        drawingRectangle = true;
        Helpers.ActionAccumulator.AddActions(new DrawRectangle_Action(SelectedStructureMember.GuidValue, data, drawOnMask));
        lastShape = new ShapeCorners(data.Center, data.Size, data.Angle);
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
        if (SelectedStructureMember is not LayerViewModel layer)
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
        var path = SelectionPath;
        if (path.IsEmpty)
            return;
        updateableChangeActive = true;
        transformingSelectionPath = true;
        var bounds = path.TightBounds;
        initialSelectionCorners = new ShapeCorners(bounds.Location, bounds.Size);
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

    public void ApplyTransform(object? param)
    {
        if (!transformingRectangle && !pastingImage && !transformingSelectionPath)
            return;

        if (transformingRectangle)
        {
            transformingRectangle = false;
            TransformViewModel.HideTransform();
            Helpers.ActionAccumulator.AddFinishedActions(new EndDrawRectangle_Action());
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

    public void StartUpdateRectSelection(VecI pos, VecI size, SelectionMode mode)
    {
        selectingRect = true;
        updateableChangeActive = true;
        Helpers.ActionAccumulator.AddActions(new SelectRectangle_Action(pos, size, mode));
    }

    public void EndRectSelection()
    {
        if (!selectingRect)
            return;
        selectingRect = false;
        updateableChangeActive = false;
        Helpers.ActionAccumulator.AddFinishedActions(new EndSelectRectangle_Action());
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
        else if (pastingImage)
        {
            if (SelectedStructureMember is null || pastedImage is null)
                return;
            Helpers.ActionAccumulator.AddActions(new PasteImage_Action(pastedImage, newCorners, SelectedStructureMember.GuidValue, false));
        }
        else if (transformingSelectionPath)
        {
            Helpers.ActionAccumulator.AddActions(new TransformSelectionPath_Action(newCorners));
        }
    }

    public void FloodFill(VecI pos, SKColor color)
    {
        if (updateableChangeActive || SelectedStructureMember is null)
            return;
        Helpers.ActionAccumulator.AddFinishedActions(new FloodFill_Action(SelectedStructureMember.GuidValue, pos, color, SelectedStructureMember.ShouldDrawOnMask));
    }

    public void AddOrUpdateViewport(ViewportLocation location)
    {
        Helpers.ActionAccumulator.AddActions(new RefreshViewport_PassthroughAction(location));
    }

    public void RemoveViewport(Guid viewportGuid)
    {
        Helpers.ActionAccumulator.AddActions(new RemoveViewport_PassthroughAction(viewportGuid));
    }

    private void PasteImage(object? args)
    {
        if (SelectedStructureMember is null || SelectedStructureMember is not LayerViewModel)
            return;
        OpenFileDialog dialog = new();
        if (dialog.ShowDialog() != true)
            return;

        pastedImage = Surface.Load(dialog.FileName);
        pastingImage = true;
        ShapeCorners corners = new(new(), pastedImage.Size);
        Helpers.ActionAccumulator.AddActions(new PasteImage_Action(pastedImage, corners, SelectedStructureMember.GuidValue, false));
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
        if (updateableChangeActive || SelectedStructureMember is null)
            return;
        Helpers.ActionAccumulator.AddFinishedActions(new DeleteStructureMember_Action(SelectedStructureMember.GuidValue));
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
        if (updateableChangeActive || SelectedStructureMember is not LayerViewModel layer)
            return;
        layer.LockTransparency = !layer.LockTransparency;
    }

    private void ResizeCanvas(object? param)
    {
        if (updateableChangeActive)
            return;
        Helpers.ActionAccumulator.AddFinishedActions(new ResizeCanvas_Action(new(ResizeWidth, ResizeHeight)));
    }

    private void ChangeSelectedItem(object? param)
    {
        SelectedStructureMember = (StructureMemberViewModel?)((RoutedPropertyChangedEventArgs<object>?)param)?.NewValue;
    }

    private void CreateMask(object? param)
    {
        if (updateableChangeActive || SelectedStructureMember is null || SelectedStructureMember.HasMask)
            return;
        Helpers.ActionAccumulator.AddFinishedActions(new CreateStructureMemberMask_Action(SelectedStructureMember.GuidValue));
    }

    private void DeleteMask(object? param)
    {
        if (updateableChangeActive || SelectedStructureMember is null || !SelectedStructureMember.HasMask)
            return;
        Helpers.ActionAccumulator.AddFinishedActions(new DeleteStructureMemberMask_Action(SelectedStructureMember.GuidValue));
    }

    private void Combine(object? param)
    {
        if (updateableChangeActive || SelectedStructureMember is null)
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
            new StructureMemberName_Action(newGuid, child.Name + "-comb"),
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
}
