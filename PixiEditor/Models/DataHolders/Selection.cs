using PixiEditor.Models.Position;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace PixiEditor.Models.DataHolders
{
    public class Selection
    {
        public Coordinates[] SelectedPoints { get; set; } = null;
        public int VisualCanvasTop => SelectedPoints != null ? SelectedPoints.Min(x => x.Y) : -1;
        public int VisualCanvasLeft => SelectedPoints != null ? SelectedPoints.Min(x => x.X) : -1;
        public int VisualWidth => SelectedPoints != null ? Math.Abs(SelectedPoints[0].X - SelectedPoints[^1].X) : 0;

        public int VisualHeight => SelectedPoints != null ? Math.Abs(SelectedPoints[0].Y - SelectedPoints[^1].Y) : 0;
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
