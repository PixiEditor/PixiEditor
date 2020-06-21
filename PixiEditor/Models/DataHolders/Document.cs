using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixiEditor.Helpers;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.Tools;
using PixiEditor.ViewModels;

namespace PixiEditor.Models.DataHolders
{
    public class Document : NotifyableObject
    {
        public int Width
        {
            get => _width;
            set
            {
                _width = value;
                RaisePropertyChanged("Width");
            }
        }

        public int Height
        {
            get => _height;
            set
            {
                _height = value;
                RaisePropertyChanged("Height");
            }
        }

        public ObservableCollection<Layer> Layers { get; set; } = new ObservableCollection<Layer>();

        public Layer ActiveLayer => Layers.Count > 0 ? Layers[ActiveLayerIndex] : null;

        public int ActiveLayerIndex
        {
            get => _activeLayerIndex;
            set
            {
                _activeLayerIndex = value;
                RaisePropertyChanged("ActiveLayerIndex");
                RaisePropertyChanged("ActiveLayer");
            }
        }

        public ObservableCollection<Color> Swatches { get; set; } = new ObservableCollection<Color>();
        private int _activeLayerIndex;
        private int _height;
        private int _width;

        public Document(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public event EventHandler<DocumentSizeChangedEventArgs> DocumentSizeChanged;


        /// <summary>
        ///     Crops canvas at specified x and y offset.
        /// </summary>
        /// <param name="x">X offset</param>
        /// <param name="y">Y offset</param>
        /// <param name="width">New width</param>
        /// <param name="height">New height</param>
        public void Crop(int x, int y, int width, int height)
        {
            object[] reverseArgs = {0, 0, x, y, Width, Height, width, height};
            object[] processArgs = {x, y, 0, 0, width, height};
            ResizeDocumentCanvas(processArgs);
            UndoManager.AddUndoChange(new Change("BitmapManager.ActiveDocument", ResizeDocumentCanvas,
                reverseArgs, ResizeDocumentCanvas, processArgs, "Crop document"));
        }

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
            int oldWidth = Width;
            int oldHeight = Height;

            int offsetX = GetOffsetXForAnchor(Width, width, anchor);
            int offsetY = GetOffsetYForAnchor(Height, height, anchor);

            Thickness[] oldOffsets = Layers.Select(x => x.Offset).ToArray();
            Thickness[] newOffsets = Layers.Select(x => new Thickness(offsetX + x.OffsetX, offsetY + x.OffsetY, 0, 0))
                .ToArray();

                object[] processArgs = {newOffsets, width, height};
            object[] reverseProcessArgs = { oldOffsets, Width, Height};

            ResizeCanvas(newOffsets, width, height);
            UndoManager.AddUndoChange(new Change("BitmapManager.ActiveDocument", ResizeDocumentCanvas,
                reverseProcessArgs, ResizeDocumentCanvas, processArgs, "Resize canvas"));
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
            UndoManager.AddUndoChange(new Change("BitmapManager.ActiveDocument", ResizeDocument, reverseArgs,
                ResizeDocument, args, "Resize document"));
        }

        private void ResizeDocument(object[] arguments)
        {
            int oldWidth = Width;
            int oldHeight = Height;

            int newWidth = (int) arguments[0];
            int newHeight = (int) arguments[1];
            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].LayerBitmap = Layers[i].LayerBitmap.Resize(newWidth, newHeight,
                    WriteableBitmapExtensions.Interpolation.NearestNeighbor);
                Layers[i].Width = newWidth;
                Layers[i].Height = newHeight;
            }

            Height = newHeight;
            Width = newWidth;
            DocumentSizeChanged?.Invoke(this,
                new DocumentSizeChangedEventArgs(oldWidth, oldHeight, newWidth, newHeight));
        }

        private void ResizeDocumentCanvas(object[] arguments)
        {
            int oldWidth = Width;
            int oldHeight = Height;

            Thickness[] offset = (Thickness[])arguments[0];
            int width = (int) arguments[1];
            int height = (int) arguments[2];
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
            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].Offset = offset[i];
            }

            Width = newWidth;
            Height = newHeight;
        }

        private DoubleCords GetEdgePoints()
        {
            Coordinates[] smallestPixels = CoordinatesCalculator.GetSmallestPixels(this);
            Coordinates[] biggestPixels = CoordinatesCalculator.GetBiggestPixels(this);

            int smallestX = smallestPixels.Min(x => x.X);
            int smallestY = smallestPixels.Min(x => x.Y);
            int biggestX = biggestPixels.Max(x => x.X);
            int biggestY = biggestPixels.Max(x => x.Y);
            return new DoubleCords(new Coordinates(smallestX, smallestY), new Coordinates(biggestX, biggestY));
        }

        /// <summary>
        ///     Resizes canvas, so it fits exactly the size of drawn content, without any transparent pixels outside.
        /// </summary>
        public void ClipCanvas()
        {
            var points = GetEdgePoints();
            int smallestX = points.Coords1.X;
            int smallestY = points.Coords1.Y;
            int biggestX = points.Coords2.X;
            int biggestY = points.Coords2.Y;

            if (smallestX == 0 && smallestY == 0 && biggestX == 0 && biggestY == 0)
                return;

            int width = biggestX - smallestX + 1;
            int height = biggestY - smallestY + 1;
            Crop(smallestX, smallestY, width, height);
        }

        public void CenterContent()
        {
            var points = GetEdgePoints();

            int smallestX = points.Coords1.X;
            int smallestY = points.Coords1.Y;
            int biggestX = points.Coords2.X;
            int biggestY = points.Coords2.Y;

            if (smallestX == 0 && smallestY == 0 && biggestX == 0 && biggestY == 0)
                return;

            var contentCenter = CoordinatesCalculator.GetCenterPoint(points.Coords1, points.Coords2);
            var documentCenter = CoordinatesCalculator.GetCenterPoint(new Coordinates(0, 0),
                new Coordinates(Width - 1, Height - 1));
            MoveTool move = new MoveTool
            {
                MoveAll = true,
                RequiresPreviewLayer = false
            };
            ViewModelMain.Current.BitmapManager.BitmapOperations.ExecuteTool(documentCenter,
                new List<Coordinates>(new[] {contentCenter, documentCenter}), move);
            ViewModelMain.Current.TriggerNewUndoChange(move);
        }
    }

    public class DocumentSizeChangedEventArgs
    {
        public int OldWidth { get; set; }
        public int OldHeight { get; set; }
        public int NewWidth { get; set; }
        public int NewHeight { get; set; }

        public DocumentSizeChangedEventArgs(int oldWidth, int oldHeight, int newWidth, int newHeight)
        {
            OldWidth = oldWidth;
            OldHeight = oldHeight;
            NewWidth = newWidth;
            NewHeight = newHeight;
        }
    }
}