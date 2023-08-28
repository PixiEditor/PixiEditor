﻿using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Media.Imaging;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Helpers.Collections;
using PixiEditor.AvaloniaUI.Helpers.Extensions;
using PixiEditor.AvaloniaUI.Models.Controllers;
using PixiEditor.AvaloniaUI.Models.DocumentModels;
using PixiEditor.AvaloniaUI.Models.DocumentModels.Public;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.AvaloniaUI.Models.Position;
using PixiEditor.AvaloniaUI.Models.Structures;
using PixiEditor.AvaloniaUI.Models.Tools;
using PixiEditor.AvaloniaUI.ViewModels.Document.TransformOverlays;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Actions.Undo;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.Vector;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.Palettes;
using Color = PixiEditor.DrawingApi.Core.ColorsImpl.Color;
using Colors = PixiEditor.DrawingApi.Core.ColorsImpl.Colors;
using Point = Avalonia.Point;

namespace PixiEditor.AvaloniaUI.ViewModels.Document;

#nullable enable
internal partial class DocumentViewModel : ObservableObject, IDocument
{
    public event EventHandler<LayersChangedEventArgs>? LayersChanged;
    public event EventHandler<DocumentSizeChangedEventArgs>? SizeChanged;

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
            OnPropertyChanged(nameof(FileName));
        }
    }
    
    public string FileName
    {
        get => fullFilePath is null ? new LocalizedString("UNNAMED") : Path.GetFileName(fullFilePath);
    }

    private Guid? lastChangeOnSave = null;
    public bool AllChangesSaved
    {
        get
        {
            return Internals.Tracker.LastChangeGuid == lastChangeOnSave;
        }
    }

    public DateTime OpenedUTC { get; } = DateTime.UtcNow;

    private bool horizontalSymmetryAxisEnabled;
    public bool HorizontalSymmetryAxisEnabledBindable
    {
        get => horizontalSymmetryAxisEnabled;
        set
        {
            if (!Internals.ChangeController.IsChangeActive)
                Internals.ActionAccumulator.AddFinishedActions(new SymmetryAxisState_Action(SymmetryAxisDirection.Horizontal, value));
        }
    }

    private bool verticalSymmetryAxisEnabled;
    public bool VerticalSymmetryAxisEnabledBindable
    {
        get => verticalSymmetryAxisEnabled;
        set
        {
            if (!Internals.ChangeController.IsChangeActive)
                Internals.ActionAccumulator.AddFinishedActions(new SymmetryAxisState_Action(SymmetryAxisDirection.Vertical, value));
        }
    }

    private VecI size = new VecI(64, 64);
    public int Width => size.X;
    public int Height => size.Y;
    public VecI SizeBindable => size;

    private double horizontalSymmetryAxisY;
    public double HorizontalSymmetryAxisYBindable => horizontalSymmetryAxisY;

    private double verticalSymmetryAxisX;
    public double VerticalSymmetryAxisXBindable => verticalSymmetryAxisX;

    private readonly HashSet<StructureMemberViewModel> softSelectedStructureMembers = new();

    public bool UpdateableChangeActive => Internals.ChangeController.IsChangeActive;
    public bool HasSavedUndo => Internals.Tracker.HasSavedUndo;
    public bool HasSavedRedo => Internals.Tracker.HasSavedRedo;

    public FolderViewModel StructureRoot { get; }
    public DocumentStructureModule StructureHelper { get; }
    public DocumentToolsModule Tools { get; }
    public DocumentOperationsModule Operations { get; }
    public DocumentEventsModule EventInlet { get; }
    public ActionDisplayList ActionDisplays { get; } = new(() => ViewModelMain.Current.NotifyToolActionDisplayChanged());
    public IStructureMemberHandler? SelectedStructureMember { get; private set; } = null;

    public Dictionary<ChunkResolution, DrawingSurface> Surfaces { get; set; } = new();
    public Dictionary<ChunkResolution, WriteableBitmap> LazyBitmaps { get; set; } = new()
    {
        [ChunkResolution.Full] = WriteableBitmapHelpers.CreateBitmap(new VecI(64, 64)),
        [ChunkResolution.Half] = WriteableBitmapHelpers.CreateBitmap(new VecI(32, 32)),
        [ChunkResolution.Quarter] = WriteableBitmapHelpers.CreateBitmap(new VecI(16, 16)),
        [ChunkResolution.Eighth] = WriteableBitmapHelpers.CreateBitmap(new VecI(8, 8)),
    };
    public WriteableBitmap PreviewBitmap { get; set; }
    public DrawingSurface PreviewSurface { get; set; }

    private VectorPath selectionPath = new VectorPath();
    public VectorPath SelectionPathBindable => selectionPath;
    public ObservableCollection<PaletteColor> Swatches { get; set; } = new(); // TODO: Replaced WPFObservableCollection, make sure it works
    public ObservableRangeCollection<PaletteColor> Palette { get; set; } = new(); // TODO: Same
    public DocumentTransformViewModel TransformViewModel { get; }
    public ReferenceLayerViewModel ReferenceLayerViewModel { get; }
    public LineToolOverlayViewModel LineToolOverlayViewModel { get; }

    public IReadOnlyCollection<IStructureMemberHandler> SoftSelectedStructureMembers => softSelectedStructureMembers;
    private DocumentInternalParts Internals { get; }
    IFolderHandler IDocument.StructureRoot => StructureRoot;
    IDocumentOperations IDocument.Operations => Operations;
    ITransformHandler IDocument.TransformHandler => TransformViewModel;
    ILineOverlayHandler IDocument.LineToolOverlayHandler => LineToolOverlayViewModel;
    public ILayerHandlerFactory LayerHandlerFactory { get; }
    public IFolderHandlerFactory FolderHandlerFactory { get; }
    IReferenceLayerHandler IDocument.ReferenceLayerHandler => ReferenceLayerViewModel;


    private DocumentViewModel()
    {
        var serviceProvider = ViewModelMain.Current.Services;
        Internals = new DocumentInternalParts(this, serviceProvider);
        Tools = new DocumentToolsModule(this, Internals);
        StructureHelper = new DocumentStructureModule(this);
        EventInlet = new DocumentEventsModule(this, Internals);
        Operations = new DocumentOperationsModule(this, Internals);

        LayerHandlerFactory = new LayerHandlerFactory(this);
        FolderHandlerFactory = new FolderHandlerFactory(this);

        StructureRoot = new FolderViewModel(this, Internals, Internals.Tracker.Document.StructureRoot.GuidValue);

        TransformViewModel = new(this);
        TransformViewModel.TransformMoved += (_, args) => Internals.ChangeController.TransformMovedInlet(args);

        LineToolOverlayViewModel = new();
        LineToolOverlayViewModel.LineMoved += (_, args) => Internals.ChangeController.LineOverlayMovedInlet(args.Item1, args.Item2);

        foreach (KeyValuePair<ChunkResolution, WriteableBitmap> bitmap in LazyBitmaps)
        {
            DrawingSurface? surface = WriteableBitmapHelpers.CreateDrawingSurface(bitmap.Value);
            Surfaces[bitmap.Key] = surface;
        }

        VecI previewSize = StructureMemberViewModel.CalculatePreviewSize(SizeBindable);
        PreviewBitmap = WriteableBitmapHelpers.CreateBitmap(previewSize);
        PreviewSurface = WriteableBitmapHelpers.CreateDrawingSurface(PreviewBitmap);

        ReferenceLayerViewModel = new(this, Internals);
    }

    /// <summary>
    /// Creates a new document using the <paramref name="builder"/>
    /// </summary>
    /// <returns>The created document</returns>
    public static DocumentViewModel Build(Action<DocumentViewModelBuilder> builder)
    {
        var builderInstance = new DocumentViewModelBuilder();
        builder(builderInstance);

        var viewModel = new DocumentViewModel();
        viewModel.Operations.ResizeCanvas(new VecI(builderInstance.Width, builderInstance.Height), ResizeAnchor.Center);

        var acc = viewModel.Internals.ActionAccumulator;

        viewModel.Internals.ChangeController.SymmetryDraggedInlet(new SymmetryAxisDragInfo(SymmetryAxisDirection.Horizontal, builderInstance.Height / 2));
        viewModel.Internals.ChangeController.SymmetryDraggedInlet(new SymmetryAxisDragInfo(SymmetryAxisDirection.Vertical, builderInstance.Width / 2));

        acc.AddActions(
            new SymmetryAxisPosition_Action(SymmetryAxisDirection.Horizontal, (double)builderInstance.Height / 2),
            new EndSymmetryAxisPosition_Action(),
            new SymmetryAxisPosition_Action(SymmetryAxisDirection.Vertical, (double)builderInstance.Width / 2),
            new EndSymmetryAxisPosition_Action());

        if (builderInstance.ReferenceLayer is { } refLayer)
        {
            acc
                .AddActions(new SetReferenceLayer_Action(refLayer.Shape, refLayer.ImagePbgra32Bytes.ToImmutableArray(), refLayer.ImageSize));
        }

        viewModel.Swatches = new ObservableCollection<PaletteColor>(builderInstance.Swatches);
        viewModel.Palette = new ObservableRangeCollection<PaletteColor>(builderInstance.Palette);

        AddMembers(viewModel.StructureRoot.GuidValue, builderInstance.Children);

        acc.AddFinishedActions(new DeleteRecordedChanges_Action());
        viewModel.MarkAsSaved();

        return viewModel;

        void AddMember(Guid parentGuid, DocumentViewModelBuilder.StructureMemberBuilder member)
        {
            acc.AddActions(
                new CreateStructureMember_Action(parentGuid, member.GuidValue, 0, member is DocumentViewModelBuilder.LayerBuilder ? StructureMemberType.Layer : StructureMemberType.Folder),
                new StructureMemberName_Action(member.GuidValue, member.Name)
            );

            if (!member.IsVisible)
                acc.AddActions(new StructureMemberIsVisible_Action(member.IsVisible, member.GuidValue));
            
            acc.AddActions(new StructureMemberBlendMode_Action(member.BlendMode, member.GuidValue));
            
            acc.AddActions(new StructureMemberClipToMemberBelow_Action(member.ClipToMemberBelow, member.GuidValue));

            if (member is DocumentViewModelBuilder.LayerBuilder layerBuilder)
            {
                acc.AddActions(new LayerLockTransparency_Action(layerBuilder.GuidValue, layerBuilder.LockAlpha));
            }

            if (member is DocumentViewModelBuilder.LayerBuilder layer && layer.Surface is not null)
            {
                PasteImage(member.GuidValue, layer.Surface, layer.Width, layer.Height, layer.OffsetX, layer.OffsetY, false);
            }
            
            acc.AddActions(
                new StructureMemberOpacity_Action(member.GuidValue, member.Opacity),
                new EndStructureMemberOpacity_Action());

            if (member.HasMask)
            {
                var maskSurface = member.Mask.Surface.Surface;

                acc.AddActions(new CreateStructureMemberMask_Action(member.GuidValue));

                if (!member.Mask.IsVisible)
                    acc.AddActions(new StructureMemberMaskIsVisible_Action(member.Mask.IsVisible, member.GuidValue));

                PasteImage(member.GuidValue, member.Mask.Surface, maskSurface.Size.X, maskSurface.Size.Y, 0, 0, true);
            }

            acc.AddFinishedActions();

            if (member is DocumentViewModelBuilder.FolderBuilder { Children: not null } folder)
            {
                AddMembers(member.GuidValue, folder.Children);
            }
        }

        void PasteImage(Guid guid, DocumentViewModelBuilder.SurfaceBuilder surface, int width, int height, int offsetX, int offsetY, bool onMask)
        {
            acc.AddActions(
                new PasteImage_Action(surface.Surface, new(new RectD(new VecD(offsetX, offsetY), new(width, height))), guid, true, onMask),
                new EndPasteImage_Action());
        }

        void AddMembers(Guid parentGuid, IEnumerable<DocumentViewModelBuilder.StructureMemberBuilder> builders)
        {
            foreach (var child in builders.Reverse())
            {
                if (child.GuidValue == default)
                {
                    child.GuidValue = Guid.NewGuid();
                }

                AddMember(parentGuid, child);
            }
        }
    }

    public void MarkAsSaved()
    {
        lastChangeOnSave = Internals.Tracker.LastChangeGuid;
        OnPropertyChanged(nameof(AllChangesSaved));
    }

    public void MarkAsUnsaved()
    {
        lastChangeOnSave = Guid.NewGuid();
        OnPropertyChanged(nameof(AllChangesSaved));
    }

    /// <summary>
    /// Tries rendering the whole document
    /// </summary>
    /// <returns><see cref="Error"/> if the ChunkyImage was disposed, otherwise a <see cref="Surface"/> of the rendered document</returns>
    public OneOf<Error, Surface> MaybeRenderWholeImage()
    {
        try
        {
            Surface finalSurface = new Surface(SizeBindable);
            VecI sizeInChunks = (VecI)((VecD)SizeBindable / ChunkyImage.FullChunkSize).Ceiling();
            for (int i = 0; i < sizeInChunks.X; i++)
            {
                for (int j = 0; j < sizeInChunks.Y; j++)
                {
                    var maybeChunk = ChunkRenderer.MergeWholeStructure(new(i, j), ChunkResolution.Full, Internals.Tracker.Document.StructureRoot);
                    if (maybeChunk.IsT1)
                        continue;
                    using Chunk chunk = maybeChunk.AsT0;
                    finalSurface.DrawingSurface.Canvas.DrawSurface(chunk.Surface.DrawingSurface, i * ChunkyImage.FullChunkSize, j * ChunkyImage.FullChunkSize);
                } 
            }
            return finalSurface;
        }
        catch (ObjectDisposedException)
        {
            return new Error();
        }
    }

    /// <summary>
    /// Takes the selected area and converts it into a surface
    /// </summary>
    /// <returns><see cref="Error"/> on error, <see cref="None"/> for empty <see cref="Surface"/>, <see cref="Surface"/> otherwise.</returns>
    public OneOf<Error, None, (Surface, RectI)> MaybeExtractSelectedArea(IStructureMemberHandler? layerToExtractFrom = null)
    {
        layerToExtractFrom ??= SelectedStructureMember;
        if (layerToExtractFrom is null || layerToExtractFrom is not LayerViewModel layerVm)
            return new Error();
        if (SelectionPathBindable.IsEmpty)
            return new None();

        IReadOnlyLayer? layer = (IReadOnlyLayer?)Internals.Tracker.Document.FindMember(layerVm.GuidValue);
        if (layer is null)
            return new Error();

        RectI bounds = (RectI)SelectionPathBindable.TightBounds;
        RectI? memberImageBounds;
        try
        {
            memberImageBounds = layer.LayerImage.FindChunkAlignedMostUpToDateBounds();
        }
        catch (ObjectDisposedException)
        {
            return new Error();
        }
        if (memberImageBounds is null)
            return new None();
        bounds = bounds.Intersect(memberImageBounds.Value);
        bounds = bounds.Intersect(new RectI(VecI.Zero, SizeBindable));
        if (bounds.IsZeroOrNegativeArea)
            return new None();

        Surface output = new(bounds.Size);

        VectorPath clipPath = new VectorPath(SelectionPathBindable) { FillType = PathFillType.EvenOdd };
        clipPath.Transform(Matrix3X3.CreateTranslation(-bounds.X, -bounds.Y));
        output.DrawingSurface.Canvas.Save();
        output.DrawingSurface.Canvas.ClipPath(clipPath);
        try
        {
            layer.LayerImage.DrawMostUpToDateRegionOn(bounds, ChunkResolution.Full, output.DrawingSurface, VecI.Zero);
        }
        catch (ObjectDisposedException)
        {
            output.Dispose();
            return new Error();
        }
        output.DrawingSurface.Canvas.Restore();

        return (output, bounds);
    }

    /// <summary>
    /// Picks the color at <paramref name="pos"/>
    /// </summary>
    /// <param name="includeReference">Should the color be picked from the reference layer</param>
    /// <param name="includeCanvas">Should the color be picked from the canvas</param>
    /// <param name="referenceTopmost">Is the reference layer topmost. (Only affects the result is includeReference and includeCanvas are set.)</param>
    public Color PickColor(VecD pos, DocumentScope scope, bool includeReference, bool includeCanvas, bool referenceTopmost = false)
    {
        if (scope == DocumentScope.SingleLayer && includeReference && includeCanvas)
            includeReference = false;

        if (includeCanvas && includeReference)
        {
            Color canvasColor = PickColorFromCanvas((VecI)pos, scope);
            Color? potentialReferenceColor = PickColorFromReferenceLayer(pos);
            if (potentialReferenceColor is not { } referenceColor)
                return canvasColor;

            if (!referenceTopmost)
            {
                return ColorHelpers.BlendColors(referenceColor, canvasColor);
            }

            byte referenceAlpha = canvasColor.A == 0 ? referenceColor.A : (byte)(referenceColor.A * ReferenceLayerViewModel.TopMostOpacity);

            referenceColor = new Color(referenceColor.R, referenceColor.G, referenceColor.B, referenceAlpha);
            return ColorHelpers.BlendColors(canvasColor, referenceColor);

        }
        if (includeCanvas)
            return PickColorFromCanvas((VecI)pos, scope);
        if (includeReference)
            return PickColorFromReferenceLayer(pos) ?? Colors.Transparent;
        return Colors.Transparent;
    }

    public Color? PickColorFromReferenceLayer(VecD pos)
    {
        WriteableBitmap? bitmap = ReferenceLayerViewModel.ReferenceBitmap; 
        if (bitmap is null)
            return null;
        
        Matrix matrix = ReferenceLayerViewModel.ReferenceTransformMatrix;
        matrix.Invert();
        var transformed = matrix.Transform(new Point(pos.X, pos.Y));

        if (transformed.X < 0 || transformed.Y < 0 || transformed.X >= bitmap.PixelSize.Width || transformed.Y >= bitmap.PixelSize.Height)
            return null;

        using var frameBuffer = bitmap.Lock();
        return frameBuffer.GetPixel((int)transformed.X, (int)transformed.Y).ToColor();
    }

    public Color PickColorFromCanvas(VecI pos, DocumentScope scope)
    {
        // there is a tiny chance that the image might get disposed by another thread
        try
        {
            // it might've been a better idea to implement this function asynchronously
            // via a passthrough action to avoid all the try catches
            if (scope == DocumentScope.AllLayers)
            {
                VecI chunkPos = OperationHelper.GetChunkPos(pos, ChunkyImage.FullChunkSize);
                return ChunkRenderer.MergeWholeStructure(chunkPos, ChunkResolution.Full, Internals.Tracker.Document.StructureRoot, new RectI(pos, VecI.One))
                    .Match<Color>(
                        (Chunk chunk) =>
                        {
                            VecI posOnChunk = pos - chunkPos * ChunkyImage.FullChunkSize;
                            var color = chunk.Surface.GetSRGBPixel(posOnChunk);
                            chunk.Dispose();
                            return color;
                        },
                        _ => Colors.Transparent
                    );
            }

            if (SelectedStructureMember is not LayerViewModel layerVm)
                return Colors.Transparent;
            IReadOnlyStructureMember? maybeMember = Internals.Tracker.Document.FindMember(layerVm.GuidValue);
            if (maybeMember is not IReadOnlyLayer layer)
                return Colors.Transparent;
            return layer.LayerImage.GetMostUpToDatePixel(pos);
        }
        catch (ObjectDisposedException)
        {
            return Colors.Transparent;
        }
    }

    #region Internal Methods
    // these are intended to only be called from DocumentUpdater

    public void RaiseLayersChanged(LayersChangedEventArgs args) => LayersChanged?.Invoke(this, args);

    public void RaiseSizeChanged(DocumentSizeChangedEventArgs args) => SizeChanged?.Invoke(this, args);

    public void ISetVerticalSymmetryAxisEnabled(bool verticalSymmetryAxisEnabled)
    {
        this.verticalSymmetryAxisEnabled = verticalSymmetryAxisEnabled;
        OnPropertyChanged(nameof(VerticalSymmetryAxisEnabledBindable));
    }

    public void SetHorizontalSymmetryAxisEnabled(bool horizontalSymmetryAxisEnabled)
    {
        this.horizontalSymmetryAxisEnabled = horizontalSymmetryAxisEnabled;
        OnPropertyChanged(nameof(HorizontalSymmetryAxisEnabledBindable));
    }

    public void SetVerticalSymmetryAxisEnabled(bool infoState)
    {
        throw new NotImplementedException();
    }

    public void SetVerticalSymmetryAxisX(double verticalSymmetryAxisX)
    {
        this.verticalSymmetryAxisX = verticalSymmetryAxisX;
        OnPropertyChanged(nameof(VerticalSymmetryAxisXBindable));
    }

    public void SetSelectedMember(IStructureMemberHandler member) =>
        SetSelectedMember((StructureMemberViewModel)member);

    public void SetHorizontalSymmetryAxisY(double horizontalSymmetryAxisY)
    {
        this.horizontalSymmetryAxisY = horizontalSymmetryAxisY;
        OnPropertyChanged(nameof(HorizontalSymmetryAxisYBindable));
    }

    public void SetSize(VecI size)
    {
        this.size = size;
        OnPropertyChanged(nameof(SizeBindable));
        OnPropertyChanged(nameof(Width));
        OnPropertyChanged(nameof(Height));
    }

    public void UpdateSelectionPath(VectorPath vectorPath)
    {
        (VectorPath? toDispose, this.selectionPath) = (this.selectionPath, vectorPath);
        toDispose.Dispose();
        OnPropertyChanged(nameof(SelectionPathBindable));
    }

    public void SetSelectedMember(StructureMemberViewModel? member)
    {
        SelectedStructureMember = member;
        OnPropertyChanged(nameof(SelectedStructureMember));
    }

    public void RemoveSoftSelectedMember(IStructureMemberHandler member)
    {
        SelectedStructureMember = member;
    }

    public void ClearSoftSelectedMembers() => softSelectedStructureMembers.Clear();
    public void AddSoftSelectedMember(IStructureMemberHandler member) => softSelectedStructureMembers.Add((StructureMemberViewModel)member);
    public void RemoveSoftSelectedMember(StructureMemberViewModel member) => softSelectedStructureMembers.Remove(member);
    #endregion

    /// <summary>
    /// Returns a list of all selected members (Hard and Soft selected)
    /// </summary>
    public List<Guid> GetSelectedMembers()
    {
        List<Guid> layerGuids = new List<Guid>();
        if (SelectedStructureMember is not null)
            layerGuids.Add(SelectedStructureMember.GuidValue);

        layerGuids.AddRange(softSelectedStructureMembers.Select(x => x.GuidValue));
        return layerGuids;
    }

    public List<Guid> ExtractSelectedLayers(bool includeFoldersWithMask = false)
    {
        var result = new List<Guid>();
        List<Guid> selectedMembers = GetSelectedMembers();
        foreach (var member in selectedMembers)
        {
            var foundMember = StructureHelper.Find(member);
            if (foundMember != null)
            {
                if (foundMember is LayerViewModel layer && selectedMembers.Contains(foundMember.GuidValue) && !result.Contains(layer.GuidValue))
                {
                    result.Add(layer.GuidValue);
                }
                else if (foundMember is FolderViewModel folder && selectedMembers.Contains(foundMember.GuidValue))
                {
                    if (includeFoldersWithMask && folder.HasMaskBindable && !result.Contains(folder.GuidValue))
                        result.Add(folder.GuidValue);
                    ExtractSelectedLayers(folder, result, includeFoldersWithMask);
                }
            }
        }
        return result;
    }

    private void ExtractSelectedLayers(FolderViewModel folder, List<Guid> list,
        bool includeFoldersWithMask)
    {
        foreach (var member in folder.Children)
        {
            if (member is LayerViewModel layer && !list.Contains(layer.GuidValue))
            {
                list.Add(layer.GuidValue);
            }
            else if (member is FolderViewModel childFolder)
            {
                if (includeFoldersWithMask && childFolder.HasMaskBindable && !list.Contains(childFolder.GuidValue))
                    list.Add(childFolder.GuidValue);

                ExtractSelectedLayers(childFolder, list, includeFoldersWithMask);
            }
        }
    }
}