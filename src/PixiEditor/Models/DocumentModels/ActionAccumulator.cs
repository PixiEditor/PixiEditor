using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Undo;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Helpers;
using PixiEditor.Models.Rendering;
using PixiEditor.Models.Rendering.RenderInfos;
using PixiEditor.ViewModels.SubViewModels.Document;

namespace PixiEditor.Models.DocumentModels;
#nullable enable
internal class ActionAccumulator
{
    private bool executing = false;

    private List<IAction> queuedActions = new();
    private DocumentViewModel document;
    private DocumentInternalParts internals;

    private CanvasUpdater canvasUpdater;
    private MemberPreviewUpdater previewUpdater;

    public ActionAccumulator(DocumentViewModel doc, DocumentInternalParts internals)
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
            foreach (IChangeInfo info in optimizedChanges)
            {
                internals.Updater.ApplyChangeFromChangeInfo(info);
            }
            if (undoBoundaryPassed)
                internals.Updater.AfterUndoBoundaryPassed();

            // render changes
            // If you are a sane person or maybe just someone who reads WPF documentation, you might think that the reasonable order of operations should be
            // 1. Lock the writeable bitmaps
            // 2. Update their contents
            // 3. Add dirty rectangles
            // 4. Unlock them
            // As it turns out, doing operations in this order leads to WPF render thread crashing in some circumstatances.
            // Then the whole app freezes without throwing any errors, because the UI thread is blocked on a mutex, waiting for the dead render thread.
            // This is despite the order clearly being adviced in the documentations here: https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.imaging.writeablebitmap?view=windowsdesktop-6.0&viewFallbackFrom=net-6.0#remarks
            // Because of that, I'm executing the operations in the order that makes a lot less sense:
            // 1. Update the contents of the bitmaps
            // 2. Lock Them
            // 3. Add dirty rectangles
            // 4. Unlock
            // The locks clearly do nothing useful here, and I'm only calling them because WriteableBitmap checks if it's locked before letting you add dirty rectangles.
            // Really, the locks are supposed to prevent me from updating the bitmap contents in step 1, but they can't since I'm doing direct unsafe memory copying
            // Somehow this all works
            // Also, there is a bug report for this on github https://github.com/dotnet/wpf/issues/5816

            // update the contents of the bitmaps
            var affectedAreas = new AffectedAreasGatherer(internals.Tracker, optimizedChanges);
            List<IRenderInfo> renderResult = new();
            renderResult.AddRange(await canvasUpdater.UpdateGatheredChunks(affectedAreas, undoBoundaryPassed));
            renderResult.AddRange(await previewUpdater.UpdateGatheredChunks(affectedAreas, undoBoundaryPassed));

            // lock bitmaps
            foreach (var (_, bitmap) in document.LazyBitmaps)
            {
                bitmap.Lock();
            }
            if (undoBoundaryPassed)
                LockPreviewBitmaps(document.StructureRoot);

            // add dirty rectangles
            AddDirtyRects(renderResult);

            // unlock bitmaps
            foreach (var (_, bitmap) in document.LazyBitmaps)
            {
                bitmap.Unlock();
            }
            if (undoBoundaryPassed)
                UnlockPreviewBitmaps(document.StructureRoot);

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

    private void LockPreviewBitmaps(FolderViewModel root)
    {
        foreach (var child in root.Children)
        {
            child.PreviewBitmap.Lock();
            if (child.MaskPreviewBitmap is not null)
                child.MaskPreviewBitmap.Lock();
            if (child is FolderViewModel innerFolder)
                LockPreviewBitmaps(innerFolder);
        }
        document.PreviewBitmap.Lock();
    }

    private void UnlockPreviewBitmaps(FolderViewModel root)
    {
        foreach (var child in root.Children)
        {
            child.PreviewBitmap.Unlock();
            if (child.MaskPreviewBitmap is not null)
                child.MaskPreviewBitmap.Unlock();
            if (child is FolderViewModel innerFolder)
                UnlockPreviewBitmaps(innerFolder);
        }
        document.PreviewBitmap.Unlock();
    }

    private void AddDirtyRects(List<IRenderInfo> changes)
    {
        foreach (IRenderInfo renderInfo in changes)
        {
            switch (renderInfo)
            {
                case DirtyRect_RenderInfo info:
                    {
                        var bitmap = document.LazyBitmaps[info.Resolution];
                        RectI finalRect = new RectI(VecI.Zero, new(bitmap.PixelWidth, bitmap.PixelHeight));

                        RectI dirtyRect = new RectI(info.Pos, info.Size).Intersect(finalRect);
                        bitmap.AddDirtyRect(new(dirtyRect.Left, dirtyRect.Top, dirtyRect.Width, dirtyRect.Height));
                    }
                    break;
                case PreviewDirty_RenderInfo info:
                    {
                        var bitmap = document.StructureHelper.Find(info.GuidValue)?.PreviewBitmap;
                        if (bitmap is null)
                            continue;
                        bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                    }
                    break;
                case MaskPreviewDirty_RenderInfo info:
                    {
                        var bitmap = document.StructureHelper.Find(info.GuidValue)?.MaskPreviewBitmap;
                        if (bitmap is null)
                            continue;
                        bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                    }
                    break;
                case CanvasPreviewDirty_RenderInfo:
                    {
                        document.PreviewBitmap.AddDirtyRect(new Int32Rect(0, 0, document.PreviewBitmap.PixelWidth, document.PreviewBitmap.PixelHeight));
                    }
                    break;
            }
        }
    }
}
