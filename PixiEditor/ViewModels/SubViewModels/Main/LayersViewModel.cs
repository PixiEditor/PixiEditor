using PixiEditor.Helpers;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Layers;
using PixiEditor.Views.UserControls;
using System;
using System.Linq;
using System.Windows.Input;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
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
            DeleteLayersCommand = new RelayCommand(DeleteLayer, CanDeleteLayer);
            DuplicateLayerCommand = new RelayCommand(DuplicateLayer, CanDuplicateLayer);
            MoveToBackCommand = new RelayCommand(MoveLayerToBack, CanMoveToBack);
            MoveToFrontCommand = new RelayCommand(MoveLayerToFront, CanMoveToFront);
            RenameLayerCommand = new RelayCommand(RenameLayer);
            MergeSelectedCommand = new RelayCommand(MergeSelected, CanMergeSelected);
            MergeWithAboveCommand = new RelayCommand(MergeWithAbove, CanMergeWithAbove);
            MergeWithBelowCommand = new RelayCommand(MergeWithBelow, CanMergeWithBelow);
            RenameGroupCommand = new RelayCommand(RenameGroup);
            DeleteGroupCommand = new RelayCommand(DeleteGroup);
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
            return (parameter is not null and (Layer or LayerGroup)) || (Owner.BitmapManager?.ActiveDocument?.ActiveLayer != null);
        }

        public void DeleteSelected(object parameter)
        {
            if (parameter is Layer layer)
            {
                DeleteLayer(Owner.BitmapManager.ActiveDocument.Layers.IndexOf(layer));
            }
            else if (parameter is LayerStructureItemContainer container)
            {
                DeleteLayer(Owner.BitmapManager.ActiveDocument.Layers.IndexOf(container.Layer));
            }
            else if (parameter is LayerGroup group)
            {
                DeleteGroup(group.GroupGuid);
            }
            else if (parameter is LayerGroupControl groupControl)
            {
                DeleteGroup(groupControl.GroupGuid);
            }
            else if (Owner.BitmapManager.ActiveDocument.ActiveLayer != null)
            {
                DeleteLayer(Owner.BitmapManager.ActiveDocument.Layers.IndexOf(Owner.BitmapManager.ActiveDocument.ActiveLayer));
            }
        }

        public void DeleteGroup(object parameter)
        {
            if (parameter is Guid guid)
            {
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
                    GuidStructureItem group = doc.LayerStructure.AddNewGroup($"{doc.ActiveLayer.Name} Group", doc.ActiveLayer.LayerGuid);

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

        public void NewLayer(object parameter)
        {
            GuidStructureItem control = GetGroupFromParameter(parameter);
            var doc = Owner.BitmapManager.ActiveDocument;
            var activeLayerParent = doc.LayerStructure.GetGroupByLayer(doc.ActiveLayerGuid);

            Guid lastActiveLayerGuid = doc.ActiveLayerGuid;

            doc.AddNewLayer($"New Layer {Owner.BitmapManager.ActiveDocument.Layers.Count}");

            if (doc.Layers.Count > 1)
            {
                doc.MoveLayerInStructure(doc.Layers[^1].LayerGuid, lastActiveLayerGuid, true);
                Guid? parent = parameter is Layer or LayerStructureItemContainer ? activeLayerParent?.GroupGuid : activeLayerParent.Parent?.GroupGuid;
                doc.LayerStructure.AssignParent(doc.ActiveLayerGuid, parent);
                doc.UndoManager.UndoStack.Pop();
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

        public void DeleteLayer(object parameter)
        {
            var doc = Owner.BitmapManager.ActiveDocument;
            doc.RemoveActiveLayers();
        }

        public bool CanDeleteLayer(object property)
        {
            return Owner.BitmapManager.ActiveDocument != null && Owner.BitmapManager.ActiveDocument.Layers.Count > 1;
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
            Guid layerToMove = Owner.BitmapManager.ActiveDocument.Layers[oldIndex].LayerGuid;
            Guid referenceLayer = Owner.BitmapManager.ActiveDocument.Layers[oldIndex + 1].LayerGuid;
            Owner.BitmapManager.ActiveDocument.MoveLayerInStructure(layerToMove, referenceLayer, true);
        }

        public void MoveLayerToBack(object parameter)
        {
            int oldIndex = (int)parameter;
            Guid layerToMove = Owner.BitmapManager.ActiveDocument.Layers[oldIndex].LayerGuid;
            Guid referenceLayer = Owner.BitmapManager.ActiveDocument.Layers[oldIndex - 1].LayerGuid;
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
                Guid layerGuid = parameter is Layer layer ? layer.LayerGuid : ((LayerStructureItemContainer)parameter).Layer.LayerGuid;
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
