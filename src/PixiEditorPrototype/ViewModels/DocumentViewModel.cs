using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Actions.Drawing;
using PixiEditor.ChangeableDocument.Actions.Drawing.Rectangle;
using PixiEditor.ChangeableDocument.Actions.Drawing.Selection;
using PixiEditor.ChangeableDocument.Actions.Properties;
using PixiEditor.ChangeableDocument.Actions.Root;
using PixiEditor.ChangeableDocument.Actions.Structure;
using PixiEditor.ChangeableDocument.Actions.Undo;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditorPrototype.Models;
using SkiaSharp;

namespace PixiEditorPrototype.ViewModels;

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
    public RelayCommand? CreateMaskCommand { get; }
    public RelayCommand? DeleteMaskCommand { get; }
    public RelayCommand? ToggleLockTransparencyCommand { get; }

    public int Width => Helpers.Tracker.Document.Size.X;
    public int Height => Helpers.Tracker.Document.Size.Y;
    public Guid GuidValue { get; } = Guid.NewGuid();

    public Dictionary<ChunkResolution, WriteableBitmap> Bitmaps { get; set; } = new()
    {
        [ChunkResolution.Full] = new WriteableBitmap(64, 64, 96, 96, PixelFormats.Pbgra32, null),
        [ChunkResolution.Half] = new WriteableBitmap(32, 32, 96, 96, PixelFormats.Pbgra32, null),
        [ChunkResolution.Quarter] = new WriteableBitmap(16, 16, 96, 96, PixelFormats.Pbgra32, null),
        [ChunkResolution.Eighth] = new WriteableBitmap(8, 8, 96, 96, PixelFormats.Pbgra32, null),
    };

    public Dictionary<ChunkResolution, SKSurface> Surfaces { get; set; } = new();

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
        CreateMaskCommand = new RelayCommand(CreateMask);
        DeleteMaskCommand = new RelayCommand(DeleteMask);
        ToggleLockTransparencyCommand = new RelayCommand(ToggleLockTransparency);

        foreach (var bitmap in Bitmaps)
        {
            var surface = SKSurface.Create(
                new SKImageInfo(bitmap.Value.PixelWidth, bitmap.Value.PixelHeight, SKColorType.Bgra8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb()),
                bitmap.Value.BackBuffer, bitmap.Value.BackBufferStride);
            Surfaces[bitmap.Key] = surface;
        }
    }

    bool startedRectangle = false;
    public void StartUpdateRectangle(ShapeData data)
    {
        if (SelectedStructureMember is null)
            return;
        bool drawOnMask = SelectedStructureMember.HasMask && SelectedStructureMember.ShouldDrawOnMask;
        if (SelectedStructureMember is not LayerViewModel && !drawOnMask)
            return;
        startedRectangle = true;
        Helpers.ActionAccumulator.AddActions(new DrawRectangle_Action(SelectedStructureMember.GuidValue, data, drawOnMask));
    }

    public void EndRectangle()
    {
        if (!startedRectangle)
            return;
        startedRectangle = false;
        Helpers.ActionAccumulator.AddFinishedActions(new EndDrawRectangle_Action());
    }

    bool startedSelection = false;
    public void StartUpdateSelection(Vector2i pos, Vector2i size)
    {
        if (!startedSelection)
            Helpers.ActionAccumulator.AddActions(new ClearSelection_Action());
        startedSelection = true;
        Helpers.ActionAccumulator.AddActions(new SelectRectangle_Action(pos, size));
    }

    public void EndSelection()
    {
        if (!startedSelection)
            return;
        startedSelection = false;
        Helpers.ActionAccumulator.AddFinishedActions(new EndSelectRectangle_Action());
    }

    public void ForceRefreshView()
    {
        owner.View?.ForceRefreshFinalImage();
    }

    public void UpdateViewportResolution(Guid viewportGuid, ChunkResolution resolution)
    {
        owner.UpdateViewportResolution(viewportGuid, resolution);
    }

    public ViewportLocation? GetViewport(Guid viewportGuid)
    {
        return owner.GetViewport(viewportGuid);
    }

    public void RefreshViewport(Guid viewportGuid)
    {
        Helpers.ActionAccumulator.AddActions(new RefreshViewport_PassthroughAction(viewportGuid));
    }

    private void ClearSelection(object? param)
    {
        Helpers.ActionAccumulator.AddFinishedActions(new ClearSelection_Action());
    }

    private void DeleteStructureMember(object? param)
    {
        if (SelectedStructureMember is not null)
            Helpers.ActionAccumulator.AddFinishedActions(new DeleteStructureMember_Action(SelectedStructureMember.GuidValue));
    }

    private void Undo(object? param)
    {
        Helpers.ActionAccumulator.AddActions(new Undo_Action());
    }

    private void Redo(object? param)
    {
        Helpers.ActionAccumulator.AddActions(new Redo_Action());
    }

    private void ToggleLockTransparency(object? param)
    {
        if (SelectedStructureMember is not LayerViewModel layer)
            return;
        layer.LockTransparency = !layer.LockTransparency;
    }

    private void ResizeCanvas(object? param)
    {
        Helpers.ActionAccumulator.AddFinishedActions(new ResizeCanvas_Action(new(ResizeWidth, ResizeHeight)));
    }

    private void ChangeSelectedItem(object? param)
    {
        SelectedStructureMember = (StructureMemberViewModel?)((RoutedPropertyChangedEventArgs<object>?)param)?.NewValue;
    }

    private void CreateMask(object? param)
    {
        if (SelectedStructureMember is null || SelectedStructureMember.HasMask)
            return;
        Helpers.ActionAccumulator.AddFinishedActions(new CreateStructureMemberMask_Action(SelectedStructureMember.GuidValue));
    }

    private void DeleteMask(object? param)
    {
        if (SelectedStructureMember is null || !SelectedStructureMember.HasMask)
            return;
        Helpers.ActionAccumulator.AddFinishedActions(new DeleteStructureMemberMask_Action(SelectedStructureMember.GuidValue));
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
        Helpers.ActionAccumulator.AddActions(
            new CreateStructureMember_Action(parent.GuidValue, newGuid, index, StructureMemberType.Layer),
            new SetStructureMemberName_Action(child.Name + "-comb", newGuid),
            new CombineStructureMembersOnto_Action(newGuid, selected.ToHashSet()));
        foreach (var member in selected)
            Helpers.ActionAccumulator.AddActions(new DeleteStructureMember_Action(member));
        Helpers.ActionAccumulator.AddActions(new ChangeBoundary_Action());
    }

    private void MoveViewport(object? param)
    {
        Helpers.ActionAccumulator.AddActions(new RefreshViewport_PassthroughAction(Guid.Empty));
    }

    private void ClearHistory(object? param)
    {
        Helpers.ActionAccumulator.AddActions(new DeleteRecordedChanges_Action());
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
