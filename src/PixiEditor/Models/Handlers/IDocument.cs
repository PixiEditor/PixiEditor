using System.Collections.Generic;
using Avalonia.Media.Imaging;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.DrawingApi.Core;
using PixiEditor.DrawingApi.Core.ColorsImpl;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surfaces.Vector;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Helpers;
using PixiEditor.Models.DocumentModels.Public;
using PixiEditor.Models.Structures;
using PixiEditor.Models.Tools;
using PixiEditor.Numerics;

namespace PixiEditor.Models.Handlers;

internal interface IDocument : IHandler
{
    public ObservableRangeCollection<PaletteColor> Palette { get; set; }
    public VecI SizeBindable { get; }
    public IStructureMemberHandler? SelectedStructureMember { get; }
    public IReferenceLayerHandler ReferenceLayerHandler { get; }
    public IAnimationHandler AnimationHandler { get; }
    public VectorPath SelectionPathBindable { get; }
    public INodeGraphHandler NodeGraphHandler { get; }
    public Dictionary<ChunkResolution, Surface> Surfaces { get; set; }
    public DocumentStructureModule StructureHelper { get; }
    public Surface PreviewSurface { get; set; }
    public bool AllChangesSaved { get; }
    public string CoordinatesString { get; set; }
    public IReadOnlyCollection<IStructureMemberHandler> SoftSelectedStructureMembers { get; }
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
    public DocumentRenderer Renderer { get; }
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
    public Color PickColor(VecD controllerLastPrecisePosition, DocumentScope scope, bool includeReference, bool includeCanvas, int frame, bool isTopMost);
    public List<Guid> ExtractSelectedLayers(bool includeFoldersWithMask = false);
    public void UpdateSavedState();
}
