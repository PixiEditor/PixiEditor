using System.Collections.Immutable;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.ChangeableDocument.ChangeInfos.Structure;

public record class SetReferenceLayer_ChangeInfo(ImmutableArray<byte> ImagePbgra32Bytes, VecI ImageSize, ShapeCorners Shape) : IChangeInfo;
