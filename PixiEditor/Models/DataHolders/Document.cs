using PixiEditor.Helpers;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.DirectoryServices;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Media.Imaging;

namespace PixiEditor.Models.DataHolders
{
    [Serializable]
    public class Document : NotifyableObject
    {
        public event EventHandler<DocumentSizeChangedEventArgs> DocumentSizeChanged;

        private int _width;
        public int Width
        {
            get => _width;
            set
            {
                _width = value;
                RaisePropertyChanged("Width");
            }
        }
        private int _height;
        public int Height
        {
            get => _height;
            set
            {
                _height = value;
                RaisePropertyChanged("Height");
            }
        }

        private ObservableCollection<Layer> _layers = new ObservableCollection<Layer>();

        public ObservableCollection<Layer> Layers
        {
            get => _layers;
            set { if (_layers != value) { _layers = value; } }
        }
        public Layer ActiveLayer => Layers.Count > 0 ? Layers[ActiveLayerIndex] : null;

        private int _activeLayerIndex;
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

        public Document(int width, int height)
        {
            Width = width;
            Height = height;
        }


        /// <summary>
        /// Crops canvas at specified x and y offset.
        /// </summary>
        /// <param name="x">X offset</param>
        /// <param name="y">Y offset</param>
        /// <param name="width">New width</param>
        /// <param name="height">New height</param>
        public void Crop(int x, int y, int width, int height)
        {
            int oldWidth = Width;
            int oldHeight = Height;

            object[] reverseArgs = new object[] { 0, 0, x, y, Width, Height, width, height};
            object[] processArgs = new object[] { x, y, 0, 0, width, height };
            ResizeDocumentCanvas(processArgs);
            UndoManager.AddUndoChange(new Change("BitmapManager.ActiveDocument", ResizeDocumentCanvas, 
                reverseArgs, ResizeDocumentCanvas, processArgs, "Crop document"));
        }

        /// <summary>
        /// Resizes canvas to specifid width and height to selected anchor
        /// </summary>
        /// <param name="width">New width of canvas</param>
        /// <param name="height">New height of canvas</param>
        /// <param name="anchor">Point that will act as "starting position" of resizing. Use pipe to connect horizontal and vertical.</param>
        public void ResizeCanvas(int width, int height, AnchorPoint anchor)
        {
            int oldWidth = Width;
            int oldHeight = Height;

            int offsetX = GetOffsetXForAnchor(Width, width, anchor);
            int offsetY = GetOffsetYForAnchor(Height, height, anchor);
            int offsetXSrc = 0;
            int offsetYSrc = 0;
            if(Width > width)
            {
                offsetXSrc = offsetX;
                offsetX = 0;
            }
            if(Height > height)
            {
                offsetYSrc = offsetY;
                offsetY = 0;
            }
            object[] processArgs = new object[] { offsetXSrc, offsetYSrc, offsetX, offsetY, width, height };
            object[] reverseProcessArgs = new object[] {offsetX, offsetY, offsetXSrc, offsetYSrc, Width, Height };

            ResizeCanvas(offsetX, offsetY, offsetXSrc, offsetYSrc, Width, Height, width, height);
            UndoManager.AddUndoChange(new Change("BitmapManager.ActiveDocument", ResizeDocumentCanvas,
                reverseProcessArgs, ResizeDocumentCanvas, processArgs, "Resize canvas"));
            DocumentSizeChanged?.Invoke(this, new DocumentSizeChangedEventArgs(oldWidth, oldHeight, width, height));
        }

        private int GetOffsetXForAnchor(int srcWidth, int destWidth, AnchorPoint anchor)
        {
            if (anchor.HasFlag(AnchorPoint.Center))
            {
                return Math.Abs(destWidth / 2 - srcWidth / 2);
            }
            else if (anchor.HasFlag(AnchorPoint.Right))
            {
                return Math.Abs(destWidth - srcWidth);
            }
            return 0;
        }

        private int GetOffsetYForAnchor(int srcHeight, int destHeight,AnchorPoint anchor)
        {
            if (anchor.HasFlag(AnchorPoint.Middle))
            {
                return Math.Abs(destHeight / 2 - srcHeight / 2);
            }
            else if (anchor.HasFlag(AnchorPoint.Bottom))
            {
                return Math.Abs(destHeight - srcHeight);
            }
            return 0;
        }

        /// <summary>
        /// Resizes all document layers using NearestNeighbor interpolation.
        /// </summary>
        /// <param name="newWidth">New document width</param>
        /// <param name="newHeight">New document height</param>
        public void Resize(int newWidth, int newHeight)
        {
            object[] reverseArgs = new object[] { Width, Height};
            object[] args = new object[] { newWidth, newHeight };
            ResizeDocument(args);
            UndoManager.AddUndoChange(new Change("BitmapManager.ActiveDocument", ResizeDocument, reverseArgs,
                ResizeDocument, args, "Resize document"));
        }

        private void ResizeDocument(object[] arguments)
        {
            int oldWidth = Width;
            int oldHeight = Height;

            int newWidth = (int)arguments[0];
            int newHeight = (int)arguments[1];
            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].LayerBitmap = Layers[i].LayerBitmap.Resize(newWidth, newHeight, WriteableBitmapExtensions.Interpolation.NearestNeighbor);
                Layers[i].Width = newWidth;
                Layers[i].Height = newHeight;
            }
            Height = newHeight;
            Width = newWidth;
            DocumentSizeChanged?.Invoke(this, new DocumentSizeChangedEventArgs(oldWidth, oldHeight, newWidth, newHeight));
        }

        private void ResizeDocumentCanvas(object[] arguments)
        {
            int oldWidth = Width;
            int oldHeight = Height;

            int x = (int)arguments[0];
            int y = (int)arguments[1];
            int destX = (int)arguments[2];
            int destY = (int)arguments[3];
            int width = (int)arguments[4];
            int height = (int)arguments[5];
            ResizeCanvas(destX, destY, x, y, Width, Height, width, height);
            Height = height;
            Width = width;
            DocumentSizeChanged?.Invoke(this, new DocumentSizeChangedEventArgs(oldWidth, oldHeight, width, height));
        }

        private void ResizeCanvas(int offsetX, int offsetY, int offsetXSrc, int offsetYSrc, int oldWidth, int oldHeight, int newWidth, int newHeight)
        {
            int sizeOfArgb = 4;
            int iteratorHeight = oldHeight > newHeight ? newHeight : oldHeight;
            for (int i = 0; i < Layers.Count; i++)
            {
                using (var srcContext = Layers[i].LayerBitmap.GetBitmapContext(ReadWriteMode.ReadOnly))
                {
                    var result = BitmapFactory.New(newWidth, newHeight);
                    using (var destContext = result.GetBitmapContext())
                    {
                        for (int line = 0; line < iteratorHeight; line++)
                        {
                            var srcOff = ((offsetYSrc + line) * oldWidth + offsetXSrc) * sizeOfArgb;
                            var dstOff = ((offsetY + line) * newWidth + offsetX) * sizeOfArgb;
                            BitmapContext.BlockCopy(srcContext, srcOff, destContext, dstOff, oldWidth * sizeOfArgb);
                        }
                        Layers[i].LayerBitmap = result;
                        Layers[i].Width = newWidth;
                        Layers[i].Height = newHeight;
                    }
                }
            }
            Width = newWidth;
            Height = newHeight;
        }

        public void ClipCanvas()
        {
            Coordinates[] smallestPixels = CoordinatesCalculator.GetSmallestPixels(this);
            Coordinates[] biggestPixels = CoordinatesCalculator.GetBiggestPixels(this);

            int smallestX = smallestPixels.Min(x => x.X);
            int smallestY = smallestPixels.Min(x => x.Y);
            int biggestX = biggestPixels.Max(x => x.X);
            int biggestY = biggestPixels.Max(x => x.Y);

            if (smallestX == 0 && smallestY == 0 && biggestX == 0 && biggestY == 0)
                return;

                int width = biggestX - smallestX + 1;
            int height = biggestY - smallestY + 1;
            Crop(smallestX, smallestY, width, height);
        }     
    }

    public class DocumentSizeChangedEventArgs
    {
        public int OldWidth { get; set; }
        public int OldHeight{ get; set; }
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
