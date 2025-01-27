using Drawie.Numerics;

namespace PixiEditor.Models.Handlers.Tools;

internal interface IMoveToolHandler : IToolHandler
{
    public bool KeepOriginalImage { get; }
    public bool TransformingSelectedArea { get; set; }
    public bool DuplicateOnMove { get; set; }
}
