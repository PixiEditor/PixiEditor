namespace PixiEditor.Models.Handlers;

internal interface ILayerHandler : IStructureMemberHandler
{
    public bool ShouldDrawOnMask { get; set; }
    public Type? QuickEditTool { get; }
    public bool HasFilters { get; }
    string LayerTypeIcon { get; }
    public bool CanLockTransparency { get; }
}
