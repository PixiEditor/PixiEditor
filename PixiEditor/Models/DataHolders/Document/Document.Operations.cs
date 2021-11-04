using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Undo;
using SkiaSharp;
using System;
using System.Linq;
using System.Windows;

namespace PixiEditor.Models.DataHolders
{
    public partial class Document
    {
        public event EventHandler<DocumentSizeChangedEventArgs> DocumentSizeChanged;

        /// <summary>
        ///     Resizes canvas to specified width and height to selected anchor.
        /// </summary>
        /// <param name="width">New width of canvas.</param>
        /// <param name="height">New height of canvas.</param>
        /// <param name="anchor">
        ///     Point that will act as "starting position" of resizing. Use pipe to connect horizontal and
        ///     vertical.
        /// </param>
        public void ResizeCanvas(int width, int height, AnchorPoint anchor)
        {
            int oldWidth = Width;
            int oldHeight = Height;

            int offsetX = GetOffsetXForAnchor(Width, width, anchor);
            int offsetY = GetOffsetYForAnchor(Height, height, anchor);

            Thickness[] oldOffsets = Layers.Select(x => x.Offset).ToArray();
            Thickness[] newOffsets = Layers.Select(x => new Thickness(offsetX + x.OffsetX, offsetY + x.OffsetY, 0, 0))
                .ToArray();

            object[] processArgs = { newOffsets, width, height };
            object[] reverseProcessArgs = { oldOffsets, Width, Height };

            ResizeCanvas(newOffsets, width, height);
            UndoManager.AddUndoChange(new Change(
                ResizeCanvasProcess,
                reverseProcessArgs,
                ResizeCanvasProcess,
                processArgs,
                "Resize canvas"));
            DocumentSizeChanged?.Invoke(this, new DocumentSizeChangedEventArgs(oldWidth, oldHeight, width, height));
        }

        public void RotateActiveLayer(float degrees)
        {
            Guid layerGuid = ActiveLayer.LayerGuid;

            object[] processArgs = { layerGuid, degrees };
            object[] reverseProcessArgs = { layerGuid, -degrees };

            RotateLayerProcess(processArgs);

            UndoManager.AddUndoChange(new Change(
                RotateLayerProcess,
                reverseProcessArgs,
                RotateLayerProcess,
                processArgs,
                "Rotate layer"));
        }

        private void RotateLayerProcess(object[] parameters)
        {
            Layer layer = Layers.First(x => x.LayerGuid == (Guid)parameters[0]);
            float degrees = (float)parameters[1];

            using (new SKAutoCanvasRestore(layer.LayerBitmap.SkiaSurface.Canvas, true))
            {
                var copy = layer.LayerBitmap.SkiaSurface.Snapshot();
                var canvas = layer.LayerBitmap.SkiaSurface.Canvas;
                canvas.Clear();
                canvas.RotateDegrees(degrees, layer.MaxWidth / 2, layer.MaxHeight / 2);
                canvas.DrawImage(copy, new SKPoint(0, 0));
                copy.Dispose();
            }

            layer.InvokeLayerBitmapChange();
        }

        /// <summary>
        ///     Resizes all document layers using NearestNeighbor interpolation.
        /// </summary>
        /// <param name="newWidth">New document width.</param>
        /// <param name="newHeight">New document height.</param>
        public void Resize(int newWidth, int newHeight)
        {
            object[] reverseArgs = { Width, Height };
            object[] args = { newWidth, newHeight };
            StorageBasedChange change = new StorageBasedChange(this, Layers);

            ResizeDocument(newWidth, newHeight);

            UndoManager.AddUndoChange(
                change.ToChange(
                    RestoreDocumentLayersProcess,
                    reverseArgs,
                    ResizeDocumentProcess,
                    args,
                    "Resize document"));
        }

        private void RestoreDocumentLayersProcess(Layer[] layers, UndoLayer[] data, object[] args)
        {
            Width = (int)args[0];
            Height = (int)args[1];
            Layers.Clear();
            Layers.AddRange(layers);
        }

        /// <summary>
        ///     Resizes canvas.
        /// </summary>
        /// <param name="offset">Offset of content in new canvas. It will move layer to that offset.</param>
        /// <param name="newWidth">New canvas size.</param>
        /// <param name="newHeight">New canvas height.</param>
        private void ResizeCanvas(Thickness[] offset, int newWidth, int newHeight)
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].Offset = offset[i];
                Layers[i].MaxWidth = newWidth;
                Layers[i].MaxHeight = newHeight;
            }

            Width = newWidth;
            Height = newHeight;
        }

        private void ResizeDocumentProcess(object[] args)
        {
            if (args.Length > 1 && args[0] is int width && args[1] is int height)
            {
                ResizeDocument(width, height);
            }
        }

        private void ResizeDocument(int newWidth, int newHeight)
        {
            int oldWidth = Width;
            int oldHeight = Height;

            for (int i = 0; i < Layers.Count; i++)
            {
                float widthRatio = (float)newWidth / Width;
                float heightRatio = (float)newHeight / Height;
                int layerWidth = Math.Max(1, (int)(Layers[i].Width * widthRatio));
                int layerHeight = Math.Max(1, (int)(Layers[i].Height * heightRatio));

                Layers[i].Resize(layerWidth, layerHeight, newWidth, newHeight);
                Layers[i].Offset = new Thickness(Math.Floor(Layers[i].OffsetX * widthRatio), Math.Floor(Layers[i].OffsetY * heightRatio), 0, 0);
            }

            Height = newHeight;
            Width = newWidth;
            DocumentSizeChanged?.Invoke(
                this,
                new DocumentSizeChangedEventArgs(oldWidth, oldHeight, newWidth, newHeight));
        }

        private void ResizeCanvasProcess(object[] arguments)
        {
            int oldWidth = Width;
            int oldHeight = Height;

            Thickness[] offset = (Thickness[])arguments[0];
            int width = (int)arguments[1];
            int height = (int)arguments[2];
            ResizeCanvas(offset, width, height);
            DocumentSizeChanged?.Invoke(this, new DocumentSizeChangedEventArgs(oldWidth, oldHeight, width, height));
        }
    }
}
