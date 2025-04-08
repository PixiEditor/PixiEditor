using Avalonia.Threading;
using PixiEditor.ChangeableDocument;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Actions.Undo;
using PixiEditor.ChangeableDocument.ChangeInfos;
using Drawie.Backend.Core.Bridge;
using PixiEditor.Helpers;
using PixiEditor.Models.DocumentPassthroughActions;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Rendering;

namespace PixiEditor.Models.DocumentModels;
#nullable enable
internal class ActionAccumulator
{
    private bool executing = false;

    private List<(ActionSource source, IAction action)> queuedActions = new();
    private IDocument document;
    private DocumentInternalParts internals;

    private CanvasUpdater canvasUpdater;
    private MemberPreviewUpdater previewUpdater;

    private bool isChangeBlockActive = false;

    public ActionAccumulator(IDocument doc, DocumentInternalParts internals)
    {
        this.document = doc;
        this.internals = internals;

        canvasUpdater = new(doc, internals);
        previewUpdater = new(doc, internals);
    }

    public void StartChangeBlock()
    {
        if (isChangeBlockActive)
            throw new InvalidOperationException("Change block is already active");

        isChangeBlockActive = true;
    }

    public void EndChangeBlock()
    {
        isChangeBlockActive = false;
        queuedActions.Add((ActionSource.Automated, new ChangeBoundary_Action()));
        TryExecuteAccumulatedActions();
    }

    public void AddFinishedActions(params IAction[] actions)
    {
        foreach (var action in actions)
        {
            queuedActions.Add((ActionSource.User, action));
        }

        if (!isChangeBlockActive)
        {
            queuedActions.Add((ActionSource.Automated, new ChangeBoundary_Action()));
            TryExecuteAccumulatedActions();
        }
    }

    public void AddActions(params IAction[] actions)
    {
        foreach (var action in actions)
        {
            queuedActions.Add((ActionSource.User, action));
        }

        if (!isChangeBlockActive)
        {
            TryExecuteAccumulatedActions();
        }
    }

    public void AddActions(ActionSource source, IAction action)
    {
        queuedActions.Add((source, action));
        TryExecuteAccumulatedActions();
    }

    internal void TryExecuteAccumulatedActions()
    {
        if (executing || queuedActions.Count == 0)
            return;
        executing = true;
        /*DispatcherTimer busyTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(2000) };
        busyTimer.Tick += (_, _) =>
        {
            busyTimer.Stop();
            document.Busy = true;
        };
        busyTimer.Start();*/

        try
        {
            while (queuedActions.Count > 0)
            {
                var toExecute = queuedActions;
                queuedActions = new();

                List<IChangeInfo?> changes;
                if (AreAllPassthrough(toExecute))
                {
                    changes = toExecute.Select(a => (IChangeInfo?)a.action).ToList();
                }
                else
                {
                    changes = internals.Tracker.ProcessActionsSync(toExecute);
                }

                List<IChangeInfo> optimizedChanges = ChangeInfoListOptimizer.Optimize(changes);
                bool undoBoundaryPassed =
                    toExecute.Any(static action =>
                        action.action is ChangeBoundary_Action or Redo_Action or Undo_Action);
                bool viewportRefreshRequest =
                    toExecute.Any(static action => action.action is RefreshViewport_PassthroughAction);
                bool changeFrameRequest =
                    toExecute.Any(static action => action.action is SetActiveFrame_PassthroughAction);
                foreach (IChangeInfo info in optimizedChanges)
                {
                    internals.Updater.ApplyChangeFromChangeInfo(info);
                }

                if (undoBoundaryPassed)
                    internals.Updater.AfterUndoBoundaryPassed();

                // update the contents of the bitmaps
                var affectedAreas = new AffectedAreasGatherer(document.AnimationHandler.ActiveFrameTime,
                    internals.Tracker,
                    optimizedChanges);
                if (DrawingBackendApi.Current.IsHardwareAccelerated)
                {
                    canvasUpdater.UpdateGatheredChunksSync(affectedAreas,
                        undoBoundaryPassed || viewportRefreshRequest);
                }
                /*else
                {
                    await canvasUpdater.UpdateGatheredChunks(affectedAreas,
                        undoBoundaryPassed || viewportRefreshRequest);
                }*/

                previewUpdater.UpdatePreviews(
                    affectedAreas.ImagePreviewAreas.Keys,
                    affectedAreas.MaskPreviewAreas.Keys,
                    affectedAreas.ChangedNodes, affectedAreas.ChangedKeyFrames);

                // force refresh viewports for better responsiveness
                foreach (var (_, value) in internals.State.Viewports)
                {
                    if (!value.Delayed)
                        value.InvalidateVisual();
                }
            }
        }
        catch (Exception e)
        {
            //busyTimer.Stop();
            document.Busy = false;
            executing = false;
#if DEBUG
            Console.WriteLine(e);
#endif
            CrashHelper.SendExceptionInfo(e);
            throw;
        }

        //busyTimer.Stop();
        if (document.Busy)
            document.Busy = false;
        executing = false;
    }

    private bool AreAllPassthrough(List<(ActionSource, IAction)> actions)
    {
        foreach (var action in actions)
        {
            if (action.Item2 is not IChangeInfo)
                return false;
        }

        return true;
    }
}
