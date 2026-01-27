using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Layers;
using PixiEditor.UI.Common.Fonts;
using PixiEditor.ViewModels.Nodes;

namespace PixiEditor.ViewModels.Document.Nodes;

[NodeViewModel("FOLDER_NODE", "STRUCTURE", PixiPerfectIcons.Folder)]
internal class FolderNodeViewModel : StructureMemberViewModel<FolderNode>, IFolderHandler
{
    private bool isOpen;
    public ObservableCollection<IStructureMemberHandler> Children { get; } = new();

    public bool IsOpen
    {
        get => isOpen;
        set => SetProperty(ref isOpen, value);
    }

    public FolderNodeViewModel()
    {
        Children.CollectionChanged += ChildrenOnCollectionChanged;
    }

    private void ChildrenOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if(e.OldItems != null)
        {
            foreach (IStructureMemberHandler oldItem in e.OldItems)
            {
                if (oldItem is INotifyPropertyChanged notifyPropertyChanged)
                {
                    notifyPropertyChanged.PropertyChanged -= ChildOnPropertyChanged;
                }
            }
        }

        if(e.NewItems != null)
        {
            foreach (IStructureMemberHandler newItem in e.NewItems)
            {
                if (newItem is INotifyPropertyChanged notifyPropertyChanged)
                {
                    notifyPropertyChanged.PropertyChanged += ChildOnPropertyChanged;
                }
            }
        }
    }

    private void ChildOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IStructureMemberHandler.Selection))
        {
            if (sender is IStructureMemberHandler { Selection: StructureMemberSelectionType.Hard })
            {
                IsOpen = true;
            }
        }
        else if (e.PropertyName == nameof(IsOpen))
        {
            if (sender is FolderNodeViewModel folder && folder.IsOpen)
            {
                IsOpen = true;
            }
        }
    }

    public int CountChildrenRecursive()
    {
        int count = 0;
        foreach (var child in Children)
        {
            if (child is FolderNodeViewModel folder)
            {
                count += folder.CountChildrenRecursive();
            }
            else
            {
                count++;
            }
        }

        return count;
    }
}
