using ChangeableDocument;
using ChangeableDocument.Actions;
using ChangeableDocument.ChangeInfos;
using ChunkyImageLib.DataHolders;
using PixiEditorPrototype.Models.Rendering;
using PixiEditorPrototype.Models.Rendering.RenderInfos;
using PixiEditorPrototype.ViewModels;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace PixiEditorPrototype.Models
{
    internal class ActionAccumulator
    {
        private bool executing = false;

        private List<IAction> queuedActions = new();
        private DocumentChangeTracker tracker;
        private DocumentViewModel document;
        private DocumentUpdater documentUpdater;
        private WriteableBitmapUpdater renderer;

        public ActionAccumulator(DocumentChangeTracker tracker, DocumentUpdater updater, DocumentViewModel document)
        {
            this.tracker = tracker;
            this.documentUpdater = updater;
            this.document = document;

            renderer = new(tracker);
        }

        public void AddAction(IAction action)
        {
            queuedActions.Add(action);
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

                var result = await tracker.ProcessActions(toExecute);
                foreach (IChangeInfo? info in result)
                {
                    documentUpdater.ApplyChangeFromChangeInfo(info);
                }

                var (bitmap, surface) = GetCorrespondingBitmap(document.RenderResolution);
                bitmap.Lock();

                var renderResult = await renderer.ProcessChanges(
                    result!,
                    surface,
                    document.RenderResolution);
                AddDirtyRects(bitmap, renderResult);

                bitmap.Unlock();
                document.View?.ForceRefreshFinalImage();
            }

            executing = false;
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
