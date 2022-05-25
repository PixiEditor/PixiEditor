using System.Diagnostics;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Undo;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Changes;

namespace ChangeableDocument;

public class DocumentChangeTracker : IDisposable
{
    private Document document;
    private bool disposed = false;
    private bool running = false;
    public IReadOnlyDocument Document => document;

    private UpdateableChange? activeUpdateableChange = null;
    private List<Change>? activePacket = null;

    private Stack<List<Change>> undoStack = new();
    private Stack<List<Change>> redoStack = new();

    public void Dispose()
    {
        if (running)
            throw new InvalidOperationException("Something is currently being processed");
        if (disposed)
            return;
        disposed = true;

        document.Dispose();

        activeUpdateableChange?.Dispose();

        if (activePacket != null)
            foreach (var change in activePacket)
                change.Dispose();

        foreach (var list in undoStack)
            foreach (var change in list)
                change.Dispose();

        foreach (var list in redoStack)
            foreach (var change in list)
                change.Dispose();
    }

    public DocumentChangeTracker()
    {
        document = new Document();
    }

    private void AddToUndo(Change change)
    {
        if (activePacket is null)
            activePacket = new();
        activePacket.Add(change);

        foreach (var changesToDispose in redoStack)
            foreach (var changeToDispose in changesToDispose)
                changeToDispose.Dispose();
        redoStack.Clear();
    }

    private void CompletePacket()
    {
        if (activePacket is null)
            return;

        // maybe merge with previous
        if (activePacket.Count == 1 &&
            undoStack.Count > 0 &&
            IsHomologous(undoStack.Peek()) &&
            undoStack.Peek()[^1].IsMergeableWith(activePacket[0]))
        {
            undoStack.Peek().Add(activePacket[0]);
        }
        else
        {
            undoStack.Push(activePacket);
        }

        activePacket = null;
    }

    private bool IsHomologous(List<Change> changes)
    {
        for (int i = 1; i < changes.Count; i++)
        {
            if (!changes[i].IsMergeableWith(changes[i - 1]))
                return false;
        }
        return true;
    }

    private List<IChangeInfo?> Undo()
    {
        if (undoStack.Count == 0)
            return new List<IChangeInfo?>();
        if (activePacket is not null || activeUpdateableChange is not null)
        {
            Trace.WriteLine("Attempted to undo while there is an active updateable change or an unfinished undo packet");
            return new List<IChangeInfo?>();
        }
        List<IChangeInfo?> changeInfos = new();
        List<Change> changePacket = undoStack.Pop();

        for (int i = changePacket.Count - 1; i >= 0; i--)
            changeInfos.Add(changePacket[i].Revert(document));

        redoStack.Push(changePacket);
        return changeInfos;
    }

    private List<IChangeInfo?> Redo()
    {
        if (redoStack.Count == 0)
            return new List<IChangeInfo?>();
        if (activePacket is not null || activeUpdateableChange is not null)
        {
            Trace.WriteLine("Attempted to redo while there is an active updateable change or an unfinished undo packet");
            return new List<IChangeInfo?>();
        }
        List<IChangeInfo?> changeInfos = new();
        List<Change> changePacket = redoStack.Pop();

        for (int i = 0; i < changePacket.Count; i++)
            changeInfos.Add(changePacket[i].Apply(document, out _));

        undoStack.Push(changePacket);
        return changeInfos;
    }

    private void DeleteAllChanges()
    {
        if (activeUpdateableChange is not null || activePacket is not null)
        {
            Trace.WriteLine("Attempted to delete all changes while there is an active updateable change or an unfinished undo packet");
            return;
        }
        foreach (var changesToDispose in redoStack)
            foreach (var changeToDispose in changesToDispose)
                changeToDispose.Dispose();
        foreach (var changesToDispose in undoStack)
            foreach (var changeToDispose in changesToDispose)
                changeToDispose.Dispose();
        redoStack.Clear();
        undoStack.Clear();
    }

    private IChangeInfo? ProcessMakeChangeAction(IMakeChangeAction act)
    {
        if (activeUpdateableChange is not null)
        {
            Trace.WriteLine($"Attempted to execute make change action {act} while {activeUpdateableChange} is active");
            return null;
        }
        var change = act.CreateCorrespondingChange();
        var validationResult = change.InitializeAndValidate(document);
        if (validationResult.IsT1)
        {
            Trace.WriteLine($"Change {change} failed validation");
            change.Dispose();
            return null;
        }

        var info = change.Apply(document, out bool ignoreInUndo);
        if (!ignoreInUndo)
            AddToUndo(change);
        else
            change.Dispose();
        return info;
    }

    private IChangeInfo? ProcessStartOrUpdateChangeAction(IStartOrUpdateChangeAction act)
    {
        if (activeUpdateableChange is null)
        {
            var newChange = act.CreateCorrespondingChange();
            var validationResult = newChange.InitializeAndValidate(document);
            if (validationResult.IsT1)
            {
                Trace.WriteLine($"Change {newChange} failed validation");
                newChange.Dispose();
                return null;
            }
            activeUpdateableChange = newChange;
        }
        else if (!act.IsChangeTypeMatching(activeUpdateableChange))
        {
            Trace.WriteLine($"Tried to start or update a change using action {act} while a change of type {activeUpdateableChange} is active");
            return null;
        }
        act.UpdateCorrespodingChange(activeUpdateableChange);
        return activeUpdateableChange.ApplyTemporarily(document);
    }

    private IChangeInfo? ProcessEndChangeAction(IEndChangeAction act)
    {
        if (activeUpdateableChange is null)
        {
            Trace.WriteLine($"Attempted to end a change using action {act} while no changes are active");
            return null;
        }
        if (!act.IsChangeTypeMatching(activeUpdateableChange))
        {
            Trace.WriteLine($"Trying to end a change with an action {act} while change {activeUpdateableChange} is active");
            return null;
        }

        var info = activeUpdateableChange.Apply(document, out bool ignoreInUndo);
        if (!ignoreInUndo)
            AddToUndo(activeUpdateableChange);
        else
            activeUpdateableChange.Dispose();
        activeUpdateableChange = null;
        return info;
    }

    private List<IChangeInfo?> ProcessActionList(IReadOnlyList<IAction> actions)
    {
        List<IChangeInfo?> changeInfos = new();
        foreach (var action in actions)
        {
            switch (action)
            {
                case IMakeChangeAction act:
                    changeInfos.Add(ProcessMakeChangeAction(act));
                    break;
                case IStartOrUpdateChangeAction act:
                    changeInfos.Add(ProcessStartOrUpdateChangeAction(act));
                    break;
                case IEndChangeAction act:
                    changeInfos.Add(ProcessEndChangeAction(act));
                    break;
                case Undo_Action act:
                    changeInfos.AddRange(Undo());
                    break;
                case Redo_Action act:
                    changeInfos.AddRange(Redo());
                    break;
                case ChangeBoundary_Action:
                    CompletePacket();
                    break;
                case DeleteRecordedChanges_Action:
                    DeleteAllChanges();
                    break;
                //used for "passthrough" actions (move viewport)
                case IChangeInfo act:
                    changeInfos.Add(act);
                    break;
            }
        }
        return changeInfos;
    }

    public async Task<List<IChangeInfo?>> ProcessActions(IReadOnlyList<IAction> actions)
    {
        if (disposed)
            throw new ObjectDisposedException(nameof(DocumentChangeTracker));
        if (running)
            throw new InvalidOperationException("Already currently processing");
        running = true;
        var result = await Task.Run(() => ProcessActionList(actions)).ConfigureAwait(true);
        running = false;
        return result;
    }

    public List<IChangeInfo?> ProcessActionsSync(IReadOnlyList<IAction> actions)
    {
        if (disposed)
            throw new ObjectDisposedException(nameof(DocumentChangeTracker));
        if (running)
            throw new InvalidOperationException("Already currently processing");
        running = true;
        var result = ProcessActionList(actions);
        running = false;
        return result;
    }
}
