using PixiEditor.Helpers;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using SkiaSharp;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace PixiEditor.Models.DataHolders
{
    [DebuggerDisplay("{SelectedPoints.Count} selected Pixels")]
    public class Selection : NotifyableObject
    {
        private readonly SKColor selectionBlue;
        private Layer selectionLayer;

        public Selection(Coordinates[] selectedPoints)
        {
            SelectedPoints = new ObservableCollection<Coordinates>(selectedPoints);
            SelectionLayer = new Layer("_selectionLayer");
            selectionBlue = new SKColor(142, 202, 255, 255);
        }

        public ObservableCollection<Coordinates> SelectedPoints { get; private set; }

        public Layer SelectionLayer
        {
            get => selectionLayer;
            set
            {
                selectionLayer = value;
                RaisePropertyChanged(nameof(SelectionLayer));
            }
        }

        public bool Empty { get => Rect.IsEmpty; }

        public Int32Rect Rect { get; private set; }
        public void SetSelection(IEnumerable<Coordinates> selection, SelectionType mode)
        {
            if (selection.Any())
            {
                var minX = selection.Min(i => i.X);
                var minY = selection.Min(i => i.Y);
                Rect = new Int32Rect(
                  minX,
                  minY,
                  selection.Max(i => i.X) - minX + 1,
                  selection.Max(i => i.Y) - minY + 1
                  );

            }
            else
            {
                Rect = default(Int32Rect);
            }

            SKColor selectionColor = selectionBlue;
            switch (mode)
            {
                case SelectionType.New:
                    SelectedPoints = new ObservableCollection<Coordinates>(selection);
                    SelectionLayer.Reset();
                    break;
                case SelectionType.Add:
                    SelectedPoints = new ObservableCollection<Coordinates>(SelectedPoints.Concat(selection).Distinct());
                    break;
                case SelectionType.Subtract:
                    SelectedPoints = new ObservableCollection<Coordinates>(SelectedPoints.Except(selection));
                    selectionColor = SKColors.Transparent;
                    break;
            }

            SelectionLayer.SetPixels(BitmapPixelChanges.FromSingleColoredArray(selection, selectionColor));
        }

        public void TranslateSelection(int dX, int dY)
        {
            selectionLayer.Offset = new Thickness(selectionLayer.OffsetX + dX, selectionLayer.OffsetY + dY, 0, 0);
            for (int i = 0; i < SelectedPoints.Count; i++)
            {
                SelectedPoints[i] = new Coordinates(SelectedPoints[i].X + dX, SelectedPoints[i].Y + dY);
            }
        }

        public void SetSelection(Int32Rect rect, bool isCirclular, SelectionType mode)
        {
            using SKPaint paint = new()
            {
                Color = selectionBlue,
                BlendMode = SKBlendMode.Src,
                Style = SKPaintStyle.StrokeAndFill,
            };
            switch (mode)
            {
                case SelectionType.New:
                    SelectionLayer.Reset();
                    break;
                case SelectionType.Subtract:
                    paint.Color = SKColors.Transparent;
                    break;
            }

            SelectionLayer.DynamicResizeAbsolute(new Int32Rect(rect.X, rect.Y, rect.Width, rect.Height));
            if (isCirclular)
            {
                float cx = rect.X + rect.Width / 2f;
                float cy = rect.Y + rect.Height / 2f;
                SelectionLayer.LayerBitmap.SkiaSurface.Canvas.DrawOval(cx, cy, rect.Width / 2f, rect.Height / 2f, paint);
            }
            else
            {
                SelectionLayer.LayerBitmap.SkiaSurface.Canvas.DrawRect(rect.X, rect.Y, rect.Width, rect.Height, paint);
            }
        }

        public void Clear()
        {
            SelectionLayer.Reset();
            SelectedPoints.Clear();
            Rect = default(Int32Rect);
        }
    }
}
