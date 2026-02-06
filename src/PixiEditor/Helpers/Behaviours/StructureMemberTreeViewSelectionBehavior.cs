using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Layers;
using PixiEditor.ViewModels.Document;
using PixiEditor.ViewModels.Document.Nodes;
using PixiEditor.Views.Layers;

namespace PixiEditor.Helpers.Behaviours;

internal class StructureMemberTreeViewSelectionBehavior : Behavior
{
    public static readonly StyledProperty<StructureTree> StructureTreeProperty = AvaloniaProperty.Register<StructureMemberTreeViewSelectionBehavior, StructureTree>(
        nameof(StructureTree));

    public StructureTree StructureTree
    {
        get => GetValue(StructureTreeProperty);
        set => SetValue(StructureTreeProperty, value);
    }

    static StructureMemberTreeViewSelectionBehavior()
    {
        StructureTreeProperty.Changed.AddClassHandler<StructureMemberTreeViewSelectionBehavior>((behavior, args) =>
        {
            if (args.OldValue is StructureTree oldHandler)
            {
                UnsubscribeMembers(oldHandler.Members, behavior);
                oldHandler.Members.CollectionChanged -= behavior.StructureTreeOnPropertyChanged;
            }

            if (args.NewValue is StructureTree newHandler)
            {
                newHandler.Members.CollectionChanged += behavior.StructureTreeOnPropertyChanged;
                SubscribeMembers(newHandler.Members, behavior);
            }
        });
    }

    private static void UnsubscribeMembers(IEnumerable<IStructureMemberHandler> items, StructureMemberTreeViewSelectionBehavior behavior)
    {
        foreach (var member in items)
        {
            member.PropertyChanged -= behavior.StructureMemberOnPropertyChanged;
            if(member is IFolderHandler folder)
            {
                UnsubscribeMembers(folder.Children, behavior);
            }
        }
    }

    private static void SubscribeMembers(IEnumerable<IStructureMemberHandler> items, StructureMemberTreeViewSelectionBehavior behavior)
    {
        foreach (var member in items)
        {
            member.PropertyChanged += behavior.StructureMemberOnPropertyChanged;
            if(member is IFolderHandler folder)
            {
                SubscribeMembers(folder.Children, behavior);
            }
        }
    }

    private void StructureTreeOnPropertyChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            SubscribeMembers(e.NewItems.Cast<IStructureMemberHandler>(), this);
        }

        if (e.OldItems != null)
        {
            UnsubscribeMembers(e.OldItems.Cast<IStructureMemberHandler>(), this);
        }
    }


    private void StructureMemberOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var structureMember = sender as IStructureMemberHandler;
        if (structureMember == null)
            return;

        if(AssociatedObject is not TreeView tree)
            return;

        if (e.PropertyName is nameof(IStructureMemberHandler.Selection))
        {
            if (structureMember.Selection == StructureMemberSelectionType.Hard)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    var scroller =
                       tree.GetVisualDescendants().FirstOrDefault(x => x is ScrollViewer) as ScrollViewer;
                    if (scroller == null)
                        return;

                    double yOffset = scroller?.Offset.Y ?? 0;

                    var visual = FindContainer(sender, tree);

                    if (visual != null)
                    {
                        if (structureMember is IFolderHandler folderHandler)
                        {
                            visual = visual.FindDescendantOfType<FolderControl>();
                        }
                        else if (structureMember is ILayerHandler)
                        {
                            visual = visual.FindDescendantOfType<LayerControl>();
                        }
                    }

                    if (visual == null)
                        return;

                    var transform = visual.TransformToVisual(scroller);
                    if (transform == null)
                        return;

                    var targetBounds = visual.Bounds;

                    var targetRectInScroller = transform.Value.Transform(targetBounds.TopLeft);
                    scroller.Offset = new Vector(0, targetRectInScroller.Y + yOffset);
                }, DispatcherPriority.Render);
            }
        }
    }

    private Visual? FindContainer(object? item, ItemsControl container)
    {
        if (item == null)
            throw new ArgumentNullException(nameof(item));

        var itemContainer = container.ItemsPanelRoot?.Children
            .FirstOrDefault(c => c.DataContext == item);
        if (itemContainer != null)
            return itemContainer;

        if(container?.ItemsPanelRoot == null)
            return null;

        foreach (var child in container.ItemsPanelRoot.Children.OfType<ItemsControl>())
        {
            var result = FindContainer(item, child);
            if (result != null)
                return result;
        }

        return null;
    }
}
