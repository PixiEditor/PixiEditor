using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.ChangeInfos.Objects;

public record TransformObject_ChangeInfo(Guid NodeGuid, AffectedArea Area) : IChangeInfo;
