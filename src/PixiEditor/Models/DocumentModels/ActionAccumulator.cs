using System.Collections.Generic;
using System.Linq;
using Avalonia.Platform;
using Avalonia.Threading;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Undo;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Helpers;
using PixiEditor.Models.DocumentPassthroughActions;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Rendering;
using PixiEditor.Models.Rendering.RenderInfos;
using PixiEditor.Numerics;

namespace PixiEditor.Models.DocumentModels;
#nullable enable
internal class ActionAccumulator
{
    private bool executing = false;

    private List<IAction> queuedActions = new();
    private IDocument document;
    private DocumentInternalParts internals;

    private CanvasUpdater canvasUpdater;
    private MemberPreviewUpdater previewUpdater;

    public ActionAccumulator(IDocument doc, DocumentInternalParts internals)
    {
        this.document = doc;
        this.internals = internals;

        canvasUpdater = new(doc, internals);
        previewUpdater = new(doc, internals);
    }

    public void AddFinishedActions(params IAction[] actions)
    {
        queuedActions.AddRange(actions);
        queuedActions.Add(new ChangeBoundary_Action());
        TryExecuteAccumulatedActions();
    }

    public void AddActions(params IAction[] actions)
    {
        queuedActions.AddRange(actions);
        TryExecuteAccumulatedActions();
    }

    private async void TryExecuteAccumulatedActions()
    {
        if (executing || queuedActions.Count == 0)
            return;
        executing = true;
        DispatcherTimer busyTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(2000) };
        busyTimer.Tick += (_, _) =>
        {
            busyTimer.Stop();
            document.Busy = true;
        };
        busyTimer.Start();

        while (queuedActions.Count > 0)
        {
            // select actions to be processed
            var toExecute = queuedActions;
            queuedActions = new List<IAction>();

            // pass them to changeabledocument for processing
            List<IChangeInfo?> changes;
            if (AreAllPassthrough(toExecute))
            {
                changes = toExecute.Select(a => (IChangeInfo?)a).ToList();
            }
            else
            {
                changes = await internals.Tracker.ProcessActions(toExecute);
            }

            // update viewmodels based on changes
            List<IChangeInfo> optimizedChanges = ChangeInfoListOptimizer.Optimize(changes);
            bool undoBoundaryPassed = toExecute.Any(static action => action is ChangeBoundary_Action or Redo_Action or Undo_Action);
            bool viewportRefreshRequest = toExecute.Any(static action => action is RefreshViewport_PassthroughAction);
            foreach (IChangeInfo info in optimizedChanges)
            {
                internals.Updater.ApplyChangeFromChangeInfo(info);
            }
            if (undoBoundaryPassed)
                internals.Updater.AfterUndoBoundaryPassed();

            // update the contents of the bitmaps
            var affectedAreas = new AffectedAreasGatherer(document.AnimationHandler.ActiveFrameTime, internals.Tracker, optimizedChanges);
            List<IRenderInfo> renderResult = new();
            renderResult.AddRange(await canvasUpdater.UpdateGatheredChunks(affectedAreas, undoBoundaryPassed || viewportRefreshRequest));
            renderResult.AddRange(await previewUpdater.UpdateGatheredChunks(affectedAreas, undoBoundaryPassed));

            if (undoBoundaryPassed)
            {
                //ClearDirtyRects();
            }

            // add dirty rectangles
            AddDirtyRects(renderResult);

            // force refresh viewports for better responsiveness
            foreach (var (_, value) in internals.State.Viewports)
            {
                if (!value.Delayed)
                    value.InvalidateVisual();
            }
        }

        busyTimer.Stop();
        if (document.Busy)
            document.Busy = false;
        executing = false;
    }

    private bool AreAllPassthrough(List<IAction> actions)
    {
        foreach (var action in actions)
        {
            if (action is not IChangeInfo)
                return false;
        }
        return true;
    }

    private void AddDirtyRects(List<IRenderInfo> changes)
    {
        foreach (IRenderInfo renderInfo in changes)
        {
            switch (renderInfo)
            {
                case DirtyRect_RenderInfo info:
                    {
                        var bitmap = document.Surfaces[info.Resolution];
                        RectI finalRect = new RectI(VecI.Zero, new(bitmap.Size.X, bitmap.Size.Y));

                        RectI dirtyRect = new RectI(info.Pos, info.Size).Intersect(finalRect);
                        // bitmap.AddDirtyRect(dirtyRect);
                    }
                    break;
                case PreviewDirty_RenderInfo info:
                    {
                        var bitmap = document.StructureHelper.Find(info.GuidValue)?.PreviewSurface;
                        if (bitmap is null)
                            continue;
                        //TODO: Implement dirty rects
                        // bitmap.AddDirtyRect(new RectI(0, 0, bitmap.Size.X, bitmap.Size.Y));
                    }
                    break;
                case MaskPreviewDirty_RenderInfo info:
                    {
                        var bitmap = document.StructureHelper.Find(info.GuidValue)?.MaskPreviewSurface;
                        if (bitmap is null)
                            continue;
                        //bitmap.AddDirtyRect(new RectI(0, 0, bitmap.Size.X, bitmap.Size.Y));
                    }
                    break;
                case CanvasPreviewDirty_RenderInfo:
                    {
                        //document.PreviewSurface.AddDirtyRect(new RectI(0, 0, document.PreviewSurface.Size.X, document.PreviewSurface.Size.Y));
                    }
                    break;
                case NodePreviewDirty_RenderInfo info:
                    {
                        var node = document.StructureHelper.Find(info.NodeId);
                        if (node is null || node.PreviewSurface is null)
                            continue;
                        //node.PreviewSurface.AddDirtyRect(new RectI(0, 0, node.PreviewSurface.Size.X, node.PreviewSurface.Size.Y));
                    }
                    break;
            }
        }
    }
}
