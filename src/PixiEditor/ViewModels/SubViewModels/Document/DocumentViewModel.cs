using System.Windows.Media;
using System.Windows.Media.Imaging;
using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Helpers;
using PixiEditor.Models.DocumentModels;
using PixiEditor.ViewModels.Prototype;
using SkiaSharp;

namespace PixiEditor.ViewModels.SubViewModels.Document;

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

    private DocumentHelpers Helpers { get; }

    private readonly ViewModelMain owner;

    private int verticalSymmetryAxisX;
    
    private bool horizontalSymmetryAxisEnabled;

    private bool verticalSymmetryAxisEnabled;
    
    private bool busy = false;
    
    private VecI size = new VecI(64, 64);

    private int horizontalSymmetryAxisY;
    
    private SKPath selectionPath = new SKPath();
    
    public DocumentViewModel(string name)
    {
    }
    
    public void SetSize(VecI size)
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
    
    public void SetSelectionPath(SKPath selectionPath)
    {
        (var toDispose, this.selectionPath) = (this.selectionPath, selectionPath);
        toDispose.Dispose();
        RaisePropertyChanged(nameof(SelectionPathBindable));
    }
}
