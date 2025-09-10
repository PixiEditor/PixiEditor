namespace PixiEditor.ChangeableDocument.Changeables.Graph.Context;

public class RepeatFuncContext : FuncContext
{
    public int CurrentIteration { get; set; }

    public int TotalIterations { get; }

    public RepeatFuncContext(int totalIterations)
    {
        HasContext = true;
        TotalIterations = totalIterations;
    }
}
