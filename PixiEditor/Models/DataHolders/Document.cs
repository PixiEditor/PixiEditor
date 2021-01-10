using System;
using System.Buffers;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PixiEditor.Helpers;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Enums;
using PixiEditor.Models.IO;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Undo;
using PixiEditor.ViewModels;

namespace PixiEditor.Models.DataHolders
{
    public class Document : NotifyableObject
    {
        private int activeLayerIndex;
        private int height;
        private int width;

        public Document(int width, int height)
        {
            Width = width;
            Height = height;
            RequestCloseDocumentCommand = new RelayCommand(RequestCloseDocument);
            SetAsActiveOnClickCommand = new RelayCommand(SetAsActiveOnClick);
            UndoManager = new UndoManager();
            XamlAccesibleViewModel = ViewModelMain.Current ?? null;
            GeneratePreviewLayer();
            DocumentSizeChanged?.Invoke(this, new DocumentSizeChangedEventArgs(0, 0, width, height));
        }

        public event EventHandler<DocumentSizeChangedEventArgs> DocumentSizeChanged;

        public event EventHandler<LayersChangedEventArgs> LayersChanged;

        public RelayCommand RequestCloseDocumentCommand { get; set; }

        public RelayCommand SetAsActiveOnClickCommand { get; set; }

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

        private string documentFilePath = string.Empty;

        public string DocumentFilePath
        {
            get => documentFilePath;
            set
            {
                documentFilePath = value;
                RaisePropertyChanged(nameof(DocumentFilePath));
                RaisePropertyChanged(nameof(Name));
            }
        }

        private bool changesSaved = true;

        public bool ChangesSaved
        {
            get => changesSaved;
            set
            {
                changesSaved = value;
                RaisePropertyChanged(nameof(ChangesSaved));
                RaisePropertyChanged(nameof(Name)); // This updates name so it shows asterisk if unsaved
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
                RaisePropertyChanged("ActiveSelection");
            }
        }

        private Layer previewLayer;

        public Layer PreviewLayer
        {
            get => previewLayer;
            set
            {
                previewLayer = value;
                RaisePropertyChanged("PreviewLayer");
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

        private double zoomPercentage = 100;

        public double ZoomPercentage
        {
            get => zoomPercentage;
            set
            {
                zoomPercentage = value;
                RaisePropertyChanged(nameof(ZoomPercentage));
            }
        }

        private Point viewPortPosition;

        public Point ViewportPosition
        {
            get => viewPortPosition;
            set
            {
                viewPortPosition = value;
                RaisePropertyChanged(nameof(ViewportPosition));
            }
        }

        private bool recenterZoombox = true;

        public bool RecenterZoombox
        {
            get => recenterZoombox;
            set
            {
                recenterZoombox = value;
                RaisePropertyChanged(nameof(RecenterZoombox));
            }
        }

        public UndoManager UndoManager { get; set; }

        public ObservableCollection<Layer> Layers { get; set; } = new ObservableCollection<Layer>();

        public Layer ActiveLayer => Layers.Count > 0 ? Layers[ActiveLayerIndex] : null;

        public int ActiveLayerIndex
        {
            get => activeLayerIndex;
            set
            {
                activeLayerIndex = value;
                RaisePropertyChanged("ActiveLayerIndex");
                RaisePropertyChanged("ActiveLayer");
            }
        }

        public void GeneratePreviewLayer()
        {
            PreviewLayer = new Layer("_previewLayer")
            {
                MaxWidth = Width,
                MaxHeight = Height
            };
        }

        public void CenterViewport()
        {
            RecenterZoombox = false; // It's a trick to trigger change in UserControl
            RecenterZoombox = true;
            ViewportPosition = default;
            ZoomPercentage = default;
        }

        public void SaveWithDialog()
        {
            bool savedSuccessfully = Exporter.SaveAsEditableFileWithDialog(this, out string path);
            DocumentFilePath = path;
            ChangesSaved = savedSuccessfully;
        }

        public void Save()
        {
            Save(DocumentFilePath);
        }

        public void Save(string path)
        {
            DocumentFilePath = Exporter.SaveAsEditableFile(this, path);
            ChangesSaved = true;
        }

        public ObservableCollection<Color> Swatches { get; set; } = new ObservableCollection<Color>();

        /// <summary>
        ///     Resizes canvas to specified width and height to selected anchor.
        /// </summary>
        /// <param name="width">New width of canvas.</param>
        /// <param name="height">New height of canvas.</param>
        /// <param name="anchor">
        ///     Point that will act as "starting position" of resizing. Use pipe to connect horizontal and
        ///     vertical.
        /// </param>
        public void ResizeCanvas(int width, int height, AnchorPoint anchor)
        {
            int oldWidth = Width;
            int oldHeight = Height;

            int offsetX = GetOffsetXForAnchor(Width, width, anchor);
            int offsetY = GetOffsetYForAnchor(Height, height, anchor);

            Thickness[] oldOffsets = Layers.Select(x => x.Offset).ToArray();
            Thickness[] newOffsets = Layers.Select(x => new Thickness(offsetX + x.OffsetX, offsetY + x.OffsetY, 0, 0))
                .ToArray();

            object[] processArgs = { newOffsets, width, height };
            object[] reverseProcessArgs = { oldOffsets, Width, Height };

            ResizeCanvas(newOffsets, width, height);
            UndoManager.AddUndoChange(new Change(
                ResizeCanvasProcess,
                reverseProcessArgs,
                ResizeCanvasProcess,
                processArgs,
                "Resize canvas"));
            DocumentSizeChanged?.Invoke(this, new DocumentSizeChangedEventArgs(oldWidth, oldHeight, width, height));
        }

        public void SetActiveLayer(int index)
        {
            if (ActiveLayerIndex <= Layers.Count - 1)
            {
                ActiveLayer.IsActive = false;
            }

            if (Layers.Any(x => x.IsActive))
            {
                Layers.First(x => x.IsActive).IsActive = false;
            }

            ActiveLayerIndex = index;
            ActiveLayer.IsActive = true;
            LayersChanged?.Invoke(this, new LayersChangedEventArgs(index, LayerAction.SetActive));
        }

        public void MoveLayerIndexBy(int layerIndex, int amount)
        {
            Layers.Move(layerIndex, layerIndex + amount);
            if (ActiveLayerIndex == layerIndex)
            {
                SetActiveLayer(layerIndex + amount);
            }

            UndoManager.AddUndoChange(new Change(
                MoveLayerProcess,
                new object[] { layerIndex + amount, -amount },
                MoveLayerProcess,
                new object[] { layerIndex, amount }, 
                "Move layer"));
        }

        public void AddNewLayer(string name, WriteableBitmap bitmap, bool setAsActive = true)
        {
            AddNewLayer(name, bitmap.PixelWidth, bitmap.PixelHeight, setAsActive);
            Layers.Last().LayerBitmap = bitmap;
        }

        public void AddNewLayer(string name, bool setAsActive = true)
        {
            AddNewLayer(name, 0, 0, setAsActive);
        }

        public void AddNewLayer(string name, int width, int height, bool setAsActive = true)
        {
            Layers.Add(new Layer(name, width, height)
            {
                MaxHeight = Height,
                MaxWidth = Width
            });
            if (setAsActive)
            {
                SetActiveLayer(Layers.Count - 1);
            }

            if (Layers.Count > 1)
            {
                StorageBasedChange storageChange = new StorageBasedChange(this, new[] { Layers[^1] });
                UndoManager.AddUndoChange(
                    storageChange.ToChange(
                        RemoveLayerProcess,
                        new object[] { Layers[^1].LayerGuid },
                        RestoreLayersProcess,
                        "Add layer"));
            }

            LayersChanged?.Invoke(this, new LayersChangedEventArgs(0, LayerAction.Add));
        }

        public void SetNextLayerAsActive(int lastLayerIndex)
        {
            if (Layers.Count > 0)
            {
                if (lastLayerIndex == 0)
                {
                    SetActiveLayer(0);
                }
                else
                {
                    SetActiveLayer(lastLayerIndex - 1);
                }
            }
        }

        public void RemoveLayer(int layerIndex)
        {
            if (Layers.Count == 0)
            {
                return;
            }

            bool wasActive = Layers[layerIndex].IsActive;

            StorageBasedChange change = new StorageBasedChange(this, new[] { Layers[layerIndex] });
            UndoManager.AddUndoChange(
                change.ToChange(RestoreLayersProcess, RemoveLayerProcess, new object[] { Layers[layerIndex].LayerGuid }, "Remove layer"));

            Layers.RemoveAt(layerIndex);
            if (wasActive)
            {
                SetNextLayerAsActive(layerIndex);
            }
        }

        /// <summary>
        ///     Resizes all document layers using NearestNeighbor interpolation.
        /// </summary>
        /// <param name="newWidth">New document width.</param>
        /// <param name="newHeight">New document height.</param>
        public void Resize(int newWidth, int newHeight)
        {
            object[] reverseArgs = { Width, Height };
            object[] args = { newWidth, newHeight };
            ResizeDocument(args);
            UndoManager.AddUndoChange(new Change(
                ResizeDocument,
                reverseArgs,
                ResizeDocument,
                args,
                "Resize document"));
        }

        /// <summary>
        ///     Resizes canvas, so it fits exactly the size of drawn content, without any transparent pixels outside.
        /// </summary>
        public void ClipCanvas()
        {
            DoubleCords points = GetEdgePoints();
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

            MoveOffsets(moveVector);
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
        /// Centers content inside document.
        /// </summary>
        public void CenterContent()
        {
            DoubleCords points = GetEdgePoints();

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

            MoveOffsets(moveVector);
            UndoManager.AddUndoChange(
                new Change(
                    MoveOffsetsProcess,
                    new object[] { new Coordinates(-moveVector.X, -moveVector.Y) },
                    MoveOffsetsProcess,
                    new object[] { moveVector },
                    "Center content"));
        }

        private void MoveLayerProcess(object[] parameter)
        {
            int layerIndex = (int)parameter[0];
            int amount = (int)parameter[1];
            MoveLayerIndexBy(layerIndex, amount);
        }

        private void RestoreLayersProcess(Layer[] layers, UndoLayer[] layersData)
        {
            for (int i = 0; i < layers.Length; i++)
            {
                Layer layer = layers[i];

                Layers.Insert(layersData[i].LayerIndex, layer);
                if (layer.IsActive)
                {
                    SetActiveLayer(Layers.IndexOf(layer));
                }
            }
        }

        private void RemoveLayerProcess(object[] parameters)
        {
            if (parameters != null && parameters.Length > 0 && parameters[0] is Guid layerGuid)
            {
                Layer layer = Layers.First(x => x.LayerGuid == layerGuid);
                int index = Layers.IndexOf(layer);
                bool wasActive = layer.IsActive;
                Layers.Remove(layer);

                if (wasActive)
                {
                    SetNextLayerAsActive(index);
                }
            }
        }

        private void SetAsActiveOnClick(object obj)
        {
            XamlAccesibleViewModel.BitmapManager.MouseController.StopRecordingMouseMovementChanges();
            XamlAccesibleViewModel.BitmapManager.MouseController.StartRecordingMouseMovementChanges(true);
            XamlAccesibleViewModel.BitmapManager.ActiveDocument = this;
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

        private void ResizeDocument(object[] arguments)
        {
            int oldWidth = Width;
            int oldHeight = Height;

            int newWidth = (int)arguments[0];
            int newHeight = (int)arguments[1];

            for (int i = 0; i < Layers.Count; i++)
            {
                float widthRatio = (float)newWidth / Width;
                float heightRatio = (float)newHeight / Height;
                int layerWidth = (int)(Layers[i].Width * widthRatio);
                int layerHeight = (int)(Layers[i].Height * heightRatio);

                Layers[i].Resize(layerWidth, layerHeight, newWidth, newHeight);
                Layers[i].Offset = new Thickness(Layers[i].OffsetX * widthRatio, Layers[i].OffsetY * heightRatio, 0, 0);
            }

            Height = newHeight;
            Width = newWidth;
            DocumentSizeChanged?.Invoke(
                this,
                new DocumentSizeChangedEventArgs(oldWidth, oldHeight, newWidth, newHeight));
        }

        private void ResizeCanvasProcess(object[] arguments)
        {
            int oldWidth = Width;
            int oldHeight = Height;

            Thickness[] offset = (Thickness[])arguments[0];
            int width = (int)arguments[1];
            int height = (int)arguments[2];
            ResizeCanvas(offset, width, height);
            DocumentSizeChanged?.Invoke(this, new DocumentSizeChangedEventArgs(oldWidth, oldHeight, width, height));
        }

        /// <summary>
        ///     Resizes canvas.
        /// </summary>
        /// <param name="offset">Offset of content in new canvas. It will move layer to that offset.</param>
        /// <param name="newWidth">New canvas size.</param>
        /// <param name="newHeight">New canvas height.</param>
        private void ResizeCanvas(Thickness[] offset, int newWidth, int newHeight)
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].Offset = offset[i];
                Layers[i].MaxWidth = newWidth;
                Layers[i].MaxHeight = newHeight;
            }

            Width = newWidth;
            Height = newHeight;
        }

        private DoubleCords GetEdgePoints()
        {
            Layer firstLayer = Layers[0];
            int smallestX = firstLayer.OffsetX;
            int smallestY = firstLayer.OffsetY;
            int biggestX = smallestX + firstLayer.Width;
            int biggestY = smallestY + firstLayer.Height;

            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].ClipCanvas();
                if (Layers[i].OffsetX < smallestX)
                {
                    smallestX = Layers[i].OffsetX;
                }

                if (Layers[i].OffsetX + Layers[i].Width > biggestX)
                {
                    biggestX = Layers[i].OffsetX + Layers[i].Width;
                }

                if (Layers[i].OffsetY < smallestY)
                {
                    smallestY = Layers[i].OffsetY;
                }

                if (Layers[i].OffsetY + Layers[i].Height > biggestY)
                {
                    biggestY = Layers[i].OffsetY + Layers[i].Height;
                }
            }

            return new DoubleCords(
                new Coordinates(smallestX, smallestY),
                new Coordinates(biggestX, biggestY));
        }

        /// <summary>
        ///     Moves offsets of layers by specified vector.
        /// </summary>
        private void MoveOffsets(Coordinates moveVector)
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                Thickness offset = Layers[i].Offset;
                Layers[i].Offset = new Thickness(offset.Left + moveVector.X, offset.Top + moveVector.Y, 0, 0);
            }
        }

        private void MoveOffsetsProcess(object[] arguments)
        {
            Coordinates vector = (Coordinates)arguments[0];
            MoveOffsets(vector);
        }
    }
}