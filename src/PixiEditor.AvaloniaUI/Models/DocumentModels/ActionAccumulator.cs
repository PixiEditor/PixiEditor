using System.Collections.Generic;
using System.Linq;
using Avalonia.Platform;
using Avalonia.Threading;
using PixiEditor.AvaloniaUI.Helpers;
using PixiEditor.AvaloniaUI.Models.DocumentPassthroughActions;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.AvaloniaUI.Models.Rendering;
using PixiEditor.AvaloniaUI.Models.Rendering.RenderInfos;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Undo;
using PixiEditor.ChangeableDocument.ChangeInfos;

namespace PixiEditor.AvaloniaUI.Models.DocumentModels;
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
                changes = toExecute.Select(a => (IChangeInfo?)a).ToList();
            else
                changes = await internals.Tracker.ProcessActions(toExecute);

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
            var affectedAreas = new AffectedAreasGatherer(internals.Tracker, optimizedChanges);
            List<IRenderInfo> renderResult = new();
            renderResult.AddRange(await canvasUpdater.UpdateGatheredChunks(affectedAreas, undoBoundaryPassed || viewportRefreshRequest));
            renderResult.AddRange(await previewUpdater.UpdateGatheredChunks(affectedAreas, undoBoundaryPassed));

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
        //TODO: Avalonia doesn't seem to have a way to add dirty rects to bitmaps
        /*foreach (IRenderInfo renderInfo in changes)
        {
            switch (renderInfo)
            {
                case DirtyRect_RenderInfo info:
                    {
                        var bitmap = document.LazyBitmaps[info.Resolution];
                        RectI finalRect = new RectI(VecI.Zero, new(bitmap.PixelSize.Width, bitmap.PixelSize.Height));

                        RectI dirtyRect = new RectI(info.Pos, info.Size).Intersect(finalRect);
                        bitmap.AddDirtyRect(new(dirtyRect.Left, dirtyRect.Top, dirtyRect.Width, dirtyRect.Height));
                    }
                    break;
                case PreviewDirty_RenderInfo info:
                    {
                        var bitmap = document.StructureHelper.Find(info.GuidValue)?.PreviewBitmap;
                        if (bitmap is null)
                            continue;
                        bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelSize.Width, bitmap.PixelSize.Height));
                    }
                    break;
                case MaskPreviewDirty_RenderInfo info:
                    {
                        var bitmap = document.StructureHelper.Find(info.GuidValue)?.MaskPreviewBitmap;
                        if (bitmap is null)
                            continue;
                        bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelSize.Width, bitmap.PixelSize.Height));
                    }
                    break;
                case CanvasPreviewDirty_RenderInfo:
                    {
                        document.PreviewBitmap.AddDirtyRect(new Int32Rect(0, 0, document.PreviewBitmap.PixelSize.Width, document.PreviewBitmap.PixelSize.Height));
                    }
                    break;
            }
        }*/
    }
}
