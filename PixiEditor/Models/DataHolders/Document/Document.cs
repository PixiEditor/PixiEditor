using PixiEditor.Helpers;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Undo;
using PixiEditor.ViewModels;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace PixiEditor.Models.DataHolders
{
    [DebuggerDisplay("'{Name, nq}' {width}x{height} {Layers.Count} Layer(s)")]
    public partial class Document : NotifyableObject
    {
        private int height;
        private int width;

        private ViewModelMain xamlAccesibleViewModel = null;

        public ViewModelMain XamlAccesibleViewModel // Used to access ViewModelMain, without changing DataContext in XAML
        {
            get => xamlAccesibleViewModel;
            set
            {
                xamlAccesibleViewModel = value;
                RaisePropertyChanged(nameof(XamlAccesibleViewModel));
            }
        }

        public string Name
        {
            get => (string.IsNullOrEmpty(DocumentFilePath) ? "Untitled" : Path.GetFileName(DocumentFilePath))
                + (!ChangesSaved ? " *" : string.Empty);
        }

        public int Width
        {
            get => width;
            set
            {
                width = value;
                RaisePropertyChanged("Width");
            }
        }

        public int Height
        {
            get => height;
            set
            {
                height = value;
                RaisePropertyChanged("Height");
            }
        }

        private Selection selection = new Selection(Array.Empty<Coordinates>());

        public Selection ActiveSelection
        {
            get => selection;
            set
            {
                selection = value;
                RaisePropertyChanged(nameof(ActiveSelection));
            }
        }

        private double mouseXonCanvas;

        private double mouseYonCanvas;

        public double MouseXOnCanvas // Mouse X coordinate relative to canvas
        {
            get => mouseXonCanvas;
            set
            {
                mouseXonCanvas = value;
                RaisePropertyChanged(nameof(MouseXOnCanvas));
            }
        }

        public double MouseYOnCanvas // Mouse Y coordinate relative to canvas
        {
            get => mouseYonCanvas;
            set
            {
                mouseYonCanvas = value;
                RaisePropertyChanged(nameof(MouseYOnCanvas));
            }
        }

        public UndoManager UndoManager { get; set; }


        public ObservableCollection<Color> Swatches { get; set; } = new ObservableCollection<Color>();

        /// <summary>
        ///     Resizes canvas, so it fits exactly the size of drawn content, without any transparent pixels outside.
        /// </summary>
        public void ClipCanvas()
        {
            DoubleCords points = GetEdgePoints(Layers);
            int smallestX = points.Coords1.X;
            int smallestY = points.Coords1.Y;
            int biggestX = points.Coords2.X;
            int biggestY = points.Coords2.Y;

            if (smallestX == 0 && smallestY == 0 && biggestX == 0 && biggestY == 0)
            {
                return;
            }

            int width = biggestX - smallestX;
            int height = biggestY - smallestY;
            Coordinates moveVector = new Coordinates(-smallestX, -smallestY);

            Thickness[] oldOffsets = Layers.Select(x => x.Offset).ToArray();
            int oldWidth = Width;
            int oldHeight = Height;

            MoveOffsets(Layers, moveVector);
            Width = width;
            Height = height;

            object[] reverseArguments = { oldOffsets, oldWidth, oldHeight };
            object[] processArguments = { Layers.Select(x => x.Offset).ToArray(), width, height };

            UndoManager.AddUndoChange(new Change(
                ResizeCanvasProcess,
                reverseArguments,
                ResizeCanvasProcess,
                processArguments,
                "Clip canvas"));
        }

        /// <summary>
        /// Centers selected, visible layers inside document.
        /// </summary>
        public void CenterContent()
        {
            var layersToCenter = Layers.Where(x => x.IsActive && x.IsVisible);
            if (layersToCenter.Count() == 0)
            {
                return;
            }

            DoubleCords points = GetEdgePoints(layersToCenter);

            int smallestX = points.Coords1.X;
            int smallestY = points.Coords1.Y;
            int biggestX = points.Coords2.X;
            int biggestY = points.Coords2.Y;

            if (smallestX == 0 && smallestY == 0 && biggestX == 0 && biggestY == 0)
            {
                return;
            }

            Coordinates contentCenter = CoordinatesCalculator.GetCenterPoint(points.Coords1, points.Coords2);
            Coordinates documentCenter = CoordinatesCalculator.GetCenterPoint(
                new Coordinates(0, 0),
                new Coordinates(Width, Height));
            Coordinates moveVector = new Coordinates(documentCenter.X - contentCenter.X, documentCenter.Y - contentCenter.Y);

            MoveOffsets(layersToCenter, moveVector);
            UndoManager.AddUndoChange(
                new Change(
                    MoveOffsetsProcess,
                    new object[] { layersToCenter, new Coordinates(-moveVector.X, -moveVector.Y) },
                    MoveOffsetsProcess,
                    new object[] { layersToCenter, moveVector },
                    "Center content"));
        }

        private void SetAsActiveOnClick(object obj)
        {
            XamlAccesibleViewModel.BitmapManager.MouseController.StopRecordingMouseMovementChanges();
            XamlAccesibleViewModel.BitmapManager.MouseController.StartRecordingMouseMovementChanges(true);
            if (XamlAccesibleViewModel.BitmapManager.ActiveDocument != this)
            {
                XamlAccesibleViewModel.BitmapManager.ActiveDocument = this;
            }
        }

        private void RequestCloseDocument(object parameter)
        {
            ViewModelMain.Current.DocumentSubViewModel.RequestCloseDocument(this);
        }

        private int GetOffsetXForAnchor(int srcWidth, int destWidth, AnchorPoint anchor)
        {
            if (anchor.HasFlag(AnchorPoint.Center))
            {
                return Math.Abs((destWidth / 2) - (srcWidth / 2));
            }

            if (anchor.HasFlag(AnchorPoint.Right))
            {
                return Math.Abs(destWidth - srcWidth);
            }

            return 0;
        }

        private int GetOffsetYForAnchor(int srcHeight, int destHeight, AnchorPoint anchor)
        {
            if (anchor.HasFlag(AnchorPoint.Middle))
            {
                return Math.Abs((destHeight / 2) - (srcHeight / 2));
            }

            if (anchor.HasFlag(AnchorPoint.Bottom))
            {
                return Math.Abs(destHeight - srcHeight);
            }

            return 0;
        }

        private DoubleCords GetEdgePoints(IEnumerable<Layer> layers)
        {
            if (Layers.Count == 0)
            {
                throw new ArgumentException("Not enough layers");
            }

            Layer firstLayer = layers.First();
            int smallestX = firstLayer.OffsetX;
            int smallestY = firstLayer.OffsetY;
            int biggestX = smallestX + firstLayer.Width;
            int biggestY = smallestY + firstLayer.Height;

            foreach (Layer layer in layers)
            {
                layer.ClipCanvas();
                if (layer.OffsetX < smallestX)
                {
                    smallestX = layer.OffsetX;
                }

                if (layer.OffsetX + layer.Width > biggestX)
                {
                    biggestX = layer.OffsetX + layer.Width;
                }

                if (layer.OffsetY < smallestY)
                {
                    smallestY = layer.OffsetY;
                }

                if (layer.OffsetY + layer.Height > biggestY)
                {
                    biggestY = layer.OffsetY + layer.Height;
                }
            }

            return new DoubleCords(
                new Coordinates(smallestX, smallestY),
                new Coordinates(biggestX, biggestY));
        }
    }
}
