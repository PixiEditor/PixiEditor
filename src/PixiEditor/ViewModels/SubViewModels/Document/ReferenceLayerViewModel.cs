using System.Collections.Immutable;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Helpers;
using PixiEditor.Models.DocumentModels;
using PixiEditor.ViewModels.SubViewModels.Tools.Tools;

namespace PixiEditor.ViewModels.SubViewModels.Document;

#nullable enable
internal class ReferenceLayerViewModel : INotifyPropertyChanged
{
    private readonly DocumentViewModel doc;
    private readonly DocumentInternalParts internals;
    public event PropertyChangedEventHandler PropertyChanged;

    public const float TopMostOpacity = 0.6f;
    
    public WriteableBitmap? ReferenceBitmap { get; private set; }

    private ShapeCorners referenceShape;
    public ShapeCorners ReferenceShapeBindable 
    { 
        get => referenceShape; 
        set
        {
            if (!doc.UpdateableChangeActive)
                internals.ActionAccumulator.AddFinishedActions(new TransformReferenceLayer_Action(value));
        }
    }
    
    public Matrix ReferenceTransformMatrix
    {
        get
        {
            if (ReferenceBitmap is null)
                return Matrix.Identity;
            Matrix3X3 skiaMatrix = OperationHelper.CreateMatrixFromPoints((ShapeCorners)ReferenceShapeBindable, new VecD(ReferenceBitmap.Width, ReferenceBitmap.Height));
            return new Matrix(skiaMatrix.ScaleX, skiaMatrix.SkewY, skiaMatrix.SkewX, skiaMatrix.ScaleY, skiaMatrix.TransX, skiaMatrix.TransY);
        }
    }

    private bool isVisible;
    public bool IsVisibleBindable
    {
        get => isVisible;
        set
        {
            if (!doc.UpdateableChangeActive)
                internals.ActionAccumulator.AddFinishedActions(new ReferenceLayerIsVisible_Action(value));
        }
    }

    private bool isTransforming;
    public bool IsTransforming
    {
        get => isTransforming;
        set
        {
            isTransforming = value;
            RaisePropertyChanged(nameof(IsTransforming));
            RaisePropertyChanged(nameof(ShowHighest));
        }
    }
    
    private bool isTopMost;
    public bool IsTopMost
    {
        get => isTopMost;
        set
        {
            if (!doc.UpdateableChangeActive)
                internals.ActionAccumulator.AddFinishedActions(new ReferenceLayerTopMost_Action(value));
        }
    }
    
    public bool ShowHighest
    {
        get => (IsTopMost || IsTransforming) && !IsColorPickerSelected();
    }

    public ReferenceLayerViewModel(DocumentViewModel doc, DocumentInternalParts internals)
    {
        this.doc = doc;
        this.internals = internals;
    }

    private bool IsColorPickerSelected()
    {
        var viewModel = ViewModelMain.Current.ToolsSubViewModel;
        
        if (viewModel.ActiveTool is ColorPickerToolViewModel colorPicker)
        {
            return colorPicker.PickFromReferenceLayer && !colorPicker.PickFromCanvas;
        }

        return false;
    }
    
    private void RaisePropertyChanged(string name)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    
    #region Internal methods

    public void RaiseShowHighestChanged() => RaisePropertyChanged(nameof(ShowHighest));
    
    public void InternalSetReferenceLayer(ImmutableArray<byte> imagePbgra32Bytes, VecI imageSize, ShapeCorners shape)
    {
        ReferenceBitmap = WriteableBitmapHelpers.FromPbgra32Array(imagePbgra32Bytes.ToArray(), imageSize);
        referenceShape = shape;
        isVisible = true;
        isTransforming = false;
        isTopMost = false;
        RaisePropertyChanged(nameof(ReferenceBitmap));
        RaisePropertyChanged(nameof(ReferenceShapeBindable));
        RaisePropertyChanged(nameof(ReferenceTransformMatrix));
        RaisePropertyChanged(nameof(IsVisibleBindable));
        RaisePropertyChanged(nameof(IsTransforming));
        RaisePropertyChanged(nameof(ShowHighest));
    }

    public void InternalDeleteReferenceLayer()
    {
        ReferenceBitmap = null;
        isVisible = false;
        RaisePropertyChanged(nameof(ReferenceBitmap));
        RaisePropertyChanged(nameof(ReferenceTransformMatrix));
        RaisePropertyChanged(nameof(IsVisibleBindable));
    }
    
    public void InternalTransformReferenceLayer(ShapeCorners shape)
    {
        referenceShape = shape;
        RaisePropertyChanged(nameof(ReferenceShapeBindable));
        RaisePropertyChanged(nameof(ReferenceTransformMatrix));
    }

    public void InternalSetReferenceLayerIsVisible(bool isVisible)
    {
        this.isVisible = isVisible;
        RaisePropertyChanged(nameof(IsVisibleBindable));
    }

    public void InternalSetReferenceLayerTopMost(bool isTopMost)
    {
        this.isTopMost = isTopMost;
        RaisePropertyChanged(nameof(IsTopMost));
        RaisePropertyChanged(nameof(ShowHighest));
    }

    #endregion
}
