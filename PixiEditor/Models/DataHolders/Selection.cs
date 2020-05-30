using PixiEditor.Models.Position;
using System;
using System.Linq;
using System.Windows;

namespace PixiEditor.Models.DataHolders
{
    public class Selection
    {
        public Coordinates[] SelectedPoints { get; set; } = null;
        public int VisualCanvasTop => SelectedPoints != null ? SelectedPoints.Min(x => x.Y) : -1;
        public int VisualCanvasLeft => SelectedPoints != null ? SelectedPoints.Min(x => x.X) : -1;
        public int VisualWidth => SelectedPoints != null ? Math.Abs(VisualCanvasLeft + 1 - (SelectedPoints.Max(x => x.X) + 1)) + 1 : 0;

        public int VisualHeight => SelectedPoints != null ? Math.Abs(VisualCanvasTop + 1 - (SelectedPoints.Max(x => x.Y) + 1)) + 1 : 0;
        public Visibility Visibility => SelectedPoints != null ? Visibility.Visible : Visibility.Collapsed;

        public Selection()
        {

        }

        public Selection(Coordinates[] selectedPoints)
        {
            SelectedPoints = selectedPoints;
        }
    }
}
