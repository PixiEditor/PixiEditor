using System.Collections.Generic;
using Avalonia.Media.Imaging;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Vector;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Helpers;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DocumentModels.Public;
using PixiEditor.Models.Rendering;
using PixiEditor.Models.Structures;
using PixiEditor.Models.Tools;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.Models.DocumentPassthroughActions;
using PixiEditor.Parser;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Models.Handlers;

internal interface IDocument : IHandler, Extensions.CommonApi.Documents.IDocument, IDisposable
{
    public Guid Id { get; }
    public string FullFilePath { get; }
    public ObservableRangeCollection<PaletteColor> Palette { get; set; }
    public VecI SizeBindable { get; }
    public IStructureMemberHandler? SelectedStructureMember { get; }
    public IReferenceLayerHandler ReferenceLayerHandler { get; }
    public IAnimationHandler AnimationHandler { get; }
    public VectorPath SelectionPathBindable { get; }
    public INodeGraphHandler NodeGraphHandler { get; }
    public DocumentStructureModule StructureHelper { get; }
    public bool AllChangesSaved { get; }
    public string CoordinatesString { get; set; }
    public IReadOnlyCollection<IStructureMemberHandler> SoftSelectedStructureMembers { get; }
    public ITransformHandler TransformHandler { get; }
    public IPathOverlayHandler PathOverlayHandler { get; }
    public ITextOverlayHandler TextOverlayHandler { get; }
    public bool Busy { get; set; }
    public ILineOverlayHandler LineToolOverlayHandler { get; }
    public bool HorizontalSymmetryAxisEnabledBindable { get; }
    public bool VerticalSymmetryAxisEnabledBindable { get; }
    public double HorizontalSymmetryAxisYBindable { get; }
    public double VerticalSymmetryAxisXBindable { get; }
    public IDocumentOperations Operations { get; }
    public DocumentRenderer Renderer { get; }
    public ISnappingHandler SnappingHandler { get; }
    public IReadOnlyCollection<Guid> SelectedMembers { get; }
    public Dictionary<Guid, Texture> SceneTextures { get; }
    public SceneRenderer SceneRenderer { get; }
    public ColorSpace ProcessingColorSpace { get; }
    public bool IsDisposed { get; }
    public void RemoveSoftSelectedMember(IStructureMemberHandler member);
    public void ClearSoftSelectedMembers();
    public void AddSoftSelectedMember(IStructureMemberHandler member);
    public void SetSelectedMember(IStructureMemberHandler member);
    public void SetHorizontalSymmetryAxisY(double infoNewPosition);
    public void SetVerticalSymmetryAxisX(double infoNewPosition);
    public void SetHorizontalSymmetryAxisEnabled(bool infoState);
    public void SetVerticalSymmetryAxisEnabled(bool infoState);
    public void UpdateSelectionPath(VectorPath infoNewPath);
    public void SetProcessingColorSpace(ColorSpace infoColorSpace);
    public void SetSize(VecI infoSize);

    public Color PickColor(VecD controllerLastPrecisePosition, DocumentScope scope, bool includeReference,
        bool includeCanvas, int frame, bool isTopMost, string? customOutput);

    public HashSet<Guid> ExtractSelectedLayers(bool includeFoldersWithMask = false);
    public List<Guid> GetSelectedMembersInOrder(bool includeNested = false);
    public void UpdateSavedState();

    internal void InternalRaiseLayersChanged(LayersChangedEventArgs e);
    internal void InternalMarkSaveState(DocumentMarkType type);
    internal void InternalRaiseKeyFrameCreated(RasterCelViewModel vm);
    public ICrossDocumentPipe<T> ShareNode<T>(Guid layerId) where T : class, IReadOnlyNode;
    public ICrossDocumentPipe<IReadOnlyNodeGraph> ShareGraph();
    public IReadOnlyDocument AccessInternalReadOnlyDocument();
    public bool IsNestedDocument { get; }
}
