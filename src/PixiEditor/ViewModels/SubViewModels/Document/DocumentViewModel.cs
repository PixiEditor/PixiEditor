using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Helpers;
using PixiEditor.Models.BitmapActions;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Position;
using SkiaSharp;

namespace PixiEditor.ViewModels.SubViewModels.Document;

#nullable enable
internal class DocumentViewModel : NotifyableObject
{
    public const string ConfirmationDialogTitle = "Unsaved changes";
    public const string ConfirmationDialogMessage = "The document has been modified. Do you want to save changes?";

    public bool Busy
    {
        get => busy;
        set
        {
            busy = value;
            RaisePropertyChanged(nameof(Busy));
        }
    }

    public FolderViewModel StructureRoot { get; }

    public int Width => size.X;
    public int Height => size.Y;

    public StructureMemberViewModel? SelectedStructureMember => FindFirstSelectedMember();

    public Dictionary<ChunkResolution, WriteableBitmap> Bitmaps { get; set; } = new()
    {
        [ChunkResolution.Full] = new WriteableBitmap(64, 64, 96, 96, PixelFormats.Pbgra32, null),
        [ChunkResolution.Half] = new WriteableBitmap(32, 32, 96, 96, PixelFormats.Pbgra32, null),
        [ChunkResolution.Quarter] = new WriteableBitmap(16, 16, 96, 96, PixelFormats.Pbgra32, null),
        [ChunkResolution.Eighth] = new WriteableBitmap(8, 8, 96, 96, PixelFormats.Pbgra32, null),
    };

    public WriteableBitmap PreviewBitmap { get; set; }
    public SKSurface PreviewSurface { get; set; }

    public Dictionary<ChunkResolution, SKSurface> Surfaces { get; set; } = new();

    public VecI SizeBindable => size;

    public StructureMemberViewModel? FindFirstSelectedMember() => Helpers.StructureHelper.FindFirstWhere(member => member.IsSelected);

    public int HorizontalSymmetryAxisYBindable => horizontalSymmetryAxisY;
    public int VerticalSymmetryAxisXBindable => verticalSymmetryAxisX;

    public bool HorizontalSymmetryAxisEnabledBindable
    {
        get => horizontalSymmetryAxisEnabled;
        set => Helpers.ActionAccumulator.AddFinishedActions(new SymmetryAxisState_Action(SymmetryAxisDirection.Horizontal, value));
    }

    public bool VerticalSymmetryAxisEnabledBindable
    {
        get => verticalSymmetryAxisEnabled;
        set => Helpers.ActionAccumulator.AddFinishedActions(new SymmetryAxisState_Action(SymmetryAxisDirection.Vertical, value));
    }

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
            var skiaMatrix = OperationHelper.CreateMatrixFromPoints(ReferenceLayer.Shape, ReferenceLayer.Image.Size);
            return new Matrix(skiaMatrix.ScaleX, skiaMatrix.SkewY, skiaMatrix.SkewX, skiaMatrix.ScaleY, skiaMatrix.TransX, skiaMatrix.TransY);
        }
    }

    public SKPath SelectionPathBindable => selectionPath;
    public DocumentTransformViewModel TransformViewModel { get; }

    private DocumentHelpers Helpers { get; }

    private readonly DocumentManagerViewModel owner;

    private int verticalSymmetryAxisX;

    private bool horizontalSymmetryAxisEnabled;

    private bool verticalSymmetryAxisEnabled;

    private bool busy = false;

    private VecI size = new VecI(64, 64);

    private int horizontalSymmetryAxisY;

    private SKPath selectionPath = new SKPath();
    private Guid testLayerGuid = Guid.NewGuid();


    public DocumentViewModel(DocumentManagerViewModel owner, string name)
    {
        this.owner = owner;
        //Name = name;
        TransformViewModel = new();
        //TransformViewModel.TransformMoved += OnTransformUpdate;

        Helpers = new DocumentHelpers(this);
        StructureRoot = new FolderViewModel(this, Helpers, Helpers.Tracker.Document.StructureRoot.GuidValue);

        /*UndoCommand = new RelayCommand(Undo);
        RedoCommand = new RelayCommand(Redo);
        ClearSelectionCommand = new RelayCommand(ClearSelection);
        CreateNewLayerCommand = new RelayCommand(_ => Helpers.StructureHelper.CreateNewStructureMember(StructureMemberType.Layer));
        CreateNewFolderCommand = new RelayCommand(_ => Helpers.StructureHelper.CreateNewStructureMember(StructureMemberType.Folder));
        DeleteStructureMemberCommand = new RelayCommand(DeleteStructureMember);
        ResizeCanvasCommand = new RelayCommand(ResizeCanvas);
        ResizeImageCommand = new RelayCommand(ResizeImage);
        CombineCommand = new RelayCommand(Combine);
        ClearHistoryCommand = new RelayCommand(ClearHistory);
        CreateMaskCommand = new RelayCommand(CreateMask);
        DeleteMaskCommand = new RelayCommand(DeleteMask);
        ToggleLockTransparencyCommand = new RelayCommand(ToggleLockTransparency);
        PasteImageCommand = new RelayCommand(PasteImage);
        CreateReferenceLayerCommand = new RelayCommand(CreateReferenceLayer);
        ApplyTransformCommand = new RelayCommand(ApplyTransform);
        DragSymmetryCommand = new RelayCommand(DragSymmetry);
        EndDragSymmetryCommand = new RelayCommand(EndDragSymmetry);
        ClipToMemberBelowCommand = new RelayCommand(ClipToMemberBelow);
        ApplyMaskCommand = new RelayCommand(ApplyMask);
        TransformSelectionPathCommand = new RelayCommand(TransformSelectionPath);
        TransformSelectedAreaCommand = new RelayCommand(TransformSelectedArea);*/

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

        Helpers.ActionAccumulator.AddFinishedActions(new CreateStructureMember_Action(StructureRoot.GuidValue, testLayerGuid, 0, StructureMemberType.Layer));
    }

    public void InternalSetSize(VecI size)
    {
        this.size = size;
        RaisePropertyChanged(nameof(SizeBindable));
        RaisePropertyChanged(nameof(Width));
        RaisePropertyChanged(nameof(Height));
    }

    #region Symmetry

    public void SetVerticalSymmetryAxisEnabled(bool verticalSymmetryAxisEnabled)
    {
        this.verticalSymmetryAxisEnabled = verticalSymmetryAxisEnabled;
        RaisePropertyChanged(nameof(VerticalSymmetryAxisEnabledBindable));
    }

    public void SetHorizontalSymmetryAxisEnabled(bool horizontalSymmetryAxisEnabled)
    {
        this.horizontalSymmetryAxisEnabled = horizontalSymmetryAxisEnabled;
        RaisePropertyChanged(nameof(HorizontalSymmetryAxisEnabledBindable));
    }

    public void SetVerticalSymmetryAxisX(int verticalSymmetryAxisX)
    {
        this.verticalSymmetryAxisX = verticalSymmetryAxisX;
        RaisePropertyChanged(nameof(VerticalSymmetryAxisXBindable));
    }

    public void SetHorizontalSymmetryAxisY(int horizontalSymmetryAxisY)
    {
        this.horizontalSymmetryAxisY = horizontalSymmetryAxisY;
        RaisePropertyChanged(nameof(HorizontalSymmetryAxisYBindable));
    }

    #endregion

    public void AddOrUpdateViewport(ViewportInfo info)
    {
        Helpers.ActionAccumulator.AddActions(new RefreshViewport_PassthroughAction(info));
    }

    public void RemoveViewport(Guid viewportGuid)
    {
        Helpers.ActionAccumulator.AddActions(new RemoveViewport_PassthroughAction(viewportGuid));
    }

    public void OnKeyDown(Key args)
    {

    }

    public void OnKeyUp(Key args)
    {

    }

    private bool drawing = false;
    public void OnCanvasLeftMouseButtonDown(VecD pos)
    {
        drawing = true;
        Helpers.ActionAccumulator.AddActions(new LineBasedPen_Action(
            testLayerGuid,
            SKColors.Black,
            (VecI)pos,
            (int)1,
            false,
            false));
    }

    public void OnCanvasMouseMove(VecD newPos)
    {
        if (!drawing)
            return;
        Helpers.ActionAccumulator.AddActions(new LineBasedPen_Action(
            testLayerGuid,
            SKColors.Black,
            (VecI)newPos,
            (int)1,
            false,
            false));
    }

    public void OnCanvasLeftMouseButtonUp()
    {
        drawing = false;
        Helpers.ActionAccumulator.AddFinishedActions(new EndLineBasedPen_Action());
    }

    public void InternalUpdateSelectionPath(SKPath selectionPath)
    {
        (var toDispose, this.selectionPath) = (this.selectionPath, selectionPath);
        toDispose.Dispose();
        RaisePropertyChanged(nameof(SelectionPathBindable));
    }
}
