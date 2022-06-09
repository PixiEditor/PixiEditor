using System;
using System.Collections.Generic;

namespace PixiEditorPrototype.Models;

internal class DocumentState
{
    public Dictionary<Guid, ViewportInfo> Viewports { get; set; } = new();
}
