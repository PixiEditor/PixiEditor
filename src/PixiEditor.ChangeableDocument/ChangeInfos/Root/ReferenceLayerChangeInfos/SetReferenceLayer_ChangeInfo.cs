using System.Collections.Immutable;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.ChangeInfos.Structure;

public record class SetReferenceLayer_ChangeInfo(ImmutableArray<byte> ImagePbgra8888Bytes, VecI ImageSize, ShapeCorners Shape) : IChangeInfo;
