using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
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

        public void RotateActiveDocument(float degrees)
        {
            object[] processArgs = { degrees };
            object[] reverseProcessArgs = { -degrees };

            RotateDocumentProcess(processArgs);

            UndoManager.AddUndoChange(new Change(
                RotateDocumentProcess,
                reverseProcessArgs,
                RotateDocumentProcess,
                processArgs,
                "Rotate layer"));
        }

        public void FlipActiveDocument(FlipType flip)
        {
            object[] processArgs = { flip };
            object[] reverseProcessArgs = { flip == FlipType.Horizontal ? FlipType.Vertical : FlipType.Horizontal };

            FlipDocumentProcess(processArgs);

            UndoManager.AddUndoChange(new Change(
                FlipDocumentProcess,
                reverseProcessArgs,
                FlipDocumentProcess,
                processArgs,
                $"Flip layer: {flip}"));
        }

        private void FlipDocumentProcess(object[] processArgs)
        {
            FlipType flip = (FlipType)processArgs[0];
            foreach (var layer in Layers)
            {
                using (new SKAutoCanvasRestore(layer.LayerBitmap.SkiaSurface.Canvas, true))
                {
                    var copy = layer.LayerBitmap.SkiaSurface.Snapshot();
                    layer.LayerBitmap.SkiaSurface.Canvas.Clear();

                    var canvas = layer.LayerBitmap.SkiaSurface.Canvas;

                    layer.ClipCanvas();

                    if (flip == FlipType.Horizontal)
                    {
                        canvas.Translate(layer.Width, 0);
                        canvas.Scale(-1, 1, 0, 0);
                    }
                    else
                    {
                        canvas.Translate(0, layer.Width);
                        canvas.Scale(1, -1, 0, 0);
                    }

                    // Flip offset based on document and layer center point
                    var documentCenter = new Coordinates(Width / 2, Height / 2);
                    var layerCenter = new Coordinates(layer.Width / 2, layer.Height / 2);

                    int newOffsetX = layer.OffsetX;
                    int newOffsetY = layer.OffsetY;

                    if (flip == FlipType.Horizontal)
                    {
                        newOffsetX += layerCenter.X;
                        int diff = documentCenter.X - newOffsetX;
                        newOffsetX = layer.OffsetX + (diff * 2);
                    }
                    else if(flip == FlipType.Vertical)
                    {
                        newOffsetY += layerCenter.Y;
                        int diff = documentCenter.Y - newOffsetY;
                        newOffsetY = layer.OffsetY + (diff * 2);
                    }

                    layer.Offset = new Thickness(newOffsetX, newOffsetY, 0, 0);

                    canvas.DrawImage(copy, default(SKPoint));
                    copy.Dispose();
                }

                layer.InvokeLayerBitmapChange();
            }
        }

        private void RotateDocumentProcess(object[] parameters)
        {
            float degrees = (float)parameters[0];

            int oldWidth = Width;
            int oldHeight = Height;

            int biggerMaxSize = Math.Max(Width, Height);


            foreach (var layer in Layers)
            {
                using (new SKAutoCanvasRestore(layer.LayerBitmap.SkiaSurface.Canvas, true))
                {
                    var copy = layer.LayerBitmap.SkiaSurface.Snapshot();
                    layer.LayerBitmap.SkiaSurface.Canvas.Clear();

                    double radians = Math.PI * degrees / 180;
                    float sine = (float)Math.Abs(Math.Sin(radians));
                    float cosine = (float)Math.Abs(Math.Cos(radians));
                    int originalWidth = layer.Width;
                    int originalHeight = layer.Height;
                    int rotatedWidth = (int)(cosine * originalWidth + sine * originalHeight);
                    int rotatedHeight = (int)(cosine * originalHeight + sine * originalWidth);

                    layer.CreateNewBitmap(rotatedWidth, rotatedHeight);

                    var surface = layer.LayerBitmap.SkiaSurface.Canvas;

                    surface.Translate(rotatedWidth / 2, rotatedHeight / 2);
                    surface.RotateDegrees((float)degrees);
                    surface.Translate(-originalWidth / 2, -originalHeight / 2);
                    surface.DrawImage(copy, default(SKPoint));

                    layer.MaxHeight = oldWidth;
                    layer.MaxWidth = oldHeight;

                    copy.Dispose();
                }

                layer.InvokeLayerBitmapChange();
            }

            Height = oldWidth;
            Width = oldHeight;
            DocumentSizeChanged?.Invoke(
                this,
                new DocumentSizeChangedEventArgs(oldWidth, oldHeight, Width, Height));
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
