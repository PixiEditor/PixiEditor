using PixiEditor.Helpers;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media.Imaging;

namespace PixiEditor.Models.DataHolders
{
    public class Document : NotifyableObject
    {
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
            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].LayerBitmap = Layers[i].LayerBitmap.Crop(x, y, width, height);
                Layers[i].Width = width;
                Layers[i].Height = height;
            }
            Height = height;
            Width = width;
        }

        public void ClipCanvas()
        {
            Coordinates[] smallestPixels = GetSmallestPixels();
            Coordinates[] biggestPixels = GetBiggestPixels();

            int smallestX = smallestPixels.Min(x => x.X);
            int smallestY = smallestPixels.Min(x => x.Y);
            int biggestX = biggestPixels.Max(x => x.X);
            int biggestY = biggestPixels.Max(x => x.Y);

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
}
