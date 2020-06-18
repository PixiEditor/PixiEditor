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

        private int _offsetX;

        public int OffsetX
        {
            get => _offsetX;
            set
            {
                _offsetX = value;
                Offset = new Thickness(value,Offset.Top, 0, 0);
                RaisePropertyChanged("OffsetX");
            }
        }

        private int _offsetY;

        public int OffsetY
        {
            get => _offsetY;
            set
            {
                _offsetY = value;
                Offset = new Thickness(Offset.Left, value,0,0);
                RaisePropertyChanged("OffsetY");
            }
        }

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
        ///     Applies pixels to layer
        /// </summary>
        /// <param name="pixels"></param>
        public void ApplyPixels(BitmapPixelChanges pixels)
        {
            if (pixels.ChangedPixels == null) return;
            DynamicResize(pixels);
            pixels.ChangedPixels = ApplyOffset(pixels.ChangedPixels);
            using (var ctx = LayerBitmap.GetBitmapContext())
            {
                foreach (var coords in pixels.ChangedPixels)
                {
                    if (coords.Key.X < 0 || coords.Key.Y < 0) continue;
                    ctx.WriteableBitmap.SetPixel(Math.Clamp(coords.Key.X, 0, Width - 1),
                        Math.Clamp(coords.Key.Y, 0, Height - 1),
                        coords.Value);
                }
            }
        }

        private Dictionary<Coordinates, Color> ApplyOffset(Dictionary<Coordinates, Color> changedPixels)
        {
            return changedPixels.ToDictionary(d => new Coordinates(d.Key.X - OffsetX, d.Key.Y - OffsetY),
                d => d.Value);
        }

        /// <summary>
        /// Resizes canvas to fit pixels outside current bounds. Clamped to MaxHeight and MaxWidth
        /// </summary>
        /// <param name="pixels"></param>
        private void DynamicResize(BitmapPixelChanges pixels)
        {
            RecalculateOffset(pixels);
            int newMaxX = pixels.ChangedPixels.Max(x => x.Key.X) - OffsetX;
            int newMaxY = pixels.ChangedPixels.Max(x => x.Key.Y) - OffsetY;
            if (newMaxX + 1 > Width || newMaxY + 1 > Height)
            {
                newMaxX = Math.Clamp(Math.Max(newMaxX + 1, Width), 0, MaxWidth);
                newMaxY = Math.Clamp(Math.Max(newMaxY + 1, Height), 0, MaxHeight);
                ResizeCanvas(0, 0, 0, 0, Width, Height, newMaxX, newMaxY);
                RecalculateOffset(pixels);
                LayerBitmap.Clear(System.Windows.Media.Colors.Blue);
            }
        }

        private void RecalculateOffset(BitmapPixelChanges pixels)
        {
            if (Width == 0 || Height == 0)
            {
                OffsetX = pixels.ChangedPixels.Min(x => x.Key.X);
                OffsetY = pixels.ChangedPixels.Min(x => x.Key.Y);
            }
        }

        private Coordinates FindOffsetForNewSize(BitmapPixelChanges changes)
        {
            int newMaxX = changes.ChangedPixels.Max(x => x.Key.X);
            int newMaxY = changes.ChangedPixels.Max(x => x.Key.Y);
            int newMinX = changes.ChangedPixels.Min(x => x.Key.X);
            int newMinY = changes.ChangedPixels.Min(x => x.Key.Y);
            return new Coordinates(0,0);
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

        public void ResizeCanvas(int offsetX, int offsetY, int offsetXSrc, int offsetYSrc, int oldWidth, int oldHeight,
            int newWidth, int newHeight)
        {
            int sizeOfArgb = 4;
            int iteratorHeight = oldHeight > newHeight ? newHeight : oldHeight;
            int count = oldWidth > newWidth ? newWidth : oldWidth;

            using (var srcContext = LayerBitmap.GetBitmapContext(ReadWriteMode.ReadOnly))
            {
                var result = BitmapFactory.New(newWidth, newHeight);
                using (var destContext = result.GetBitmapContext())
                {
                    for (int line = 0; line < iteratorHeight; line++)
                    {
                        var srcOff = ((offsetYSrc + line) * oldWidth + offsetXSrc) * sizeOfArgb;
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