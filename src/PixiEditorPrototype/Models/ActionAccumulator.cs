using System.Collections.Generic;
using System.Linq;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Undo;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditorPrototype.Models.Rendering;
using PixiEditorPrototype.Models.Rendering.RenderInfos;
using PixiEditorPrototype.ViewModels;
using SkiaSharp;

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
            var toExecute = queuedActions;
            queuedActions = new List<IAction>();

            List<IChangeInfo?> result = AreAllPassthrough(toExecute) ?
                toExecute.Select(a => (IChangeInfo?)a).ToList() :
                await helpers.Tracker.ProcessActions(toExecute);

            foreach (IChangeInfo? info in result)
            {
                helpers.Updater.ApplyChangeFromChangeInfo(info);
            }

            foreach (var (_, bitmap) in document.Bitmaps)
            {
                bitmap.Lock();
            }

            var renderResult = await renderer.ProcessChanges(result);
            AddDirtyRects(renderResult);

            foreach (var (_, bitmap) in document.Bitmaps)
            {
                bitmap.Unlock();
            }

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

    private void AddDirtyRects(List<IRenderInfo> changes)
    {
        foreach (IRenderInfo info in changes)
        {
            if (info is not DirtyRect_RenderInfo dirtyRectInfo)
                continue;
            var bitmap = document.Bitmaps[dirtyRectInfo.Resolution];
            SKRectI finalRect = SKRectI.Create(0, 0, bitmap.PixelWidth, bitmap.PixelHeight);

            SKRectI dirtyRect = SKRectI.Create(dirtyRectInfo.Pos, dirtyRectInfo.Size);
            dirtyRect.Intersect(finalRect);
            bitmap.AddDirtyRect(new(dirtyRect.Left, dirtyRect.Top, dirtyRect.Width, dirtyRect.Height));
        }
    }
}
