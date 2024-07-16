using System.Collections.Immutable;
using System.Reflection;
using PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Numerics;

namespace PixiEditor.ChangeableDocument.ChangeInfos.Structure;

public abstract record class CreateStructureMember_ChangeInfo(
    string InternalName,
    Guid ParentGuid,
    float Opacity,
    bool IsVisible,
    bool ClipToMemberBelow,
    string Name,
    BlendMode BlendMode,
    Guid Id,
    bool HasMask,
    bool MaskIsVisible,
    ImmutableArray<NodePropertyInfo> InputProperties,
    ImmutableArray<NodePropertyInfo> OutputProperties
) : CreateNode_ChangeInfo(InternalName, Name, new VecD(0, 0), Id, InputProperties, OutputProperties)
{
    public ImmutableArray<NodePropertyInfo> InputProperties { get; init; } = InputProperties;
    public ImmutableArray<NodePropertyInfo> OutputProperties { get; init; } = OutputProperties;
}
