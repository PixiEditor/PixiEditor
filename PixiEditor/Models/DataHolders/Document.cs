using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using PixiEditor.Helpers;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;

namespace PixiEditor.Models.DataHolders
{
    public class Document : NotifyableObject
    {
        private int activeLayerIndex;
        private int height;
        private int width;

        public Document(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public int Width
        {
            get => width;
            set
            {
                width = value;
                RaisePropertyChanged("Width");
            }
        }

        public int Height
        {
            get => height;
            set
            {
                height = value;
                RaisePropertyChanged("Height");
            }
        }

        public ObservableCollection<Layer> Layers { get; set; } = new ObservableCollection<Layer>();

        public Layer ActiveLayer => Layers.Count > 0 ? Layers[ActiveLayerIndex] : null;

        public int ActiveLayerIndex
        {
            get => activeLayerIndex;
            set
            {
                activeLayerIndex = value;
                RaisePropertyChanged("ActiveLayerIndex");
                RaisePropertyChanged("ActiveLayer");
            }
        }

        public ObservableCollection<Color> Swatches { get; set; } = new ObservableCollection<Color>();

        public event EventHandler<DocumentSizeChangedEventArgs> DocumentSizeChanged;

        /// <summary>
        ///     Resizes canvas to specified width and height to selected anchor
        /// </summary>
        /// <param name="width">New width of canvas</param>
        /// <param name="height">New height of canvas</param>
        /// <param name="anchor">
        ///     Point that will act as "starting position" of resizing. Use pipe to connect horizontal and
        ///     vertical.
        /// </param>
        public void ResizeCanvas(int width, int height, AnchorPoint anchor)
        {
            var oldWidth = Width;
            var oldHeight = Height;

            var offsetX = GetOffsetXForAnchor(Width, width, anchor);
            var offsetY = GetOffsetYForAnchor(Height, height, anchor);

            var oldOffsets = Layers.Select(x => x.Offset).ToArray();
            var newOffsets = Layers.Select(x => new Thickness(offsetX + x.OffsetX, offsetY + x.OffsetY, 0, 0))
                .ToArray();

            object[] processArgs = {newOffsets, width, height};
            object[] reverseProcessArgs = {oldOffsets, Width, Height};

            ResizeCanvas(newOffsets, width, height);
            UndoManager.AddUndoChange(new Change(ResizeCanvasProcess,
                reverseProcessArgs, ResizeCanvasProcess, processArgs, "Resize canvas"));
            DocumentSizeChanged?.Invoke(this, new DocumentSizeChangedEventArgs(oldWidth, oldHeight, width, height));
        }

        private int GetOffsetXForAnchor(int srcWidth, int destWidth, AnchorPoint anchor)
        {
            if (anchor.HasFlag(AnchorPoint.Center))
                return Math.Abs(destWidth / 2 - srcWidth / 2);
            if (anchor.HasFlag(AnchorPoint.Right)) return Math.Abs(destWidth - srcWidth);
            return 0;
        }

        private int GetOffsetYForAnchor(int srcHeight, int destHeight, AnchorPoint anchor)
        {
            if (anchor.HasFlag(AnchorPoint.Middle))
                return Math.Abs(destHeight / 2 - srcHeight / 2);
            if (anchor.HasFlag(AnchorPoint.Bottom)) return Math.Abs(destHeight - srcHeight);
            return 0;
        }

        /// <summary>
        ///     Resizes all document layers using NearestNeighbor interpolation.
        /// </summary>
        /// <param name="newWidth">New document width</param>
        /// <param name="newHeight">New document height</param>
        public void Resize(int newWidth, int newHeight)
        {
            object[] reverseArgs = {Width, Height};
            object[] args = {newWidth, newHeight};
            ResizeDocument(args);
            UndoManager.AddUndoChange(new Change(ResizeDocument, reverseArgs,
                ResizeDocument, args, "Resize document"));
        }

        private void ResizeDocument(object[] arguments)
        {
            var oldWidth = Width;
            var oldHeight = Height;

            var newWidth = (int) arguments[0];
            var newHeight = (int) arguments[1];

            for (var i = 0; i < Layers.Count; i++)
            {
                var widthRatio = (float) newWidth / Width;
                var heightRatio = (float) newHeight / Height;
                var layerWidth = (int) (Layers[i].Width * widthRatio);
                var layerHeight = (int) (Layers[i].Height * heightRatio);

                Layers[i].Resize(layerWidth, layerHeight, newWidth, newHeight);
                Layers[i].Offset = new Thickness(Layers[i].OffsetX * widthRatio, Layers[i].OffsetY * heightRatio, 0, 0);
            }

            Height = newHeight;
            Width = newWidth;
            DocumentSizeChanged?.Invoke(this,
                new DocumentSizeChangedEventArgs(oldWidth, oldHeight, newWidth, newHeight));
        }

        private void ResizeCanvasProcess(object[] arguments)
        {
            var oldWidth = Width;
            var oldHeight = Height;

            var offset = (Thickness[]) arguments[0];
            var width = (int) arguments[1];
            var height = (int) arguments[2];
            ResizeCanvas(offset, width, height);
            DocumentSizeChanged?.Invoke(this, new DocumentSizeChangedEventArgs(oldWidth, oldHeight, width, height));
        }

        /// <summary>
        ///     Resizes canvas
        /// </summary>
        /// <param name="offset">Offset of content in new canvas. It will move layer to that offset</param>
        /// <param name="newWidth">New canvas size.</param>
        /// <param name="newHeight">New canvas height.</param>
        private void ResizeCanvas(Thickness[] offset, int newWidth, int newHeight)
        {
            for (var i = 0; i < Layers.Count; i++)
            {
                Layers[i].Offset = offset[i];
                Layers[i].MaxWidth = newWidth;
                Layers[i].MaxHeight = newHeight;
            }

            Width = newWidth;
            Height = newHeight;
        }

        /// <summary>
        ///     Resizes canvas, so it fits exactly the size of drawn content, without any transparent pixels outside.
        /// </summary>
        public void ClipCanvas()
        {
            var points = GetEdgePoints();
            var smallestX = points.Coords1.X;
            var smallestY = points.Coords1.Y;
            var biggestX = points.Coords2.X;
            var biggestY = points.Coords2.Y;

            if (smallestX == 0 && smallestY == 0 && biggestX == 0 && biggestY == 0)
                return;

            var width = biggestX - smallestX;
            var height = biggestY - smallestY;
            var moveVector = new Coordinates(-smallestX, -smallestY);

            var oldOffsets = Layers.Select(x => x.Offset).ToArray();
            var oldWidth = Width;
            var oldHeight = Height;

            MoveOffsets(moveVector);
            Width = width;
            Height = height;

            object[] reverseArguments = {oldOffsets, oldWidth, oldHeight};
            object[] processArguments = {Layers.Select(x => x.Offset).ToArray(), width, height};

            UndoManager.AddUndoChange(new Change(ResizeCanvasProcess, reverseArguments,
                ResizeCanvasProcess, processArguments, "Clip canvas"));
        }

        private DoubleCords GetEdgePoints()
        {
            var firstLayer = Layers[0];
            var smallestX = firstLayer.OffsetX;
            var smallestY = firstLayer.OffsetY;
            var biggestX = smallestX + firstLayer.Width;
            var biggestY = smallestY + firstLayer.Height;

            for (var i = 0; i < Layers.Count; i++)
            {
                Layers[i].ClipCanvas();
                if (Layers[i].OffsetX < smallestX)
                    smallestX = Layers[i].OffsetX;
                if (Layers[i].OffsetX + Layers[i].Width > biggestX) biggestX = Layers[i].OffsetX + Layers[i].Width;

                if (Layers[i].OffsetY < smallestY)
                    smallestY = Layers[i].OffsetY;
                if (Layers[i].OffsetY + Layers[i].Height > biggestY) biggestY = Layers[i].OffsetY + Layers[i].Height;
            }

            return new DoubleCords(new Coordinates(smallestX, smallestY),
                new Coordinates(biggestX, biggestY));
        }

        /// <summary>
        ///     Moves offsets of layers by specified vector.
        /// </summary>
        /// <param name="moveVector"></param>
        private void MoveOffsets(Coordinates moveVector)
        {
            for (var i = 0; i < Layers.Count; i++)
            {
                var offset = Layers[i].Offset;
                Layers[i].Offset = new Thickness(offset.Left + moveVector.X, offset.Top + moveVector.Y, 0, 0);
            }
        }

        private void MoveOffsetsProcess(object[] arguments)
        {
            var vector = (Coordinates) arguments[0];
            MoveOffsets(vector);
        }

        public void CenterContent()
        {
            var points = GetEdgePoints();

            var smallestX = points.Coords1.X;
            var smallestY = points.Coords1.Y;
            var biggestX = points.Coords2.X;
            var biggestY = points.Coords2.Y;

            if (smallestX == 0 && smallestY == 0 && biggestX == 0 && biggestY == 0)
                return;

            var contentCenter = CoordinatesCalculator.GetCenterPoint(points.Coords1, points.Coords2);
            var documentCenter = CoordinatesCalculator.GetCenterPoint(new Coordinates(0, 0),
                new Coordinates(Width, Height));
            var moveVector = new Coordinates(documentCenter.X - contentCenter.X, documentCenter.Y - contentCenter.Y);

            MoveOffsets(moveVector);
            UndoManager.AddUndoChange(new Change(MoveOffsetsProcess, new object[] {new Coordinates(-moveVector.X, -moveVector.Y)}, MoveOffsetsProcess,
                new object[] {moveVector}, "Center content"));
        }
    }

    public class DocumentSizeChangedEventArgs
    {
        public DocumentSizeChangedEventArgs(int oldWidth, int oldHeight, int newWidth, int newHeight)
        {
            OldWidth = oldWidth;
            OldHeight = oldHeight;
            NewWidth = newWidth;
            NewHeight = newHeight;
        }

        public int OldWidth { get; set; }
        public int OldHeight { get; set; }
        public int NewWidth { get; set; }
        public int NewHeight { get; set; }
    }
}