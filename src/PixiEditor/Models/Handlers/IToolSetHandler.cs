namespace PixiEditor.Models.Handlers;

internal interface IToolSetHandler : IHandler
{
    public string Name { get; }
    public string Icon { get; }
    public IList<IToolHandler> Tools { get; }
    public void ApplyToolSetSettings();
    public IReadOnlyDictionary<IToolHandler, string> IconOverwrites { get; }
    public bool IconIsPixiPerfect { get; }
}
