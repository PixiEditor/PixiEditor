using PixiEditor.Helpers;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Undo;
using SkiaSharp;
using System;
using System.Diagnostics;
using System.Windows;

namespace PixiEditor.Models.DataHolders
{
    [DebuggerDisplay("{SelectedPoints.Count} selected Pixels")]
    public class Selection : NotifyableObject
    {
        public bool isEmpty { get; private set; } = true;

        private readonly SKColor selectionBlue = new SKColor(142, 202, 255, 255);
        private Layer selectionLayer = new Layer("_selectionLayer");
        private Document owner;

        public Selection(Document owner)
        {
            this.owner = owner;
        }

        public Layer SelectionLayer
        {
            get => selectionLayer;
            private set
            {
                selectionLayer = value;
                RaisePropertyChanged(nameof(SelectionLayer));
            }
        }

        public void TranslateSelection(int dX, int dY)
        {
            selectionLayer.Offset = new Thickness(selectionLayer.OffsetX + dX, selectionLayer.OffsetY + dY, 0, 0);
        }

        public void SetSelectionWithUndo(Int32Rect rect, SelectionShape shape, SelectionType mode)
        {
            Int32Rect updateRect;
            if (mode == SelectionType.New)
                updateRect = rect.Expand(new(selectionLayer.OffsetX, selectionLayer.OffsetY, selectionLayer.Width, selectionLayer.Height));
            else
                updateRect = rect;
            LayerChunk chunk = new LayerChunk(selectionLayer, new(updateRect.X, updateRect.Y, updateRect.X + updateRect.Width, updateRect.Y + updateRect.Height));
            StorageBasedChange change = new(owner, new LayerChunk[] { chunk });

            DrawSelection(rect, shape, mode);

            owner.UndoManager.AddUndoChange(change.ToChange(SelectionUndoProcess, new object[] { owner }, "Update selection"));
        }

        private void DrawSelection(Int32Rect rect, SelectionShape shape, SelectionType mode)
        {
            using SKPaint paint = new()
            {
                Color = selectionBlue,
                BlendMode = SKBlendMode.Src,
                Style = SKPaintStyle.StrokeAndFill,
            };
            switch (mode)
            {
                case SelectionType.New:
                    SelectionLayer.Reset();
                    break;
                case SelectionType.Subtract:
                    paint.Color = SKColors.Transparent;
                    break;
            }

            if (!rect.HasArea)
                return;
            isEmpty = false;
            SelectionLayer.DynamicResizeAbsolute(new Int32Rect(rect.X, rect.Y, rect.Width, rect.Height));
            if (shape == SelectionShape.Circle)
            {
                float cx = rect.X + rect.Width / 2f;
                float cy = rect.Y + rect.Height / 2f;
                SelectionLayer.LayerBitmap.SkiaSurface.Canvas.DrawOval(cx, cy, rect.Width / 2f, rect.Height / 2f, paint);
            }
            else if (shape == SelectionShape.Rectangle)
            {
                SelectionLayer.LayerBitmap.SkiaSurface.Canvas.DrawRect(rect.X, rect.Y, rect.Width, rect.Height, paint);
            }
            else
            {
                throw new NotImplementedException($"Selection shape '{shape}' has not been implemented");
            }
        }

        private static void SelectionUndoProcess(Layer[] layers, UndoLayer[] data, object[] args)
        {
            if (args.Length > 0 && args[0] is Document document)
            {
                for (int i = 0; i < layers.Length; i++)
                {
                    Layer layer = layers[i];
                    UndoLayer layerData = data[i];
                    var foundLayer = document.ActiveSelection.SelectionLayer;
                    StorageBasedChange.ApplyChunkToLayer(foundLayer, layerData, layer.LayerBitmap);
                }
            }
        }

        public void ClearWithUndo()
        {
            SKRectI updateRect = new SKRectI(selectionLayer.OffsetX, selectionLayer.OffsetY, selectionLayer.OffsetX + selectionLayer.Width, selectionLayer.OffsetY + selectionLayer.Height);
            StorageBasedChange change = new(owner, new LayerChunk[] { new(selectionLayer, updateRect) });
            SelectionLayer.Reset();
            isEmpty = true;
            owner.UndoManager.AddUndoChange(change.ToChange(SelectionUndoProcess, new object[] { owner }, "Clear selection"));
        }
    }
}
