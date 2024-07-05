using System.Collections;
using System.Collections.Immutable;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

public record CreateNode_ChangeInfo(string NodeName, VecD Position, Guid Id, 
    ImmutableArray<NodePropertyInfo> Inputs,
    ImmutableArray<NodePropertyInfo> Outputs) : IChangeInfo;
