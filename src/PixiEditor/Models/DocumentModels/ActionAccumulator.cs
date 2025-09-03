using System.Diagnostics;
using Avalonia.Threading;
using Drawie.Backend.Core;
using PixiEditor.ChangeableDocument;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Actions.Undo;
using PixiEditor.ChangeableDocument.ChangeInfos;
using Drawie.Backend.Core.Bridge;
using PixiEditor.ChangeableDocument.Rendering;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings.PixiEditor;
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

    private MemberPreviewUpdater previewUpdater;

    private bool isChangeBlockActive = false;

    public ActionAccumulator(IDocument doc, DocumentInternalParts internals)
    {
        this.document = doc;
        this.internals = internals;

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

    internal async Task TryExecuteAccumulatedActions()
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
                bool allPassthrough = AreAllPassthrough(toExecute);
                if (allPassthrough)
                {
                    changes = toExecute.Select(a => (IChangeInfo?)a.action).ToList();
                }
                else
                {
                    changes = await internals.Tracker.ProcessActions(toExecute);
                }

                List<IChangeInfo> optimizedChanges = ChangeInfoListOptimizer.Optimize(changes);
                bool undoBoundaryPassed =
                    toExecute.Any(static action =>
                        action.action is ChangeBoundary_Action or Redo_Action or Undo_Action);
                bool viewportRefreshRequest =
                    toExecute.Any(static action => action.action is RefreshViewport_PassthroughAction);
                bool refreshPreviewsRequest =
                    toExecute.Any(static action => action.action is RefreshPreviews_PassthroughAction);
                bool refreshPreviewRequest =
                    toExecute.Any(static action => action.action is RefreshPreview_PassthroughAction);
                bool changeFrameRequest =
                    toExecute.Any(static action => action.action is SetActiveFrame_PassthroughAction);

                foreach (IChangeInfo info in optimizedChanges)
                {
                    internals.Updater.ApplyChangeFromChangeInfo(info);
                }

                if (undoBoundaryPassed)
                    internals.Updater.AfterUndoBoundaryPassed();

                var affectedAreas = new AffectedAreasGatherer(document.AnimationHandler.ActiveFrameTime,
                    internals.Tracker,
                    optimizedChanges, refreshPreviewsRequest);

                bool previewsDisabled = PixiEditorSettings.Performance.DisablePreviews.Value;
                bool updateDelayed = undoBoundaryPassed || viewportRefreshRequest || changeFrameRequest ||
                                     document.SizeBindable.LongestAxis <= LiveUpdatePerformanceThreshold;

                Dictionary<Guid, List<PreviewRenderRequest>>? previewTextures = null;

                if (!previewsDisabled)
                {
                    if (undoBoundaryPassed || viewportRefreshRequest || refreshPreviewsRequest ||
                        refreshPreviewRequest || changeFrameRequest ||
                        document.SizeBindable.LongestAxis <= LiveUpdatePerformanceThreshold)
                    {
                        previewTextures = previewUpdater.GatherPreviewsToUpdate(
                            affectedAreas.ChangedMembers,
                            affectedAreas.ChangedMasks,
                            affectedAreas.ChangedNodes, affectedAreas.ChangedKeyFrames,
                            affectedAreas.IgnoreAnimationPreviews,
                            undoBoundaryPassed || refreshPreviewsRequest);
                    }
                }

                List<Action>? updatePreviewActions = previewTextures?.Values
                    .Select(x => x.Select(r => r.TextureUpdatedAction))
                    .SelectMany(x => x).ToList();

                await document.SceneRenderer.RenderAsync(internals.State.Viewports, affectedAreas.MainImageArea,
                    !previewsDisabled && updateDelayed, previewTextures);

                NotifyUpdatedPreviews(updatePreviewActions);
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

    private static void NotifyUpdatedPreviews(List<Action>? updatePreviewActions)
    {
        if (updatePreviewActions != null)
        {
            foreach (var action in updatePreviewActions)
            {
                action();
            }
        }
    }

    private const int LiveUpdatePerformanceThreshold = 2048;

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
