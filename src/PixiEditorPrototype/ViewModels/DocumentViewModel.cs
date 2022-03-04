using ChangeableDocument;
using ChangeableDocument.Actions;
using PixiEditorPrototype.Models;
using System.ComponentModel;
using System.Windows;

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


        public DocumentViewModel()
        {
            Tracker = new DocumentChangeTracker();
            Updater = new DocumentUpdater(this);
            StructureRoot = new FolderViewModel(this, Tracker.Document.ReadOnlyStructureRoot);
            ActionAccumulator = new ActionAccumulator(Tracker, Updater);
            StructureHelper = new DocumentStructureHelper(this);

            UndoCommand = new RelayCommand(Undo, _ => true);
            RedoCommand = new RelayCommand(Redo, _ => true);
            CreateNewLayerCommand = new RelayCommand(_ => StructureHelper.CreateNewStructureMember(StructureMemberType.Layer), _ => true);
            CreateNewFolderCommand = new RelayCommand(_ => StructureHelper.CreateNewStructureMember(StructureMemberType.Folder), _ => true);
            DeleteStructureMemberCommand = new RelayCommand(DeleteStructureMember, _ => true);
            ChangeSelectedItemCommand = new RelayCommand(ChangeSelectedItem, _ => true);
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
