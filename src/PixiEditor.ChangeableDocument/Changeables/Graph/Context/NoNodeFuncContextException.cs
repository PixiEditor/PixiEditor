namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public class NoNodeFuncContextException : Exception
{
    public NoNodeFuncContextException() : base("The node field requires context")
    { }
}
