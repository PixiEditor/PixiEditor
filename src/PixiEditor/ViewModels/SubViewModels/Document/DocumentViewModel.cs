using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using PixiEditor.ChangeableDocument.Actions.Undo;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.Helpers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.DocumentModels.UpdateableChangeExecutors;
using PixiEditor.Models.DocumentPassthroughActions;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Position;

namespace PixiEditor.ViewModels.SubViewModels.Document;

#nullable enable
internal class DocumentViewModel : NotifyableObject
{
    private bool busy = false;
    public bool Busy
    {
        get => busy;
        set => SetProperty(ref busy, value);
    }

    private string coordinatesString = "";
    public string CoordinatesString
    {
        get => coordinatesString;
        set => SetProperty(ref coordinatesString, value);
    }

    private string? fullFilePath = null;
    public string? FullFilePath 
    { 
        get => fullFilePath;
        set
        {
            SetProperty(ref fullFilePath, value);
            RaisePropertyChanged(nameof(FileName));
        }
    }
    public string FileName
    {
        get => fullFilePath is null ? "Unnamed" : Path.GetFileName(fullFilePath);
    }

    private Guid? lastChangeOnSave = null;
    public bool AllChangesSaved
    {
        get
        {
            return Helpers.Tracker.LastChangeGuid == lastChangeOnSave;
        }
    }

    private bool horizontalSymmetryAxisEnabled;
    public bool HorizontalSymmetryAxisEnabledBindable
    {
        get => horizontalSymmetryAxisEnabled;
        set => Helpers.ActionAccumulator.AddFinishedActions(new SymmetryAxisState_Action(SymmetryAxisDirection.Horizontal, value));
    }

    private bool verticalSymmetryAxisEnabled;
    public bool VerticalSymmetryAxisEnabledBindable
    {
        get => verticalSymmetryAxisEnabled;
        set => Helpers.ActionAccumulator.AddFinishedActions(new SymmetryAxisState_Action(SymmetryAxisDirection.Vertical, value));
    }

    private VecI size = new VecI(64, 64);
    public int Width => size.X;
    public int Height => size.Y;
    public VecI SizeBindable => size;

    private int horizontalSymmetryAxisY;
    public int HorizontalSymmetryAxisYBindable => horizontalSymmetryAxisY;

    private int verticalSymmetryAxisX;
    public int VerticalSymmetryAxisXBindable => verticalSymmetryAxisX;

    private HashSet<StructureMemberViewModel> softSelectedStructureMembers = new();
    public IReadOnlyCollection<StructureMemberViewModel> SoftSelectedStructureMembers => softSelectedStructureMembers;


    public bool UpdateableChangeActive => Helpers.ChangeController.IsChangeActive;
    public bool HasSavedUndo => Helpers.Tracker.HasSavedUndo;
    public bool HasSavedRedo => Helpers.Tracker.HasSavedRedo;
    public IReadOnlyReferenceLayer? ReferenceLayer => Helpers.Tracker.Document.ReferenceLayer;
    public BitmapSource? ReferenceBitmap => ReferenceLayer?.Image.ToWriteableBitmap();
    public VecI ReferenceBitmapSize => ReferenceLayer?.Image.Size ?? VecI.Zero;
    public ShapeCorners ReferenceShape => ReferenceLayer?.Shape ?? default;
    public Matrix ReferenceTransformMatrix
    {
        get
        {
            if (ReferenceLayer is null)
                return Matrix.Identity;
            SKMatrix skiaMatrix = OperationHelper.CreateMatrixFromPoints(ReferenceLayer.Shape, ReferenceLayer.Image.Size);
            return new Matrix(skiaMatrix.ScaleX, skiaMatrix.SkewY, skiaMatrix.SkewX, skiaMatrix.ScaleY, skiaMatrix.TransX, skiaMatrix.TransY);
        }
    }

    public FolderViewModel StructureRoot { get; }
    public DocumentStructureViewModel StructureViewModel { get; }
    public StructureMemberViewModel? SelectedStructureMember { get; private set; } = null;

    public Dictionary<ChunkResolution, SKSurface> Surfaces { get; set; } = new();
    public Dictionary<ChunkResolution, WriteableBitmap> Bitmaps { get; set; } = new()
    {
        [ChunkResolution.Full] = new WriteableBitmap(64, 64, 96, 96, PixelFormats.Pbgra32, null),
        [ChunkResolution.Half] = new WriteableBitmap(32, 32, 96, 96, PixelFormats.Pbgra32, null),
        [ChunkResolution.Quarter] = new WriteableBitmap(16, 16, 96, 96, PixelFormats.Pbgra32, null),
        [ChunkResolution.Eighth] = new WriteableBitmap(8, 8, 96, 96, PixelFormats.Pbgra32, null),
    };
    public WriteableBitmap PreviewBitmap { get; set; }
    public SKSurface PreviewSurface { get; set; }


    private SKPath selectionPath = new SKPath();
    public SKPath SelectionPathBindable => selectionPath;

    public WpfObservableRangeCollection<SKColor> Swatches { get; set; } = new WpfObservableRangeCollection<SKColor>();
    public WpfObservableRangeCollection<SKColor> Palette { get; set; } = new WpfObservableRangeCollection<SKColor>();

    public ExecutionTrigger<VecI> CenterViewportTrigger { get; } = new ExecutionTrigger<VecI>();
    public ExecutionTrigger<double> ZoomViewportTrigger { get; } = new ExecutionTrigger<double>();
    public DocumentTransformViewModel TransformViewModel { get; }


    private DocumentHelpers Helpers { get; }

    public DocumentViewModel()
    {
        //Name = name;
        Helpers = new DocumentHelpers(this);
        StructureViewModel = new DocumentStructureViewModel(this);
        StructureRoot = new FolderViewModel(this, Helpers, Helpers.Tracker.Document.StructureRoot.GuidValue);

        TransformViewModel = new();
        TransformViewModel.TransformMoved += (_, args) => Helpers.ChangeController.OnTransformMoved(args);

        foreach (KeyValuePair<ChunkResolution, WriteableBitmap> bitmap in Bitmaps)
        {
            SKSurface? surface = SKSurface.Create(
                new SKImageInfo(bitmap.Value.PixelWidth, bitmap.Value.PixelHeight, SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb()),
                bitmap.Value.BackBuffer, bitmap.Value.BackBufferStride);
            Surfaces[bitmap.Key] = surface;
        }

        VecI previewSize = StructureMemberViewModel.CalculatePreviewSize(SizeBindable);
        PreviewBitmap = new WriteableBitmap(previewSize.X, previewSize.Y, 96, 96, PixelFormats.Pbgra32, null);
        PreviewSurface = SKSurface.Create(new SKImageInfo(previewSize.X, previewSize.Y, SKColorType.Bgra8888), PreviewBitmap.BackBuffer, PreviewBitmap.BackBufferStride);
    }

    #region Internal Methods
    // these are intended to only be called from DocumentUpdater
    public void InternalSetVerticalSymmetryAxisEnabled(bool verticalSymmetryAxisEnabled)
    {
        this.verticalSymmetryAxisEnabled = verticalSymmetryAxisEnabled;
        RaisePropertyChanged(nameof(VerticalSymmetryAxisEnabledBindable));
    }

    public void InternalSetHorizontalSymmetryAxisEnabled(bool horizontalSymmetryAxisEnabled)
    {
        this.horizontalSymmetryAxisEnabled = horizontalSymmetryAxisEnabled;
        RaisePropertyChanged(nameof(HorizontalSymmetryAxisEnabledBindable));
    }

    public void InternalSetVerticalSymmetryAxisX(int verticalSymmetryAxisX)
    {
        this.verticalSymmetryAxisX = verticalSymmetryAxisX;
        RaisePropertyChanged(nameof(VerticalSymmetryAxisXBindable));
    }

    public void InternalSetHorizontalSymmetryAxisY(int horizontalSymmetryAxisY)
    {
        this.horizontalSymmetryAxisY = horizontalSymmetryAxisY;
        RaisePropertyChanged(nameof(HorizontalSymmetryAxisYBindable));
    }

    public void InternalSetSize(VecI size)
    {
        this.size = size;
        RaisePropertyChanged(nameof(SizeBindable));
        RaisePropertyChanged(nameof(Width));
        RaisePropertyChanged(nameof(Height));
    }

    public void InternalUpdateSelectionPath(SKPath selectionPath)
    {
        (SKPath? toDispose, this.selectionPath) = (this.selectionPath, selectionPath);
        toDispose.Dispose();
        RaisePropertyChanged(nameof(SelectionPathBindable));
    }

    public void InternalSetSelectedMember(StructureMemberViewModel? member)
    {
        SelectedStructureMember = member;
        RaisePropertyChanged(nameof(SelectedStructureMember));
    }

    public void InternalClearSoftSelectedMembers() => softSelectedStructureMembers.Clear();

    public void InternalAddSoftSelectedMember(StructureMemberViewModel member) => softSelectedStructureMembers.Add(member);
    public void InternalRemoveSoftSelectedMember(StructureMemberViewModel member) => softSelectedStructureMembers.Remove(member);

    #endregion

    public void SetMemberOpacity(Guid memberGuid, float value)
    {
        if (Helpers.ChangeController.IsChangeActive || value is > 1 or < 0)
            return;
        Helpers.ActionAccumulator.AddFinishedActions(
            new StructureMemberOpacity_Action(memberGuid, value),
            new EndStructureMemberOpacity_Action());
    }

    public void AddOrUpdateViewport(ViewportInfo info) => Helpers.ActionAccumulator.AddActions(new RefreshViewport_PassthroughAction(info));

    public void RemoveViewport(Guid viewportGuid) => Helpers.ActionAccumulator.AddActions(new RemoveViewport_PassthroughAction(viewportGuid));

    public void ClearUndo()
    {
        if (Helpers.ChangeController.IsChangeActive)
            return;
        Helpers.ActionAccumulator.AddActions(new DeleteRecordedChanges_Action());
    }

    public void CreateStructureMember(StructureMemberType type)
    {
        if (Helpers.ChangeController.IsChangeActive)
            return;
        Helpers.StructureHelper.CreateNewStructureMember(type);
    }

    public void DuplicateLayer(Guid guidValue)
    {
        if (Helpers.ChangeController.IsChangeActive)
            return;
        Helpers.ActionAccumulator.AddFinishedActions(new DuplicateLayer_Action(guidValue));
    }

    public void DeleteStructureMember(Guid guidValue)
    {
        if (Helpers.ChangeController.IsChangeActive)
            return;
        Helpers.ActionAccumulator.AddFinishedActions(new DeleteStructureMember_Action(guidValue));
    }

    public void DeleteStructureMembers(IReadOnlyList<Guid> guids)
    {
        if (Helpers.ChangeController.IsChangeActive)
            return;
        Helpers.ActionAccumulator.AddFinishedActions(guids.Select(static guid => new DeleteStructureMember_Action(guid)).ToArray());
    }

    public void ResizeCanvas(VecI newSize, ResizeAnchor anchor)
    {
        if (Helpers.ChangeController.IsChangeActive || newSize.X > 9999 || newSize.Y > 9999 || newSize.X < 1 || newSize.Y < 1)
            return;
        Helpers.ActionAccumulator.AddFinishedActions(new ResizeCanvas_Action(newSize, anchor));
    }

    public void ResizeImage(VecI newSize, ResamplingMethod resampling)
    {
        if (Helpers.ChangeController.IsChangeActive || newSize.X > 9999 || newSize.Y > 9999 || newSize.X < 1 || newSize.Y < 1)
            return;
        Helpers.ActionAccumulator.AddFinishedActions(new ResizeImage_Action(newSize, resampling));
    }

    public void ReplaceColor(SKColor oldColor, SKColor newColor)
    {
        if (Helpers.ChangeController.IsChangeActive || oldColor == newColor)
            return;
        Helpers.ActionAccumulator.AddFinishedActions(new ReplaceColor_Action(oldColor, newColor));
    }

    public void SetSelectedMember(Guid memberGuid) => Helpers.ActionAccumulator.AddActions(new SetSelectedMember_PassthroughAction(memberGuid));

    public void AddSoftSelectedMember(Guid memberGuid) => Helpers.ActionAccumulator.AddActions(new AddSoftSelectedMember_PassthroughAction(memberGuid));

    public void RemoveSoftSelectedMember(Guid memberGuid) => Helpers.ActionAccumulator.AddActions(new RemoveSoftSelectedMember_PassthroughAction(memberGuid));

    public void ClearSoftSelectedMembers() => Helpers.ActionAccumulator.AddActions(new ClearSoftSelectedMembers_PassthroughAction());

    public void UseOpacitySlider() => Helpers.ChangeController.TryStartUpdateableChange<StructureMemberOpacityExecutor>();

    public void UsePenTool() => Helpers.ChangeController.TryStartUpdateableChange<PenToolExecutor>();

    public void UseEllipseTool() => Helpers.ChangeController.TryStartUpdateableChange<EllipseToolExecutor>();

    public void Undo()
    {
        if (Helpers.ChangeController.IsChangeActive)
            return;
        Helpers.ActionAccumulator.AddActions(new Undo_Action());
    }

    public void Redo()
    {
        if (Helpers.ChangeController.IsChangeActive)
            return;
        Helpers.ActionAccumulator.AddActions(new Redo_Action());
    }

    public void MoveStructureMember(Guid memberToMove, Guid memberToMoveIntoOrNextTo, StructureMemberPlacement placement)
    {
        if (Helpers.ChangeController.IsChangeActive || memberToMove == memberToMoveIntoOrNextTo)
            return;
        Helpers.StructureHelper.TryMoveStructureMember(memberToMove, memberToMoveIntoOrNextTo, placement);
    }

    public void MergeStructureMembers(IReadOnlyList<Guid> members)
    {
        if (Helpers.ChangeController.IsChangeActive || members.Count < 2)
            return;
        var (child, parent) = StructureViewModel.FindChildAndParent(members[0]);
        if (child is null || parent is null)
            return;
        int index = parent.Children.IndexOf(child);
        Guid newGuid = Guid.NewGuid();

        //make a new layer, put combined image onto it, delete layers that were merged
        Helpers.ActionAccumulator.AddActions(
            new CreateStructureMember_Action(parent.GuidValue, newGuid, index, StructureMemberType.Layer),
            new StructureMemberName_Action(newGuid, child.NameBindable),
            new CombineStructureMembersOnto_Action(members.ToHashSet(), newGuid));
        foreach (var member in members)
            Helpers.ActionAccumulator.AddActions(new DeleteStructureMember_Action(member));
        Helpers.ActionAccumulator.AddActions(new ChangeBoundary_Action());
    }

    public void MarkAsSaved()
    {
        lastChangeOnSave = Helpers.Tracker.LastChangeGuid;
        RaisePropertyChanged(nameof(AllChangesSaved));
    }

    public SKColor PickColor(VecI pos, bool fromAllLayers)
    {
        // there is a tiny chance that the image might get disposed by another thread
        try
        {
            // it might've been a better idea to implement this function asynchonously
            // via a passthrough action to avoid all the try catches
            if (fromAllLayers)
            {
                VecI chunkPos = OperationHelper.GetChunkPos(pos, ChunkyImage.FullChunkSize);
                return ChunkRenderer.MergeWholeStructure(chunkPos, ChunkResolution.Full, Helpers.Tracker.Document.StructureRoot)
                    .Match<SKColor>(
                        (Chunk chunk) =>
                        {
                            VecI posOnChunk = pos - chunkPos * ChunkyImage.FullChunkSize;
                            SKColor color = chunk.Surface.GetSRGBPixel(posOnChunk);
                            chunk.Dispose();
                            return color;
                        },
                        _ => SKColors.Transparent
                    );
            }

            if (SelectedStructureMember is not LayerViewModel layerVm)
                return SKColors.Transparent;
            IReadOnlyStructureMember? maybeMember = Helpers.Tracker.Document.FindMember(layerVm.GuidValue);
            if (maybeMember is not IReadOnlyLayer layer)
                return SKColors.Transparent;
            return layer.LayerImage.GetMostUpToDatePixel(pos);
        }
        catch (ObjectDisposedException)
        {
            return SKColors.Transparent;
        }
    }


    #region Events
    public void OnKeyDown(Key args) => Helpers.ChangeController.OnKeyDown(args);
    public void OnKeyUp(Key args) => Helpers.ChangeController.OnKeyUp(args);

    public void OnCanvasLeftMouseButtonDown(VecD pos) => Helpers.ChangeController.OnLeftMouseButtonDown(pos);
    public void OnCanvasMouseMove(VecD newPos)
    {
        CoordinatesString = $"X: {(int)newPos.X} Y: {(int)newPos.Y}";
        Helpers.ChangeController.OnMouseMove(newPos);
    }
    public void OnCanvasLeftMouseButtonUp() => Helpers.ChangeController.OnLeftMouseButtonUp();
    public void OnOpacitySliderDragStarted() => Helpers.ChangeController.OnOpacitySliderDragStarted();
    public void OnOpacitySliderDragged(float newValue) => Helpers.ChangeController.OnOpacitySliderDragged(newValue);
    public void OnOpacitySliderDragEnded() => Helpers.ChangeController.OnOpacitySliderDragEnded();
    public void OnApplyTransform() => Helpers.ChangeController.OnTransformApplied();
    #endregion
}
