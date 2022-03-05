using ChangeableDocument;
using ChangeableDocument.Actions;
using ChangeableDocument.ChangeInfos;
using System.Collections.Generic;

namespace PixiEditorPrototype.Models
{
    internal class ActionAccumulator
    {
        private bool executing = false;
        private bool applying = false;

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
            if (applying)
                return;
            queuedActions.Add(action);
            TryExecuteAccumulatedActions();
        }

        public async void TryExecuteAccumulatedActions()
        {
            if (executing || queuedActions.Count == 0)
                return;
            executing = true;
            while (queuedActions.Count > 0)
            {
                var toExecute = queuedActions;
                queuedActions = new List<IAction>();

                var result = await tracker.ProcessActions(toExecute);
                applying = true;
                foreach (IChangeInfo? info in result)
                {
                    documentUpdater.ApplyChangeFromChangeInfo(info);
                }
                applying = false;
            }

            executing = false;
        }
    }
}
