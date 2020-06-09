using PixiEditor.Helpers;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using System;
using System.Collections.ObjectModel;
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

        public void Crop(int x, int y, int width, int height)
        {
            object[] reverseArgs = new object[] { x, y, Width, Height, width, height};
            CropDocument(x, y, width, height);
            UndoManager.AddUndoChange(new Change("BitmapManager.ActiveDocument", ReverseCrop, 
                reverseArgs, this, "Crop document"));
            DocumentSizeChanged?.Invoke(this, new DocumentSizeChangedEventArgs(Width, Height, width, height));
        }

        public void ResizeCanvas(int width, int height, AnchorPoint anchor)
        {
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
            ResizeCanvas(offsetX, offsetY, offsetXSrc, offsetYSrc, Width, width, height);
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

        public void Resize(int newWidth, int newHeight)
        {
            int oldWidth = Width;
            int oldHeight = Height;
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

        private void CropDocument(int x, int y, int width, int height)
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].LayerBitmap = Layers[i].LayerBitmap.Crop(x, y, width, height);
                Layers[i].Width = width;
                Layers[i].Height = height;
            }
            Height = height;
            Width = width;
        }

        private void ResizeCanvas(int offsetX, int offsetY, int offsetXSrc, int offsetYSrc, int oldWidth, int newWidth, int newHeight)
        {
            int sizeOfArgb = 4;
            for (int i = 0; i < Layers.Count; i++)
            {
                using (var srcContext = Layers[i].LayerBitmap.GetBitmapContext(ReadWriteMode.ReadOnly))
                {
                    var result = BitmapFactory.New(newWidth, newHeight);
                    using (var destContext = result.GetBitmapContext())
                    {
                        for (int line = 0; line < oldWidth; line++)
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

        private void ReverseCrop(object[] arguments) //Reverses process of cropping
        {
            int offsetX = (int)arguments[0];
            int offsetY = (int)arguments[1];
            int oldWidth = (int)arguments[2]; //oldWidth is the width before first crop
            int oldHeight = (int)arguments[3];
            int newWidth = (int)arguments[4]; //newWidth is the width after first crop
            int newHeight = (int)arguments[5];
            if (offsetX < 0) offsetX = 0;
            if (offsetX + newWidth > oldWidth) newWidth = oldWidth - offsetX;
            if (offsetY < 0) offsetY = 0;

            ResizeCanvas(offsetX, offsetY, 0, 0, newWidth, oldWidth, oldHeight);
        }

        public void ClipCanvas()
        {
            Coordinates[] smallestPixels = GetSmallestPixels();
            Coordinates[] biggestPixels = GetBiggestPixels();

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

        private Coordinates[] GetSmallestPixels()
        {
            Coordinates[] smallestPixels = new Coordinates[Layers.Count];
            for (int i = 0; i < smallestPixels.Length; i++)
            {
                Coordinates point = CoordinatesCalculator.FindMinEdgeNonTransparentPixel(Layers[i].LayerBitmap);
                if(point.X >= 0 && point.Y >=0)
                    smallestPixels[i] = point;
            }
            return smallestPixels;
        }

        private Coordinates[] GetBiggestPixels()
        {
            Coordinates[] biggestPixels = new Coordinates[Layers.Count];
            for (int i = 0; i < biggestPixels.Length; i++)
            {
                Coordinates point = CoordinatesCalculator.FindMostEdgeNonTransparentPixel(Layers[i].LayerBitmap);
                if (point.X >= 0 && point.Y >= 0)
                    biggestPixels[i] = point;
            }
            return biggestPixels;
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
