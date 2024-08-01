using System.Collections.Generic;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Media;
using PixiEditor.Models.Commands;
using PixiEditor.Extensions.UI;
using Dock_LayoutManager = PixiEditor.ViewModels.Dock.LayoutManager;
using LayoutManager = PixiEditor.ViewModels.Dock.LayoutManager;

namespace PixiEditor.ViewModels.Menu.MenuBuilders;

internal class OpenDockablesMenuBuilder : MenuItemBuilder
{
    public Dock_LayoutManager LayoutManager { get; set; }

    public OpenDockablesMenuBuilder(Dock_LayoutManager layoutManager)
    {
        LayoutManager = layoutManager;
    }

    public override void ModifyMenuTree(ICollection<MenuItem> tree)
    {
        if (TryFindMenuItem(tree, "VIEW", out MenuItem? viewItem))
        {
            MenuItem dockablesItem = new MenuItem();
            Translator.SetKey(dockablesItem, "OPEN_DOCKABLE_MENU");

            viewItem!.Items.Add(dockablesItem);

            foreach (var dockable in LayoutManager.RegisteredDockables)
            {
                MenuItem dockableItem = new MenuItem();
                Translator.SetKey(dockableItem, dockable.Title);

                string commandId = "PixiEditor.Window.ShowDockWindow";

                dockableItem.Command =
                    (ICommand)new Models.Commands.XAML.Command(commandId) { UseProvided = true }
                        .ProvideValue(null);
                dockableItem.CommandParameter = dockable.Id;

                if (dockable.TabCustomizationSettings.Icon is IImage image)
                {
                    dockableItem.Icon = new Image()
                    {
                        Source = image,
                        Width = Models.Commands.XAML.Menu.IconDimensions,
                        Height = Models.Commands.XAML.Menu.IconDimensions,
                    };
                }
                else if(dockable.TabCustomizationSettings.Icon is TextBlock tb)
                {
                    dockableItem.Icon = new TextBlock()
                    {
                        Text = tb.Text,
                        FontSize = Models.Commands.XAML.Menu.IconFontSize,
                        FontFamily = tb.FontFamily,
                    };
                }
                
                dockablesItem.Items.Add(dockableItem);
            }
        }
    }
}
