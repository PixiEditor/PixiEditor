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
    public class Document : NotifyableObject, IEquatable<Document>
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
            UndoManager.AddUndoChange(new Change( ResizeDocument, reverseArgs,
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
                float widthRatio = (float)newWidth / Width;
                float heightRatio = (float)newHeight / Height;
                int layerWidth = (int)(Layers[i].Width * widthRatio);
                int layerHeight = (int)(Layers[i].Height * heightRatio);

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
            DoubleCords points = GetEdgePoints();
            int smallestX = points.Coords1.X;
            int smallestY = points.Coords1.Y;
            int biggestX = points.Coords2.X;
            int biggestY = points.Coords2.Y;

            if (smallestX == 0 && smallestY == 0 && biggestX == 0 && biggestY == 0)
                return;

            int width = biggestX - smallestX;
            int height = biggestY - smallestY;
            var moveVector = new Coordinates(-smallestX, -smallestY);

            Thickness[] oldOffsets = Layers.Select(x => x.Offset).ToArray();
            int oldWidth = Width;
            int oldHeight = Height;

            MoveOffsets(moveVector);
            Width = width;
            Height = height;

            object[] reverseArguments = { oldOffsets, oldWidth, oldHeight };
            object[] processArguments = { Layers.Select(x => x.Offset).ToArray(), width, height};

            UndoManager.AddUndoChange(new Change(ResizeCanvasProcess, reverseArguments, 
                ResizeCanvasProcess, processArguments, "Clip canvas"));
        }

        private DoubleCords GetEdgePoints()
        {
            var firstLayer = Layers[0];
            int smallestX = firstLayer.OffsetX;
            int smallestY = firstLayer.OffsetY;
            int biggestX = smallestX + firstLayer.Width;
            int biggestY = smallestY + firstLayer.Height;

            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].ClipCanvas();
                if (Layers[i].OffsetX < smallestX)
                    smallestX = Layers[i].OffsetX;
                if (Layers[i].OffsetX + Layers[i].Width > biggestX)
                {
                    biggestX = Layers[i].OffsetX + Layers[i].Width;
                }

                if (Layers[i].OffsetY < smallestY)
                    smallestY = Layers[i].OffsetY;
                if (Layers[i].OffsetY + Layers[i].Height > biggestY)
                {
                    biggestY = Layers[i].OffsetY + Layers[i].Height;
                }
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
            for (int i = 0; i < Layers.Count; i++)
            {
                var offset = Layers[i].Offset;
                Layers[i].Offset = new Thickness(offset.Left + moveVector.X, offset.Top + moveVector.Y, 0,0 );
            }
        }

        private void MoveOffsetsProcess(object[] arguments)
        {
            Coordinates vector = (Coordinates) arguments[0];
            MoveOffsets(vector);
        }

        public void CenterContent()
        {
            DoubleCords points = GetEdgePoints();

            int smallestX = points.Coords1.X;
            int smallestY = points.Coords1.Y;
            int biggestX = points.Coords2.X;
            int biggestY = points.Coords2.Y;

            if (smallestX == 0 && smallestY == 0 && biggestX == 0 && biggestY == 0)
                return;

            var contentCenter = CoordinatesCalculator.GetCenterPoint(points.Coords1, points.Coords2);
            var documentCenter = CoordinatesCalculator.GetCenterPoint(new Coordinates(0, 0),
                new Coordinates(Width, Height));
            Coordinates moveVector = new Coordinates(documentCenter.X - contentCenter.X, documentCenter.Y - contentCenter.Y);

            MoveOffsets(moveVector);
            UndoManager.AddUndoChange(new Change(MoveOffsetsProcess, new object[]{ new Coordinates(-moveVector.X, -moveVector.Y) }, MoveOffsetsProcess, 
                new object[]{moveVector}, "Center content"));
        }

        public Document Clone()
        {
            Document clone = new Document(Width, Height)
            {
                Layers = new ObservableCollection<Layer>(Layers.Select(l => l.Clone())),
                ActiveLayerIndex = ActiveLayerIndex,
                Swatches = new ObservableCollection<Color>(Swatches.Select(s => Color.FromArgb(s.A, s.R, s.G, s.B)))
            };
            return clone;
        }

        public bool Equals(Document other)
        {
            if (other == null)
            {
                return false;
            }

            if (other.Width != Width && other.Height != Height)
            {
                return false;
            }

            foreach(Layer layer in Layers)
            {
                Layer otherLayer = other.Layers.FirstOrDefault(l => l.Name == layer.Name);
                if (otherLayer == null)
                {
                    return false;
                }

                if (!layer.Equals(otherLayer))
                {
                    return false;
                }
            }

            return true;
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