using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Undo;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditor.ChangeableDocument.Changes;

namespace ChangeableDocument
{
    public class DocumentChangeTracker
    {
        private Document document;
        public IReadOnlyDocument Document => document;

        private UpdateableChange? activeChange = null;
        private List<Change>? activePacket = null;

        private Stack<List<Change>> undoStack = new();
        private Stack<List<Change>> redoStack = new();

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
            if (activePacket is not null || activeChange is not null)
                throw new InvalidOperationException("Cannot undo while there is an active updateable change or an unfinished undo packet");
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
            if (activePacket is not null || activeChange is not null)
                throw new InvalidOperationException("Cannot redo while there is an active updateable change or an unfinished undo packet");
            List<IChangeInfo?> changeInfos = new();
            List<Change> changePacket = redoStack.Pop();

            for (int i = 0; i < changePacket.Count; i++)
                changeInfos.Add(changePacket[i].Apply(document, out _));

            undoStack.Push(changePacket);
            return changeInfos;
        }

        private void DeleteAllChanges()
        {
            if (activeChange is not null || activePacket is not null)
                throw new InvalidOperationException("Cannot delete all changes while there is an active updateable change or an unfinished undo packet");
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
            if (activeChange is not null)
                throw new InvalidOperationException("Can't make a change while another change is active");
            var change = act.CreateCorrespondingChange();
            change.Initialize(document);
            var info = change.Apply(document, out bool ignoreInUndo);
            if (!ignoreInUndo)
                AddToUndo(change);
            else
                change.Dispose();
            return info;
        }

        private IChangeInfo? ProcessStartOrUpdateChangeAction(IStartOrUpdateChangeAction act)
        {
            if (activeChange is null)
            {
                activeChange = act.CreateCorrespondingChange();
                activeChange.Initialize(document);
            }
            act.UpdateCorrespodingChange(activeChange);
            return activeChange.ApplyTemporarily(document);
        }

        private IChangeInfo? ProcessEndChangeAction(IEndChangeAction act)
        {
            if (activeChange is null)
                throw new InvalidOperationException("Can't end a change: no changes are active");
            if (!act.IsChangeTypeMatching(activeChange))
                throw new InvalidOperationException($"Trying to end a change with an action of type {act.GetType()} while a change of type {activeChange.GetType()} is active");

            var info = activeChange.Apply(document, out bool ignoreInUndo);
            if (!ignoreInUndo)
                AddToUndo(activeChange);
            else
                activeChange.Dispose();
            activeChange = null;
            return info;
        }

        private List<IChangeInfo?> ProcessActionList(List<IAction> actions)
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

        public async Task<List<IChangeInfo?>> ProcessActions(List<IAction> actions)
        {
            return await Task.Run(() => ProcessActionList(actions)).ConfigureAwait(true);
        }

        public List<IChangeInfo?> ProcessActionsSync(List<IAction> actions)
        {
            return ProcessActionList(actions);
        }
    }
}
