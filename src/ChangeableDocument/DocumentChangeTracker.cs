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

        private IUpdateableChange? activeChange = null;

        private Stack<IChange> undoStack = new();
        private Stack<IChange> redoStack = new();

        public DocumentChangeTracker()
        {
            document = new Document();
        }

        private void AddToUndo(IChange change)
        {
            undoStack.Push(change);
            redoStack.Clear();
        }

        private IChangeInfo? Undo()
        {
            if (undoStack.Count == 0)
                return null;
            IChange change = undoStack.Pop();
            var info = change.Revert(document);
            redoStack.Push(change);
            return info;
        }

        private IChangeInfo? Redo()
        {
            if (redoStack.Count == 0)
                return null;
            IChange change = redoStack.Pop();
            var info = change.Apply(document);
            undoStack.Push(change);
            return info;
        }

        public async Task<List<IChangeInfo?>> ProcessActions(List<IAction> actions)
        {
            List<IChangeInfo?> result = await Task.Run(() =>
            {
                List<IChangeInfo?> changeInfos = new();
                foreach (var action in actions)
                {
                    switch (action)
                    {
                        case IMakeChangeAction act:
                            if (activeChange != null)
                                throw new Exception("Can't make a change while another change is active");
                            var change = act.CreateCorrespondingChange();
                            change.Initialize(document);
                            changeInfos.Add(change.Apply(document));
                            AddToUndo(change);
                            break;
                        case IStartOrUpdateChangeAction act:
                            if (activeChange == null)
                            {
                                activeChange = act.CreateCorrespondingChange();
                                activeChange.Initialize(document);
                            }
                            act.UpdateCorrespodingChange(activeChange);
                            changeInfos.Add(activeChange.ApplyTemporarily(document));
                            break;
                        case IEndChangeAction act:
                            if (activeChange == null)
                                throw new Exception("Can't end a change: no changes are active");
                            if (!act.IsChangeTypeMatching(activeChange))
                                throw new Exception($"Trying to end a change via action of type {act.GetType()} while a change of type {activeChange.GetType()} is active");
                            changeInfos.Add(activeChange.Apply(document));
                            AddToUndo(activeChange);
                            activeChange = null;
                            break;
                        case UndoAction act:
                            changeInfos.Add(Undo());
                            break;
                        case RedoAction act:
                            changeInfos.Add(Redo());
                            break;
                        default:
                            throw new Exception("Unknown action type");
                    }
                }
                return changeInfos;
            }).ConfigureAwait(true);
            return result;
        }
    }
}
