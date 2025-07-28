namespace PixiEditor.ChangeableDocument.ChangeInfos.NodeGraph;

public record PropertyValueUpdated_ChangeInfo(Guid NodeId, string Property, object Value) : IChangeInfo
{
    public string? Errors { get; set; }
}
