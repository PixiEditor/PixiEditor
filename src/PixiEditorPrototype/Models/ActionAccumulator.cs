using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions;
using PixiEditor.ChangeableDocument.Actions.Undo;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditorPrototype.Models.Rendering;
using PixiEditorPrototype.Models.Rendering.RenderInfos;
using PixiEditorPrototype.ViewModels;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;

namespace PixiEditorPrototype.Models
{
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

            renderer = new(helpers);
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

                var (bitmap, surface) = GetCorrespondingBitmap(document.RenderResolution);
                bitmap.Lock();

                var renderResult = await renderer.ProcessChanges(
                    result!,
                    surface,
                    document.RenderResolution);
                AddDirtyRects(bitmap, renderResult);

                bitmap.Unlock();
                document.ForceRefreshView();
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

        private (WriteableBitmap, SKSurface) GetCorrespondingBitmap(ChunkResolution res)
        {
            var result = res switch
            {
                ChunkResolution.Full => (document.BitmapFull, document.SurfaceFull),
                ChunkResolution.Half => (document.BitmapHalf, document.SurfaceHalf),
                ChunkResolution.Quarter => (document.BitmapQuarter, document.SurfaceQuarter),
                ChunkResolution.Eighth => (document.BitmapEighth, document.SurfaceEighth),
                _ => (document.BitmapFull, document.SurfaceFull),
            };
            if (result.Item1 is null || result.Item2 is null)
                throw new InvalidOperationException("Trying to get a bitmap of a non existing resolution");
            return result!;
        }

        private static void AddDirtyRects(WriteableBitmap bitmap, List<IRenderInfo> changes)
        {
            SKRectI finalRect = SKRectI.Create(0, 0, bitmap.PixelWidth, bitmap.PixelHeight);
            foreach (IRenderInfo info in changes)
            {
                if (info is DirtyRect_RenderInfo dirtyRectInfo)
                {
                    SKRectI dirtyRect = SKRectI.Create(dirtyRectInfo.Pos, dirtyRectInfo.Size);
                    dirtyRect.Intersect(finalRect);
                    bitmap.AddDirtyRect(new(dirtyRect.Left, dirtyRect.Top, dirtyRect.Width, dirtyRect.Height));
                }
            }
        }
    }
}
