using System;
using System.Collections.Generic;

namespace PixiEditorPrototype.Models;

internal class DocumentState
{
    public Dictionary<Guid, ViewportLocation> Viewports { get; set; } = new();
}
