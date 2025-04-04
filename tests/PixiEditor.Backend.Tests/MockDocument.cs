using Drawie.Backend.Core.Surfaces.ImageData;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Rendering;

namespace PixiEditor.Backend.Tests;

public class MockDocument : IReadOnlyDocument
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public Guid DocumentId { get; }
    public IReadOnlyNodeGraph NodeGraph { get; }
    public IReadOnlySelection Selection { get; }
    public IReadOnlyAnimationData AnimationData { get; }
    public VecI Size { get; } = new VecI(16, 16);
    public bool HorizontalSymmetryAxisEnabled { get; }
    public bool VerticalSymmetryAxisEnabled { get; }
    public double HorizontalSymmetryAxisY { get; }
    public double VerticalSymmetryAxisX { get; }
    public void ForEveryReadonlyMember(Action<IReadOnlyStructureNode> action)
    {
        throw new NotImplementedException();
    }

    public Image? GetLayerRasterizedImage(Guid layerGuid, int frame)
    {
        throw new NotImplementedException();
    }

    public RectI? GetChunkAlignedLayerBounds(Guid layerGuid, int frame)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyNode FindNode(Guid guid)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyStructureNode? FindMember(Guid guid)
    {
        throw new NotImplementedException();
    }

    public bool TryFindMember<T>(Guid guid, out T? member) where T : IReadOnlyStructureNode
    {
        throw new NotImplementedException();
    }

    public bool TryFindMember(Guid guid, out IReadOnlyStructureNode? member)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyStructureNode FindMemberOrThrow(Guid guid)
    {
        throw new NotImplementedException();
    }

    public (IReadOnlyStructureNode, IReadOnlyNode) FindChildAndParentOrThrow(Guid childGuid)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyList<IReadOnlyStructureNode> FindMemberPath(Guid guid)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyReferenceLayer? ReferenceLayer { get; }
    public DocumentRenderer Renderer { get; }
    public ColorSpace ProcessingColorSpace { get; }
    public void InitProcessingColorSpace(ColorSpace processingColorSpace)
    {
        throw new NotImplementedException();
    }

    public List<IReadOnlyStructureNode> GetParents(Guid memberGuid)
    {
        throw new NotImplementedException();
    }
}