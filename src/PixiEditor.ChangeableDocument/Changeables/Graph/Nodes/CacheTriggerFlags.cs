namespace PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;

[Flags]
public enum CacheTriggerFlags
{
    None = 0,
    Inputs = 1,
    Timeline = 2,
    RenderSize = 4,
    All = Inputs | Timeline | RenderSize
}
