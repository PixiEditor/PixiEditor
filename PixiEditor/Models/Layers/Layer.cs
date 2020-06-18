using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;

namespace PixiEditor.Models.Layers
{
    public class Layer : BasicLayer
    {
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

        public Layer(string name, int width, int height)
        {
            Name = name;
            Layer layer = LayerGenerator.Generate(width, height);
            LayerBitmap = layer.LayerBitmap;
            Width = width;
            Height = height;
        }


        public Layer(WriteableBitmap layerBitmap)
        {
            LayerBitmap = layerBitmap;
            Width = (int) layerBitmap.Width;
            Height = (int) layerBitmap.Height;
        }

        /// <summary>
        ///     Returns pixel color by x and y coordinates relative to document using (x - OffsetX + 1) formula.
        /// </summary>
        /// <param name="x">Viewport relative X</param>
        /// <param name="y">Viewport relative Y</param>
        /// <returns>Color of a pixel</returns>
        public Color GetPixelWithOffset(int x, int y)
        {
            return LayerBitmap.GetPixel(x - OffsetX + 1, y - OffsetY + 1);
        }

        /// <summary>
        ///     Applies pixels to layer
        /// </summary>
        /// <param name="pixels"></param>
        public void ApplyPixels(BitmapPixelChanges pixels)
        {
            if (pixels.ChangedPixels == null || pixels.ChangedPixels.Count == 0) return;
            DynamicResize(pixels);
            pixels.ChangedPixels = ApplyOffset(pixels.ChangedPixels);
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

        private Dictionary<Coordinates, Color> ApplyOffset(Dictionary<Coordinates, Color> changedPixels)
        {
            return changedPixels.ToDictionary(d => new Coordinates(d.Key.X - OffsetX, d.Key.Y - OffsetY),
                d => d.Value);
        }

        public Coordinates[] ConvertToRelativeCoordinates(Coordinates[] nonRelativeCords)
        {
            Coordinates[] result = new Coordinates[nonRelativeCords.Length];
            for (int i = 0; i < result.Length; i++)
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
            ResetOffset(pixels);
            int newMaxX = pixels.ChangedPixels.Max(x => x.Key.X) - OffsetX;
            int newMaxY = pixels.ChangedPixels.Max(x => x.Key.Y) - OffsetY;
            int newMinX = pixels.ChangedPixels.Min(x => x.Key.X) - OffsetX;
            int newMinY = pixels.ChangedPixels.Min(x => x.Key.Y) - OffsetY;

            if (pixels.ChangedPixels.Any(x => x.Value.A != 0))
            {
                if (newMaxX + 1 > Width || newMaxY + 1 > Height)
                {
                    IncreaseSizeToBottom(newMaxX, newMaxY);
                }
                else if (newMinX < 0 || newMinY < 0)
                {
                    IncreaseSizeToTop(newMinX, newMinY);
                }
            } 
            
            if(pixels.ChangedPixels.Any(x=> IsBorderPixel(x.Key) && x.Value.A == 0))
            {
                _clipRequested = true;
            }
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
            newMaxX = Math.Clamp(Math.Max(newMaxX + 1, Width), 0, MaxWidth - OffsetX);
            newMaxY = Math.Clamp(Math.Max(newMaxY + 1, Height), 0, MaxHeight - OffsetY);

            ResizeCanvas(0, 0, 0, 0, newMaxX, newMaxY);
        }

        private void IncreaseSizeToTop(int newMinX, int newMinY)
        {
            newMinX = Math.Clamp(Math.Min(newMinX, Width), -OffsetX, 0);
            newMinY = Math.Clamp(Math.Min(newMinY, Height), -OffsetY, 0);

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

        public void Clear()
        {
            LayerBitmap.Clear();
        }

        public byte[] ConvertBitmapToBytes()
        {
            LayerBitmap.Lock();
            byte[] byteArray = LayerBitmap.ToByteArray();
            LayerBitmap.Unlock();
            return byteArray;
        }

        public void ResizeCanvas(int offsetX, int offsetY, int offsetXSrc, int offsetYSrc, int newWidth, int newHeight)
        {
            int sizeOfArgb = 4;
            int iteratorHeight = Height > newHeight ? newHeight : Height;
            int count = Width > newWidth ? newWidth : Width;

            using (var srcContext = LayerBitmap.GetBitmapContext(ReadWriteMode.ReadOnly))
            {
                var result = BitmapFactory.New(newWidth, newHeight);
                using (var destContext = result.GetBitmapContext())
                {
                    for (int line = 0; line < iteratorHeight; line++)
                    {
                        var srcOff = ((offsetYSrc + line) * Width + offsetXSrc) * sizeOfArgb;
                        var dstOff = ((offsetY + line) * newWidth + offsetX) * sizeOfArgb;
                        BitmapContext.BlockCopy(srcContext, srcOff, destContext, dstOff, count * sizeOfArgb);
                    }

                    LayerBitmap = result;
                    Width = newWidth;
                    Height = newHeight;
                }
            }
        }
    }
}