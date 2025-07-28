using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using Drawie.Numerics;

namespace PixiEditor.ChangeableDocument.ChangeInfos.Vectors;

    public record VectorShape_ChangeInfo(Guid LayerId, AffectedArea Affected) : IChangeInfo;
