using System.Diagnostics.CodeAnalysis;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;
using Drawie.Backend.Core.Numerics;
using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.Changeables.Interfaces;

public interface IReadOnlyDocument : IDisposable, ICloneable
{    
    public Guid DocumentId { get; }
    /// <summary>
    /// The root folder of the document
    /// </summary>
    IReadOnlyNodeGraph NodeGraph { get; }

    /// <summary>
    /// The selection of the document
    /// </summary>
    IReadOnlySelection Selection { get; }
    
    IReadOnlyAnimationData AnimationData { get; }

    /// <summary>
    /// The size of the document
    /// </summary>
    VecI Size { get; }

    /// <summary>
    /// Is the horizontal symmetry axis enabled (Mirrors top and bottom)
    /// </summary>
    bool HorizontalSymmetryAxisEnabled { get; }

    /// <summary>
    /// Is the vertical symmetry axis enabled (Mirrors left and right)
    /// </summary>
    bool VerticalSymmetryAxisEnabled { get; }

    /// <summary>
    /// The position of the horizontal symmetry axis (Mirrors top and bottom)
    /// </summary>
    double HorizontalSymmetryAxisY { get; }

    /// <summary>
    /// The position of the vertical symmetry axis (Mirrors left and right)
    /// </summary>
    double VerticalSymmetryAxisX { get; }

    /// <summary>
    /// Performs the specified action on each readonly member of the document
    /// </summary>
    void ForEveryReadonlyMember(Action<IReadOnlyStructureNode> action);
    
    public Image? GetLayerRasterizedImage(Guid layerGuid, int frame);
    public RectI? GetChunkAlignedLayerBounds(Guid layerGuid, int frame);

    IReadOnlyNode FindNode(Guid guid);
    
    /// <summary>
    /// Finds the member with the <paramref name="guid"/> or returns null if not found
    /// </summary>
    /// <param name="guid">The <see cref="IReadOnlyStructureNode.GuidValue"/> of the member</param>
    IReadOnlyStructureNode? FindMember(Guid guid);

    /// <summary>
    /// Tries finding the member with the <paramref name="guid"/> of type <typeparamref name="T"/> and returns true if it was found
    /// </summary>
    /// <param name="guid">The <see cref="StructureMember.GuidValue"/> of the <paramref name="member"/></param>
    /// <param name="member">The member</param>
    /// <returns>True if the member could be found, otherwise false</returns>
    bool TryFindMember<T>(Guid guid, [NotNullWhen(true)] out T? member) where T : IReadOnlyStructureNode;

    /// <summary>
    /// Tries finding the member with the <paramref name="guid"/> and returns true if it was found
    /// </summary>
    /// <param name="guid">The <see cref="StructureMember.GuidValue"/> of the <paramref name="member"/></param>
    /// <param name="member">The member</param>
    /// <returns>True if the member could be found, otherwise false</returns>
    bool TryFindMember(Guid guid, [NotNullWhen(true)] out IReadOnlyStructureNode? member);

    /// <summary>
    /// Finds the member with the <paramref name="guid"/> or throws a ArgumentException if not found
    /// </summary>
    /// <param name="guid">The <see cref="StructureMember.GuidValue"/> of the member</param>
    /// <exception cref="ArgumentException">Thrown if the member could not be found</exception>
    IReadOnlyStructureNode FindMemberOrThrow(Guid guid);

    /// <summary>
    /// Finds a member with the <paramref name="childGuid"/>  and its parent, throws a ArgumentException if they can't be found
    /// </summary>
    /// <param name="childGuid">The <see cref="IReadOnlyStructureNode.GuidValue"/> of the member</param>
    /// <returns>A value tuple consisting of child (<see cref="ValueTuple{T, T}.Item1"/>) and parent (<see cref="ValueTuple{T, T}.Item2"/>)</returns>
    /// <exception cref="ArgumentException">Thrown if the member and parent could not be found</exception>
    (IReadOnlyStructureNode, IReadOnlyNode) FindChildAndParentOrThrow(Guid childGuid);

    /// <summary>
    /// Finds the path to the member with <paramref name="guid"/>, the first element will be the member
    /// </summary>
    /// <param name="guid">The <see cref="IReadOnlyStructureNode.GuidValue"/> of the member</param>
    IReadOnlyList<IReadOnlyStructureNode> FindMemberPath(Guid guid);
    IReadOnlyReferenceLayer? ReferenceLayer { get; }
    public DocumentRenderer Renderer { get; }
    public IReadOnlyBlackboard Blackboard { get; }
    public ColorSpace ProcessingColorSpace { get; }
    public void InitProcessingColorSpace(ColorSpace processingColorSpace);
    public List<IReadOnlyStructureNode> GetParents(Guid memberGuid);
    public ICrossDocumentPipe<T> CreateNodePipe<T>(Guid layerId) where T : class, IReadOnlyNode;
    public ICrossDocumentPipe<IReadOnlyNodeGraph> CreateGraphPipe();
    public IReadOnlyDocument Clone();
}
