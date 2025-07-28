namespace PixiEditor.Models.Handlers;

internal interface ILayerHandler : IStructureMemberHandler
{
    public bool ShouldDrawOnMask { get; set; }
    public Type? QuickEditTool { get; }
}
