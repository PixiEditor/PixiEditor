using ChangeableDocument.Actions;
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

        private Stack<IChange> undoStack = new();
        private Stack<IChange> redoStack = new();

        public DocumentChangeTracker()
        {
            document = new Document();
        }

        private IChangeInfo? InitAndApplyWithUndo(IChange change)
        {
            change.Initialize(document);
            var info = change.Apply(document);
            undoStack.Push(change);
            redoStack.Clear();
            return info;
        }

        private IChangeInfo? MoveStructureMember(Guid member, Guid targetFolder, int index)
        {
            return InitAndApplyWithUndo(new Document_MoveStructureMember_Change(member, targetFolder, index));
        }

        private IChangeInfo? CreateStructureMember(Guid parentGuid, int index, StructureMemberType type)
        {
            return InitAndApplyWithUndo(new Document_CreateStructureMember_Change(parentGuid, index, type));
        }

        private IChangeInfo? DeleteStructureMember(Guid member)
        {
            return InitAndApplyWithUndo(new Document_DeleteStructureMember_Change(member));
        }

        private IChangeInfo? SetStructureMemberVisibility(Guid guid, bool isVisible)
        {
            Document_UpdateStructureMemberProperties_Change change = new(guid)
            {
                NewIsVisible = isVisible,
            };
            return InitAndApplyWithUndo(change);
        }

        private IChangeInfo? SetStructureMemberName(Guid guid, string name)
        {
            Document_UpdateStructureMemberProperties_Change change = new(guid)
            {
                NewName = name,
            };
            return InitAndApplyWithUndo(change);
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
                List<IChangeInfo?> changes = new();
                foreach (var action in actions)
                {
                    switch (action)
                    {
                        case CreateStructureMemberAction act:
                            changes.Add(CreateStructureMember(act.ParentGuid, act.Index, act.Type));
                            break;
                        case MoveStructureMemberAction act:
                            changes.Add(MoveStructureMember(act.Member, act.TargetFolder, act.Index));
                            break;
                        case SetStructureMemberNameAction act:
                            changes.Add(SetStructureMemberName(act.GuidValue, act.Name));
                            break;
                        case SetStructureMemberVisibilityAction act:
                            changes.Add(SetStructureMemberVisibility(act.GuidValue, act.isVisible));
                            break;
                        case DeleteStructureMemberAction act:
                            changes.Add(DeleteStructureMember(act.GuidValue));
                            break;
                        case UndoAction act:
                            changes.Add(Undo());
                            break;
                        case RedoAction act:
                            changes.Add(Redo());
                            break;
                    }
                }
                return changes;
            }).ConfigureAwait(true);
            return result;
        }
    }

    class OperationStateMachine
    {
        public void ExecuteSingularChange(IChange change)
        {

        }

        public void StartUpdateableChange()
        {

        }

        public void EndUpdateableChange()
        {

        }
    }
}
