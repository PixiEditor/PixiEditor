namespace PixiEditor.ChangeableDocument.Changeables.Graph.Interfaces;

public class NoNodeFieldContextException : Exception
{
    public NoNodeFieldContextException() : base("The node field requires context")
    { }
}
