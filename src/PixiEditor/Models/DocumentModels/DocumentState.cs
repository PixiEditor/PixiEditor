using System.Collections.Generic;
using PixiEditor.Models.Position;

namespace PixiEditor.Models.DocumentModels;
#nullable enable
internal class DocumentState
{
    public Dictionary<Guid, ViewportInfo> Viewports { get; set; } = new();
}
