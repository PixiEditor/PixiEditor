using System.Collections.Generic;

namespace PixiEditor.ViewModels.Document.TransformOverlays;

internal class UndoStack<TState> : IDisposable where TState : class
{
    private struct StackItem<TState>
    {
        public TState State { get; set; }

        public StackItem(TState state)
        {
            State = state;
        }
    }

    private Stack<StackItem<TState>> undoStack = new();
    private Stack<StackItem<TState>> redoStack = new();
    private StackItem<TState>? current;

    public void AddState(TState state)
    {
        foreach (var item in redoStack)
            (item.State as IDisposable)?.Dispose();
        redoStack.Clear();
        
        if (current is not null)
            undoStack.Push(current.Value);

        current = new(state);
    }

    public TState? PeekCurrent() => current?.State;

    public int UndoCount => undoStack.Count;
    public int RedoCount => redoStack.Count;

    public TState? Undo()
    {
        if (current is null || undoStack.Count == 0)
            return null;

        DoUndoStep();
        return current.Value.State;
    }

    public TState? Redo()
    {
        if (current is null || redoStack.Count == 0)
            return null;

        DoRedoStep();

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

    public void Dispose()
    {
        foreach (var item in undoStack)
            (item.State as IDisposable)?.Dispose();
        foreach (var item in redoStack)
            (item.State as IDisposable)?.Dispose();
        (current?.State as IDisposable)?.Dispose();

        undoStack.Clear();
        redoStack.Clear();
        current = null;
    }
}
