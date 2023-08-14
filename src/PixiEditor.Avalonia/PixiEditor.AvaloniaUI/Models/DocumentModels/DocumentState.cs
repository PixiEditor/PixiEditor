using System.Collections.Generic;
using PixiEditor.AvaloniaUI.Models.Position;

namespace PixiEditor.AvaloniaUI.Models.DocumentModels;
#nullable enable
internal class DocumentState
{
    public Dictionary<Guid, ViewportInfo> Viewports { get; set; } = new();
}
