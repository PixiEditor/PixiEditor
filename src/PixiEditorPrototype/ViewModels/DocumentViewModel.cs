using ChangeableDocument;
using ChangeableDocument.Actions;
using ChangeableDocument.Actions.Drawing;
using PixiEditorPrototype.Models;
using SkiaSharp;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PixiEditorPrototype.ViewModels
{
    internal class DocumentViewModel : INotifyPropertyChanged
    {
        private StructureMemberViewModel? selectedStructureMember;
        public StructureMemberViewModel? SelectedStructureMember
        {
            get => selectedStructureMember;
            private set
            {
                selectedStructureMember = value;
                PropertyChanged?.Invoke(this, new(nameof(SelectedStructureMember)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ActionAccumulator ActionAccumulator { get; }
        public DocumentChangeTracker Tracker { get; }
        public DocumentStructureHelper StructureHelper { get; }
        private DocumentUpdater Updater { get; }


        public FolderViewModel StructureRoot { get; }
        public RelayCommand? UndoCommand { get; }
        public RelayCommand? RedoCommand { get; }
        public RelayCommand? CreateNewLayerCommand { get; }
        public RelayCommand? CreateNewFolderCommand { get; }
        public RelayCommand? DeleteStructureMemberCommand { get; }
        public RelayCommand? ChangeSelectedItemCommand { get; }

        public RelayCommand? MouseDownCommand { get; }
        public RelayCommand? MouseMoveCommand { get; }
        public RelayCommand? MouseUpCommand { get; }

        private WriteableBitmap finalBitmap = new WriteableBitmap(64, 64, 96, 96, PixelFormats.Pbgra32, null);
        public WriteableBitmap FinalBitmap
        {
            get => finalBitmap;
            set
            {
                finalBitmap = value;
                PropertyChanged?.Invoke(this, new(nameof(FinalBitmap)));
            }
        }
        public SKSurface FinalBitmapSurface { get; set; }

        public Color SelectedColor { get; set; } = Colors.Black;

        public DocumentViewModel()
        {
            Tracker = new DocumentChangeTracker();
            Updater = new DocumentUpdater(this);
            StructureRoot = new FolderViewModel(this, Tracker.Document.ReadOnlyStructureRoot);
            ActionAccumulator = new ActionAccumulator(Tracker, Updater, this);
            StructureHelper = new DocumentStructureHelper(this);

            UndoCommand = new RelayCommand(Undo, _ => true);
            RedoCommand = new RelayCommand(Redo, _ => true);
            CreateNewLayerCommand = new RelayCommand(_ => StructureHelper.CreateNewStructureMember(StructureMemberType.Layer), _ => true);
            CreateNewFolderCommand = new RelayCommand(_ => StructureHelper.CreateNewStructureMember(StructureMemberType.Folder), _ => true);
            DeleteStructureMemberCommand = new RelayCommand(DeleteStructureMember, _ => true);
            ChangeSelectedItemCommand = new RelayCommand(ChangeSelectedItem, _ => true);

            MouseDownCommand = new RelayCommand(MouseDown);
            MouseMoveCommand = new RelayCommand(MouseMove);
            MouseUpCommand = new RelayCommand(MouseUp);

            FinalBitmapSurface = SKSurface.Create(
                new SKImageInfo(FinalBitmap.PixelWidth, FinalBitmap.PixelHeight, SKColorType.Bgra8888, SKAlphaType.Premul),
                FinalBitmap.BackBuffer,
                FinalBitmap.BackBufferStride);
        }

        private bool drawing = false;
        private int mouseDownX = 0;
        private int mouseDownY = 0;
        public void MouseDown(object? param)
        {
            if (SelectedStructureMember != null && SelectedStructureMember is LayerViewModel)
            {
                drawing = true;
                var args = (MouseButtonEventArgs)(param!);
                var source = (System.Windows.Controls.Image)args.Source;
                var pos = args.GetPosition(source);
                mouseDownX = (int)(pos.X / source.Width * FinalBitmap.PixelHeight);
                mouseDownY = (int)(pos.Y / source.Height * FinalBitmap.PixelHeight);
            }
        }

        public void MouseMove(object? param)
        {
            if (!drawing)
                return;
            var args = (MouseEventArgs)(param!);
            var source = (System.Windows.Controls.Image)args.Source;
            var pos = args.GetPosition(source);
            int curX = (int)(pos.X / source.Width * FinalBitmap.PixelHeight);
            int curY = (int)(pos.Y / source.Height * FinalBitmap.PixelHeight);
            ActionAccumulator.AddAction
                (
                    new DrawRectangle_Action
                    (
                        SelectedStructureMember!.GuidValue,
                        new(new(mouseDownX, mouseDownY),
                        new(curX - mouseDownX, curY - mouseDownY),
                        1,
                        new SKColor(SelectedColor.R, SelectedColor.G, SelectedColor.B, SelectedColor.A),
                        SKColors.Transparent)
                    )
                );
        }

        public void MouseUp(object? param)
        {
            if (!drawing)
                return;
            ActionAccumulator.AddAction(new EndDrawRectangle_Action());
            drawing = false;
        }

        public void DeleteStructureMember(object? param)
        {
            if (SelectedStructureMember != null)
                ActionAccumulator.AddAction(new DeleteStructureMember_Action(SelectedStructureMember.GuidValue));
        }

        public void Undo(object? param)
        {
            ActionAccumulator.AddAction(new UndoAction());
        }

        public void Redo(object? param)
        {
            ActionAccumulator.AddAction(new RedoAction());
        }

        private void ChangeSelectedItem(object? param)
        {
            SelectedStructureMember = (StructureMemberViewModel?)((RoutedPropertyChangedEventArgs<object>?)param)?.NewValue;
        }
    }
}
