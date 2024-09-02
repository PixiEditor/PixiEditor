namespace PixiEditor.Models.Handlers;

internal interface IToolSetHandler : IHandler
{
    public ICollection<IToolHandler> Tools { get; }
}
