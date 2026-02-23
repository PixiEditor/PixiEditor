using System.Collections;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using Avalonia.Threading;
using ChunkyImageLib.DataHolders;
using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Models.DocumentPassthroughActions;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Actions.Undo;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using PixiEditor.Extensions.CommonApi.Palettes;
using PixiEditor.Helpers;
using PixiEditor.Helpers.Collections;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.DocumentModels.Public;
using PixiEditor.Models.DocumentModels.UpdateableChangeExecutors.Features;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Rendering;
using PixiEditor.Models.Serialization;
using PixiEditor.Models.Serialization.Factories;
using PixiEditor.Models.Structures;
using PixiEditor.Models.Tools;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Workspace;
using PixiEditor.Models.IO;
using PixiEditor.Parser;
using PixiEditor.Parser.Skia;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Document.Nodes;
using PixiEditor.ViewModels.Document.Nodes.Workspace;
using PixiEditor.ViewModels.Document.TransformOverlays;
using PixiEditor.Views.Overlays.SymmetryOverlay;
using BlendMode = Drawie.Backend.Core.Surfaces.BlendMode;
using Color = Drawie.Backend.Core.ColorsImpl.Color;
using Colors = Drawie.Backend.Core.ColorsImpl.Colors;

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


    public bool IsNestedDocument => referenceId != Guid.Empty;

    public Guid ReferenceId
    {
        get => referenceId;
        set
        {
            SetProperty(ref referenceId, value);
            OnPropertyChanged(nameof(IsNestedDocument));
        }
    }

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

    public NodeGraphViewModel NodeGraph { get; private set; }
    public DocumentStructureModule StructureHelper { get; private set; }
    public DocumentToolsModule Tools { get; private set; }
    public DocumentOperationsModule Operations { get; private set; }
    public DocumentRenderer Renderer { get; private set; }
    public SceneRenderer SceneRenderer { get; private set; }
    public DocumentEventsModule EventInlet { get; private set; }

    public ActionDisplayList ActionDisplays { get; } =
        new(() => ViewModelMain.Current.NotifyToolActionDisplayChanged());

    public IStructureMemberHandler? SelectedStructureMember { get; private set; } = null;

    public Dictionary<Guid, Texture> SceneTextures { get; } = new();

    private VectorPath selectionPath = new VectorPath();
    public VectorPath SelectionPathBindable => selectionPath;
    public ObservableCollection<PaletteColor> Swatches { get; set; } = new();
    public Guid Id => Internals.Tracker.Document.DocumentId;
    public ObservableRangeCollection<PaletteColor> Palette { get; set; } = new();
    public SnappingViewModel SnappingViewModel { get; set; }
    ISnappingHandler IDocument.SnappingHandler => SnappingViewModel;
    public IReadOnlyCollection<Guid> SelectedMembers => GetSelectedMembers().AsReadOnly();
    public DocumentTransformViewModel TransformViewModel { get; set; }
    public PathOverlayViewModel PathOverlayViewModel { get; set; }
    public ReferenceLayerViewModel ReferenceLayerViewModel { get; set; }
    public LineToolOverlayViewModel LineToolOverlayViewModel { get; set; }
    public AnimationDataViewModel AnimationDataViewModel { get; set; }
    public TextOverlayViewModel TextOverlayViewModel { get; set; }
    private DocumentInternalParts Internals { get; }
    public AutosaveDocumentViewModel AutosaveViewModel { get; set; }
    public IReadOnlyCollection<IStructureMemberHandler> SoftSelectedStructureMembers => softSelectedStructureMembers;
    INodeGraphHandler IDocument.NodeGraphHandler => NodeGraph;
    IDocumentOperations IDocument.Operations => Operations;
    ITransformHandler IDocument.TransformHandler => TransformViewModel;
    ITextOverlayHandler IDocument.TextOverlayHandler => TextOverlayViewModel;
    IPathOverlayHandler IDocument.PathOverlayHandler => PathOverlayViewModel;
    ILineOverlayHandler IDocument.LineToolOverlayHandler => LineToolOverlayViewModel;
    IReferenceLayerHandler IDocument.ReferenceLayerHandler => ReferenceLayerViewModel;
    IAnimationHandler IDocument.AnimationHandler => AnimationDataViewModel;

    public bool UsesSrgbBlending { get; private set; }

    private bool isDisposed = false;
    private Guid referenceId = Guid.Empty;
    private Queue<Action> queuedLayerReadyToUseActions = new();
    private Queue<Action> queuedKeyFrameReadyToUseActions = new();

    private DocumentViewModel()
    {
        var serviceProvider = ViewModelMain.Current.Services;
        Internals = new DocumentInternalParts(this, serviceProvider);
        InitializeViewModel();
    }

    internal DocumentViewModel(IReadOnlyDocument document, Guid referenceId)
    {
        var serviceProvider = ViewModelMain.Current.Services;
        Internals = new DocumentInternalParts(this, serviceProvider, document);
        InitializeViewModel();

        SetSize(document.Size);
        SetProcessingColorSpace(document.ProcessingColorSpace);
        SetHorizontalSymmetryAxisEnabled(document.HorizontalSymmetryAxisEnabled);
        SetVerticalSymmetryAxisEnabled(document.VerticalSymmetryAxisEnabled);
        SetHorizontalSymmetryAxisY(document.HorizontalSymmetryAxisY);
        SetVerticalSymmetryAxisX(document.VerticalSymmetryAxisX);

        NodeGraph.InitFrom(document.NodeGraph);
        AnimationDataViewModel.InitFrom(document.AnimationData);
        ReferenceLayerViewModel.InitFrom(document.ReferenceLayer);
        UpdateSelectionPath(new VectorPath(document.Selection.SelectionPath));
        NodeGraph.StructureTree.Update(NodeGraph);

        ReferenceId = referenceId;
    }

    private void InitializeViewModel()
    {
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
            /*if (args.LayerChangeType == LayerAction.Add)
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
            }*/
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
        var changeBlock = viewModel.Operations.StartChangeBlock();
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

        AddBlackboard(builderInstance.Graph.Blackboard);
        AddNodes(builderInstance.Graph);

        if (builderInstance.Graph.AllNodes.Count == 0 ||
            builderInstance.Graph.AllNodes.All(x => x.UniqueNodeName != OutputNode.UniqueName))
        {
            Guid outputNodeGuid = Guid.NewGuid();
            acc.AddActions(new CreateNode_Action(typeof(OutputNode), outputNodeGuid, Guid.Empty));
        }

        AddAnimationData(builderInstance.AnimationData, mappedNodeIds, mappedKeyFrameIds);

        if (builderInstance.FitToContent)
        {
            acc.AddFinishedActions(new ClipCanvas_Action(0));
        }

        changeBlock.ExecuteQueuedActions();
        changeBlock.Dispose();

        acc.AddFinishedActions(new ChangeBoundary_Action(), new DeleteRecordedChanges_Action());
        acc.AddActions(new InvokeAction_PassthroughAction(() =>
        {
            viewModel.MarkAsSaved();
        }));

        acc.AddActions(new SetActiveFrame_PassthroughAction(1), new RefreshPreviews_PassthroughAction());

        foreach (var factory in allFactories)
        {
            factory.ResourceLocator = null;
        }

        viewModel.NodeGraph.FinalizeCreation();

        return viewModel;

        void AddBlackboard(NodeGraphBuilder.BlackboardBuilder blackboard)
        {
            if (blackboard is null)
                return;

            foreach (var varBuilder in blackboard.Variables)
            {
                object value =
                    SerializationUtil.Deserialize(varBuilder.Value, config, allFactories, serializerData);
                var wellKnownType = SerializationUtil.GetTypeForWellKnownTypeName(varBuilder.Type, allFactories);
                if (value == null && wellKnownType != null && wellKnownType.IsValueType)
                {
                    value = Activator.CreateInstance(wellKnownType);
                }

                acc.AddActions(new SetBlackboardVariable_Action(varBuilder.Name, value,
                        wellKnownType ?? typeof(object),
                        varBuilder.Min ?? double.MinValue,
                        varBuilder.Max ?? double.MaxValue, varBuilder.Unit, varBuilder.IsExposed),
                    new EndSetBlackboardVariable_Action());
            }
        }


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
            }

            foreach (var node in graph.AllNodes)
            {
                Guid nodeGuid = mappedNodeIds[node.Id];
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
                    acc.AddActions(new UpdatePropertyValue_Action(guid, propertyValue.Key, value),
                        new EndUpdatePropertyValue_Action());
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
            acc.AddActions(new SetDefaultEndFrame_Action(data.DefaultEndFrame));
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

            // Before 2.1.0.11, the fallback animation to layer image was the only behavior, after the default is to have it off
            if (data.FallbackAnimationToLayerImage ||
                SerializationUtil.IsFilePreVersion(serializerData, new Version(2, 1, 0, 11)))
            {
                acc.AddActions(new SetFallbackAnimationToLayerImage_Action(true));
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
            Dictionary<int, byte[]> resourceData = new();

            foreach (var resource in resources.Resources)
            {
                string formattedGuid = resource.CacheId.ToString("N");
                if (!string.IsNullOrEmpty(Path.GetExtension(resource.FileName)))
                {
                    string filePath = Path.Combine(resourcesPath,
                        $"{formattedGuid}{Path.GetExtension(resource.FileName)}");
                    File.WriteAllBytes(filePath, resource.Data);
                    mapping.Add(resource.Handle, filePath);
                }
                else
                {
                    resourceData.Add(resource.Handle, resource.Data);
                }
            }

            return new ResourceStorageLocator(mapping, resourcesPath, resourceData);
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

    public void InternalRaiseKeyFrameCreated(RasterCelViewModel vm)
    {
        while (queuedKeyFrameReadyToUseActions.Count > 0)
        {
            var action = queuedKeyFrameReadyToUseActions.Dequeue();
            action();
        }
    }


    public (string name, VecI originalSize)[] GetAvailableExportOutputs()
    {
        var allExportNodes = NodeGraph.AllNodes.Where(x => x is CustomOutputNodeViewModel).ToArray();

        if (allExportNodes.Length == 0)
        {
            return [("DEFAULT", SizeBindable)];
        }

        var exportNodes = Internals.Tracker.Document.NodeGraph.AllNodes.Where(x => x is CustomOutputNode).ToArray();
        var exportNames = new List<(string name, VecI origianlSize)>();
        exportNames.Add(("DEFAULT", SizeBindable));

        foreach (var node in exportNodes)
        {
            if (node is not CustomOutputNode exportZone)
                continue;

            var name = exportZone.InputProperties.FirstOrDefault(x =>
                x.InternalPropertyName == CustomOutputNode.OutputNamePropertyName);


            if (name?.Value is not string finalName)
                continue;

            if (string.IsNullOrEmpty(finalName))
            {
                continue;
            }

            VecI originalSize =
                exportZone.InputProperties
                    .FirstOrDefault(x => x.InternalPropertyName == CustomOutputNode.SizePropertyName)
                    ?.Value as VecI? ?? SizeBindable;
            if (originalSize.ShortestAxis <= 0)
            {
                originalSize = SizeBindable;
            }

            exportNames.Add((finalName, originalSize));
        }

        return exportNames.ToArray();
    }

    public VecI GetDefaultRenderSize(out string? renderOutputName)
    {
        var allExportNodes = NodeGraph.AllNodes.Where(x => x is CustomOutputNodeViewModel).ToArray();

        renderOutputName = "DEFAULT";
        if (allExportNodes.Length == 0)
        {
            return SizeBindable;
        }

        using var block = Operations.StartChangeBlock();
        foreach (var node in allExportNodes)
        {
            if (node is not CustomOutputNodeViewModel exportZone)
                continue;

            Internals.ActionAccumulator.AddActions(new EvaluateGraph_Action(node.Id,
                AnimationDataViewModel.ActiveFrameTime));

            Internals.ActionAccumulator.AddActions(
                new GetComputedPropertyValue_Action(node.Id, CustomOutputNode.OutputNamePropertyName, true),
                new GetComputedPropertyValue_Action(node.Id, CustomOutputNode.IsDefaultExportPropertyName, true),
                new GetComputedPropertyValue_Action(node.Id, CustomOutputNode.SizePropertyName, true));
        }

        block.ExecuteQueuedActions();

        var exportNodes = NodeGraph.AllNodes.Where(x => x is CustomOutputNodeViewModel exportZone
                                                        && exportZone.Inputs.Any(x => x is
                                                        {
                                                            PropertyName: CustomOutputNode.IsDefaultExportPropertyName,
                                                            ComputedValue: true
                                                        })).ToArray();

        if (exportNodes.Length == 0)
            return SizeBindable;

        var exportNode = exportNodes.FirstOrDefault();

        if (exportNode is null)
            return SizeBindable;

        var exportSize =
            exportNode.Inputs.FirstOrDefault(x => x.PropertyName == CustomOutputNode.SizePropertyName);

        if (exportSize is null)
            return SizeBindable;

        if (exportSize.ComputedValue is VecI finalSize)
        {
            if (exportNode.Inputs.FirstOrDefault(x => x.PropertyName == CustomOutputNode.OutputNamePropertyName) is
                { } name)
            {
                renderOutputName = name.ComputedValue?.ToString();
            }

            if (finalSize.ShortestAxis <= 0)
            {
                finalSize = SizeBindable;
            }

            return finalSize;
        }

        return SizeBindable;
    }

    public ICrossDocumentPipe<T> ShareNode<T>(Guid layerId) where T : class, IReadOnlyNode
    {
        return Internals.Tracker.Document.CreateNodePipe<T>(layerId);
    }

    public ICrossDocumentPipe<IReadOnlyNodeGraph> ShareGraph()
    {
        return Internals.Tracker.Document.CreateGraphPipe();
    }

    /// <summary>
    ///      Gives access to the internal <see cref="IReadOnlyDocument"/>. Use with caution, as it is not tracked by the <see cref="DocumentViewModel"/>.
    /// <remarks>Never, ever, EVER, update the readonly document or dispose it if view model is in use.</remarks>
    /// </summary>
    public IReadOnlyDocument AccessInternalReadOnlyDocument()
    {
        return Internals.Tracker.Document;
    }

    public OneOf<Error, Surface> TryRenderWholeImage(KeyFrameTime frameTime, VecI renderSize)
    {
        return TryRenderWholeImage(frameTime, renderSize, SizeBindable);
    }

    public OneOf<Error, Surface> TryRenderWholeImage(KeyFrameTime frameTime, string? renderOutputName)
    {
        (string name, VecI originalSize)[] outputs = [];

        Dispatcher.UIThread.Invoke(() =>
        {
            outputs = GetAvailableExportOutputs();
        });

        string outputName = renderOutputName ?? "DEFAULT";
        var output = outputs.FirstOrDefault(x => x.name == outputName);
        VecI originalSize = string.IsNullOrEmpty(output.name) ? SizeBindable : output.originalSize;
        if (originalSize.ShortestAxis <= 0)
            return new Error();

        return TryRenderWholeImage(frameTime, originalSize, originalSize, renderOutputName);
    }

    public OneOf<Error, Surface> TryRenderWholeImage(KeyFrameTime frameTime, VecI renderSize, string? renderOutputName)
    {
        (string name, VecI originalSize)[] outputs = [];

        Dispatcher.UIThread.Invoke(() =>
        {
            outputs = GetAvailableExportOutputs();
        });

        string outputName = renderOutputName ?? "DEFAULT";
        var output = outputs.FirstOrDefault(x => x.name == outputName);
        VecI originalSize = string.IsNullOrEmpty(output.name) ? SizeBindable : output.originalSize;
        if (originalSize.ShortestAxis <= 0)
            return new Error();

        return TryRenderWholeImage(frameTime, renderSize, originalSize, renderOutputName);
    }

    public OneOf<Error, Surface> TryRenderWholeImage(KeyFrameTime frameTime, VecI renderSize, VecI originalSize,
        string? renderOutputName = null)
    {
        if (renderSize.ShortestAxis <= 0)
            return new Error();

        try
        {
            Surface finalSurface = null;
            DrawingBackendApi.Current.RenderingDispatcher.Invoke(() =>
            {
                finalSurface = Surface.ForDisplay(renderSize);
                finalSurface.DrawingSurface.Canvas.Save();
                VecD scaling = new VecD(renderSize.X / (double)originalSize.X, renderSize.Y / (double)originalSize.Y);

                finalSurface.DrawingSurface.Canvas.Scale((float)scaling.X, (float)scaling.Y);
                Renderer.RenderDocument(finalSurface.DrawingSurface, frameTime, renderSize, renderOutputName);

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

        var toAdd = new HashSet<Guid>();
        foreach (var layer in selectedLayers)
        {
            var parents = StructureHelper.GetParents(layer);
            if (parents is null)
                continue;

            foreach (var parent in parents)
            {
                toAdd.Add(parent.Id);
            }
        }

        selectedLayers.UnionWith(toAdd);

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

        Surface output = Surface.ForDisplay(finalBounds.Size);

        VectorPath clipPath = new VectorPath(SelectionPathBindable) { FillType = PathFillType.EvenOdd };
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

        return bitmap.GetSrgbPixel(new VecI((int)transformed.X, (int)transformed.Y));
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
            if (SizeBindable.X <= 0 || SizeBindable.Y <= 0)
                return Colors.Transparent;

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
                    using Surface texture = new Surface(SizeBindable);
                    using Paint paint = new Paint();
                    rasterizable.Rasterize(texture.DrawingSurface.Canvas, paint, frameTime.Frame);
                    return texture.GetSrgbPixel(pos);
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

    public void InternalRaiseLayersChanged(LayersChangedEventArgs args)
    {
        LayersChanged?.Invoke(this, args);
        if (queuedLayerReadyToUseActions.Count > 0)
        {
            foreach (var action in queuedLayerReadyToUseActions)
            {
                action();
            }

            queuedLayerReadyToUseActions.Clear();
        }
    }

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

    public ColorSpace ProcessingColorSpace =>
        UsesSrgbBlending ? ColorSpace.CreateSrgb() : ColorSpace.CreateSrgbLinear();

    public bool IsDisposed => isDisposed;

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
                    if (parents.Any(x => selectedMembers.Contains(x.Id)))
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

    public Image[] RenderFrames(Func<Surface, Surface> processFrameAction = null, string? renderOutput = null,
        CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
            return [];

        int firstFrame = 1;
        int lastFrame = AnimationDataViewModel.GetLastVisibleFrame();

        int framesCount = lastFrame;

        Image[] images = new Image[framesCount - firstFrame];

        // TODO: Multi-threading
        for (int i = firstFrame; i < lastFrame; i++)
        {
            if (token.IsCancellationRequested)
                return [];

            double normalizedTime = (double)(i - firstFrame) / framesCount;
            KeyFrameTime frameTime = new KeyFrameTime(i, normalizedTime);
            var surface = TryRenderWholeImage(frameTime, renderOutput);
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
    /// <param name="token">Cancellation token to cancel the rendering</param>
    public void RenderFramesProgressive(Action<Surface, int> processFrameAction, CancellationToken token,
        string? renderOutput)
    {
        int firstFrame = 1;
        int lastFrame = AnimationDataViewModel.GetLastVisibleFrame();
        int totalFrames = lastFrame - firstFrame;
        int activeFrame = AnimationDataViewModel.ActiveFrameBindable;

        for (int i = firstFrame; i < lastFrame; i++)
        {
            if (token.IsCancellationRequested)
                return;

            KeyFrameTime frameTime = new KeyFrameTime(i, (double)(i - firstFrame) / totalFrames);

            var surface = TryRenderWholeImage(frameTime, renderOutput);
            if (surface.IsT0)
            {
                continue;
            }

            processFrameAction(surface.AsT1, i - firstFrame);
            surface.AsT1.Dispose();
        }
    }

    public bool RenderFrames(List<Image> frames, Func<Surface, Surface> processFrameAction = null,
        string? renderOutput = null)
    {
        var firstFrame = 1;
        var lastFrame = AnimationDataViewModel.GetLastVisibleFrame();

        for (int i = firstFrame; i < lastFrame; i++)
        {
            KeyFrameTime frameTime = new KeyFrameTime(i, (double)(i - firstFrame) / (lastFrame - firstFrame));
            var surface = TryRenderWholeImage(frameTime, renderOutput);
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
        try
        {
            if (isDisposed)
                return;

            isDisposed = true;
            NodeGraph.Dispose();
            Renderer.Dispose();
            foreach (var texture in SceneTextures)
            {
                texture.Value?.Dispose();
            }

            AnimationDataViewModel.Dispose();
            Internals.ChangeController.TryStopActiveExecutor();
            Internals.Tracker.Dispose();
            Internals.Tracker.Document.Dispose();
        }
        catch (Exception ex)
        {
            CrashHelper.SendExceptionInfo(ex);
        }
    }

    public VecI GetRenderOutputSize(string renderOutputName)
    {
        var exportOutputs = GetAvailableExportOutputs();
        var exportOutput = exportOutputs.FirstOrDefault(x => x.name == renderOutputName);

        VecI size = SizeBindable;
        if (exportOutput != default)
        {
            size = exportOutput.originalSize;

            if (size.ShortestAxis <= 0)
            {
                size = SizeBindable;
            }
        }

        return size;
    }

    void Extensions.CommonApi.Documents.IDocument.Resize(int width, int height)
    {
        Operations.ResizeImage(new VecI(width, height), ResamplingMethod.NearestNeighbor);
    }

    public void UpdateDocumentReferences(Guid referenceId, DocumentViewModel newDoc)
    {
        var nestedNodes = NodeGraph.AllNodes.Where(x => x is NestedDocumentNodeViewModel)
            .Cast<NestedDocumentNodeViewModel>();
        using var changeBlock = Operations.StartChangeBlock();
        foreach (var node in nestedNodes)
        {
            if (node.InputPropertyMap[NestedDocumentNode.DocumentPropertyName].Value is not DocumentReference docRef ||
                (docRef.ReferenceId != referenceId && docRef.OriginalFilePath != newDoc.FullFilePath))
                continue;

            Internals.ActionAccumulator.AddActions(new UpdatePropertyValue_Action(node.Id,
                    NestedDocumentNode.DocumentPropertyName,
                    new DocumentReference(newDoc.FullFilePath, referenceId,
                        newDoc.AccessInternalReadOnlyDocument().Clone(true))),
                new EndUpdatePropertyValue_Action());
        }
    }

    public void UpdateNestedLinkedStatus(Guid referenceId)
    {
        var nestedNodes = NodeGraph.AllNodes.Where(x => x is NestedDocumentNodeViewModel)
            .Cast<NestedDocumentNodeViewModel>();

        foreach (var nodeVm in nestedNodes)
        {
            if (nodeVm.InputPropertyMap[NestedDocumentNode.DocumentPropertyName]
                    .Value is not DocumentReference docRef ||
                docRef.ReferenceId != referenceId)
                continue;

            nodeVm.UpdateLinkedStatus();
        }
    }

    public void SubscribeLayerReadyToUseOnce(Action action)
    {
        queuedLayerReadyToUseActions.Enqueue(action);
    }

    public void SubscribeKeyFrameReadyToUseOnce(Action action)
    {
        queuedKeyFrameReadyToUseActions.Enqueue(action);
    }
}
