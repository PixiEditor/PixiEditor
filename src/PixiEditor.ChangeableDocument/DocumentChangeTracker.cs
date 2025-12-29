using System.Diagnostics;
using Drawie.Backend.Core.Bridge;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Undo;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.Changes;

namespace PixiEditor.ChangeableDocument;

public class DocumentChangeTracker : IDisposable
{
    private Document document;
    private bool disposed = false;
    private bool running = false;
    public IReadOnlyDocument Document => document;
    public bool HasSavedUndo => undoStack.Any();
    public bool HasSavedRedo => redoStack.Any();
    public bool IsRunning => running;

    public Guid? LastChangeGuid
    {
        get
        {
            if (!undoStack.Any())
                return null;
            var list = undoStack.Peek();
            if (list.changes.Count == 0)
                return null;
            return list.changes[^1].ChangeGuid;
        }
    }

    public bool IsDisposed => disposed;

    private UpdateableChange? activeUpdateableChange = null;
    private List<(ActionSource source, Change change)>? activePacket = null;

    private Stack<(ActionSource source, List<Change> changes)> undoStack = new();
    private Stack<(ActionSource source, List<Change> changes)> redoStack = new();

    public void Dispose()
    {
        if (running)
            throw new InvalidOperationException("Something is currently being processed");

        if (activeUpdateableChange != null)
        {
            try
            {
                activeUpdateableChange.Apply(document, false, out var _);
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Failed to apply active updateable change {activeUpdateableChange}: {e}");
            }
        }

        if (disposed)
            return;
        disposed = true;

        document.Dispose();

        activeUpdateableChange?.Dispose();

        if (activePacket != null)
        {
            foreach (var change in activePacket)
                change.change.Dispose();
        }

        foreach (var list in undoStack)
        {
            foreach (var change in list.changes)
                change.Dispose();
        }

        foreach (var list in redoStack)
        {
            foreach (var change in list.changes)
                change.Dispose();
        }
    }

    public DocumentChangeTracker()
    {
        document = new Document();
    }

    public DocumentChangeTracker(IReadOnlyDocument doc)
    {
        if(doc is not Document actDoc)
            throw new ArgumentException("Document must be of type Document", nameof(doc));
        document = actDoc;
    }

    private void AddToUndo(Change change, ActionSource source)
    {
        if (activePacket is null)
            activePacket = new();
        activePacket.Add((source, change));

        foreach (var changesToDispose in redoStack)
        {
            foreach (var changeToDispose in changesToDispose.changes)
                changeToDispose.Dispose();
        }

        redoStack.Clear();
    }

    private void CompletePacket(ActionSource source)
    {
        if (activePacket is null)
            return;

        // maybe merge with previous
        if (activePacket.Count == 1 &&
            undoStack.Count > 0 &&
            (undoStack.Peek().source == ActionSource.Automated ||
             (IsHomologous(undoStack.Peek()) &&
              undoStack.Peek().changes[^1].IsMergeableWith(activePacket[0].change))))
        {
            var last = undoStack.Pop();
            last.changes.Add(activePacket[0].change);
            last.source = activePacket.Any(x => x.source == ActionSource.User) ? ActionSource.User : source;
            undoStack.Push(last);
        }
        else
        {
            undoStack.Push(
                (activePacket.Any(x => x.source == ActionSource.User) ? ActionSource.User : source,
                    activePacket.Select(x => x.change).ToList()));
        }

        activePacket = null;
    }

    private bool IsHomologous((ActionSource source, List<Change> changes) changes)
    {
        for (int i = 1; i < changes.changes.Count; i++)
        {
            if (!changes.changes[i].IsMergeableWith(changes.changes[i - 1]))
                return false;
        }

        return true;
    }

    private List<IChangeInfo> Undo()
    {
        if (undoStack.Count == 0)
            return new List<IChangeInfo>();
        if (activePacket is not null || activeUpdateableChange is not null)
        {
            Trace.WriteLine(
                "Attempted to undo while there is an active updateable change or an unfinished undo packet");
            return new List<IChangeInfo>();
        }

        List<IChangeInfo> changeInfos = new();
        bool handledUserChange = false;

        (ActionSource source, List<Change> changes) changePacket;
        do
        {
            changePacket = undoStack.Pop();
            for (int i = changePacket.changes.Count - 1; i >= 0; i--)
            {
                changePacket.changes[i].Revert(document).Switch(
                    (None _) => { },
                    (IChangeInfo info) => changeInfos.Add(info),
                    (List<IChangeInfo> infos) => changeInfos.AddRange(infos));
            }

            if (changePacket.source == ActionSource.User)
                handledUserChange = true;

            redoStack.Push(changePacket);
        } while (undoStack.Count > 0 &&
                 ((changePacket.source == ActionSource.Automated && !handledUserChange)
                  || undoStack.Peek().source == ActionSource.Automated));

        return changeInfos;
    }

    private List<IChangeInfo> Redo()
    {
        if (redoStack.Count == 0)
            return new List<IChangeInfo>();
        if (activePacket is not null || activeUpdateableChange is not null)
        {
            Trace.WriteLine(
                "Attempted to redo while there is an active updateable change or an unfinished undo packet");
            return new List<IChangeInfo>();
        }

        List<IChangeInfo> changeInfos = new();
        (ActionSource source, List<Change> changes) changePacket;
        bool handledUserChange = false;

        do
        {
            changePacket = redoStack.Pop();
            for (int i = 0; i < changePacket.changes.Count; i++)
            {
                changePacket.changes[i].Apply(document, false, out _).Switch(
                    (None _) => { },
                    (IChangeInfo info) => changeInfos.Add(info),
                    (List<IChangeInfo> infos) => changeInfos.AddRange(infos));
            }

            if (changePacket.source == ActionSource.User)
                handledUserChange = true;

            undoStack.Push(changePacket);
        } while (redoStack.Count > 0
                 && ((changePacket.source == ActionSource.Automated && !handledUserChange)
                     || redoStack.Peek().source == ActionSource.Automated));

        return changeInfos;
    }

    private void DeleteAllChanges()
    {
        if (activeUpdateableChange is not null || activePacket is not null)
        {
            Trace.WriteLine(
                "Attempted to delete all changes while there is an active updateable change or an unfinished undo packet");
            return;
        }

        foreach (var changesToDispose in redoStack)
        {
            foreach (var changeToDispose in changesToDispose.changes)
                changeToDispose.Dispose();
        }

        foreach (var changesToDispose in undoStack)
        {
            foreach (var changeToDispose in changesToDispose.changes)
                changeToDispose.Dispose();
        }

        redoStack.Clear();
        undoStack.Clear();
    }

    private OneOf<None, IChangeInfo, List<IChangeInfo>> ProcessMakeChangeAction(IMakeChangeAction act,
        ActionSource source)
    {
        if (activeUpdateableChange is not null && activeUpdateableChange is not InterruptableUpdateableChange)
        {
            Trace.WriteLine($"Attempted to execute make change action {act} while {activeUpdateableChange} is active");
            return new None();
        }

        bool ignoreInUndo = false;
        System.Collections.Generic.List<IChangeInfo> changeInfos = new();

        if (activeUpdateableChange is InterruptableUpdateableChange interruptable)
        {
            var applyInfo = interruptable.Apply(document, false, out ignoreInUndo);
            if (!ignoreInUndo)
                AddToUndo(interruptable, source);
            else
                interruptable.Dispose();

            applyInfo.Switch(
                static (None _) => { },
                (IChangeInfo info) => changeInfos.Add(info),
                (List<IChangeInfo> infos) => changeInfos.AddRange(infos));

            activeUpdateableChange = null;
        }

        var change = act.CreateCorrespondingChange();
        var validationResult = change.InitializeAndValidate(document);
        if (!validationResult)
        {
            string? failedMessage = change.FailedMessage;
            Trace.WriteLine($"Change {change} failed validation. Reason: {failedMessage}");
            change.Dispose();
            return string.IsNullOrEmpty(failedMessage) ? new None() : new ChangeError_Info(failedMessage);
        }

        var info = change.Apply(document, true, out ignoreInUndo);

        info.Switch(
            static (None _) => { },
            (IChangeInfo changeInfo) => changeInfos.Add(changeInfo),
            (List<IChangeInfo> infos) => changeInfos.AddRange(infos));

        if (!ignoreInUndo)
            AddToUndo(change, source);
        else
            change.Dispose();
        return changeInfos;
    }

    private OneOf<None, IChangeInfo, List<IChangeInfo>> ProcessStartOrUpdateChangeAction(IStartOrUpdateChangeAction act,
        ActionSource source)
    {
        if (activeUpdateableChange is null)
        {
            if (CreateUpdateableChange(act, out var processStartOrUpdateChangeAction))
            {
                return processStartOrUpdateChangeAction;
            }
        }
        else if (!act.IsChangeTypeMatching(activeUpdateableChange))
        {
            if (activeUpdateableChange is InterruptableUpdateableChange)
            {
                var applyInfo = activeUpdateableChange.Apply(document, false, out bool ignoreInUndo);
                if (!ignoreInUndo)
                    AddToUndo(activeUpdateableChange, source);
                else
                    activeUpdateableChange.Dispose();

                activeUpdateableChange = null;

                List<IChangeInfo> changeInfos = new();
                applyInfo.Switch(
                    static (None _) => { },
                    (IChangeInfo info) => changeInfos.Add(info),
                    (List<IChangeInfo> infos) => changeInfos.AddRange(infos));

                if (CreateUpdateableChange(act, out var processStartOrUpdateChangeAction))
                {
                    processStartOrUpdateChangeAction.Switch(
                        static (None _) => { },
                        (IChangeInfo info) => changeInfos.Add(info),
                        (List<IChangeInfo> infos) => changeInfos.AddRange(infos));
                }

                return changeInfos;
            }

            Trace.WriteLine(
                $"Tried to start or update a change using action {act} while a change of type {activeUpdateableChange} is active");
            return new None();
        }

        act.UpdateCorrespodingChange(activeUpdateableChange);
        return activeUpdateableChange.ApplyTemporarily(document);
    }

    private bool CreateUpdateableChange(IStartOrUpdateChangeAction act,
        out OneOf<None, IChangeInfo, List<IChangeInfo>> processStartOrUpdateChangeAction)
    {
        var newChange = act.CreateCorrespondingChange();
        var validationResult = newChange.InitializeAndValidate(document);
        if (!validationResult)
        {
            Trace.WriteLine($"Change {newChange} failed validation");
            newChange.Dispose();
            processStartOrUpdateChangeAction = new None();
            return true;
        }

        activeUpdateableChange = newChange;
        processStartOrUpdateChangeAction = new None();
        return false;
    }

    private OneOf<None, IChangeInfo, List<IChangeInfo>> ProcessEndChangeAction(IEndChangeAction act,
        ActionSource source)
    {
        if (activeUpdateableChange is null)
        {
            Trace.WriteLine($"Attempted to end a change using action {act} while no changes are active");
            return new None();
        }

        if (!act.IsChangeTypeMatching(activeUpdateableChange))
        {
            Trace.WriteLine(
                $"Trying to end a change with an action {act} while change {activeUpdateableChange} is active");
            return new None();
        }

        var info = activeUpdateableChange.Apply(document, true, out bool ignoreInUndo);
        if (!ignoreInUndo)
            AddToUndo(activeUpdateableChange, source);
        else
            activeUpdateableChange.Dispose();
        activeUpdateableChange = null;
        return info;
    }

    private List<IChangeInfo?> ProcessActionList(IReadOnlyList<(ActionSource, IAction)> actions)
    {
        List<IChangeInfo?> changeInfos = new();

        void AddInfo(OneOf<None, IChangeInfo, List<IChangeInfo>> info) =>
            info.Switch(
                static (None _) => { },
                (IChangeInfo changeInfo) => changeInfos.Add(changeInfo),
                (List<IChangeInfo> infos) => changeInfos.AddRange(infos));

        foreach (var action in actions)
        {
            switch (action.Item2)
            {
                case IMakeChangeAction act:
                    AddInfo(ProcessMakeChangeAction(act, action.Item1));
                    break;
                case IStartOrUpdateChangeAction act:
                    AddInfo(ProcessStartOrUpdateChangeAction(act, action.Item1));
                    break;
                case IEndChangeAction act:
                    AddInfo(ProcessEndChangeAction(act, action.Item1));
                    break;
                case Undo_Action:
                    AddInfo(Undo());
                    break;
                case Redo_Action:
                    AddInfo(Redo());
                    break;
                case ChangeBoundary_Action:
                    CompletePacket(action.Item1);
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

    public async Task<List<IChangeInfo?>> ProcessActions(List<(ActionSource, IAction)> actions)
    {
        if (disposed)
            throw new ObjectDisposedException(nameof(DocumentChangeTracker));
        if (running)
            throw new InvalidOperationException("Already currently processing");
        running = true;
        var result = await DrawingBackendApi.Current.RenderingDispatcher.InvokeAsync(() => ProcessActionList(actions));
        running = false;
        return result;
    }

    public List<IChangeInfo?> ProcessActionsSync(IReadOnlyList<(ActionSource, IAction)> actions)
    {
        if (disposed)
            throw new ObjectDisposedException(nameof(DocumentChangeTracker));
        if (running)
            throw new InvalidOperationException("Already currently processing");
        running = true;
        try
        {
            var result = ProcessActionList(actions);
            return result;
        }
        finally
        {
            running = false;
        }
    }
}

public enum ActionSource
{
    User,
    Automated
}
