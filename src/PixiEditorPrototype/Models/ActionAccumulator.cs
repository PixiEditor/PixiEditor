using ChangeableDocument;
using ChangeableDocument.Actions;
using ChangeableDocument.ChangeInfos;
using System.Collections.Generic;

namespace PixiEditorPrototype.Models
{
    internal class ActionAccumulator
    {
        private bool executing = false;
        private List<IAction> queuedActions = new();
        private DocumentChangeTracker tracker;
        private DocumentUpdater documentUpdater;

        public ActionAccumulator(DocumentChangeTracker tracker, DocumentUpdater updater)
        {
            this.tracker = tracker;
            this.documentUpdater = updater;
        }

        public void AddAction(IAction action)
        {
            queuedActions.Add(action);
            TryExecuteAccumulatedActions();
        }

        public async void TryExecuteAccumulatedActions()
        {
            if (executing)
                return;
            executing = true;
            var toExecute = queuedActions;
            queuedActions = new List<IAction>();

            var result = await tracker.ProcessActions(toExecute);
            foreach (IChangeInfo? info in result)
            {
                documentUpdater.ApplyChangeFromChangeInfo(info);
            }


            executing = false;
        }
    }
}
