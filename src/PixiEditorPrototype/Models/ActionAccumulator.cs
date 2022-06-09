using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Undo;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditorPrototype.Models.Rendering;
using PixiEditorPrototype.Models.Rendering.RenderInfos;
using PixiEditorPrototype.ViewModels;

namespace PixiEditorPrototype.Models;

internal class ActionAccumulator
{
    private bool executing = false;

    private List<IAction> queuedActions = new();
    private DocumentViewModel document;
    private DocumentHelpers helpers;

    private WriteableBitmapUpdater renderer;

    public ActionAccumulator(DocumentViewModel doc, DocumentHelpers helpers)
    {
        this.document = doc;
        this.helpers = helpers;

        renderer = new(doc, helpers);
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
                changes = await helpers.Tracker.ProcessActions(toExecute);

            // update viewmodels based on changes
            foreach (IChangeInfo? info in changes)
            {
                helpers.Updater.ApplyChangeFromChangeInfo(info);
            }

            // lock bitmaps that need to be updated
            var affectedChunks = new AffectedChunkGatherer(helpers.Tracker, changes);

            foreach (var (_, bitmap) in document.Bitmaps)
            {
                bitmap.Lock();
            }
            bool refreshDelayed = toExecute.Any(static action => action is ChangeBoundary_Action or Redo_Action or Undo_Action);
            if (refreshDelayed)
                LockPreviewBitmaps(document.StructureRoot);

            // update bitmaps
            var renderResult = await renderer.UpdateGatheredChunks(affectedChunks, refreshDelayed);
            AddDirtyRects(renderResult);

            // unlock bitmaps
            foreach (var (_, bitmap) in document.Bitmaps)
            {
                bitmap.Unlock();
            }
            if (refreshDelayed)
                UnlockPreviewBitmaps(document.StructureRoot);

            // force refresh viewports for better responsiveness
            foreach (var (_, value) in helpers.State.Viewports)
            {
                value.InvalidateVisual();
            }
        }

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
    }

    private void AddDirtyRects(List<IRenderInfo> changes)
    {
        foreach (IRenderInfo renderInfo in changes)
        {
            switch (renderInfo)
            {
                case DirtyRect_RenderInfo info:
                    {
                        var bitmap = document.Bitmaps[info.Resolution];
                        RectI finalRect = new RectI(VecI.Zero, new(bitmap.PixelWidth, bitmap.PixelHeight));

                        RectI dirtyRect = new RectI(info.Pos, info.Size).Intersect(finalRect);
                        bitmap.AddDirtyRect(new(dirtyRect.Left, dirtyRect.Top, dirtyRect.Width, dirtyRect.Height));
                    }
                    break;
                case PreviewDirty_RenderInfo info:
                    {
                        var bitmap = helpers.StructureHelper.Find(info.GuidValue)?.PreviewBitmap;
                        if (bitmap is null)
                            continue;
                        bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                    }
                    break;
                case MaskPreviewDirty_RenderInfo info:
                    {
                        var bitmap = helpers.StructureHelper.Find(info.GuidValue)?.MaskPreviewBitmap;
                        if (bitmap is null)
                            continue;
                        bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                    }
                    break;
            }
        }

    }
}
