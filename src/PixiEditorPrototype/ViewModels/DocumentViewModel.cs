using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument;
using PixiEditor.ChangeableDocument.Actions.Document;
using PixiEditor.ChangeableDocument.Actions.Drawing;
using PixiEditor.ChangeableDocument.Actions.Drawing.Rectangle;
using PixiEditor.ChangeableDocument.Actions.Drawing.Selection;
using PixiEditor.ChangeableDocument.Actions.Properties;
using PixiEditor.ChangeableDocument.Actions.Structure;
using PixiEditor.ChangeableDocument.Actions.Undo;
using PixiEditor.Zoombox;
using PixiEditorPrototype.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
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

        public void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public FolderViewModel StructureRoot { get; }
        public RelayCommand? UndoCommand { get; }
        public RelayCommand? RedoCommand { get; }
        public RelayCommand? ClearSelectionCommand { get; }
        public RelayCommand? CreateNewLayerCommand { get; }
        public RelayCommand? CreateNewFolderCommand { get; }
        public RelayCommand? DeleteStructureMemberCommand { get; }
        public RelayCommand? ChangeSelectedItemCommand { get; }
        public RelayCommand? ResizeCanvasCommand { get; }
        public RelayCommand? CombineCommand { get; }
        public RelayCommand? ClearHistoryCommand { get; }
        public RelayCommand? MoveViewportCommand { get; }


        public SKSurface SurfaceFull { get; set; }
        public WriteableBitmap BitmapFull { get; set; } = new WriteableBitmap(64, 64, 96, 96, PixelFormats.Pbgra32, null);
        public SKSurface? SurfaceHalf { get; set; } = null;
        public WriteableBitmap? BitmapHalf { get; set; } = null;
        public SKSurface? SurfaceQuarter { get; set; } = null;
        public WriteableBitmap? BitmapQuarter { get; set; } = null;
        public SKSurface? SurfaceEighth { get; set; } = null;
        public WriteableBitmap? BitmapEighth { get; set; } = null;

        public WriteableBitmap RenderBitmap
        {
            get => GetCorrespondingBitmap(RenderResolution)!;
        }

        public ChunkResolution RenderResolution
        {
            get
            {
                var targetRes = GetResolutionForViewport(Helpers.State.ViewportSize, Helpers.State.ViewportRealSize);
                if (GetCorrespondingBitmap(targetRes) is not null)
                    return targetRes;
                return ChunkResolution.Full;
            }
        }

        public ChunkResolution GetResolutionForViewport(Vector2d size, Vector2d realSize)
        {
            Vector2d densityVec = size.Divide(realSize);
            double density = Math.Min(densityVec.X, densityVec.Y);
            if (density > 8.01)
                return ChunkResolution.Eighth;
            else if (density > 4.01)
                return ChunkResolution.Quarter;
            else if (density > 2.01)
                return ChunkResolution.Half;
            return ChunkResolution.Full;
        }

        public WriteableBitmap? GetCorrespondingBitmap(ChunkResolution resolution)
        {
            return resolution switch
            {
                ChunkResolution.Full => BitmapFull,
                ChunkResolution.Half => BitmapHalf,
                ChunkResolution.Quarter => BitmapQuarter,
                ChunkResolution.Eighth => BitmapEighth,
                _ => BitmapFull,
            };
        }

        public int ResizeWidth { get; set; }
        public int ResizeHeight { get; set; }

        private DocumentHelpers Helpers { get; }

        private ViewModelMain owner;

        public DocumentViewModel(ViewModelMain owner)
        {
            this.owner = owner;

            Helpers = new DocumentHelpers(this);
            StructureRoot = new FolderViewModel(this, Helpers, Helpers.Tracker.Document.ReadOnlyStructureRoot);


            UndoCommand = new RelayCommand(Undo);
            RedoCommand = new RelayCommand(Redo);
            ClearSelectionCommand = new RelayCommand(ClearSelection);
            CreateNewLayerCommand = new RelayCommand(_ => Helpers.StructureHelper.CreateNewStructureMember(StructureMemberType.Layer));
            CreateNewFolderCommand = new RelayCommand(_ => Helpers.StructureHelper.CreateNewStructureMember(StructureMemberType.Folder));
            DeleteStructureMemberCommand = new RelayCommand(DeleteStructureMember);
            ChangeSelectedItemCommand = new RelayCommand(ChangeSelectedItem);
            ResizeCanvasCommand = new RelayCommand(ResizeCanvas);
            CombineCommand = new RelayCommand(Combine);
            ClearHistoryCommand = new RelayCommand(ClearHistory);
            MoveViewportCommand = new RelayCommand(MoveViewport);

            SurfaceFull = SKSurface.Create(
                new SKImageInfo(BitmapFull.PixelWidth, BitmapFull.PixelHeight, SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb()),
                BitmapFull.BackBuffer,
                BitmapFull.BackBufferStride);
        }

        bool startedRectangle = false;
        public void StartUpdateRectangle(ShapeData data)
        {
            if (SelectedStructureMember is null)
                return;
            startedRectangle = true;
            Helpers.ActionAccumulator.AddAction(new DrawRectangle_Action(SelectedStructureMember.GuidValue, data));
        }

        public void EndRectangle()
        {
            if (!startedRectangle)
                return;
            startedRectangle = false;
            Helpers.ActionAccumulator.AddAction(new EndDrawRectangle_Action());
        }

        bool startedSelection = false;
        public void StartUpdateSelection(Vector2i pos, Vector2i size)
        {
            if (!startedSelection)
                Helpers.ActionAccumulator.AddAction(new ClearSelection_Action());
            startedSelection = true;
            Helpers.ActionAccumulator.AddAction(new SelectRectangle_Action(pos, size));
        }

        public void EndSelection()
        {
            if (!startedSelection)
                return;
            startedSelection = false;
            Helpers.ActionAccumulator.AddAction(new EndSelectRectangle_Action());
        }

        public void ForceRefreshView()
        {
            owner.View?.ForceRefreshFinalImage();
        }

        private void ClearSelection(object? param)
        {
            Helpers.ActionAccumulator.AddAction(new ClearSelection_Action());
        }

        private void DeleteStructureMember(object? param)
        {
            if (SelectedStructureMember is not null)
                Helpers.ActionAccumulator.AddAction(new DeleteStructureMember_Action(SelectedStructureMember.GuidValue));
        }

        private void Undo(object? param)
        {
            Helpers.ActionAccumulator.AddAction(new Undo_Action());
        }

        private void Redo(object? param)
        {
            Helpers.ActionAccumulator.AddAction(new Redo_Action());
        }

        private void ResizeCanvas(object? param)
        {
            Helpers.ActionAccumulator.AddAction(new ResizeCanvas_Action(new(ResizeWidth, ResizeHeight)));
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

            var (child, parent) = Helpers.StructureHelper.FindChildAndParentOrThrow(selected[0]);
            int index = parent.Children.IndexOf(child);
            Guid newGuid = Guid.NewGuid();

            //make a new layer, put combined image onto it, delete layers that were merged
            Helpers.ActionAccumulator.AddAction(new CreateStructureMember_Action(parent.GuidValue, newGuid, index, StructureMemberType.Layer));
            Helpers.ActionAccumulator.AddAction(new SetStructureMemberName_Action(child.Name + "-comb", newGuid));
            Helpers.ActionAccumulator.AddAction(new CombineStructureMembersOnto_Action(newGuid, selected.ToHashSet()));
            foreach (var member in selected)
                Helpers.ActionAccumulator.AddAction(new DeleteStructureMember_Action(member));
            Helpers.ActionAccumulator.AddAction(new MergeLatestChanges_Action(3 + selected.Count));
        }

        private void MoveViewport(object? param)
        {
            if (param is null)
                throw new ArgumentNullException(nameof(param));
            var args = (ViewportRoutedEventArgs)param;
            Helpers.ActionAccumulator.AddAction(new MoveViewport_PassthroughAction(args.Center, args.Size / 2, args.Angle, args.RealSize / 2));
        }

        private void ClearHistory(object? param)
        {
            Helpers.ActionAccumulator.AddAction(new DeleteRecordedChanges_Action());
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
    }
}
