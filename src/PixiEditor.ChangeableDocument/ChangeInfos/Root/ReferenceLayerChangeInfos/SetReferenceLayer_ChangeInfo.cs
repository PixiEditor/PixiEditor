using System.Collections.Immutable;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.ChangeInfos.Structure;

// TODO: Make sure Pbgra8888 is all right
public record class SetReferenceLayer_ChangeInfo(ImmutableArray<byte> ImagePbgra8888Bytes, VecI ImageSize, ShapeCorners Shape) : IChangeInfo;
