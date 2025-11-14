using System.Collections.Immutable;
using System.Linq;
using Avalonia;
using Avalonia.Media.Imaging;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.Helpers;
using PixiEditor.ChangeableDocument.Actions.Generated;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces.ImageData;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos.Root.ReferenceLayerChangeInfos;
using PixiEditor.ViewModels.Tools.Tools;

namespace PixiEditor.ViewModels.Document;

#nullable enable
internal class ReferenceLayerViewModel : PixiObservableObject, IReferenceLayerHandler
{
    private readonly DocumentViewModel doc;
    private readonly DocumentInternalParts internals;

    public const double TopMostOpacity = 0.6;
    
    public Texture? ReferenceTexture { get; private set; }

    private ShapeCorners referenceShape;
    public ShapeCorners ReferenceShapeBindable 
    { 
        get => referenceShape; 
        set
        {
            if (!doc.BlockingUpdateableChangeActive)
                internals.ActionAccumulator.AddFinishedActions(new TransformReferenceLayer_Action(value));
        }
    }
    
    public Matrix3X3 ReferenceTransformMatrix
    {
        get
        {
            if (ReferenceTexture is null)
                return Matrix3X3.Identity;

            return OperationHelper.CreateMatrixFromPoints(ReferenceShapeBindable, new VecD(ReferenceTexture.Size.X, ReferenceTexture.Size.Y));
        }
    }

    private bool isVisible;
    public bool IsVisibleBindable
    {
        get => isVisible;
        set
        {
            if (!doc.BlockingUpdateableChangeActive)
                internals.ActionAccumulator.AddFinishedActions(new ReferenceLayerIsVisible_Action(value));
        }
    }

    private bool isTransforming;
    bool IReferenceLayerHandler.IsVisible => isVisible;

    public bool IsTransforming
    {
        get => isTransforming;
        set
        {
            isTransforming = value;
            OnPropertyChanged(nameof(IsTransforming));
            OnPropertyChanged(nameof(ShowHighest));
        }
    }
    
    private bool isTopMost;
    public bool IsTopMost
    {
        get => isTopMost;
        set
        {
            if (!doc.BlockingUpdateableChangeActive)
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

    internal void InitFrom(IReadOnlyReferenceLayer? documentReferenceLayer)
    {
        if (documentReferenceLayer is not null)
        {
            internals.Updater.ApplyChangeFromChangeInfo(new SetReferenceLayer_ChangeInfo(documentReferenceLayer.ImageBgra8888Bytes, documentReferenceLayer.ImageSize, documentReferenceLayer.Shape));
            internals.Updater.ApplyChangeFromChangeInfo(new ReferenceLayerIsVisible_ChangeInfo(documentReferenceLayer.IsVisible));
            internals.Updater.ApplyChangeFromChangeInfo(new ReferenceLayerTopMost_ChangeInfo(documentReferenceLayer.IsTopMost));
        }
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

    #region Internal methods

    public void RaiseShowHighestChanged() => OnPropertyChanged(nameof(ShowHighest));
    
    public void SetReferenceLayer(ImmutableArray<byte> imageBgra8888Bytes, VecI imageSize, ShapeCorners shape)
    {
        ReferenceTexture = Texture.Load(imageBgra8888Bytes.ToArray(), ColorType.Bgra8888, imageSize); 
        referenceShape = shape;
        isVisible = true;
        isTransforming = false;
        isTopMost = false;
        OnPropertyChanged(nameof(ReferenceTexture));
        OnPropertyChanged(nameof(ReferenceShapeBindable));
        OnPropertyChanged(nameof(ReferenceTransformMatrix));
        OnPropertyChanged(nameof(IsVisibleBindable));
        OnPropertyChanged(nameof(IsTransforming));
        OnPropertyChanged(nameof(ShowHighest));
    }

    public void DeleteReferenceLayer()
    {
        ReferenceTexture = null;
        isVisible = false;
        OnPropertyChanged(nameof(ReferenceTexture));
        OnPropertyChanged(nameof(ReferenceTransformMatrix));
        OnPropertyChanged(nameof(IsVisibleBindable));
    }
    
    public void TransformReferenceLayer(ShapeCorners shape)
    {
        referenceShape = shape;
        OnPropertyChanged(nameof(ReferenceShapeBindable));
        OnPropertyChanged(nameof(ReferenceTransformMatrix));
    }

    public void SetReferenceLayerIsVisible(bool isVisible)
    {
        this.isVisible = isVisible;
        OnPropertyChanged(nameof(IsVisibleBindable));
    }

    public void SetReferenceLayerTopMost(bool isTopMost)
    {
        this.isTopMost = isTopMost;
        OnPropertyChanged(nameof(IsTopMost));
        OnPropertyChanged(nameof(ShowHighest));
    }

    #endregion
}
