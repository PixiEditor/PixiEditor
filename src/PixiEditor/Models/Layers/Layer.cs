using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Position;
using PixiEditor.Models.Undo;
using PixiEditor.ViewModels;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using PixiEditor.Models.Dialogs;

namespace PixiEditor.Models.Layers
{
    [DebuggerDisplay("'{name,nq}' {width}x{height}")]
    public class Layer : BasicLayer
    {
        private bool clipRequested;

        private bool isActive;

        private bool isRenaming;
        private bool isVisible = true;
        private Surface layerBitmap;

        private string name;

        private Thickness offset;

        private float opacity = 1f;

        private string layerHighlightColor = "#666666";


        public Layer(string name, int maxWidth, int maxHeight)
        {
            Name = name;
            LayerBitmap = new Surface(1, 1);
            IsReset = true;
            Width = 1;
            Height = 1;
            MaxWidth = maxWidth;
            MaxHeight = maxHeight;
            GuidValue = Guid.NewGuid();
        }

        public Layer(string name, int width, int height, int maxWidth, int maxHeight)
        {
            Name = name;
            LayerBitmap = new Surface(width, height);
            IsReset = true;
            Width = width;
            Height = height;
            MaxWidth = maxWidth;
            MaxHeight = maxHeight;
            GuidValue = Guid.NewGuid();
        }

        public Layer(string name, Surface layerBitmap, int maxWidth, int maxHeight)
        {
            Name = name;
            LayerBitmap = layerBitmap;
            Width = layerBitmap.Width;
            Height = layerBitmap.Height;
            MaxWidth = maxWidth;
            MaxHeight = maxHeight;
            GuidValue = Guid.NewGuid();
        }

        public Dictionary<Coordinates, SKColor> LastRelativeCoordinates { get; set; }

        public string LayerHighlightColor
        {
            get => IsActive ? layerHighlightColor : "#00000000";
            set
            {
                SetProperty(ref layerHighlightColor, value);
            }
        }

        public string Name
        {
            get => name;
            set
            {
                name = value;
                RaisePropertyChanged(nameof(Name));
            }
        }

        public bool IsActive
        {
            get => isActive;
            set
            {
                isActive = value;
                RaisePropertyChanged(nameof(IsActive));
                RaisePropertyChanged(nameof(LayerHighlightColor));
            }
        }

        public bool IsVisible
        {
            get => isVisible;
            set
            {
                isVisible = value;
                RaisePropertyChanged(nameof(IsVisibleUndoTriggerable));
                RaisePropertyChanged(nameof(IsVisible));
                ViewModelMain.Current?.ToolsSubViewModel?.TriggerCacheOutdated();
                InvokeLayerBitmapChange();
            }
        }

        public bool IsVisibleUndoTriggerable
        {
            get => IsVisible;
            set
            {
                if (value != IsVisible)
                {
                    ViewModelMain.Current?.BitmapManager?.ActiveDocument?.UndoManager
                        .AddUndoChange(
                        new Change(
                            nameof(IsVisible),
                            isVisible,
                            value,
                            LayerHelper.FindLayerByGuidProcess,
                            new object[] { GuidValue },
                            "Change layer visibility"));
                    IsVisible = value;
                    InvokeLayerBitmapChange();
                }
            }
        }

        public bool IsRenaming
        {
            get => isRenaming;
            set
            {
                isRenaming = value;
                RaisePropertyChanged("IsRenaming");
            }
        }

        public Surface LayerBitmap
        {
            get => layerBitmap;
            set
            {
                Int32Rect prevRect = new Int32Rect(OffsetX, OffsetY, Width, Height);
                layerBitmap = value;
                Width = layerBitmap.Width;
                Height = layerBitmap.Height;
                Int32Rect curRect = new Int32Rect(OffsetX, OffsetY, Width, Height);
                RaisePropertyChanged(nameof(LayerBitmap));
                InvokeLayerBitmapChange(prevRect.Expand(curRect));
            }
        }

        public float Opacity
        {
            get => opacity;
            set
            {
                opacity = value;
                RaisePropertyChanged(nameof(OpacityUndoTriggerable));
                ViewModelMain.Current?.ToolsSubViewModel?.TriggerCacheOutdated();
                InvokeLayerBitmapChange();
            }
        }

        public float OpacityUndoTriggerable
        {
            get => Opacity;
            set
            {
                if (value != Opacity)
                {
                    ViewModelMain.Current?.BitmapManager?.ActiveDocument?.UndoManager
                    .AddUndoChange(
                                   new Change(
                                   nameof(Opacity),
                                   opacity,
                                   value,
                                   LayerHelper.FindLayerByGuidProcess,
                                   new object[] { GuidValue },
                                   "Change layer opacity"));
                    Opacity = value;
                }
            }
        }

        public int OffsetX => (int)Offset.Left;

        public int OffsetY => (int)Offset.Top;

        public Thickness Offset
        {
            get => offset;
            set
            {
                Int32Rect prevRect = new Int32Rect(OffsetX, OffsetY, Width, Height);
                offset = value;
                Int32Rect curRect = new Int32Rect(OffsetX, OffsetY, Width, Height);
                RaisePropertyChanged(nameof(Offset));
                InvokeLayerBitmapChange(prevRect.Expand(curRect));
            }
        }

        public int MaxWidth { get; set; } = int.MaxValue;

        public int MaxHeight { get; set; } = int.MaxValue;

        public bool IsReset { get; private set; }

        public Int32Rect TightBounds => GetContentDimensions();
        public Int32Rect Bounds => new Int32Rect(OffsetX, OffsetY, Width, Height);

        public event EventHandler<Int32Rect> LayerBitmapChanged;

        public void InvokeLayerBitmapChange()
        {
            IsReset = false;
            LayerBitmapChanged?.Invoke(this, new Int32Rect(OffsetX, OffsetY, Width, Height));
        }

        public void InvokeLayerBitmapChange(Int32Rect dirtyArea)
        {
            IsReset = false;
            LayerBitmapChanged?.Invoke(this, dirtyArea);
        }


        /// <summary>
        /// Changes Guid of layer.
        /// </summary>
        /// <param name="newGuid">Guid to set.</param>
        /// <remarks>This is potentially destructive operation, use when absolutelly necessary.</remarks>
        public void ChangeGuid(Guid newGuid)
        {
            GuidValue = newGuid;
        }

        public IEnumerable<Layer> GetLayers()
        {
            return new Layer[] { this };
        }

        /// <summary>
        ///     Returns clone of layer.
        /// </summary>
        public Layer Clone(bool generateNewGuid = false)
        {
            return new Layer(Name, new Surface(LayerBitmap), MaxWidth, MaxHeight)
            {
                IsVisible = IsVisible,
                Offset = Offset,
                Opacity = Opacity,
                IsActive = IsActive,
                IsRenaming = IsRenaming,
                GuidValue = generateNewGuid ? Guid.NewGuid() : GuidValue
            };
        }

        public void RaisePropertyChange(string property)
        {
            RaisePropertyChanged(property);
        }

        /// <summary>
        ///     Resizes bitmap with it's content using NearestNeighbor interpolation.
        /// </summary>
        /// <param name="width">New width.</param>
        /// <param name="height">New height.</param>
        /// <param name="newMaxWidth">New layer maximum width, this should be document width.</param>
        /// <param name="newMaxHeight">New layer maximum height, this should be document height.</param>
        public void Resize(int width, int height, int newMaxWidth, int newMaxHeight)
        {
            LayerBitmap = LayerBitmap.ResizeNearestNeighbor(width, height);
            Width = width;
            Height = height;
            MaxWidth = newMaxWidth;
            MaxHeight = newMaxHeight;
        }

        /// <summary>
        ///     Converts coordinates relative to viewport to relative to layer.
        /// </summary>
        public Coordinates GetRelativePosition(Coordinates cords)
        {
            return new Coordinates(cords.X - OffsetX, cords.Y - OffsetY);
        }

        /// <summary>
        ///     Returns pixel color of x and y coordinates relative to document using (x - OffsetX) formula.
        /// </summary>
        /// <param name="x">Viewport relative X.</param>
        /// <param name="y">Viewport relative Y.</param>
        /// <returns>Color of a pixel.</returns>
        public SKColor GetPixelWithOffset(int x, int y)
        {
            //This does not use GetRelativePosition for better performance
            return GetPixel(x - OffsetX, y - OffsetY);
        }

        /// <summary>
        ///     Returns pixel color on x and y.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y Coordinate.</param>
        /// <returns>Color of pixel, if out of bounds, returns transparent pixel.</returns>
        public SKColor GetPixel(int x, int y)
        {
            if (x > Width - 1 || x < 0 || y > Height - 1 || y < 0)
            {
                return SKColors.Empty;
            }

            return LayerBitmap.GetSRGBPixel(x, y);
        }

        public void SetPixelWithOffset(Coordinates coordinates, SKColor color)
        {
            LayerBitmap.SetSRGBPixel(coordinates.X - OffsetX, coordinates.Y - OffsetY, color);
        }

        public void SetPixelWithOffset(int x, int y, SKColor color)
        {
            LayerBitmap.SetSRGBPixel(x - OffsetX, y - OffsetY, color);
        }

        /// <summary>
        ///     Applies pixels to layer.
        /// </summary>
        /// <param name="pixels">Pixels to apply.</param>
        /// <param name="dynamicResize">Resizes bitmap to fit content.</param>
        /// <param name="applyOffset">Converts pixels coordinates to relative to bitmap.</param>
        public void SetPixels(BitmapPixelChanges pixels, bool dynamicResize = true, bool applyOffset = true)
        {
            if (pixels.ChangedPixels == null || pixels.ChangedPixels.Count == 0)
            {
                return;
            }

            if (applyOffset)
            {
                pixels.ChangedPixels = GetRelativePosition(pixels.ChangedPixels);
            }

            if (dynamicResize)
            {
                DynamicResize(pixels);
            }

            LastRelativeCoordinates = pixels.ChangedPixels;

            int minX = int.MaxValue;
            int maxX = int.MinValue;
            int minY = int.MaxValue;
            int maxY = int.MinValue;

            foreach (KeyValuePair<Coordinates, SKColor> coords in pixels.ChangedPixels)
            {
                if (OutOfBounds(coords.Key))
                {
                    continue;
                }

                LayerBitmap.SetSRGBPixel(coords.Key.X, coords.Key.Y, coords.Value);
                minX = Math.Min(minX, coords.Key.X);
                minY = Math.Min(minY, coords.Key.Y);
                maxX = Math.Max(maxX, coords.Key.X);
                maxY = Math.Max(maxY, coords.Key.Y);
            }

            ClipIfNecessary();
            if (minX != int.MaxValue)
                InvokeLayerBitmapChange(new Int32Rect(minX + OffsetX, minY + OffsetY, maxX - minX + 1, maxY - minY + 1));
        }

        /// <summary>
        ///     Converts absolute coordinates array to relative to this layer coordinates array.
        /// </summary>
        /// <param name="nonRelativeCords">absolute coordinates array.</param>
        public Coordinates[] ConvertToRelativeCoordinates(Coordinates[] nonRelativeCords)
        {
            Coordinates[] result = new Coordinates[nonRelativeCords.Length];
            for (int i = 0; i < nonRelativeCords.Length; i++)
            {
                result[i] = new Coordinates(nonRelativeCords[i].X - OffsetX, nonRelativeCords[i].Y - OffsetY);
            }

            return result;
        }

        public void CreateNewBitmap(int width, int height)
        {
            LayerBitmap = new Surface(width, height);

            Width = width;
            Height = height;
        }


        /// <summary>
        ///     Resizes canvas to fit pixels outside current bounds. Clamped to MaxHeight and MaxWidth.
        /// </summary>
        public void DynamicResize(BitmapPixelChanges pixels)
        {
            if (pixels.ChangedPixels.Count == 0)
            {
                return;
            }

            ResetOffset(pixels);
            Tuple<DoubleCoords, bool> borderData = ExtractBorderData(pixels);
            DoubleCoords minMaxCords = borderData.Item1;
            int newMaxX = minMaxCords.Coords2.X;
            int newMaxY = minMaxCords.Coords2.Y;
            int newMinX = minMaxCords.Coords1.X;
            int newMinY = minMaxCords.Coords1.Y;

            if (!(pixels.WasBuiltAsSingleColored && pixels.ChangedPixels.First().Value.Alpha == 0))
            {
                DynamicResizeRelative(newMaxX, newMaxY, newMinX, newMinY);
            }

            // if clip is requested
            if (borderData.Item2)
            {
                clipRequested = true;
            }
        }

        public void DynamicResizeAbsolute(Int32Rect newSize)
        {
            newSize = newSize.Intersect(new Int32Rect(0, 0, MaxWidth, MaxHeight));
            if (newSize.IsEmpty)
                return;
            if (IsReset)
            {
                Offset = new Thickness(newSize.X, newSize.Y, 0, 0);
            }

            int relX = newSize.X - OffsetX;
            int relY = newSize.Y - OffsetY;
            int maxX = relX + newSize.Width - 1;
            int maxY = relY + newSize.Height - 1;

            DynamicResizeRelative(maxX, maxY, relX, relY);
        }

        /// <summary>
        ///     Resizes canvas to fit pixels outside current bounds. Clamped to MaxHeight and MaxWidth.
        /// </summary>
        public void DynamicResizeRelative(int newMaxX, int newMaxY, int newMinX, int newMinY)
        {
            if ((newMaxX + 1 > Width && Width < MaxWidth) || (newMaxY + 1 > Height && Height < MaxHeight))
            {
                newMaxX = Math.Max(newMaxX, (int)(Width * 1.5f));
                newMaxY = Math.Max(newMaxY, (int)(Height * 1.5f));
                IncreaseSizeToBottomAndRight(newMaxX, newMaxY);
            }

            if ((newMinX < 0 && Width < MaxWidth) || (newMinY < 0 && Height < MaxHeight))
            {
                newMinX = Math.Min(newMinX, Width - (int)(Width * 1.5f));
                newMinY = Math.Min(newMinY, Height - (int)(Height * 1.5f));
                IncreaseSizeToTopAndLeft(newMinX, newMinY);
            }
        }

        public Int32Rect GetContentDimensions()
        {
            DoubleCoords points = GetEdgePoints();
            int smallestX = points.Coords1.X;
            int smallestY = points.Coords1.Y;
            int biggestX = points.Coords2.X;
            int biggestY = points.Coords2.Y;

            if (smallestX < 0 && smallestY < 0 && biggestX < 0 && biggestY < 0)
            {
                return Int32Rect.Empty;
            }

            int width = biggestX - smallestX + 1;
            int height = biggestY - smallestY + 1;
            return new Int32Rect(smallestX, smallestY, width, height);
        }

        /// <summary>
        ///     Changes size of bitmap to fit content.
        /// </summary>
        public void ClipCanvas()
        {
            var dimensions = GetContentDimensions();
            if (dimensions == Int32Rect.Empty)
            {
                Reset();
                return;
            }

            ResizeCanvas(0, 0, dimensions.X, dimensions.Y, dimensions.Width, dimensions.Height);
            Offset = new Thickness(OffsetX + dimensions.X, OffsetY + dimensions.Y, 0, 0);
        }

        public void Reset()
        {
            if (IsReset)
                return;
            var dirtyRect = new Int32Rect(OffsetX, OffsetY, Width, Height);
            LayerBitmap?.Dispose();
            LayerBitmap = new Surface(1, 1);
            Width = 1;
            Height = 1;
            Offset = new Thickness(0, 0, 0, 0);
            IsReset = true;
            LayerBitmapChanged?.Invoke(this, dirtyRect);
        }

        public void ClearCanvas()
        {
            if (IsReset)
                return;
            LayerBitmap.SkiaSurface.Canvas.Clear();
            InvokeLayerBitmapChange();
        }

        /// <summary>
        ///     Converts layer WriteableBitmap to byte array.
        /// </summary>
        public byte[] ConvertBitmapToBytes()
        {
            return LayerBitmap.ToByteArray();
        }

        public SKRectI GetRect() => SKRectI.Create(OffsetX, OffsetY, Width, Height);

        public void CropIntersect(SKRectI rect)
        {
            SKRectI layerRect = GetRect();
            SKRectI intersect = SKRectI.Intersect(layerRect, rect);

            Crop(intersect);
        }

        public void Crop(SKRectI intersect)
        {
            if (intersect == SKRectI.Empty)
            {
                return;
            }

            using var oldSurface = LayerBitmap;

            int offsetX = (int)(Offset.Left - intersect.Left);
            int offsetY = (int)(Offset.Top - intersect.Top);

            Width = intersect.Width;
            Height = intersect.Height;
            LayerBitmap = LayerBitmap.Crop(offsetX, offsetY, Width, Height);

            Offset = new(intersect.Left, intersect.Top, 0, 0);
        }

        private Dictionary<Coordinates, SKColor> GetRelativePosition(Dictionary<Coordinates, SKColor> changedPixels)
        {
            return changedPixels.ToDictionary(
                d => new Coordinates(d.Key.X - OffsetX, d.Key.Y - OffsetY),
                d => d.Value);
        }

        private Tuple<DoubleCoords, bool> ExtractBorderData(BitmapPixelChanges pixels)
        {
            Coordinates firstCords = pixels.ChangedPixels.First().Key;
            int minX = firstCords.X;
            int minY = firstCords.Y;
            int maxX = minX;
            int maxY = minY;
            bool clipRequested = false;

            foreach (KeyValuePair<Coordinates, SKColor> pixel in pixels.ChangedPixels)
            {
                if (pixel.Key.X < minX)
                {
                    minX = pixel.Key.X;
                }
                else if (pixel.Key.X > maxX)
                {
                    maxX = pixel.Key.X;
                }

                if (pixel.Key.Y < minY)
                {
                    minY = pixel.Key.Y;
                }
                else if (pixel.Key.Y > maxY)
                {
                    maxY = pixel.Key.Y;
                }

                if (clipRequested == false && IsBorderPixel(pixel.Key) && pixel.Value.Alpha == 0)
                {
                    clipRequested = true;
                }
            }

            return new Tuple<DoubleCoords, bool>(
                new DoubleCoords(new Coordinates(minX, minY), new Coordinates(maxX, maxY)), clipRequested);
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
            if (clipRequested)
            {
                ClipCanvas();
                clipRequested = false;
            }
        }

        private void IncreaseSizeToBottomAndRight(int newMaxX, int newMaxY)
        {
            if (MaxWidth - OffsetX < 0 || MaxHeight - OffsetY < 0)
            {
                return;
            }

            newMaxX = Math.Clamp(Math.Max(newMaxX + 1, Width), 1, MaxWidth - OffsetX);
            newMaxY = Math.Clamp(Math.Max(newMaxY + 1, Height), 1, MaxHeight - OffsetY);

            ResizeCanvas(0, 0, 0, 0, newMaxX, newMaxY);
        }

        private void IncreaseSizeToTopAndLeft(int newMinX, int newMinY)
        {
            newMinX = Math.Clamp(Math.Min(newMinX, Width), Math.Min(-OffsetX, OffsetX), 0);
            newMinY = Math.Clamp(Math.Min(newMinY, Height), Math.Min(-OffsetY, OffsetY), 0);

            Offset = new Thickness(
                Math.Clamp(OffsetX + newMinX, 0, MaxWidth),
                Math.Clamp(OffsetY + newMinY, 0, MaxHeight),
                0,
                0);

            int newWidth = Math.Clamp(Width - newMinX, 0, MaxWidth);
            int newHeight = Math.Clamp(Height - newMinY, 0, MaxHeight);

            int offsetX = Math.Abs(newWidth - Width);
            int offsetY = Math.Abs(newHeight - Height);

            ResizeCanvas(offsetX, offsetY, 0, 0, newWidth, newHeight);
        }

        private DoubleCoords GetEdgePoints()
        {
            Coordinates smallestPixel = CoordinatesCalculator.FindMinEdgeNonTransparentPixel(LayerBitmap);
            Coordinates biggestPixel = CoordinatesCalculator.FindMostEdgeNonTransparentPixel(LayerBitmap);

            return new DoubleCoords(smallestPixel, biggestPixel);
        }

        private void ResetOffset(BitmapPixelChanges pixels)
        {
            if (Width == 0 || Height == 0)
            {
                int offsetX = Math.Max(pixels.ChangedPixels.Min(x => x.Key.X), 0);
                int offsetY = Math.Max(pixels.ChangedPixels.Min(x => x.Key.Y), 0);
                Offset = new Thickness(offsetX, offsetY, 0, 0);
            }
        }

        /// <summary>
        ///     Resizes canvas to new size with specified offset.
        /// </summary>
        private void ResizeCanvas(int offsetX, int offsetY, int offsetXSrc, int offsetYSrc, int newWidth, int newHeight)
        {
            Surface result = new Surface(newWidth, newHeight);
            LayerBitmap.SkiaSurface.Draw(result.SkiaSurface.Canvas, offsetX - offsetXSrc, offsetY - offsetYSrc, Surface.ReplacingPaint);
            LayerBitmap?.Dispose();
            LayerBitmap = result;
            Width = newWidth;
            Height = newHeight;
        }


        public void ReplaceColor(SKColor oldColor, SKColor newColor)
        {
            if (LayerBitmap == null)
            {
                return;
            }

            int maxThreads = Environment.ProcessorCount;
            int rowsPerThread = Height / maxThreads;

            Parallel.For(0, maxThreads, i =>
            {
                int startRow = i * rowsPerThread;
                int endRow = (i + 1) * rowsPerThread;
                if (i == maxThreads - 1)
                {
                    endRow = Height;
                }

                for (int y = startRow; y < endRow; y++)
                {
                    for (int x = 0; x < Width; x++)
                    {
                        if (LayerBitmap.GetSRGBPixel(x, y) == oldColor)
                        {
                            LayerBitmap.SetSRGBPixelUnmanaged(x, y, newColor);
                        }
                    }
                }
            });

            layerBitmap.SkiaSurface.Canvas.DrawPaint(new SKPaint { BlendMode = SKBlendMode.Dst });

            InvokeLayerBitmapChange();
        }
    }
}
