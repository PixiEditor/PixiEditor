using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using ChunkyImageLib.Operations;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Models.DocumentPassthroughActions;
using PixiEditor.Models.Position;
using PixiEditor.ViewModels.SubViewModels;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Actions.Undo;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditor.ChangeableDocument.Changes.NodeGraph;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Helpers;
using PixiEditor.Helpers.Collections;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.DocumentModels.Public;
using PixiEditor.Models.DocumentModels.UpdateableChangeExecutors.Features;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Rendering;
using PixiEditor.Models.Serialization;
using PixiEditor.Models.Serialization.Factories;
using PixiEditor.Models.Structures;
using PixiEditor.Models.Tools;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.Models.IO;
using PixiEditor.Parser;
using PixiEditor.Parser.Skia;
using PixiEditor.ViewModels.Document.Nodes;
using PixiEditor.ViewModels.Document.TransformOverlays;
using PixiEditor.Views.Overlays.SymmetryOverlay;
using BlendMode = Drawie.Backend.Core.Surfaces.BlendMode;
using Color = Drawie.Backend.Core.ColorsImpl.Color;
using Colors = Drawie.Backend.Core.ColorsImpl.Colors;
using Node = PixiEditor.Parser.Graph.Node;
using Point = Avalonia.Point;

namespace PixiEditor.ViewModels.Document;

#nullable enable
internal partial class DocumentViewModel : PixiObservableObject, IDocument
{
    public event EventHandler<LayersChangedEventArgs>? LayersChanged;
    public event EventHandler<DocumentSizeChangedEventArgs>? SizeChanged;
    public event Action ToolSessionFinished;

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
    private Guid? lastChangeOnAutosave = null;

    public bool AllChangesSaved
    {
        get
        {
            return Internals.Tracker.LastChangeGuid == lastChangeOnSave;
        }
    }

    public bool AllChangesAutosaved
    {
        get
        {
            return Internals.Tracker.LastChangeGuid == lastChangeOnAutosave;
        }
    }

    public DateTime OpenedUTC { get; } = DateTime.UtcNow;

    private bool horizontalSymmetryAxisEnabled;

    public bool HorizontalSymmetryAxisEnabledBindable
    {
        get => horizontalSymmetryAxisEnabled;
        set
        {
            if (!Internals.ChangeController.IsBlockingChangeActive)
                Internals.ActionAccumulator.AddFinishedActions(
                    new SymmetryAxisState_Action(SymmetryAxisDirection.Horizontal, value));
        }
    }

    private bool verticalSymmetryAxisEnabled;

    public bool VerticalSymmetryAxisEnabledBindable
    {
        get => verticalSymmetryAxisEnabled;
        set
        {
            if (!Internals.ChangeController.IsBlockingChangeActive)
                Internals.ActionAccumulator.AddFinishedActions(
                    new SymmetryAxisState_Action(SymmetryAxisDirection.Vertical, value));
        }
    }

    public bool AnySymmetryAxisEnabledBindable =>
        HorizontalSymmetryAxisEnabledBindable || VerticalSymmetryAxisEnabledBindable;


    public bool OverlayEventsSuppressed => overlaySuppressors.Count > 0;

    private readonly HashSet<string> overlaySuppressors = new();

    private VecI size = new VecI(64, 64);
    public int Width => size.X;
    public int Height => size.Y;
    public VecI SizeBindable => size;

    private double horizontalSymmetryAxisY;
    public double HorizontalSymmetryAxisYBindable => horizontalSymmetryAxisY;

    private double verticalSymmetryAxisX;
    public double VerticalSymmetryAxisXBindable => verticalSymmetryAxisX;

    private readonly HashSet<IStructureMemberHandler> softSelectedStructureMembers = new();

    public bool BlockingUpdateableChangeActive => Internals.ChangeController.IsBlockingChangeActive;

    public bool IsChangeFeatureActive<T>() where T : IExecutorFeature =>
        Internals.ChangeController.IsChangeOfTypeActive<T>();

    public T? TryGetExecutorFeature<T>() where T : IExecutorFeature =>
        Internals.ChangeController.TryGetExecutorFeature<T>();

    public bool PointerDragChangeInProgress =>
        Internals.ChangeController.IsBlockingChangeActive && Internals.ChangeController.LeftMousePressed;

    public bool HasSavedUndo => Internals.Tracker.HasSavedUndo;
    public bool HasSavedRedo => Internals.Tracker.HasSavedRedo;

    public NodeGraphViewModel NodeGraph { get; }
    public DocumentStructureModule StructureHelper { get; }
    public DocumentToolsModule Tools { get; }
    public DocumentOperationsModule Operations { get; }
    public DocumentRenderer Renderer { get; }
    public SceneRenderer SceneRenderer { get; }
    public DocumentEventsModule EventInlet { get; }

    public ActionDisplayList ActionDisplays { get; } =
        new(() => ViewModelMain.Current.NotifyToolActionDisplayChanged());

    public IStructureMemberHandler? SelectedStructureMember { get; private set; } = null;


    private PreviewPainter previewSurface;

    public PreviewPainter PreviewPainter
    {
        get => previewSurface;
        set
        {
            SetProperty(ref previewSurface, value);
        }
    }

    private VectorPath selectionPath = new VectorPath();
    public VectorPath SelectionPathBindable => selectionPath;
    public ObservableCollection<PaletteColor> Swatches { get; set; } = new();
    public Guid Id => Internals.Tracker.Document.DocumentId;
    public ObservableRangeCollection<PaletteColor> Palette { get; set; } = new();
    public SnappingViewModel SnappingViewModel { get; }
    ISnappingHandler IDocument.SnappingHandler => SnappingViewModel;
    public IReadOnlyCollection<Guid> SelectedMembers => GetSelectedMembers().AsReadOnly();
    public DocumentTransformViewModel TransformViewModel { get; }
    public PathOverlayViewModel PathOverlayViewModel { get; }
    public ReferenceLayerViewModel ReferenceLayerViewModel { get; }
    public LineToolOverlayViewModel LineToolOverlayViewModel { get; }
    public AnimationDataViewModel AnimationDataViewModel { get; }
    public TextOverlayViewModel TextOverlayViewModel { get; }


    public IReadOnlyCollection<IStructureMemberHandler> SoftSelectedStructureMembers => softSelectedStructureMembers;
    private DocumentInternalParts Internals { get; }
    INodeGraphHandler IDocument.NodeGraphHandler => NodeGraph;
    IDocumentOperations IDocument.Operations => Operations;
    ITransformHandler IDocument.TransformHandler => TransformViewModel;
    ITextOverlayHandler IDocument.TextOverlayHandler => TextOverlayViewModel;
    IPathOverlayHandler IDocument.PathOverlayHandler => PathOverlayViewModel;
    ILineOverlayHandler IDocument.LineToolOverlayHandler => LineToolOverlayViewModel;
    IReferenceLayerHandler IDocument.ReferenceLayerHandler => ReferenceLayerViewModel;
    IAnimationHandler IDocument.AnimationHandler => AnimationDataViewModel;
    public bool UsesSrgbBlending { get; private set; }
    public AutosaveDocumentViewModel AutosaveViewModel { get; }

    private DocumentViewModel()
    {
        var serviceProvider = ViewModelMain.Current.Services;
        Internals = new DocumentInternalParts(this, serviceProvider);
        Internals.ChangeController.ToolSessionFinished += () => ToolSessionFinished?.Invoke();

        Tools = new DocumentToolsModule(this, Internals);
        StructureHelper = new DocumentStructureModule(this);
        EventInlet = new DocumentEventsModule(this, Internals);
        Operations = new DocumentOperationsModule(this, Internals);

        AnimationDataViewModel = new(this, Internals);

        NodeGraph = new NodeGraphViewModel(this, Internals);
        AutosaveViewModel = new AutosaveDocumentViewModel(this, Internals);

        TransformViewModel = new(this);
        TransformViewModel.TransformChanged += (args) => Internals.ChangeController.TransformChangedInlet(args);
        TransformViewModel.TransformDragged += (from, to) => Internals.ChangeController.TransformDraggedInlet(from, to);
        TransformViewModel.TransformStopped += () => Internals.ChangeController.TransformStoppedInlet();

        PathOverlayViewModel = new(this, Internals);
        PathOverlayViewModel.PathChanged += path =>
        {
            Internals.ChangeController.PathOverlayChangedInlet(path);
        };

        LineToolOverlayViewModel = new();
        LineToolOverlayViewModel.LineMoved += (_, args) =>
            Internals.ChangeController.LineOverlayMovedInlet(args.Item1, args.Item2);

        TextOverlayViewModel = new TextOverlayViewModel();
        TextOverlayViewModel.TextChanged += text =>
        {
            Internals.ChangeController.TextOverlayTextChangedInlet(text);
        };

        SnappingViewModel = new();
        SnappingViewModel.AddFromDocumentSize(SizeBindable);
        SizeChanged += (_, args) =>
        {
            SnappingViewModel.AddFromDocumentSize(args.NewSize);
        };
        LayersChanged += (sender, args) =>
        {
            if (args.LayerChangeType == LayerAction.Add)
            {
                IReadOnlyStructureNode layer = Internals.Tracker.Document.FindMember(args.LayerAffectedGuid);
                if (layer is not null)
                {
                    SnappingViewModel.AddFromBounds(layer.Id.ToString(),
                        () => layer.GetTightBounds(AnimationDataViewModel.ActiveFrameTime) ?? RectD.Empty);
                }
            }
            else if (args.LayerChangeType == LayerAction.Remove)
            {
                SnappingViewModel.Remove(args.LayerAffectedGuid.ToString());
            }
        };

        ReferenceLayerViewModel = new(this, Internals);

        Renderer = new DocumentRenderer(Internals.Tracker.Document);
        SceneRenderer = new SceneRenderer(Internals.Tracker.Document, this);
    }

    /// <summary>
    /// Creates a new document using the <paramref name="builder"/>
    /// </summary>
    /// <returns>The created document</returns>
    public static DocumentViewModel Build(Action<DocumentViewModelBuilder> builder)
    {
        var builderInstance = new DocumentViewModelBuilder();
        builder(builderInstance);

        (string serializerName, string serializerVersion) serializerData = (builderInstance.SerializerName,
            builderInstance.SerializerVersion);

        Dictionary<int, Guid> mappedNodeIds = new();
        Dictionary<int, Guid> mappedKeyFrameIds = new();

        ResourceStorageLocator? resourceLocator = null;
        if (builderInstance.DocumentResources != null)
        {
            resourceLocator = ExtractResources(builderInstance.DocumentResources);
        }

        var viewModel = new DocumentViewModel();
        viewModel.Operations.ResizeCanvas(new VecI(builderInstance.Width, builderInstance.Height), ResizeAnchor.Center);

        var acc = viewModel.Internals.ActionAccumulator;

        ColorSpace targetProcessingColorSpace = ColorSpace.CreateSrgbLinear();
        if (builderInstance.UsesSrgbColorBlending ||
            IsFileWithSrgbColorBlending(serializerData, builderInstance.PixiParserVersionUsed))
        {
            targetProcessingColorSpace = ColorSpace.CreateSrgb();
            viewModel.Internals.Tracker.Document.InitProcessingColorSpace(ColorSpace.CreateSrgb());
            viewModel.UsesSrgbBlending = true;
        }

        viewModel.Internals.ChangeController.SymmetryDraggedInlet(
            new SymmetryAxisDragInfo(SymmetryAxisDirection.Horizontal, builderInstance.Height / 2));
        viewModel.Internals.ChangeController.SymmetryDraggedInlet(
            new SymmetryAxisDragInfo(SymmetryAxisDirection.Vertical, builderInstance.Width / 2));

        acc.AddActions(
            new SymmetryAxisPosition_Action(SymmetryAxisDirection.Horizontal, (double)builderInstance.Height / 2),
            new EndSymmetryAxisPosition_Action(),
            new SymmetryAxisPosition_Action(SymmetryAxisDirection.Vertical, (double)builderInstance.Width / 2),
            new EndSymmetryAxisPosition_Action());

        if (builderInstance.ReferenceLayer is { } refLayer)
        {
            acc.AddActions(new SetReferenceLayer_Action(refLayer.Shape, refLayer.ImageBgra8888Bytes.ToImmutableArray(),
                refLayer.ImageSize));
        }

        viewModel.Swatches = new ObservableCollection<PaletteColor>(builderInstance.Swatches);
        viewModel.Palette = new ObservableRangeCollection<PaletteColor>(builderInstance.Palette);

        SerializationConfig config =
            new SerializationConfig(BuiltInEncoders.Encoders[builderInstance.ImageEncoderUsed],
                targetProcessingColorSpace);

        List<SerializationFactory> allFactories =
            ViewModelMain.Current.Services.GetServices<SerializationFactory>().ToList();

        foreach (var factory in allFactories)
        {
            factory.ResourceLocator = resourceLocator;
        }

        AddNodes(builderInstance.Graph);

        if (builderInstance.Graph.AllNodes.Count == 0 || !builderInstance.Graph.AllNodes.Any(x => x is OutputNode))
        {
            Guid outputNodeGuid = Guid.NewGuid();
            acc.AddActions(new CreateNode_Action(typeof(OutputNode), outputNodeGuid, Guid.Empty));
        }

        AddAnimationData(builderInstance.AnimationData, mappedNodeIds, mappedKeyFrameIds);

        acc.AddFinishedActions(new ChangeBoundary_Action(), new DeleteRecordedChanges_Action());
        acc.AddActions(new InvokeAction_PassthroughAction(() =>
        {
            viewModel.MarkAsSaved();
        }));

        foreach (var factory in allFactories)
        {
            factory.ResourceLocator = null;
        }

        return viewModel;


        void AddNodes(NodeGraphBuilder graph)
        {
            foreach (var node in graph.AllNodes)
            {
                AddNode(node.Id, node);
            }

            foreach (var node in graph.AllNodes)
            {
                Guid nodeGuid = mappedNodeIds[node.Id];

                var serializedNode = graph.AllNodes.First(x => x.Id == node.Id);

                if (serializedNode.AdditionalData != null && serializedNode.AdditionalData.Count > 0)
                {
                    acc.AddActions(new DeserializeNodeAdditionalData_Action(nodeGuid,
                        SerializationUtil.DeserializeDict(serializedNode.AdditionalData, config, allFactories,
                            serializerData)));
                }

                if (node.InputConnections != null)
                {
                    foreach (var connections in node.InputConnections)
                    {
                        if (mappedNodeIds.TryGetValue(connections.Key, out Guid outputNodeId))
                        {
                            foreach (var connection in connections.Value)
                            {
                                acc.AddActions(new ConnectProperties_Action(nodeGuid, outputNodeId,
                                    connection.inputPropName, connection.outputPropName));
                            }
                        }
                    }
                }
            }
        }

        void AddNode(int id, NodeGraphBuilder.NodeBuilder serializedNode)
        {
            Guid guid = Guid.NewGuid();
            mappedNodeIds.Add(id, guid);
            Guid pairGuid = Guid.Empty;

            if (serializedNode.PairId != null &&
                mappedNodeIds.TryGetValue(serializedNode.PairId.Value, out Guid pairId))
            {
                pairGuid = pairId;
            }

            acc.AddActions(new CreateNodeFromName_Action(serializedNode.UniqueNodeName, guid, pairGuid));
            acc.AddFinishedActions(new NodePosition_Action([guid], serializedNode.Position.ToVecD()),
                new EndNodePosition_Action());

            if (serializedNode.InputValues != null)
            {
                foreach (var propertyValue in serializedNode.InputValues)
                {
                    object value =
                        SerializationUtil.Deserialize(propertyValue.Value, config, allFactories, serializerData);
                    acc.AddActions(new UpdatePropertyValue_Action(guid, propertyValue.Key, value));
                }
            }

            if (serializedNode.KeyFrames != null)
            {
                foreach (var keyFrame in serializedNode.KeyFrames)
                {
                    Guid keyFrameGuid = Guid.NewGuid();
                    /*Add should be here I think, but it crashes while deserializing multiple layers with no frames*/
                    mappedKeyFrameIds[keyFrame.Id] = keyFrameGuid;
                    acc.AddActions(
                        new SetKeyFrameData_Action(
                            guid,
                            keyFrameGuid,
                            SerializationUtil.Deserialize(keyFrame.Data, config, allFactories, serializerData),
                            keyFrame.StartFrame,
                            keyFrame.Duration, keyFrame.AffectedElement, keyFrame.IsVisible));
                }
            }

            if (!string.IsNullOrEmpty(serializedNode.Name))
            {
                acc.AddActions(new SetNodeName_Action(guid, serializedNode.Name));
            }
        }

        void AddAnimationData(AnimationDataBuilder? data, Dictionary<int, Guid> mappedIds,
            Dictionary<int, Guid> mappedKeyFrameIds)
        {
            if (data is null)
                return;

            acc.AddActions(new SetFrameRate_Action(data.FrameRate));
            acc.AddActions(new SetOnionSettings_Action(data.OnionFrames, data.OnionOpacity));
            foreach (var keyFrame in data.KeyFrameGroups)
            {
                if (keyFrame is GroupKeyFrameBuilder group)
                {
                    foreach (var child in group.Children)
                    {
                        acc.AddActions(
                            new CreateCel_Action(
                                mappedIds[child.NodeId],
                                mappedKeyFrameIds[child.KeyFrameId],
                                -1, -1, default));

                        acc.AddFinishedActions();
                    }
                }
            }
        }

        bool IsFileWithSrgbColorBlending((string serializerName, string serializerVersion) serializerData,
            Version? pixiParserVersionUsed)
        {
            if (pixiParserVersionUsed != null && pixiParserVersionUsed.Major < 5)
            {
                return true;
            }

            if (serializerData.serializerVersion == null || serializerData.serializerName == null)
            {
                return false;
            }

            try
            {
                Version parsedVersion = new Version(serializerData.serializerVersion);

                return serializerData.serializerName == "PixiEditor"
                       && parsedVersion is { Major: 2, Minor: 0, Build: 0, Revision: >= 28 and <= 31 };
            }
            catch (Exception)
            {
                return false;
            }
        }

        ResourceStorageLocator ExtractResources(ResourceStorage? resources)
        {
            if (resources is null)
                return null;

            string resourcesPath = Paths.TempResourcesPath;
            if (!Directory.Exists(resourcesPath))
                Directory.CreateDirectory(resourcesPath);

            Dictionary<int, string> mapping = new();

            foreach (var resource in resources.Resources)
            {
                string formattedGuid = resource.CacheId.ToString("N");
                string filePath = Path.Combine(resourcesPath, $"{formattedGuid}{Path.GetExtension(resource.FileName)}");
                File.WriteAllBytes(filePath, resource.Data);
                mapping.Add(resource.Handle, filePath);
            }

            return new ResourceStorageLocator(mapping, resourcesPath);
        }
    }

    public void MarkAsSaved()
    {
        Internals.ActionAccumulator.AddActions(new MarkAsAutosaved_PassthroughAction(DocumentMarkType.Saved));
    }

    public void MarkAsAutosaved()
    {
        Internals.ActionAccumulator.AddActions(new MarkAsAutosaved_PassthroughAction(DocumentMarkType.Autosaved));
    }

    public void MarkAsUnsaved()
    {
        Internals.ActionAccumulator.AddActions(new MarkAsAutosaved_PassthroughAction(DocumentMarkType.Unsaved));
    }

    public void InternalMarkSaveState(DocumentMarkType type)
    {
        switch (type)
        {
            case DocumentMarkType.Saved:
                lastChangeOnSave = Internals.Tracker.LastChangeGuid;
                OnPropertyChanged(nameof(AllChangesSaved));
                break;
            case DocumentMarkType.Unsaved:
                lastChangeOnSave = Guid.NewGuid();
                OnPropertyChanged(nameof(AllChangesSaved));
                break;
            case DocumentMarkType.Autosaved:
                lastChangeOnAutosave = Internals.Tracker.LastChangeGuid;
                OnPropertyChanged(nameof(AllChangesAutosaved));
                break;
            case DocumentMarkType.UnAutosaved:
                lastChangeOnAutosave = Guid.NewGuid();
                OnPropertyChanged(nameof(AllChangesAutosaved));
                break;
        }
    }

    public ICrossDocumentPipe<T> ShareNode<T>(Guid layerId) where T : class, IReadOnlyNode
    {
        return Internals.Tracker.Document.CreateNodePipe<T>(layerId);
    }

    public OneOf<Error, Surface> TryRenderWholeImage(KeyFrameTime frameTime, VecI renderSize)
    {
        try
        {
            Surface finalSurface = null;
            DrawingBackendApi.Current.RenderingDispatcher.Invoke(() =>
            {
                finalSurface = Surface.ForDisplay(renderSize);
                finalSurface.DrawingSurface.Canvas.Save();
                VecD scaling = new VecD(renderSize.X / (double)SizeBindable.X, renderSize.Y / (double)SizeBindable.Y);

                finalSurface.DrawingSurface.Canvas.Scale((float)scaling.X, (float)scaling.Y);
                Renderer.RenderDocument(finalSurface.DrawingSurface, frameTime, renderSize);

                finalSurface.DrawingSurface.Canvas.Restore();
            });

            return finalSurface;
        }
        catch (ObjectDisposedException)
        {
            return new Error();
        }
    }

    /// <summary>
    /// Tries rendering the whole document
    /// </summary>
    /// <returns><see cref="Error"/> if the ChunkyImage was disposed, otherwise a <see cref="Surface"/> of the rendered document</returns>
    public OneOf<Error, Surface> TryRenderWholeImage(KeyFrameTime frameTime)
    {
        return TryRenderWholeImage(frameTime, SizeBindable);
    }


    /// <summary>
    /// Takes the selected area and converts it into a surface
    /// </summary>
    /// <returns><see cref="Error"/> on error, <see cref="None"/> for empty <see cref="Surface"/>, <see cref="Surface"/> otherwise.</returns>
    public OneOf<Error, None, (Surface, RectI)> TryExtractAreaFromSelected(
        RectI bounds)
    {
        var selectedLayers = ExtractSelectedLayers(true);
        if (selectedLayers.Count == 0)
            return new Error();
        if (bounds.IsZeroOrNegativeArea)
            return new None();

        RectI finalBounds = default;

        for (int i = 0; i < selectedLayers.Count; i++)
        {
            var memberVm = StructureHelper.Find(selectedLayers.ElementAt(i));
            IReadOnlyStructureNode? layer = Internals.Tracker.Document.FindMember(memberVm.Id);
            if (layer is null)
                return new Error();

            RectI? memberImageBounds;
            try
            {
                memberImageBounds = (RectI?)layer.GetTightBounds(AnimationDataViewModel.ActiveFrameTime);
            }
            catch (ObjectDisposedException)
            {
                return new Error();
            }

            if (memberImageBounds is null)
                continue;

            RectI combinedBounds = bounds.Intersect(memberImageBounds.Value);
            combinedBounds = combinedBounds.Intersect(new RectI(VecI.Zero, SizeBindable));

            if (combinedBounds.IsZeroOrNegativeArea)
                continue;

            if (i == 0 || finalBounds == default)
            {
                finalBounds = combinedBounds;
            }
            else
            {
                finalBounds = finalBounds.Union(combinedBounds);
            }
        }

        if (finalBounds.IsZeroOrNegativeArea)
            return new None();

        Surface output = new(finalBounds.Size);

        VectorPath clipPath = new VectorPath(SelectionPathBindable) { FillType = PathFillType.EvenOdd };
        //clipPath.Transform(Matrix3X3.CreateTranslation(-bounds.X, -bounds.Y));
        output.DrawingSurface.Canvas.Save();
        output.DrawingSurface.Canvas.Translate(-finalBounds.X, -finalBounds.Y);
        if (!clipPath.IsEmpty)
        {
            output.DrawingSurface.Canvas.ClipPath(clipPath);
        }

        using Paint paint = new Paint() { BlendMode = BlendMode.SrcOver };

        DrawingBackendApi.Current.RenderingDispatcher.Invoke(() =>
        {
            try
            {
                Renderer.RenderLayers(output.DrawingSurface, selectedLayers.ToHashSet(),
                    AnimationDataViewModel.ActiveFrameBindable, ChunkResolution.Full, SizeBindable);
            }
            catch (ObjectDisposedException)
            {
                output?.Dispose();
            }
        });

        output.DrawingSurface.Canvas.Restore();
        return (output, finalBounds);
    }

    /// <summary>
    /// Picks the color at <paramref name="pos"/>
    /// </summary>
    /// <param name="includeReference">Should the color be picked from the reference layer</param>
    /// <param name="includeCanvas">Should the color be picked from the canvas</param>
    /// <param name="referenceTopmost">Is the reference layer topmost. (Only affects the result is includeReference and includeCanvas are set.)</param>
    public Color PickColor(VecD pos, DocumentScope scope, bool includeReference, bool includeCanvas, int frame,
        bool referenceTopmost = false, string? customOutput = null)
    {
        if (scope == DocumentScope.SingleLayer && includeReference && includeCanvas)
            includeReference = false;

        if (includeCanvas && includeReference)
        {
            Color canvasColor = PickColorFromCanvas((VecI)pos, scope, frame);
            Color? potentialReferenceColor = PickColorFromReferenceLayer(pos);
            if (potentialReferenceColor is not { } referenceColor)
                return canvasColor;

            if (!referenceTopmost)
            {
                return ColorHelpers.BlendColors(referenceColor, canvasColor);
            }

            byte referenceAlpha = canvasColor.A == 0
                ? referenceColor.A
                : (byte)(referenceColor.A * ReferenceLayerViewModel.TopMostOpacity);

            referenceColor = new Color(referenceColor.R, referenceColor.G, referenceColor.B, referenceAlpha);
            return ColorHelpers.BlendColors(canvasColor, referenceColor);
        }

        if (includeCanvas)
        {
            return PickColorFromCanvas((VecI)pos, scope, frame, customOutput);
        }

        if (includeReference)
            return PickColorFromReferenceLayer(pos) ?? Colors.Transparent;
        return Colors.Transparent;
    }

    public Color? PickColorFromReferenceLayer(VecD pos)
    {
        Texture? bitmap = ReferenceLayerViewModel.ReferenceTexture;
        if (bitmap is null)
            return null;

        Matrix3X3 matrix = ReferenceLayerViewModel.ReferenceTransformMatrix;
        matrix = matrix.Invert();
        var transformed = matrix.MapPoint(pos);

        if (transformed.X < 0 || transformed.Y < 0 || transformed.X >= bitmap.Size.X || transformed.Y >= bitmap.Size.Y)
            return null;

        return bitmap.GetSRGBPixel(new VecI((int)transformed.X, (int)transformed.Y));
    }

    public void SuppressAllOverlayEvents(string suppressor)
    {
        overlaySuppressors.Add(suppressor);
        OnPropertyChanged(nameof(OverlayEventsSuppressed));
    }

    public void RestoreAllOverlayEvents(string suppressor)
    {
        overlaySuppressors.Remove(suppressor);
        OnPropertyChanged(nameof(OverlayEventsSuppressed));
    }

    public Color PickColorFromCanvas(VecI pos, DocumentScope scope, KeyFrameTime frameTime, string? customOutput = null)
    {
        // there is a tiny chance that the image might get disposed by another thread
        try
        {
            // it might've been a better idea to implement this function asynchronously
            // via a passthrough action to avoid all the try catches
            if (scope == DocumentScope.Canvas)
            {
                using Surface
                    tmpSurface =
                        new Surface(SizeBindable); // new Surface is on purpose, Surface.ForDisplay doesn't work here
                Renderer.RenderDocument(tmpSurface.DrawingSurface, frameTime, SizeBindable, customOutput);

                return tmpSurface.GetSrgbPixel(pos);
            }

            if (SelectedStructureMember is not ILayerHandler layerVm)
                return Colors.Transparent;
            IReadOnlyStructureNode? maybeMember = Internals.Tracker.Document.FindMember(layerVm.Id);
            if (maybeMember is not IReadOnlyImageNode layer)
            {
                if (maybeMember is IRasterizable rasterizable)
                {
                    using Texture texture = Texture.ForDisplay(SizeBindable);
                    using Paint paint = new Paint();
                    rasterizable.Rasterize(texture.DrawingSurface, paint);
                    return texture.GetSRGBPixel(pos);
                }
            }
            else
            {
                return layer.GetLayerImageAtFrame(frameTime.Frame).GetMostUpToDatePixel(pos);
            }

            return Colors.Transparent;
        }
        catch (ObjectDisposedException)
        {
            return Colors.Transparent;
        }
    }

    #region Internal Methods

// these are intended to only be called from DocumentUpdater

    public void InternalRaiseLayersChanged(LayersChangedEventArgs args) => LayersChanged?.Invoke(this, args);

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
        OnPropertyChanged(nameof(AnySymmetryAxisEnabledBindable));
    }

    public void SetVerticalSymmetryAxisEnabled(bool infoState)
    {
        verticalSymmetryAxisEnabled = infoState;
        OnPropertyChanged(nameof(VerticalSymmetryAxisEnabledBindable));
        OnPropertyChanged(nameof(AnySymmetryAxisEnabledBindable));
    }

    public void SetVerticalSymmetryAxisX(double verticalSymmetryAxisX)
    {
        this.verticalSymmetryAxisX = verticalSymmetryAxisX;
        OnPropertyChanged(nameof(VerticalSymmetryAxisXBindable));
    }

    public void SetSelectedMember(IStructureMemberHandler member)
    {
        SelectedStructureMember = member;
        Internals.ChangeController.MembersSelectedInlet(GetSelectedMembers());
        OnPropertyChanged(nameof(SelectedStructureMember));
    }

    public void SetHorizontalSymmetryAxisY(double horizontalSymmetryAxisY)
    {
        this.horizontalSymmetryAxisY = horizontalSymmetryAxisY;
        OnPropertyChanged(nameof(HorizontalSymmetryAxisYBindable));
    }

    public void SetProcessingColorSpace(ColorSpace infoColorSpace)
    {
        UsesSrgbBlending = infoColorSpace.IsSrgb;
    }

    public void SetSize(VecI size)
    {
        var oldSize = size;
        this.size = size;
        OnPropertyChanged(nameof(SizeBindable));
        OnPropertyChanged(nameof(Width));
        OnPropertyChanged(nameof(Height));

        // TODO: Make sure this is correct, it was in InternalRaiseSizeChanged previously, check DocumentUpdater.cs ProcessSize
        SizeChanged?.Invoke(this, new DocumentSizeChangedEventArgs(this, oldSize, size));
    }

    public void UpdateSelectionPath(VectorPath vectorPath)
    {
        (VectorPath? toDispose, this.selectionPath) = (this.selectionPath, vectorPath);
        toDispose.Dispose();
        OnPropertyChanged(nameof(SelectionPathBindable));
    }

    public void AddSoftSelectedMember(IStructureMemberHandler member)
    {
        softSelectedStructureMembers.Add(member);
        Internals.ChangeController.MembersSelectedInlet(GetSelectedMembers());
        OnPropertyChanged(nameof(SoftSelectedStructureMembers));
    }

    public void RemoveSoftSelectedMember(IStructureMemberHandler member)
    {
        softSelectedStructureMembers.Remove(member);
        Internals.ChangeController.MembersSelectedInlet(GetSelectedMembers());
        OnPropertyChanged(nameof(SoftSelectedStructureMembers));
    }

    public void ClearSoftSelectedMembers()
    {
        softSelectedStructureMembers.Clear();
        Internals.ChangeController.MembersSelectedInlet(GetSelectedMembers());
        OnPropertyChanged(nameof(SoftSelectedStructureMembers));
    }

    #endregion

    /// <summary>
    /// Returns a list of all selected members (Hard and Soft selected)
    /// </summary>
    public List<Guid> GetSelectedMembers()
    {
        List<Guid> layerGuids = new List<Guid>();
        if (SelectedStructureMember is not null)
            layerGuids.Add(SelectedStructureMember.Id);

        foreach (var member in softSelectedStructureMembers)
        {
            if (member.Id != SelectedStructureMember?.Id)
            {
                layerGuids.Add(member.Id);
            }
        }

        return layerGuids;
    }


    public List<Guid> GetSelectedMembersInOrder(bool includeNested = false)
    {
        var selectedMembers = GetSelectedMembers();
        List<Guid> orderedMembers = new List<Guid>();
        var allMembers = StructureHelper.TraverseAllMembers();

        for (var index = 0; index < allMembers.Count; index++)
        {
            var member = allMembers[index];
            if (selectedMembers.Contains(member.Id))
            {
                if (!includeNested)
                {
                    var parents = StructureHelper.GetParents(member.Id);
                    if(parents.Any(x => selectedMembers.Contains(x.Id)))
                        continue;
                }
                orderedMembers.Add(member.Id);
            }
        }

        return orderedMembers;
    }

    /// <summary>
    ///     Gets all selected layers extracted from selected folders.
    /// </summary>
    /// <param name="includeFoldersWithMask">Should folders with mask be included</param>
    /// <returns>A list of GUIDs of selected layers</returns>
    public HashSet<Guid> ExtractSelectedLayers(bool includeFoldersWithMask = false)
    {
        var result = new HashSet<Guid>();
        List<Guid> selectedMembers = GetSelectedMembers();
        var allLayers = StructureHelper.GetAllMembers();
        foreach (var member in allLayers)
        {
            if (!selectedMembers.Contains(member.Id))
                continue;

            if (member is ILayerHandler)
            {
                result.Add(member.Id);
            }
            else if (member is IFolderHandler folder)
            {
                if (includeFoldersWithMask && folder.HasMaskBindable)
                    result.Add(folder.Id);

                ExtractSelectedLayers(folder, result, includeFoldersWithMask);
            }
        }

        return result;
    }

    public void UpdateSavedState()
    {
        OnPropertyChanged(nameof(AllChangesSaved));
        OnPropertyChanged(nameof(HasSavedUndo));
        OnPropertyChanged(nameof(HasSavedRedo));
    }

    private void ExtractSelectedLayers(IFolderHandler folder, HashSet<Guid> list,
        bool includeFoldersWithMask)
    {
        foreach (var member in folder.Children)
        {
            if (member is ILayerHandler layer && !list.Contains(layer.Id))
            {
                list.Add(layer.Id);
            }
            else if (member is IFolderHandler childFolder)
            {
                if (includeFoldersWithMask && childFolder.HasMaskBindable && !list.Contains(childFolder.Id))
                    list.Add(childFolder.Id);

                ExtractSelectedLayers(childFolder, list, includeFoldersWithMask);
            }
        }
    }

    public Image[] RenderFrames(Func<Surface, Surface> processFrameAction = null, CancellationToken token = default)
    {
        if (AnimationDataViewModel.KeyFrames.Count == 0)
            return [];

        if (token.IsCancellationRequested)
            return [];

        int firstFrame = AnimationDataViewModel.GetFirstVisibleFrame();
        int lastFrame = AnimationDataViewModel.GetLastVisibleFrame();

        int framesCount = lastFrame - firstFrame;

        Image[] images = new Image[framesCount];

        // TODO: Multi-threading
        for (int i = firstFrame; i < lastFrame; i++)
        {
            if (token.IsCancellationRequested)
                return [];

            double normalizedTime = (double)(i - firstFrame) / framesCount;
            KeyFrameTime frameTime = new KeyFrameTime(i, normalizedTime);
            var surface = TryRenderWholeImage(frameTime);
            if (surface.IsT0)
            {
                continue;
            }

            if (processFrameAction is not null)
            {
                surface = processFrameAction(surface.AsT1);
            }

            images[i - firstFrame] = surface.AsT1.DrawingSurface.Snapshot();
            surface.AsT1.Dispose();
        }

        return images;
    }

    /// <summary>
    ///     Render frames progressively and disposes the surface after processing.
    /// </summary>
    /// <param name="processFrameAction">Action to perform on rendered frame</param>
    /// <param name="token"></param>
    public void RenderFramesProgressive(Action<Surface, int> processFrameAction, CancellationToken token)
    {
        if (AnimationDataViewModel.KeyFrames.Count == 0)
            return;

        int firstFrame = AnimationDataViewModel.GetFirstVisibleFrame();
        int framesCount = AnimationDataViewModel.GetLastVisibleFrame();
        int lastFrame = firstFrame + framesCount;

        int activeFrame = AnimationDataViewModel.ActiveFrameBindable;

        for (int i = firstFrame; i < lastFrame; i++)
        {
            if (token.IsCancellationRequested)
                return;

            KeyFrameTime frameTime = new KeyFrameTime(i, (double)(i - firstFrame) / framesCount);

            var surface = TryRenderWholeImage(frameTime);
            if (surface.IsT0)
            {
                continue;
            }

            processFrameAction(surface.AsT1, i - firstFrame);
            surface.AsT1.Dispose();
        }
    }

    public bool RenderFrames(List<Image> frames, Func<Surface, Surface> processFrameAction = null)
    {
        var keyFrames = AnimationDataViewModel.KeyFrames;
        int firstFrame = 0;
        int lastFrame = AnimationDataViewModel.GetVisibleFramesCount();

        lastFrame = Math.Max(1, lastFrame);

        if (keyFrames.Count > 0)
        {
            firstFrame = AnimationDataViewModel.GetFirstVisibleFrame();
            lastFrame = AnimationDataViewModel.GetLastVisibleFrame();
        }

        for (int i = firstFrame; i < lastFrame; i++)
        {
            KeyFrameTime frameTime = new KeyFrameTime(i, (double)(i - firstFrame) / (lastFrame - firstFrame));
            var surface = TryRenderWholeImage(frameTime);
            if (surface.IsT0)
            {
                return false;
            }

            if (processFrameAction is not null)
            {
                surface = processFrameAction(surface.AsT1);
            }


            var snapshot = surface.AsT1.DrawingSurface.Snapshot();
            frames.Add(snapshot);
        }

        return true;
    }

    private static void ClearTempFolder(string tempRenderingPath)
    {
        string[] files = Directory.GetFiles(tempRenderingPath);
        for (var i = 0; i < files.Length; i++)
        {
            var file = files[i];
            File.Delete(file);
        }
    }

    public void Dispose()
    {
        NodeGraph.Dispose();
        Renderer.Dispose();
        SceneRenderer.Dispose();
        AnimationDataViewModel.Dispose();
        Internals.ChangeController.TryStopActiveExecutor();
        Internals.Tracker.Dispose();
        Internals.Tracker.Document.Dispose();
    }
}
