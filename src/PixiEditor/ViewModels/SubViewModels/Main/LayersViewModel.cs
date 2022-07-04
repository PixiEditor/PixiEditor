using PixiEditor.Helpers;
using PixiEditor.Models.Commands.Attributes;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Layers;
using PixiEditor.Views.UserControls.Layers;
using System;
using System.Linq;
using System.Windows.Input;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    [Command.Group("PixiEditor.Layer", "Image")]
    public class LayersViewModel : SubViewModel<ViewModelMain>
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
            Owner.BitmapManager.DocumentChanged += BitmapManager_DocumentChanged;
        }

        public void CreateGroupFromActiveLayers(object parameter)
        {
            // var doc = Owner.BitmapManager.ActiveDocument;
            // if (doc != null)
            // {
            //    doc.LayerStructure.AddNewGroup($"{Owner.BitmapManager.ActiveLayer.Name} Group", doc.Layers.Where(x => x.IsActive).Reverse(), Owner.BitmapManager.ActiveDocument.ActiveLayerGuid);
            // }
        }

        public bool CanDeleteSelected(object parameter)
        {
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
            }
            return false;
        }

        public void DeleteSelected(object parameter)
        {
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
            }
        }

        public bool CanDeleteGroup(object parameter)
        {
            if (parameter is not Guid guid)
                return false;

            var document = Owner.BitmapManager.ActiveDocument;
            if (document == null)
                return false;

            var group = document.LayerStructure.GetGroupByGuid(guid);
            if (group == null)
                return false;

            return document.LayerStructure.GetGroupLayers(group).Count < document.Layers.Count;
        }

        public void DeleteGroup(object parameter)
        {
            if (parameter is Guid guid)
            {
                foreach (var layer in Owner.BitmapManager.ActiveDocument?.Layers)
                {
                    layer.IsActive = false;
                }

                var group = Owner.BitmapManager.ActiveDocument?.LayerStructure.GetGroupByGuid(guid);
                var layers = Owner.BitmapManager.ActiveDocument?.LayerStructure.GetGroupLayers(group);
                foreach (var layer in layers)
                {
                    layer.IsActive = true;
                }

                Owner.BitmapManager.ActiveDocument?.RemoveActiveLayers();
            }
        }

        public void RenameGroup(object parameter)
        {
            if (parameter is Guid guid)
            {
                var group = Owner.BitmapManager.ActiveDocument?.LayerStructure.GetGroupByGuid(guid);
                group.IsRenaming = true;
            }
        }

        public void NewGroup(object parameter)
        {
            GuidStructureItem control = GetGroupFromParameter(parameter);
            var doc = Owner.BitmapManager.ActiveDocument;
            if (doc != null)
            {
                var lastGroups = doc.LayerStructure.CloneGroups();
                if (parameter is Layer or LayerStructureItemContainer)
                {
                    GuidStructureItem group = doc.LayerStructure.AddNewGroup($"{doc.ActiveLayer.Name} Group", doc.ActiveLayer.GuidValue);

                    Owner.BitmapManager.ActiveDocument.LayerStructure.ExpandParentGroups(group);
                }
                else if (control != null)
                {
                    doc.LayerStructure.AddNewGroup($"{control.Name} Group", control);
                }

                doc.AddLayerStructureToUndo(lastGroups);
                doc.RaisePropertyChange(nameof(doc.LayerStructure));
            }
        }

        public bool CanAddNewGroup(object property)
        {
            return CanCreateNewLayer(property) && Owner.BitmapManager.ActiveLayer != null;
        }

        public bool CanMergeSelected(object obj)
        {
            return Owner.BitmapManager.ActiveDocument?.Layers.Count(x => x.IsActive) > 1;
        }

        public bool CanCreateGroupFromSelected(object obj)
        {
            return Owner.BitmapManager.ActiveDocument?.Layers.Count(x => x.IsActive) > 0;
        }

        [Command.Basic("PixiEditor.Layer.New", "New Layer", "Create new layer", CanExecute = "PixiEditor.HasDocument", Key = Key.N, Modifiers = ModifierKeys.Control | ModifierKeys.Shift, IconPath = "Layer-add.png")]
        public void NewLayer(object parameter)
        {
            GuidStructureItem control = GetGroupFromParameter(parameter);
            var doc = Owner.BitmapManager.ActiveDocument;
            var activeLayerParent = doc.LayerStructure.GetGroupByLayer(doc.ActiveLayerGuid);

            Guid lastActiveLayerGuid = doc.ActiveLayerGuid;


            doc.AddNewLayer($"New Layer {Owner.BitmapManager.ActiveDocument.Layers.Count}");

            var oldGroups = doc.LayerStructure.CloneGroups();

            if (doc.Layers.Count > 1)
            {
                doc.MoveLayerInStructure(doc.Layers[^1].GuidValue, lastActiveLayerGuid, true);
                Guid? parent = null;
                if (activeLayerParent != null)
                {
                    parent = parameter is Layer or LayerStructureItemContainer ? activeLayerParent?.GroupGuid : activeLayerParent.Parent?.GroupGuid;
                }
                doc.LayerStructure.AssignParent(doc.ActiveLayerGuid, parent);
                doc.AddLayerStructureToUndo(oldGroups);
                doc.UndoManager.SquashUndoChanges(3, "Add New Layer");
            }
            if (control != null)
            {
                control.IsExpanded = true;
                doc.RaisePropertyChange(nameof(doc.LayerStructure));
            }
        }

        public bool CanCreateNewLayer(object parameter)
        {
            return Owner.BitmapManager.ActiveDocument != null;
        }

        public void SetActiveLayer(object parameter)
        {
            int index = (int)parameter;

            var doc = Owner.BitmapManager.ActiveDocument;

            if (doc.Layers[index].IsActive && Mouse.RightButton == MouseButtonState.Pressed)
            {
                return;
            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                doc.ToggleLayer(index);
            }
            else if (Keyboard.IsKeyDown(Key.LeftShift) && Owner.BitmapManager.ActiveDocument.Layers.Any(x => x.IsActive))
            {
                doc.SelectLayersRange(index);
            }
            else
            {
                doc.SetMainActiveLayer(index);
            }
        }

        public void DeleteActiveLayers(object unusedParameter)
        {
            var doc = Owner.BitmapManager.ActiveDocument;
            doc.RemoveActiveLayers();
        }

        public bool CanDeleteActiveLayers(object unusedParam)
        {
            if (Owner.BitmapManager.ActiveDocument == null)
                return false;
            int activeLayerCount = Owner.BitmapManager.ActiveDocument.Layers.Where(layer => layer.IsActive).Count();
            return Owner.BitmapManager.ActiveDocument.Layers.Count > activeLayerCount;
        }

        public void DuplicateLayer(object parameter)
        {
            Owner.BitmapManager.ActiveDocument.DuplicateLayer((int)parameter);
        }

        public bool CanDuplicateLayer(object property)
        {
            return Owner.BitmapManager.ActiveDocument != null;
        }

        public void RenameLayer(object parameter)
        {
            if (Owner.BitmapManager.ActiveDocument == null)
                return;

            int? index = (int?)parameter;

            if (index == null)
            {
                index = Owner.BitmapManager.ActiveDocument.Layers.IndexOf(Owner.BitmapManager.ActiveDocument.ActiveLayer);
            }

            Owner.BitmapManager.ActiveDocument.Layers[(int)index].IsRenaming = true;
        }

        public bool CanRenameLayer(object parameter)
        {
            return Owner.BitmapManager.ActiveDocument != null;
        }

        public void MoveLayerToFront(object parameter)
        {
            int oldIndex = (int)parameter;
            Guid layerToMove = Owner.BitmapManager.ActiveDocument.Layers[oldIndex].GuidValue;
            Guid referenceLayer = Owner.BitmapManager.ActiveDocument.Layers[oldIndex + 1].GuidValue;
            Owner.BitmapManager.ActiveDocument.MoveLayerInStructure(layerToMove, referenceLayer, true);
        }

        public void MoveLayerToBack(object parameter)
        {
            int oldIndex = (int)parameter;
            Guid layerToMove = Owner.BitmapManager.ActiveDocument.Layers[oldIndex].GuidValue;
            Guid referenceLayer = Owner.BitmapManager.ActiveDocument.Layers[oldIndex - 1].GuidValue;
            Owner.BitmapManager.ActiveDocument.MoveLayerInStructure(layerToMove, referenceLayer, false);
        }

        public bool CanMoveToFront(object property)
        {
            if (property == null)
            {
                return false;
            }

            return Owner.DocumentIsNotNull(null) && Owner.BitmapManager.ActiveDocument.Layers.Count - 1 > (int)property;
        }

        public bool CanMoveToBack(object property)
        {
            if (property == null)
            {
                return false;
            }

            return (int)property > 0;
        }

        public void MergeSelected(object parameter)
        {
            Owner.BitmapManager.ActiveDocument.MergeLayers(Owner.BitmapManager.ActiveDocument.Layers.Where(x => x.IsActive).ToArray(), false);
        }

        public void MergeWithAbove(object parameter)
        {
            int index = (int)parameter;
            Layer layer1 = Owner.BitmapManager.ActiveDocument.Layers[index];
            Layer layer2 = Owner.BitmapManager.ActiveDocument.Layers[index + 1];
            Owner.BitmapManager.ActiveDocument.MergeLayers(new Layer[] { layer1, layer2 }, false);
        }

        public void MergeWithBelow(object parameter)
        {
            int index = (int)parameter;
            Layer layer1 = Owner.BitmapManager.ActiveDocument.Layers[index - 1];
            Layer layer2 = Owner.BitmapManager.ActiveDocument.Layers[index];
            Owner.BitmapManager.ActiveDocument.MergeLayers(new Layer[] { layer1, layer2 }, true);
        }

        public bool CanMergeWithAbove(object property)
        {
            if (property == null)
            {
                return false;
            }
            int index = (int)property;
            return Owner.DocumentIsNotNull(null) && index != Owner.BitmapManager.ActiveDocument.Layers.Count - 1
                && Owner.BitmapManager.ActiveDocument.Layers.Count(x => x.IsActive) == 1;
        }

        public bool CanMergeWithBelow(object property)
        {
            if (property == null)
            {
                return false;
            }

            int index = (int)property;
            return Owner.DocumentIsNotNull(null) && index != 0 && Owner.BitmapManager.ActiveDocument.Layers.Count(x => x.IsActive) == 1;
        }

        private GuidStructureItem GetGroupFromParameter(object parameter)
        {
            if (parameter is LayerGroupControl)
            {
                return ((LayerGroupControl)parameter).GroupData;
            }
            else if (parameter is Layer || parameter is LayerStructureItemContainer)
            {
                Guid layerGuid = parameter is Layer layer ? layer.GuidValue : ((LayerStructureItemContainer)parameter).Layer.GuidValue;
                var group = Owner.BitmapManager.ActiveDocument.LayerStructure.GetGroupByLayer(layerGuid);
                if (group != null)
                {
                    while (group.IsExpanded && group.Parent != null)
                    {
                        group = group.Parent;
                    }
                }
                return group;
            }

            return null;
        }

        private void BitmapManager_DocumentChanged(object sender, Models.Events.DocumentChangedEventArgs e)
        {
            if (e.OldDocument != null)
            {
                e.OldDocument.LayersChanged -= Document_LayersChanged;
            }

            if (e.NewDocument != null)
            {
                e.NewDocument.LayersChanged += Document_LayersChanged;
            }
        }

        private void Document_LayersChanged(object sender, LayersChangedEventArgs e)
        {
            if (e.LayerChangeType == Models.Enums.LayerAction.SetActive)
            {
                Owner.BitmapManager.ActiveDocument.UpdateLayersColor();
                Owner.BitmapManager.ActiveDocument.LayerStructure.ExpandParentGroups(e.LayerAffectedGuid);
            }
            else
            {
                Owner.BitmapManager.ActiveDocument.ChangesSaved = false;
            }
        }
    }
}
