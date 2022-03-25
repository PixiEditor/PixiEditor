using ChangeableDocument;
using ChangeableDocument.Actions.Document;
using ChangeableDocument.Actions.Drawing;
using ChangeableDocument.Actions.Drawing.Rectangle;
using ChangeableDocument.Actions.Drawing.Selection;
using ChangeableDocument.Actions.Properties;
using ChangeableDocument.Actions.Structure;
using ChangeableDocument.Actions.Undo;
using ChunkyImageLib.DataHolders;
using PixiEditorPrototype.Models;
using PixiEditorPrototype.Views;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

        private Tool activeTool = Tool.Rectangle;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ActionAccumulator ActionAccumulator { get; }
        public DocumentChangeTracker Tracker { get; }
        public DocumentStructureHelper StructureHelper { get; }
        public IDocumentView? View { get; set; }
        private DocumentUpdater Updater { get; }


        public FolderViewModel StructureRoot { get; }
        public RelayCommand? UndoCommand { get; }
        public RelayCommand? RedoCommand { get; }
        public RelayCommand? ClearSelectionCommand { get; }
        public RelayCommand? CreateNewLayerCommand { get; }
        public RelayCommand? CreateNewFolderCommand { get; }
        public RelayCommand? DeleteStructureMemberCommand { get; }
        public RelayCommand? ChangeSelectedItemCommand { get; }
        public RelayCommand? ChangeActiveToolCommand { get; }
        public RelayCommand? ResizeCanvasCommand { get; }
        public RelayCommand? CombineCommand { get; }
        public RelayCommand? ClearHistoryCommand { get; }

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
        public int ResizeWidth { get; set; }
        public int ResizeHeight { get; set; }

        public DocumentViewModel()
        {
            Tracker = new DocumentChangeTracker();
            Updater = new DocumentUpdater(this);
            StructureRoot = new FolderViewModel(this, Tracker.Document.ReadOnlyStructureRoot);
            ActionAccumulator = new ActionAccumulator(Tracker, Updater, this);
            StructureHelper = new DocumentStructureHelper(this);

            UndoCommand = new RelayCommand(Undo);
            RedoCommand = new RelayCommand(Redo);
            ClearSelectionCommand = new RelayCommand(ClearSelection);
            CreateNewLayerCommand = new RelayCommand(_ => StructureHelper.CreateNewStructureMember(StructureMemberType.Layer));
            CreateNewFolderCommand = new RelayCommand(_ => StructureHelper.CreateNewStructureMember(StructureMemberType.Folder));
            DeleteStructureMemberCommand = new RelayCommand(DeleteStructureMember);
            ChangeSelectedItemCommand = new RelayCommand(ChangeSelectedItem);
            ChangeActiveToolCommand = new RelayCommand(ChangeActiveTool);
            ResizeCanvasCommand = new RelayCommand(ResizeCanvas);
            CombineCommand = new RelayCommand(Combine);
            ClearHistoryCommand = new RelayCommand(ClearHistory);

            MouseDownCommand = new RelayCommand(MouseDown);
            MouseMoveCommand = new RelayCommand(MouseMove);
            MouseUpCommand = new RelayCommand(MouseUp);

            FinalBitmapSurface = SKSurface.Create(
                new SKImageInfo(FinalBitmap.PixelWidth, FinalBitmap.PixelHeight, SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb()),
                FinalBitmap.BackBuffer,
                FinalBitmap.BackBufferStride);
        }

        private bool mouseIsDown = false;
        private int mouseDownCanvasX = 0;
        private int mouseDownCanvasY = 0;

        private bool startedDrawingRect = false;
        private bool startedSelectingRect = false;

        public void MouseDown(object? param)
        {
            mouseIsDown = true;
            var args = (MouseButtonEventArgs)(param!);
            var source = (System.Windows.Controls.Image)args.Source;
            var pos = args.GetPosition(source);
            mouseDownCanvasX = (int)(pos.X / source.Width * FinalBitmap.PixelHeight);
            mouseDownCanvasY = (int)(pos.Y / source.Height * FinalBitmap.PixelHeight);
        }

        public void MouseMove(object? param)
        {
            if (!mouseIsDown)
                return;
            var args = (MouseEventArgs)(param!);
            var source = (System.Windows.Controls.Image)args.Source;
            var pos = args.GetPosition(source);
            int curX = (int)(pos.X / source.Width * FinalBitmap.PixelHeight);
            int curY = (int)(pos.Y / source.Height * FinalBitmap.PixelHeight);

            ProcessToolMouseMove(curX, curY);
        }

        private void ProcessToolMouseMove(int canvasX, int canvasY)
        {
            if (activeTool == Tool.Rectangle)
            {
                if (SelectedStructureMember is null)
                    return;
                startedDrawingRect = true;
                ActionAccumulator.AddAction(new DrawRectangle_Action(
                        SelectedStructureMember.GuidValue,
                        new ShapeData(
                            new(mouseDownCanvasX, mouseDownCanvasY),
                            new(canvasX - mouseDownCanvasX, canvasY - mouseDownCanvasY),
                            90,
                            new SKColor(SelectedColor.R, SelectedColor.G, SelectedColor.B, SelectedColor.A),
                            SKColors.Transparent)
                    ));
            }
            else if (activeTool == Tool.Select)
            {
                if (!startedSelectingRect)
                    ActionAccumulator.AddAction(new ClearSelection_Action());
                startedSelectingRect = true;
                ActionAccumulator.AddAction(new SelectRectangle_Action(
                        new(mouseDownCanvasX, mouseDownCanvasY),
                        new(canvasX - mouseDownCanvasX, canvasY - mouseDownCanvasY)
                    ));
            }
        }

        public void MouseUp(object? param)
        {
            if (!mouseIsDown)
                return;
            mouseIsDown = false;
            ProcessToolMouseUp();
        }

        private void ProcessToolMouseUp()
        {
            if (startedDrawingRect)
            {
                startedDrawingRect = false;
                ActionAccumulator.AddAction(new EndDrawRectangle_Action());
            }
            if (startedSelectingRect)
            {
                startedSelectingRect = false;
                ActionAccumulator.AddAction(new EndSelectRectangle_Action());
                ActionAccumulator.AddAction(new MergeLatestChanges_Action(2));
            }
        }

        public void ClearSelection(object? param)
        {
            ActionAccumulator.AddAction(new ClearSelection_Action());
        }

        public void DeleteStructureMember(object? param)
        {
            if (SelectedStructureMember is not null)
                ActionAccumulator.AddAction(new DeleteStructureMember_Action(SelectedStructureMember.GuidValue));
        }

        public void Undo(object? param)
        {
            ActionAccumulator.AddAction(new Undo_Action());
        }

        public void Redo(object? param)
        {
            ActionAccumulator.AddAction(new Redo_Action());
        }

        public void ResizeCanvas(object? param)
        {
            ActionAccumulator.AddAction(new ResizeCanvas_Action(new(ResizeWidth, ResizeHeight)));
        }

        private void ChangeSelectedItem(object? param)
        {
            SelectedStructureMember = (StructureMemberViewModel?)((RoutedPropertyChangedEventArgs<object>?)param)?.NewValue;
        }

        private void Combine(object? param)
        {
            if (SelectedStructureMember is null)
                return;
            List<Guid> selected = new();
            AddSelectedMembers(StructureRoot, selected);
            if (selected.Count < 2)
                return;

            var (child, parent) = StructureHelper.FindChildAndParentOrThrow(selected[0]);
            int index = parent.Children.IndexOf(child);
            Guid newGuid = Guid.NewGuid();

            //make a new layer, put combined image onto it, delete layers that were merged
            ActionAccumulator.AddAction(new CreateStructureMember_Action(parent.GuidValue, newGuid, index, StructureMemberType.Layer));
            ActionAccumulator.AddAction(new SetStructureMemberName_Action(child.Name + "-comb", newGuid));
            ActionAccumulator.AddAction(new CombineStructureMembersOnto_Action(newGuid, selected.ToHashSet()));
            foreach (var member in selected)
                ActionAccumulator.AddAction(new DeleteStructureMember_Action(member));
            ActionAccumulator.AddAction(new MergeLatestChanges_Action(3 + selected.Count));
        }

        private void ClearHistory(object? param)
        {
            ActionAccumulator.AddAction(new DeleteRecordedChanges_Action());
        }

        private void AddSelectedMembers(FolderViewModel folder, List<Guid> collection)
        {
            foreach (var child in folder.Children)
            {
                if (child.IsSelected)
                    collection.Add(child.GuidValue);
                if (child is FolderViewModel innerFolder)
                    AddSelectedMembers(innerFolder, collection);
            }
        }

        private void ChangeActiveTool(object? param)
        {
            if (param is null)
                return;
            activeTool = (Tool)param;
        }
    }
}
