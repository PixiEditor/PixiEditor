using PixiEditor.Helpers;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Layers.Utils;
using PixiEditor.Models.Position;
using PixiEditor.Models.Undo;
using PixiEditor.ViewModels;
using SkiaSharp;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace PixiEditor.Models.DataHolders
{
    [DebuggerDisplay("'{Name, nq}' {width}x{height} {Layers.Count} Layer(s)")]
    public partial class Document : NotifyableObject, IDisposable
    {

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

        private int width = 1;
        public int Width
        {
            get => width;
            private set
            {
                width = value;
                RaisePropertyChanged(nameof(Width));
            }
        }

        private int height = 1;
        public int Height
        {
            get => height;
            private set
            {
                height = value;
                RaisePropertyChanged("Height");
            }
        }

        private Selection selection;

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
        public double MouseXOnCanvas // Mouse X coordinate relative to canvas
        {
            get => mouseXonCanvas;
            set
            {
                mouseXonCanvas = value;
                RaisePropertyChanged(nameof(MouseXOnCanvas));
            }
        }

        private double mouseYonCanvas;
        public double MouseYOnCanvas // Mouse Y coordinate relative to canvas
        {
            get => mouseYonCanvas;
            set
            {
                mouseYonCanvas = value;
                RaisePropertyChanged(nameof(MouseYOnCanvas));
            }
        }

        public bool Disposed { get; private set; } = false;

        public ExecutionTrigger<Size> CenterViewportTrigger { get; } = new();
        public ExecutionTrigger<double> ZoomViewportTrigger { get; } = new();

        public UndoManager UndoManager { get; set; }

        public ObservableCollection<SKColor> Swatches { get; set; } = new ObservableCollection<SKColor>();

        public void RaisePropertyChange(string name)
        {
            RaisePropertyChanged(name);
        }

        /// <summary>
        ///     Resizes canvas, so it fits exactly the size of drawn content, without any transparent pixels outside.
        /// </summary>
        public void ClipCanvas()
        {
            DoubleCoords? maybePoints = GetEdgePoints(Layers);

            if (maybePoints == null)
            {
                //all layers are empty
                return;
            }
            DoubleCoords points = maybePoints.Value;

            int smallestX = points.Coords1.X;
            int smallestY = points.Coords1.Y;
            int biggestX = points.Coords2.X;
            int biggestY = points.Coords2.Y;

            int width = biggestX - smallestX;
            int height = biggestY - smallestY;
            Coordinates moveVector = new Coordinates(-smallestX, -smallestY);

            Thickness[] oldOffsets = Layers.Select(x => x.Offset).ToArray();
            int oldWidth = Width;
            int oldHeight = Height;

            StorageBasedChange change = new StorageBasedChange(this, Layers);

            object[] reverseArguments = { oldWidth, oldHeight };
            object[] processArguments = { Layers.Select(x => new Thickness(x.OffsetX - smallestX, x.OffsetY - smallestY, 0, 0)).ToArray(), width, height };

            ResizeCanvasProcess(processArguments);

            UndoManager.AddUndoChange(change.ToChange(
                RestoreDocumentLayersProcess,
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
            var layersToCenter = Layers.Where(x => x.IsActive && LayerStructureUtils.GetFinalLayerIsVisible(x, LayerStructure)).ToList();
            if (layersToCenter.Count == 0)
            {
                return;
            }

            List<Int32Rect> oldBounds = layersToCenter.Select(x => x.Bounds).ToList();

            DoubleCoords? maybePoints = ClipLayersAndGetEdgePoints(layersToCenter);
            if (maybePoints == null)
                return;
            DoubleCoords points = maybePoints.Value;

            int smallestX = points.Coords1.X;
            int smallestY = points.Coords1.Y;
            int biggestX = points.Coords2.X;
            int biggestY = points.Coords2.Y;

            Coordinates contentCenter = CoordinatesCalculator.GetCenterPoint(points.Coords1, points.Coords2);
            Coordinates documentCenter = CoordinatesCalculator.GetCenterPoint(
                new Coordinates(0, 0),
                new Coordinates(Width, Height));
            Coordinates moveVector = new Coordinates(documentCenter.X - contentCenter.X, documentCenter.Y - contentCenter.Y);

            List<Int32Rect> newBounds = layersToCenter.Select(x => x.Bounds).ToList();

            MoveOffsets(layersToCenter, newBounds, moveVector);
            List<Guid> guids = layersToCenter.Select(x => x.GuidValue).ToList();
            UndoManager.AddUndoChange(
                new Change(
                    MoveOffsetsProcess,
                    new object[] { guids, oldBounds, new Coordinates(-moveVector.X, -moveVector.Y) },
                    MoveOffsetsProcess,
                    new object[] { guids, newBounds, moveVector },
                    "Center content"));
        }

        public void Dispose()
        {
            if (Disposed)
                return;
            Disposed = true;
            DisposeLayerBitmaps();
            UndoManager.Dispose();

            GC.SuppressFinalize(this);
        }

        private void SetAsActiveOnClick(object obj)
        {
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
                return (destWidth / 2) - (srcWidth / 2);
            }

            if (anchor.HasFlag(AnchorPoint.Right))
            {
                return destWidth - srcWidth;
            }

            return 0;
        }

        private int GetOffsetYForAnchor(int srcHeight, int destHeight, AnchorPoint anchor)
        {
            if (anchor.HasFlag(AnchorPoint.Middle))
            {
                return (destHeight / 2) - (srcHeight / 2);
            }

            if (anchor.HasFlag(AnchorPoint.Bottom))
            {
                return destHeight - srcHeight;
            }

            return 0;
        }

        private DoubleCoords? GetEdgePoints(IEnumerable<Layer> layers)
        {
            if (Layers.Count == 0)
                throw new ArgumentException("Not enough layers");

            int smallestX = int.MaxValue;
            int smallestY = int.MaxValue;
            int biggestX = int.MinValue;
            int biggestY = int.MinValue;

            bool allLayersSkipped = true;

            foreach (Layer layer in layers)
            {
                Int32Rect bounds = layer.TightBounds;
                if (layer.IsReset || !bounds.HasArea)
                    continue;
                allLayersSkipped = false;

                if (layer.OffsetX + bounds.X < smallestX)
                    smallestX = layer.OffsetX + bounds.X;

                if (layer.OffsetX + bounds.X + bounds.Width > biggestX)
                    biggestX = layer.OffsetX + bounds.X + bounds.Width;

                if (layer.OffsetY + bounds.Y < smallestY)
                    smallestY = layer.OffsetY + bounds.Y;

                if (layer.OffsetY + bounds.Y + bounds.Height > biggestY)
                    biggestY = layer.OffsetY + bounds.Y + bounds.Height;
            }

            if (allLayersSkipped)
                return null;

            return new DoubleCoords(
                new Coordinates(smallestX, smallestY),
                new Coordinates(biggestX, biggestY));
        }

        private DoubleCoords? ClipLayersAndGetEdgePoints(IEnumerable<Layer> layers)
        {
            if (Layers.Count == 0)
            {
                throw new ArgumentException("Not enough layers");
            }

            int smallestX = int.MaxValue;
            int smallestY = int.MaxValue;
            int biggestX = int.MinValue;
            int biggestY = int.MinValue;

            bool allLayersSkipped = true;

            foreach (Layer layer in layers)
            {
                layer.ClipCanvas();
                if (layer.IsReset)
                    continue;
                allLayersSkipped = false;

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

            if (allLayersSkipped)
                return null;

            return new DoubleCoords(
                new Coordinates(smallestX, smallestY),
                new Coordinates(biggestX, biggestY));
        }
    }
}
