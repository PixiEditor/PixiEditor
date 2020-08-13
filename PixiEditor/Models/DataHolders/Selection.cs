using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using ReactiveUI;

namespace PixiEditor.Models.DataHolders
{
    public class Selection : ReactiveObject
    {
        public ObservableCollection<Coordinates> SelectedPoints { get; private set; }

        public Layer SelectionLayer
        {
            get => _selectionLayer;
            set
            {
                _selectionLayer = value;
                this.RaisePropertyChanged("SelectionLayer");
            }
        }

        private readonly Color _selectionBlue;
        private Layer _selectionLayer;

        public Selection(Coordinates[] selectedPoints)
        {
            SelectedPoints = new ObservableCollection<Coordinates>(selectedPoints);
            SelectionLayer = new Layer("_selectionLayer");
            _selectionBlue = Color.FromArgb(127, 142, 202, 255);
        }

        public void SetSelection(Coordinates[] selection, SelectionType mode)
        {
            Color selectionColor = _selectionBlue;
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
                    selectionColor = Color.FromArgb(0,0,0,0);
                    break;
            }

            SelectionLayer.SetPixels(BitmapPixelChanges.FromSingleColoredArray(selection, selectionColor));
        }

        public void Clear()
        {
            SelectionLayer = new Layer("_selectionLayer");
            SelectedPoints.Clear();
        }
    }
}