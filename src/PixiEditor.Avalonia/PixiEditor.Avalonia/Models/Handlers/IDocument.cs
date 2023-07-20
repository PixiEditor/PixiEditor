using System.Collections.Generic;
using Avalonia.Media.Imaging;
using ChunkyImageLib.DataHolders;
using PixiEditor.Avalonia.Helpers;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using PixiEditor.DrawingApi.Core.Surface.Vector;
using PixiEditor.Extensions.Palettes;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.DocumentModels.Public;
using PixiEditor.Models.Enums;

namespace PixiEditor.Models.Containers;

internal interface IDocument : IHandler
{
    public List<PaletteColor> Palette { get; set; }
    public VecI SizeBindable { get; set; }
    public IStructureMemberHandler? SelectedStructureMember { get; protected set; }
    public IReferenceLayerHandler ReferenceLayerHandler { get; }
    public VectorPath SelectionPathBindable { get; }
    public IFolderHandler StructureRoot { get; set; }
    public Dictionary<ChunkResolution, DrawingSurface> Surfaces { get; set; }
    public DocumentStructureModule StructureHelper { get; }
    public DrawingSurface PreviewSurface { get; set; }
    public bool AllChangesSaved { get; set; }
    public string CoordinatesString { get; set; }
    public IReadOnlyCollection<IStructureMemberHandler?> SoftSelectedStructureMembers { get; set; }
    public Dictionary<ChunkResolution, WriteableBitmap> LazyBitmaps { get; set; }
    public WriteableBitmap PreviewBitmap { get; set; }
    public ILayerHandlerFactory LayerHandlerFactory { get; }
    public IFolderHandlerFactory FolderHandlerFactory { get; set; }
    public ITransformHandler TransformHandler { get; }
    public bool Busy { get; set; }
    public ILineOverlayHandler LineToolOverlayHandler { get; set; }
    public bool HorizontalSymmetryAxisEnabledBindable { get; set; }
    public bool VerticalSymmetryAxisEnabledBindable { get; set; }
    public double HorizontalSymmetryAxisYBindable { get; set; }
    public double VerticalSymmetryAxisXBindable { get; set; }
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
