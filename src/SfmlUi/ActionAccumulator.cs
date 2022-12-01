using System.Diagnostics;
using System.Windows;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Undo;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.DrawingApi.Core.Surface;
using SfmlUi.Rendering;

namespace SfmlUi;

internal class ActionAccumulator
{
    private bool executing = false;

    private List<IAction> queuedActions = new();

    private WriteableBitmapUpdater renderer;
    private readonly DocumentViewModel document;

    public ActionAccumulator(DocumentViewModel document)
    {
        Dictionary<ChunkResolution, DrawingSurface> docSurfaces = new()
        {
            [ChunkResolution.Full] = document.Textures[ChunkResolution.Full].Surface,
            [ChunkResolution.Half] = document.Textures[ChunkResolution.Half].Surface,
            [ChunkResolution.Quarter] = document.Textures[ChunkResolution.Quarter].Surface,
            [ChunkResolution.Eighth] = document.Textures[ChunkResolution.Eighth].Surface,
        };

        renderer = new(docSurfaces, document);
        this.document = document;
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

    private void TryExecuteAccumulatedActions()
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
            List<IChangeInfo?> changes = document.Tracker.ProcessActionsSync(toExecute);

            // update viewmodels based on changes
            bool undoBoundaryPassed = toExecute.Any(static action => action is ChangeBoundary_Action or Redo_Action or Undo_Action);
            //foreach (IChangeInfo? info in changes)
            {
                //internals.Updater.ApplyChangeFromChangeInfo(info);
            }

            // render changes
            // update the contents of the bitmaps
            var affectedChunks = new AffectedChunkGatherer(document.Tracker, changes);
            var renderResult = renderer.Render(affectedChunks, document.Viewport?.VisibleArea ?? new RectD(), ChunkResolution.Full);
            
            // add dirty rectangles
            AddDirtyRects(renderResult);

            // update bitmaps
            foreach (var (_, texture) in document.Textures)
            {
                texture.UpdateTextureFromBuffer();
            }
        }

        executing = false;
    }

    private void AddDirtyRects(List<DirtyRect_RenderInfo> changes)
    {
        foreach (DirtyRect_RenderInfo renderInfo in changes)
        {
            var texture = document.Textures[renderInfo.Resolution];
            RectI finalRect = new RectI(VecI.Zero, texture.Size);
            RectI dirtyRect = new RectI(renderInfo.Pos, renderInfo.Size).Intersect(finalRect);
            texture.AddDirtyRect(dirtyRect);
        }
    }
}
