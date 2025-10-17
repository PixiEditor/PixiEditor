using System.Diagnostics.CodeAnalysis;
using PixiEditor.ChangeableDocument.Changeables.Animations;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Vector;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables;

internal class Document : IChangeable, IReadOnlyDocument
{
    public Guid DocumentId { get; } = Guid.NewGuid();
    IReadOnlyNodeGraph IReadOnlyDocument.NodeGraph => NodeGraph;
    IReadOnlySelection IReadOnlyDocument.Selection => Selection;
    IReadOnlyAnimationData IReadOnlyDocument.AnimationData => AnimationData;
    IReadOnlyStructureNode? IReadOnlyDocument.FindMember(Guid guid) => FindMember(guid);

    bool IReadOnlyDocument.TryFindMember(Guid guid, [NotNullWhen(true)] out IReadOnlyStructureNode? member) =>
        TryFindMember(guid, out member);

    IReadOnlyList<IReadOnlyStructureNode> IReadOnlyDocument.FindMemberPath(Guid guid) => FindMemberPath(guid);
    IReadOnlyStructureNode IReadOnlyDocument.FindMemberOrThrow(Guid guid) => FindMemberOrThrow(guid);

    (IReadOnlyStructureNode, IReadOnlyNode) IReadOnlyDocument.FindChildAndParentOrThrow(Guid guid) =>
        FindChildAndParentOrThrow(guid);

    IReadOnlyReferenceLayer? IReadOnlyDocument.ReferenceLayer => ReferenceLayer;
    public DocumentRenderer Renderer { get; }
    public IReadOnlyBlackboard Blackboard => NodeGraph.Blackboard;
    public ColorSpace ProcessingColorSpace { get; internal set; } = ColorSpace.CreateSrgbLinear();

    /// <summary>
    /// The default size for a new document
    /// </summary>
    public static VecI DefaultSize { get; } = new VecI(64, 64);

    internal NodeGraph NodeGraph { get; private set; } = new();
    internal Selection Selection { get; private set; } = new();
    internal ReferenceLayer? ReferenceLayer { get; set; }
    internal AnimationData AnimationData { get; private set; }
    public VecI Size { get; set; } = DefaultSize;
    public bool HorizontalSymmetryAxisEnabled { get; set; }
    public bool VerticalSymmetryAxisEnabled { get; set; }
    public double HorizontalSymmetryAxisY { get; set; }
    public double VerticalSymmetryAxisX { get; set; }
    public bool IsDisposed { get; private set; }


    public Document()
    {
        AnimationData = new AnimationData(this);
        Renderer = new DocumentRenderer(this);
    }

    public void Dispose()
    {
        if (IsDisposed) return;

        IsDisposed = true;
        NodeGraph.Dispose();
        Selection.Dispose();
    }

    /// <summary>
    ///     Creates a surface for layer image.
    /// </summary>
    /// <param name="layerGuid">Guid of the layer inside structure.</param>
    /// <returns>Surface if the layer has some drawn pixels, null if the image is empty.</returns>
    /// <exception cref="ArgumentException">Exception when guid is not found inside structure or if it's not a layer</exception>
    /// <remarks>So yeah, welcome folks to the multithreaded world, where possibilities are endless! (and chances of objects getting
    /// edited, in between of processing you want to make exist). You might encounter ObjectDisposedException and other mighty creatures here if
    /// you are lucky enough. Have fun!</remarks>
    public Image? GetLayerRasterizedImage(Guid layerGuid, int frame)
    {
        var layer = (IReadOnlyLayerNode?)FindMember(layerGuid);

        if (layer is null)
            throw new ArgumentException(@"The given guid does not belong to a layer.", nameof(layerGuid));


        RectI? tightBounds = (RectI)layer.GetTightBounds(frame);

        if (tightBounds is null)
            return null;

        tightBounds = tightBounds.Value.Intersect(RectI.Create(0, 0, Size.X, Size.Y));

        Surface surface = Surface.ForProcessing(tightBounds.Value.Size, ProcessingColorSpace);

        using var paint = new Paint();

        Surface image;

        if (layer is IReadOnlyImageNode imageNode)
        {
            var chunkyImage = imageNode.GetLayerImageAtFrame(frame);
            using Surface chunkSurface =
                Surface.ForProcessing(chunkyImage.CommittedSize, chunkyImage.ProcessingColorSpace);
            chunkyImage.DrawCommittedRegionOn(
                new RectI(0, 0, chunkyImage.CommittedSize.X, chunkyImage.CommittedSize.Y),
                ChunkResolution.Full,
                chunkSurface.DrawingSurface,
                VecI.Zero);

            image = chunkSurface;
        }
        else
        {
            return null;
            /*TODO: this*/
            // image = new Surface(layer.Execute(new RenderingContext(frame, Size)));
        }

        //todo: idk if it's correct
        surface.DrawingSurface.Canvas.DrawSurface(image.DrawingSurface, 0, 0, paint);

        var snapshot = surface.DrawingSurface.Snapshot();
        surface.Dispose();
        image.Dispose();

        return snapshot;
    }

    public RectI? GetChunkAlignedLayerBounds(Guid layerGuid, int frame)
    {
        var layer = (IReadOnlyLayerNode?)FindMember(layerGuid);

        if (layer is null)
            throw new ArgumentException(@"The given guid does not belong to a layer.", nameof(layerGuid));


        return (RectI)layer.GetTightBounds(frame);
    }

    public void ForEveryReadonlyMember(Action<IReadOnlyStructureNode> action) =>
        ForEveryReadonlyMember(NodeGraph, action);

    /// <summary>
    /// Performs the specified action on each member of the document
    /// </summary>
    public void ForEveryMember(Action<StructureNode> action) => ForEveryMember(NodeGraph, action);

    public void InitProcessingColorSpace(ColorSpace processingColorSpace)
    {
        ProcessingColorSpace = processingColorSpace;
    }

    public List<IReadOnlyStructureNode> GetParents(Guid memberGuid)
    {
        var childNode = FindNode<StructureNode>(memberGuid);
        if (childNode == null)
            return new List<IReadOnlyStructureNode>();

        List<IReadOnlyStructureNode> parents = new();
        childNode.TraverseForwards((node, input) =>
        {
            if (node is IReadOnlyStructureNode parent &&
                input is { InternalPropertyName: FolderNode.ContentInternalName })
                parents.Add(parent);
            return true;
        });

        return parents;
    }

    public ICrossDocumentPipe<T> CreateNodePipe<T>(Guid layerId) where T : class, IReadOnlyNode
    {
        return new DocumentNodePipe<T>(this, layerId);
    }

    public ICrossDocumentPipe<IReadOnlyNodeGraph> CreateGraphPipe()
    {
        return new DocumentGraphPipe(this);
    }

    public IReadOnlyDocument Clone()
    {
        var clone = new Document
        {
            Size = Size,
            ProcessingColorSpace = ProcessingColorSpace,
            HorizontalSymmetryAxisEnabled = HorizontalSymmetryAxisEnabled,
            VerticalSymmetryAxisEnabled = VerticalSymmetryAxisEnabled,
            HorizontalSymmetryAxisY = HorizontalSymmetryAxisY,
            VerticalSymmetryAxisX = VerticalSymmetryAxisX,
            ReferenceLayer = ReferenceLayer?.Clone(),
            NodeGraph = NodeGraph?.Clone() as NodeGraph,
            AnimationData = AnimationData?.Clone() as AnimationData,
            Selection = Selection != null ? new Selection() { SelectionPath = Selection.SelectionPath != null ? new VectorPath(Selection.SelectionPath) : null } : null
        };

        return clone;
    }

    private void ForEveryReadonlyMember(IReadOnlyNodeGraph graph, Action<IReadOnlyStructureNode> action)
    {
        graph.TryTraverse((node) =>
        {
            if (node is not IReadOnlyStructureNode structureNode)
            {
                return;
            }

            action(structureNode);
        });
    }

    private void ForEveryMember(NodeGraph graph, Action<StructureNode> action)
    {
        graph.TryTraverse((node) =>
        {
            if (node is not StructureNode structureNode)
            {
                return;
            }

            action(structureNode);
        });
    }

    /// <summary>
    ///     Checks if a node in NodeGraph with the given <paramref name="id"/> exists.
    /// </summary>
    /// <param name="id">The <see cref="Node.Id"/> of the node.</param>
    /// <returns>True if the node exists, otherwise false.</returns>
    public bool HasNode(Guid id)
    {
        return NodeGraph.Nodes.Any(x => x.Id == id);
    }

    /// <summary>
    ///     Checks if a node in NodeGraph with the given <paramref name="id"/> exists and is of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="id">The <see cref="Node.Id"/> of the node.</param>
    /// <typeparam name="T">The type of the node.</typeparam>
    /// <returns>True if the node exists and is of type <typeparamref name="T"/>, otherwise false.</returns>
    public bool HasNode<T>(Guid id) where T : Node
    {
        return NodeGraph.Nodes.Any(x => x.Id == id && x is T);
    }

    /// <summary>
    /// Checks if a member with the <paramref name="guid"/> exists
    /// </summary>
    /// <param name="guid">The <see cref="StructureNode.Id"/> of the member</param>
    /// <returns>True if the member can be found, otherwise false</returns>
    public bool HasMember(Guid guid)
    {
        return HasNode<StructureNode>(guid);
    }

    /// <summary>
    /// Checks if a member with the <paramref name="guid"/> exists and is of type <typeparamref name="T"/>
    /// </summary>
    /// <param name="guid">The <see cref="StructureNode.Id"/> of the member</param>
    /// <returns>True if the member can be found and is of type <typeparamref name="T"/>, otherwise false</returns>
    public bool HasMember<T>(Guid guid)
        where T : StructureNode
    {
        var list = FindMemberPath(guid);
        return list.Count > 0 && list[0] is T;
    }

    /// <summary>
    /// Finds the member with the <paramref name="guid"/> or throws a ArgumentException if not found
    /// </summary>
    /// <param name="guid">The <see cref="StructureNode.Id"/> of the member</param>
    /// <exception cref="ArgumentException">Thrown if the member could not be found</exception>
    public StructureNode FindMemberOrThrow(Guid guid) =>
        FindMember(guid) ?? throw new ArgumentException($"Could not find member with guid '{guid}'");

    /// <summary>
    /// Finds the member of type <typeparamref name="T"/> with the <paramref name="guid"/> or throws an exception
    /// </summary>
    /// <param name="guid">The <see cref="StructureNode.Id"/> of the member</param>
    /// <exception cref="ArgumentException">Thrown if the member could not be found</exception>
    /// <exception cref="InvalidCastException">Thrown if the member is not of type <typeparamref name="T"/></exception>
    public T FindMemberOrThrow<T>(Guid guid) where T : StructureNode => (T?)FindMember(guid) ??
                                                                        throw new ArgumentException(
                                                                            $"Could not find member with guid '{guid}'");

    public T FindNodeOrThrow<T>(Guid guid) where T : Node => (T?)FindNode(guid) ??
                                                             throw new ArgumentException(
                                                                 $"Could not find node with guid '{guid}'");

    /// <summary>
    /// Finds the member with the <paramref name="guid"/> or returns null if not found
    /// </summary>
    /// <param name="guid">The <see cref="StructureNode.Id"/> of the member</param>
    public StructureNode? FindMember(Guid guid)
    {
        return FindNode<StructureNode>(guid);
    }

    /// <summary>
    ///     Finds the node with the given <paramref name="guid"/>.
    /// </summary>
    /// <param name="guid">The <see cref="Node.Id"/> of the node.</param>
    /// <returns>The node with the given <paramref name="guid"/> or null if it doesn't exist.</returns>
    public Node? FindNode(Guid guid)
    {
        return NodeGraph.FindNode(guid);
    }

    IReadOnlyNode IReadOnlyDocument.FindNode(Guid guid) => FindNodeOrThrow<Node>(guid);

    public T? FindNode<T>(Guid guid) where T : Node
    {
        return NodeGraph.FindNode<T>(guid);
    }

    /// <summary>
    ///     Tries to find the node with the given <paramref name="id"/> and returns true if it was found.
    /// </summary>
    /// <param name="id">The <see cref="Node.Id"/> of the node.</param>
    /// <param name="node">The node.</param>
    /// <typeparam name="T">The type of the node.</typeparam>
    /// <returns>True if the node could be found, otherwise false.</returns>
    public bool TryFindNode<T>(Guid id, out T node) where T : Node
    {
        node = (T?)NodeGraph.FindNode<T>(id) ?? null;
        return node != null;
    }

    /// <summary>
    /// Tries finding the member with the <paramref name="guid"/> and returns true if it was found
    /// </summary>
    /// <param name="guid">The <see cref="StructureNode.Id"/> of the <paramref name="member"/></param>
    /// <param name="member">The member</param>
    /// <returns>True if the member could be found, otherwise false</returns>
    public bool TryFindMember(Guid guid, [NotNullWhen(true)] out StructureNode? member)
    {
        member = FindMember(guid);
        return member != null;
    }

    /// <summary>
    /// Tries finding the member with the <paramref name="guid"/> of type <typeparamref name="T"/> and returns true if it was found
    /// </summary>
    /// <param name="guid">The <see cref="StructureNode.Id"/> of the <paramref name="member"/></param>
    /// <param name="member">The member</param>
    /// <typeparam name="T">The type of the <see cref="StructureNode"/></typeparam>
    /// <returns>True if the member could be found and is of type <typeparamref name="T"/>, otherwise false</returns>
    public bool TryFindMember<T>(Guid guid, [NotNullWhen(true)] out T? member)
        where T : IReadOnlyStructureNode
    {
        if (!TryFindMember(guid, out var structureMember) || structureMember is not T cast)
        {
            member = default;
            return false;
        }

        member = cast;
        return true;
    }


    /// <summary>
    /// Finds a member with the <paramref name="childGuid"/>  and its parent, throws a ArgumentException if they can't be found
    /// </summary>
    /// <param name="childGuid">The <see cref="StructureNode.Id"/> of the member</param>
    /// <returns>A value tuple consisting of child (<see cref="ValueTuple{T, T}.Item1"/>) and parent (<see cref="ValueTuple{T, T}.Item2"/>)</returns>
    /// <exception cref="ArgumentException">Thrown if the member and parent could not be found</exception>
    public (StructureNode, Node) FindChildAndParentOrThrow(Guid childGuid)
    {
        var path = FindNodePath(childGuid);
        if (path.Count < 2)
            throw new ArgumentException("Couldn't find child and parent");
        return (path[0] as StructureNode, path[1]);
    }

    /// <summary>
    /// Finds a node with the <paramref name="childGuid"/> and its parent
    /// </summary>
    /// <param name="childGuid">The <see cref="StructureNode.Id"/> of the member</param>
    /// <returns>A value tuple consisting of child (<see cref="ValueTuple{T, T}.Item1"/>) and parent (<see cref="ValueTuple{T, T}.Item2"/>)<para>Child and parent can be null if not found!</para></returns>
    public (StructureNode?, Node?) FindChildAndParent(Guid childGuid)
    {
        var path = FindNodePath(childGuid);
        return path.Count switch
        {
            1 => (path[0] as StructureNode, null),
            > 1 => (path[0] as StructureNode, path[1]),
            _ => (null, null),
        };
    }

    /// <summary>
    ///     Finds the path to the node with the given <paramref name="guid"/>.
    /// </summary>
    /// <param name="guid">The <see cref="Node.Id"/> of the node.</param>
    /// <returns>The path to the node.</returns>
    public List<Node> FindNodePath(Guid guid)
    {
        if (NodeGraph.OutputNode == null) return [];

        var list = new List<Node>();

        var targetNode = FindNode(guid);
        if (targetNode == null)
        {
            return [];
        }

        FillNodePath(targetNode, list);
        return list;
    }

    /// <summary>
    /// Finds the path to the member with <paramref name="guid"/>, the first element will be the member
    /// </summary>
    /// <param name="guid">The <see cref="StructureNode.Id"/> of the member</param>
    public List<StructureNode> FindMemberPath(Guid guid)
    {
        //if (NodeGraph.OutputNode == null) return [];

        var list = new List<StructureNode>();
        var targetNode = FindNode(guid);
        if (targetNode == null)
        {
            return [];
        }

        FillNodePath<StructureNode>(targetNode, list);
        return list.ToList();
    }

    private bool FillNodePath<T>(Node node, List<T> toFill) where T : Node
    {
        node.TraverseForwards(newNode =>
        {
            if (newNode is T strNode)
            {
                toFill.Add(strNode);
            }

            return true;
        });

        return true;
    }

    public List<Guid> ExtractLayers(IList<Guid> members)
    {
        var result = new List<Guid>();
        foreach (var member in members)
        {
            if (TryFindMember(member, out var structureMember))
            {
                if (structureMember is LayerNode layer && !result.Contains(layer.Id))
                {
                    result.Add(layer.Id);
                }
                else if (structureMember is FolderNode folder)
                {
                    ExtractLayers(folder, result);
                }
            }
        }

        return result;
    }

    private void ExtractLayers(FolderNode folder, List<Guid> list)
    {
        if (folder.Content.Connection == null) return;
        folder.Content.Connection.Node.TraverseBackwards(node =>
        {
            if (node is LayerNode layer && !list.Contains(layer.Id))
            {
                list.Add(layer.Id);
            }

            return true;
        });
    }

    object ICloneable.Clone()
    {
        return Clone();
    }
}
