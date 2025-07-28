namespace PixiEditor.ViewModels.Document.TransformOverlays;
internal class TransformOverlayUndoStack<TState> where TState : struct
{
    private struct StackItem<TState>
    {
        public TState State { get; set; }
        public TransformOverlayStateType Type { get; set; }

        public StackItem(TState state, TransformOverlayStateType type)
        {
            State = state;
            Type = type;
        }
    }

    private Stack<StackItem<TState>> undoStack = new();
    private Stack<StackItem<TState>> redoStack = new();
    private StackItem<TState>? current;

    public void AddState(TState state, TransformOverlayStateType type)
    {
        redoStack.Clear();
        if (current is not null)
            undoStack.Push(current.Value);

        current = new(state, type);
    }

    public TState? PeekCurrent() => current?.State;
    
    public int UndoCount => undoStack.Count;
    public int RedoCount => redoStack.Count; 

    public TState? Undo()
    {
        if (current is null || undoStack.Count == 0)
            return null;

        while (true)
        {
            TransformOverlayStateType oldType = current.Value.Type;
            DoUndoStep();
            TransformOverlayStateType newType = current.Value.Type;
            if (oldType != newType || !oldType.IsMergeable() || undoStack.Count == 0)
                break;
        }
        return current.Value.State;
    }

    public TState? Redo()
    {
        if (current is null || redoStack.Count == 0)
            return null;

        while (true)
        {
            TransformOverlayStateType oldType = current.Value.Type;
            DoRedoStep();
            TransformOverlayStateType newType = current.Value.Type;
            if (oldType != newType || !oldType.IsMergeable() || redoStack.Count == 0)
                break;
        }
        return current.Value.State;
    }

    private void DoUndoStep()
    {
        redoStack.Push(current.Value);
        current = undoStack.Pop();
    }

    private void DoRedoStep()
    {
        undoStack.Push(current.Value);
        current = redoStack.Pop();
    }
}
