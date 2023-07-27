using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Media.Imaging;
using ChunkyImageLib.DataHolders;
using PixiEditor.Avalonia.Helpers;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.Vector;
using PixiEditor.Extensions.Palettes;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.DocumentModels.Public;
using PixiEditor.Models.Enums;

namespace PixiEditor.Models.Containers;

internal interface IDocument : IHandler
{
    public ObservableRangeCollection<PaletteColor> Palette { get; set; }
    public VecI SizeBindable { get; }
    public IStructureMemberHandler? SelectedStructureMember { get; }
    public IReferenceLayerHandler ReferenceLayerHandler { get; }
    public VectorPath SelectionPathBindable { get; }
    public IFolderHandler StructureRoot { get; }
    public Dictionary<ChunkResolution, DrawingSurface> Surfaces { get; set; }
    public DocumentStructureModule StructureHelper { get; }
    public DrawingSurface PreviewSurface { get; set; }
    public bool AllChangesSaved { get; }
    public string CoordinatesString { get; set; }
    public IReadOnlyCollection<IStructureMemberHandler> SoftSelectedStructureMembers { get; }
    public Dictionary<ChunkResolution, WriteableBitmap> LazyBitmaps { get; set; }
    public WriteableBitmap PreviewBitmap { get; set; }
    public ILayerHandlerFactory LayerHandlerFactory { get; }
    public IFolderHandlerFactory FolderHandlerFactory { get; }
    public ITransformHandler TransformHandler { get; }
    public bool Busy { get; set; }
    public ILineOverlayHandler LineToolOverlayHandler { get; }
    public bool HorizontalSymmetryAxisEnabledBindable { get; }
    public bool VerticalSymmetryAxisEnabledBindable { get; }
    public double HorizontalSymmetryAxisYBindable { get; }
    public double VerticalSymmetryAxisXBindable { get; }
    public IDocumentOperations Operations { get; }
    public void RemoveSoftSelectedMember(IStructureMemberHandler member);
    public void ClearSoftSelectedMembers();
    public void AddSoftSelectedMember(IStructureMemberHandler member);
    public void SetSelectedMember(IStructureMemberHandler member);
    public void SetHorizontalSymmetryAxisY(double infoNewPosition);
    public void SetVerticalSymmetryAxisX(double infoNewPosition);
    public void SetHorizontalSymmetryAxisEnabled(bool infoState);
    public void SetVerticalSymmetryAxisEnabled(bool infoState);
    public void UpdateSelectionPath(VectorPath infoNewPath);
    public void SetSize(VecI infoSize);
    public Color PickColor(VecD controllerLastPrecisePosition, DocumentScope scope, bool includeReference, bool includeCanvas, bool isTopMost);
    public List<Guid> ExtractSelectedLayers(bool includeFoldersWithMask = false);
}
