using System.Windows.Input;
using PixiEditor.Helpers;
using PixiEditor.Models.Commands.Attributes;

namespace PixiEditor.ViewModels.SubViewModels.Main;

[Command.Group("PixiEditor.Layer", "Image")]
internal class LayersViewModel : SubViewModel<ViewModelMain>
{
    public RelayCommand SetActiveLayerCommand { get; set; }

    public RelayCommand NewLayerCommand { get; set; }

    public RelayCommand NewGroupCommand { get; set; }

    public RelayCommand CreateGroupFromActiveLayersCommand { get; set; }

    public RelayCommand DeleteSelectedCommand { get; set; }

    public RelayCommand DeleteGroupCommand { get; set; }

    public RelayCommand DeleteLayersCommand { get; set; }

    public RelayCommand DuplicateLayerCommand { get; set; }

    public RelayCommand RenameLayerCommand { get; set; }

    public RelayCommand RenameGroupCommand { get; set; }

    public RelayCommand MoveToBackCommand { get; set; }

    public RelayCommand MoveToFrontCommand { get; set; }

    public RelayCommand MergeSelectedCommand { get; set; }

    public RelayCommand MergeWithAboveCommand { get; set; }

    public RelayCommand MergeWithBelowCommand { get; set; }

    public LayersViewModel(ViewModelMain owner)
        : base(owner)
    {
        SetActiveLayerCommand = new RelayCommand(SetActiveLayer);
        NewLayerCommand = new RelayCommand(NewLayer, CanCreateNewLayer);
        NewGroupCommand = new RelayCommand(NewGroup, CanCreateNewLayer);
        CreateGroupFromActiveLayersCommand = new RelayCommand(CreateGroupFromActiveLayers, CanCreateGroupFromSelected);
        DeleteLayersCommand = new RelayCommand(DeleteActiveLayers, CanDeleteActiveLayers);
        DuplicateLayerCommand = new RelayCommand(DuplicateLayer, CanDuplicateLayer);
        MoveToBackCommand = new RelayCommand(MoveLayerToBack, CanMoveToBack);
        MoveToFrontCommand = new RelayCommand(MoveLayerToFront, CanMoveToFront);
        RenameLayerCommand = new RelayCommand(RenameLayer);
        MergeSelectedCommand = new RelayCommand(MergeSelected, CanMergeSelected);
        MergeWithAboveCommand = new RelayCommand(MergeWithAbove, CanMergeWithAbove);
        MergeWithBelowCommand = new RelayCommand(MergeWithBelow, CanMergeWithBelow);
        RenameGroupCommand = new RelayCommand(RenameGroup);
        DeleteGroupCommand = new RelayCommand(DeleteGroup, CanDeleteGroup);
        DeleteSelectedCommand = new RelayCommand(DeleteSelected, CanDeleteSelected);
    }

    public void CreateGroupFromActiveLayers(object parameter)
    {

    }

    public bool CanDeleteSelected(object parameter)
    {
        /*
        bool paramIsLayerOrGroup = parameter is not null and (Layer or LayerGroup);
        bool activeLayerExists = Owner.BitmapManager?.ActiveDocument?.ActiveLayer != null;
        bool activeDocumentExists = Owner.BitmapManager.ActiveDocument != null;
        bool allGood = (paramIsLayerOrGroup || activeLayerExists) && activeDocumentExists;
        if (!allGood)
            return false;

        if (parameter is Layer or LayerStructureItemContainer)
        {
            return CanDeleteActiveLayers(null);
        }
        else if (parameter is LayerGroup group)
        {
            return CanDeleteGroup(group.GuidValue);
        }
        else if (parameter is LayerGroupControl groupControl)
        {
            return CanDeleteGroup(groupControl.GroupGuid);
        }
        else if (Owner.BitmapManager.ActiveDocument.ActiveLayer != null)
        {
            return CanDeleteActiveLayers(null);
        }*/
        return false;
    }

    public void DeleteSelected(object parameter)
    {
        /*
        if (parameter is Layer or LayerStructureItemContainer)
        {
            DeleteActiveLayers(null);
        }
        else if (parameter is LayerGroup group)
        {
            DeleteGroup(group.GuidValue);
        }
        else if (parameter is LayerGroupControl groupControl)
        {
            DeleteGroup(groupControl.GroupGuid);
        }
        else if (Owner.BitmapManager.ActiveDocument.ActiveLayer != null)
        {
            DeleteActiveLayers(null);
        }*/
    }

    public bool CanDeleteGroup(object parameter)
    {
        return false;
    }

    public void DeleteGroup(object parameter)
    {

    }

    public void RenameGroup(object parameter)
    {
    }

    public void NewGroup(object parameter)
    {

    }

    public bool CanAddNewGroup(object property)
    {
        return CanCreateNewLayer(property) && false;
    }

    public bool CanMergeSelected(object obj)
    {
        return false;
    }

    public bool CanCreateGroupFromSelected(object obj)
    {
        return false;
    }

    [Command.Basic("PixiEditor.Layer.New", "New Layer", "Create new layer", CanExecute = "PixiEditor.HasDocument", Key = Key.N, Modifiers = ModifierKeys.Control | ModifierKeys.Shift, IconPath = "Layer-add.png")]
    public void NewLayer(object parameter)
    {

    }

    public bool CanCreateNewLayer(object parameter)
    {
        return false;
    }

    public void SetActiveLayer(object parameter)
    {
        //int index = (int)parameter;

        //var doc = Owner.BitmapManager.ActiveDocument;

        /*if (doc.Layers[index].IsActive && Mouse.RightButton == MouseButtonState.Pressed)
        {
            return;
        }*/

        if (Keyboard.IsKeyDown(Key.LeftCtrl))
        {
            //doc.ToggleLayer(index);
        }
        //else if (Keyboard.IsKeyDown(Key.LeftShift) && Owner.BitmapManager.ActiveDocument.Layers.Any(x => x.IsActive))
        {
            //doc.SelectLayersRange(index);
        }
        //else
        {
            //doc.SetMainActiveLayer(index);
        }
    }

    public void DeleteActiveLayers(object unusedParameter)
    {

    }

    public bool CanDeleteActiveLayers(object unusedParam)
    {
        return false;
    }

    public void DuplicateLayer(object parameter)
    {

    }

    public bool CanDuplicateLayer(object property)
    {
        return false;
    }

    public void RenameLayer(object parameter)
    {

    }

    public bool CanRenameLayer(object parameter)
    {
        return false;
    }

    public void MoveLayerToFront(object parameter)
    {

    }

    public void MoveLayerToBack(object parameter)
    {

    }

    public bool CanMoveToFront(object property)
    {
        return false;
    }

    public bool CanMoveToBack(object property)
    {
        return false;
    }

    public void MergeSelected(object parameter)
    {

    }

    public void MergeWithAbove(object parameter)
    {

    }

    public void MergeWithBelow(object parameter)
    {

    }

    public bool CanMergeWithAbove(object property)
    {
        return false;
    }

    public bool CanMergeWithBelow(object property)
    {
        return false;
    }
}
