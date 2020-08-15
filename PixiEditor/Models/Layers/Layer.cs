using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;

namespace PixiEditor.Models.Layers
{
    public class Layer : BasicLayer
    {

        private const int SizeOfArgb = 4;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                RaisePropertyChanged("IsActive");
            }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                RaisePropertyChanged("IsVisible");
            }
        }

        public bool IsRenaming
        {
            get => _isRenaming;
            set
            {
                _isRenaming = value;
                RaisePropertyChanged("IsRenaming");
            }
        }


        public WriteableBitmap LayerBitmap
        {
            get => _layerBitmap;
            set
            {
                _layerBitmap = value;
                RaisePropertyChanged("LayerBitmap");
            }
        }

        private float _opacity = 1;

        public float Opacity
        {
            get => _opacity;
            set
            {
                _opacity = value;
                RaisePropertyChanged("Opacity");
            }
        }

        public int OffsetX => (int) Offset.Left;

        public int OffsetY => (int) Offset.Top;

        private Thickness _offset;

        public Thickness Offset
        {
            get => _offset;
            set
            {
                _offset = value;
                RaisePropertyChanged("Offset");
            }
        }

        private bool _isActive;

        private bool _isRenaming;
        private bool _isVisible = true;
        private WriteableBitmap _layerBitmap;

        private string _name;
        private bool _clipRequested;

        public int MaxWidth { get; set; } = int.MaxValue;
        public int MaxHeight { get; set; } = int.MaxValue;

        public Dictionary<Coordinates,Color> LastRelativeCoordinates;

        public Layer(string name)
        {
            Name = name;
            LayerBitmap = BitmapFactory.New(0, 0);
            Width = 0;
            Height = 0;
        }

        public Layer(string name, int width, int height)
        {
            Name = name;
            LayerBitmap = BitmapFactory.New(width, height);
            Width = width;
            Height = height;
        }


        public Layer(string name, WriteableBitmap layerBitmap)
        {
            Name = name;
            LayerBitmap = layerBitmap;
            Width = layerBitmap.PixelWidth;
            Height = layerBitmap.PixelHeight;
        }

        /// <summary>
        /// Returns clone of layer
        /// </summary>
        /// <returns></returns>
        public Layer Clone()
        {
            return new Layer(Name, LayerBitmap.Clone())
            {
                IsVisible = this.IsVisible,
                Offset = this.Offset,
                MaxHeight = this.MaxHeight,
                MaxWidth = this.MaxWidth,
                Opacity = this.Opacity,
                IsActive = this.IsActive,
                IsRenaming = this.IsRenaming
            };
        }

        /// <summary>
        ///     Resizes bitmap with it's content using NearestNeighbor interpolation
        /// </summary>
        /// <param name="width">New width</param>
        /// <param name="height">New height</param>
        /// <param name="newMaxWidth">New layer maximum width, this should be document width</param>
        /// <param name="newMaxHeight">New layer maximum height, this should be document height</param>
        public void Resize(int width, int height, int newMaxWidth, int newMaxHeight)
        {
            LayerBitmap = LayerBitmap.Resize(width, height, WriteableBitmapExtensions.Interpolation.NearestNeighbor);
            Width = width;
            Height = height;
            MaxWidth = newMaxWidth;
            MaxHeight = newMaxHeight;
        }

        /// <summary>
        ///     Converts coordinates relative to viewport to relative to layer
        /// </summary>
        /// <param name="cords"></param>
        /// <returns></returns>
        public Coordinates GetRelativePosition(Coordinates cords)
        {
            return new Coordinates(cords.X - OffsetX, cords.Y - OffsetY);
        }

        /// <summary>
        ///     Returns pixel color of x and y coordinates relative to document using (x - OffsetX) formula.
        /// </summary>
        /// <param name="x">Viewport relative X</param>
        /// <param name="y">Viewport relative Y</param>
        /// <returns>Color of a pixel</returns>
        public Color GetPixelWithOffset(int x, int y)
        {
            Coordinates cords = GetRelativePosition(new Coordinates(x, y));
            return GetPixel(cords.X, cords.Y);
        }

        /// <summary>
        ///     Returns pixel color on x and y.
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y Coordinate</param>
        /// <returns>Color of pixel, if out of bounds, returns transparent pixel.</returns>
        public Color GetPixel(int x, int y)
        {
            if (x > Width - 1 || x < 0 || y > Height - 1 || y < 0)
            {
                return Color.FromArgb(0, 0, 0, 0);
            }

            return LayerBitmap.GetPixel(x, y);
        }

        /// <summary>
        ///     Applies pixel to layer
        /// </summary>
        /// <param name="coordinates">Position of pixel</param>
        /// <param name="color">Color of pixel</param>
        /// <param name="dynamicResize">Resizes bitmap to fit content</param>
        /// <param name="applyOffset">Converts pixels coordinates to relative to bitmap</param>
        public void SetPixel(Coordinates coordinates, Color color, bool dynamicResize = true, bool applyOffset = true)
        {
            SetPixels(BitmapPixelChanges.FromSingleColoredArray(new []{ coordinates }, color), dynamicResize, applyOffset);
        }

        /// <summary>
        ///     Applies pixels to layer
        /// </summary>
        /// <param name="pixels">Pixels to apply</param>
        /// <param name="dynamicResize">Resizes bitmap to fit content</param>
        /// <param name="applyOffset">Converts pixels coordinates to relative to bitmap</param>
        public void SetPixels(BitmapPixelChanges pixels, bool dynamicResize = true, bool applyOffset = true)
        {
            if (pixels.ChangedPixels == null || pixels.ChangedPixels.Count == 0) return;
            if(dynamicResize)
                DynamicResize(pixels);
            if(applyOffset)
                pixels.ChangedPixels = GetRelativePosition(pixels.ChangedPixels);
            LastRelativeCoordinates = pixels.ChangedPixels;

            using (var ctx = LayerBitmap.GetBitmapContext())
            {
                foreach (var coords in pixels.ChangedPixels)
                {
                    if (OutOfBounds(coords.Key)) continue;
                    ctx.WriteableBitmap.SetPixel(coords.Key.X, coords.Key.Y, coords.Value);
                }
            }
            ClipIfNecessary();
        }

        private Dictionary<Coordinates, Color> GetRelativePosition(Dictionary<Coordinates, Color> changedPixels)
        {
            return changedPixels.ToDictionary(d => new Coordinates(d.Key.X - OffsetX, d.Key.Y - OffsetY),
                d => d.Value);
        }

        /// <summary>
        ///     Converts absolute coordinates array to relative to this layer coordinates array.
        /// </summary>
        /// <param name="nonRelativeCords">absolute coordinates array</param>
        /// <returns></returns>
        public Coordinates[] ConvertToRelativeCoordinates(Coordinates[] nonRelativeCords)
        {
            Coordinates[] result = new Coordinates[nonRelativeCords.Length];
            for (int i = 0; i < nonRelativeCords.Length; i++)
            {
                result[i] = new Coordinates(nonRelativeCords[i].X - OffsetX, nonRelativeCords[i].Y - OffsetY);
            }

            return result;
        }

        /// <summary>
        ///     Resizes canvas to fit pixels outside current bounds. Clamped to MaxHeight and MaxWidth
        /// </summary>
        /// <param name="pixels"></param>
        public void DynamicResize(BitmapPixelChanges pixels)
        {
            if (pixels.ChangedPixels.Count == 0) return;
            ResetOffset(pixels);
            var borderData = ExtractBorderData(pixels);
            DoubleCords minMaxCords = borderData.Item1;
            int newMaxX = minMaxCords.Coords2.X - OffsetX;
            int newMaxY = minMaxCords.Coords2.Y - OffsetY;
            int newMinX = minMaxCords.Coords1.X - OffsetX;
            int newMinY = minMaxCords.Coords1.Y - OffsetY;

            if (!(pixels.WasBuiltAsSingleColored && pixels.ChangedPixels.First().Value.A == 0))
            {
                if (newMaxX + 1 > Width || newMaxY + 1 > Height)
                {
                    IncreaseSizeToBottom(newMaxX, newMaxY);
                }
                if (newMinX < 0 || newMinY < 0)
                {
                    IncreaseSizeToTop(newMinX, newMinY);
                }
            }

            if(borderData.Item2) //if clip is requested
            {
                _clipRequested = true;
            }
        }

        private Tuple<DoubleCords, bool> ExtractBorderData(BitmapPixelChanges pixels)
        {
            Coordinates firstCords = pixels.ChangedPixels.First().Key;
            int minX = firstCords.X;
            int minY = firstCords.Y;
            int maxX = minX;
            int maxY = minY;
            bool clipRequested = false;

            foreach (var pixel in pixels.ChangedPixels)
            {
                if (pixel.Key.X < minX) minX = pixel.Key.X;
                else if (pixel.Key.X > maxX) maxX = pixel.Key.X;

                if (pixel.Key.Y < minY) minY = pixel.Key.Y;
                else if (pixel.Key.Y > maxY) maxY = pixel.Key.Y;

                if (clipRequested == false && IsBorderPixel(pixel.Key) && pixel.Value.A == 0)
                    clipRequested = true;

            }
            return new Tuple<DoubleCords, bool>(
                new DoubleCords(new Coordinates(minX, minY), new Coordinates(maxX, maxY)), clipRequested);
        }

        private bool IsBorderPixel(Coordinates cords)
        {
            return cords.X - OffsetX == 0 || cords.Y - OffsetY == 0 || cords.X - OffsetX == Width - 1 ||
                   cords.Y - OffsetY == Height - 1;
        }

        private bool OutOfBounds(Coordinates cords)
        {
            return cords.X < 0 || cords.X > Width - 1 || cords.Y < 0 || cords.Y > Height - 1;
        }

        private void ClipIfNecessary()
        {
            if (_clipRequested)
            {
                ClipCanvas();
                _clipRequested = false;
            }
        }

        /// <summary>
        ///     Changes size of bitmap to fit content
        /// </summary>
        public void ClipCanvas()
        {
            var points = GetEdgePoints();
            int smallestX = points.Coords1.X;
            int smallestY = points.Coords1.Y;
            int biggestX = points.Coords2.X;
            int biggestY = points.Coords2.Y;

            if (smallestX < 0 && smallestY < 0 && biggestX < 0 && biggestY < 0)
                return;

            int width = biggestX - smallestX + 1;
            int height = biggestY - smallestY + 1;
            ResizeCanvas(0,0, smallestX, smallestY, width, height);
            Offset = new Thickness(OffsetX + smallestX, OffsetY + smallestY, 0, 0);
        }

        private void IncreaseSizeToBottom(int newMaxX, int newMaxY)
        {
            if (MaxWidth - OffsetX < 0 || MaxHeight - OffsetY < 0) return;
            newMaxX = Math.Clamp(Math.Max(newMaxX + 1, Width), 0, MaxWidth - OffsetX);
            newMaxY = Math.Clamp(Math.Max(newMaxY + 1, Height), 0, MaxHeight - OffsetY);

            ResizeCanvas(0, 0, 0, 0, newMaxX, newMaxY);
        }

        private void IncreaseSizeToTop(int newMinX, int newMinY)
        {
            newMinX = Math.Clamp(Math.Min(newMinX, Width), Math.Min(-OffsetX, OffsetX), 0);
            newMinY = Math.Clamp(Math.Min(newMinY, Height), Math.Min(-OffsetY, OffsetY), 0);

            Offset = new Thickness(Math.Clamp(OffsetX + newMinX, 0, MaxWidth),
                Math.Clamp(OffsetY + newMinY, 0, MaxHeight), 0, 0);

            int newWidth = Math.Clamp(Width - newMinX, 0, MaxWidth);
            int newHeight = Math.Clamp(Height - newMinY, 0, MaxHeight);

            int offsetX = Math.Abs(newWidth - Width);
            int offsetY = Math.Abs(newHeight - Height);

            ResizeCanvas(offsetX, offsetY, 0, 0, newWidth, newHeight);
        }

        private DoubleCords GetEdgePoints()
        {
            Coordinates smallestPixel = CoordinatesCalculator.FindMinEdgeNonTransparentPixel(LayerBitmap);
            Coordinates biggestPixel = CoordinatesCalculator.FindMostEdgeNonTransparentPixel(LayerBitmap);

            return new DoubleCords(smallestPixel, biggestPixel);
        }

        private void ResetOffset(BitmapPixelChanges pixels)
        {
            if (Width == 0 || Height == 0)
            {
                int offsetX = pixels.ChangedPixels.Min(x => x.Key.X);
                int offsetY = pixels.ChangedPixels.Min(x => x.Key.Y);
                Offset = new Thickness(offsetX, offsetY, 0,0);
            }
        }

        /// <summary>
        ///     Clears bitmap
        /// </summary>
        public void Clear()
        {
            LayerBitmap.Clear();
        }

        /// <summary>
        ///     Converts layer WriteableBitmap to byte array
        /// </summary>
        /// <returns></returns>
        public byte[] ConvertBitmapToBytes()
        {
            LayerBitmap.Lock();
            byte[] byteArray = LayerBitmap.ToByteArray();
            LayerBitmap.Unlock();
            return byteArray;
        }

        /// <summary>
        ///     Resizes canvas to new size with specified offset.
        /// </summary>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        /// <param name="offsetXSrc"></param>
        /// <param name="offsetYSrc"></param>
        /// <param name="newWidth"></param>
        /// <param name="newHeight"></param>
        private void ResizeCanvas(int offsetX, int offsetY, int offsetXSrc, int offsetYSrc, int newWidth, int newHeight)
        {
            int iteratorHeight = Height > newHeight ? newHeight : Height;
            int count = Width > newWidth ? newWidth : Width;

            using (var srcContext = LayerBitmap.GetBitmapContext(ReadWriteMode.ReadOnly))
            {
                var result = BitmapFactory.New(newWidth, newHeight);
                using (var destContext = result.GetBitmapContext())
                {
                    for (int line = 0; line < iteratorHeight; line++)
                    {
                        var srcOff = ((offsetYSrc + line) * Width + offsetXSrc) * SizeOfArgb;
                        var dstOff = ((offsetY + line) * newWidth + offsetX) * SizeOfArgb;
                        BitmapContext.BlockCopy(srcContext, srcOff, destContext, dstOff, count * SizeOfArgb);
                    }

                    LayerBitmap = result;
                    Width = newWidth;
                    Height = newHeight;
                }
            }
        }
    }
}