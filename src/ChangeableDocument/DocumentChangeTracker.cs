using ChangeableDocument.Actions;
using ChangeableDocument.Actions.Undo;
using ChangeableDocument.Changeables;
using ChangeableDocument.Changeables.Interfaces;
using ChangeableDocument.ChangeInfos;
using ChangeableDocument.Changes;

namespace ChangeableDocument
{
    public class DocumentChangeTracker
    {
        private Document document;
        public IReadOnlyDocument Document => document;

        private UpdateableChange? activeChange = null;

        private Stack<List<Change>> undoStack = new();
        private Stack<List<Change>> redoStack = new();

        public DocumentChangeTracker()
        {
            document = new Document();
        }

        private void AddToUndo(Change change)
        {
            List<Change> targetPacket = GetOrCreatePacket(change);
            targetPacket.Add(change);

            foreach (var changesToDispose in redoStack)
                foreach (var changeToDispose in changesToDispose)
                    changeToDispose.Dispose();
            redoStack.Clear();
        }

        private List<Change> GetOrCreatePacket(Change change)
        {
            if (undoStack.Count != 0 && change.IsMergeableWith(undoStack.Peek()[^1]))
                return undoStack.Peek();
            var newPacket = new List<Change>();
            undoStack.Push(newPacket);
            return newPacket;
        }

        private List<IChangeInfo?> Undo()
        {
            if (undoStack.Count == 0)
                return new List<IChangeInfo?>();
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
            List<IChangeInfo?> changeInfos = new();
            List<Change> changePacket = redoStack.Pop();

            for (int i = 0; i < changePacket.Count; i++)
                changeInfos.Add(changePacket[i].Apply(document, out _));

            undoStack.Push(changePacket);
            return changeInfos;
        }

        private IChangeInfo? ProcessMakeChangeAction(IMakeChangeAction act)
        {
            if (activeChange != null)
                throw new Exception("Can't make a change while another change is active");
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
            if (activeChange == null)
            {
                activeChange = act.CreateCorrespondingChange();
                activeChange.Initialize(document);
            }
            act.UpdateCorrespodingChange(activeChange);
            return activeChange.ApplyTemporarily(document);
        }

        private IChangeInfo? ProcessEndChangeAction(IEndChangeAction act)
        {
            if (activeChange == null)
                throw new Exception("Can't end a change: no changes are active");
            if (!act.IsChangeTypeMatching(activeChange))
                throw new Exception($"Trying to end a change via action of type {act.GetType()} while a change of type {activeChange.GetType()} is active");

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
                    default:
                        throw new Exception("Unknown action type");
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
