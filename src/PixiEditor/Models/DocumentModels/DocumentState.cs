using PixiEditor.Models.Position;

namespace PixiEditor.Models.DocumentModels;

internal class DocumentState
{
    public Dictionary<Guid, ViewportInfo> Viewports { get; set; } = new();
}
