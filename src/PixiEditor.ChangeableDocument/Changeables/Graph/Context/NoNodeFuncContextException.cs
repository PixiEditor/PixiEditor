namespace PixiEditor.ChangeableDocument.Changeables.Graph.Context;

public class NoNodeFuncContextException : Exception
{
    public NoNodeFuncContextException() : base("The node field requires context")
    { }
}
