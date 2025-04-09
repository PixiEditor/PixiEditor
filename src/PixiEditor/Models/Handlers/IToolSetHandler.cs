namespace PixiEditor.Models.Handlers;

internal interface IToolSetHandler : IHandler
{
    public string Name { get; }
    public string Icon { get; }
    public ICollection<IToolHandler> Tools { get; }
    public void ApplyToolSetSettings();
    public IReadOnlyDictionary<IToolHandler, string> IconOverwrites { get; }
}
