using PixiEditor.Helpers;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using SkiaSharp;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

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
            selectionBlue = new SKColor(142, 202, 255, 127);
        }

        public ObservableCollection<Coordinates> SelectedPoints { get; private set; }

        public Layer SelectionLayer
        {
            get => selectionLayer;
            set
            {
                selectionLayer = value;
                RaisePropertyChanged("SelectionLayer");
            }
        }

        public void SetSelection(IEnumerable<Coordinates> selection, SelectionType mode)
        {
            SKColor selectionColor = selectionBlue;
            switch (mode)
            {
                case SelectionType.New:
                    SelectedPoints = new ObservableCollection<Coordinates>(selection);
                    SelectionLayer.Clear();
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

        public void Clear()
        {
            SelectionLayer.Clear();
            SelectedPoints.Clear();
        }
    }
}
