using System.Collections.Generic;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using PixiEditor.Models.Commands;
using PixiEditor.Extensions.UI;
using PixiEditor.Helpers.Extensions;
using PixiEditor.UI.Common.Localization;
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
                else if (dockable.TabCustomizationSettings.Icon is TextBlock tb)
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

    public override void ModifyMenuTree(ICollection<NativeMenuItem> tree)
    {
        if (TryFindMenuItem(tree, "VIEW", out NativeMenuItem? viewItem))
        {
            NativeMenuItem dockablesItem = new NativeMenuItem();
            dockablesItem.Menu = new NativeMenu();

            Translator.SetKey(dockablesItem, "OPEN_DOCKABLE_MENU");
            PixiEditor.Models.Commands.XAML.NativeMenu.SetLocalizationKeyHeader(dockablesItem, "OPEN_DOCKABLE_MENU");

            viewItem!.Menu.Items.Add(dockablesItem);

            foreach (var dockable in LayoutManager.RegisteredDockables)
            {
                NativeMenuItem dockableItem = new NativeMenuItem();
                Translator.SetKey(dockableItem, dockable.Title);

                string commandId = "PixiEditor.Window.ShowDockWindow";

                dockableItem.Command =
                    (ICommand)new Models.Commands.XAML.Command(commandId) { UseProvided = true }
                        .ProvideValue(null);
                dockableItem.CommandParameter = dockable.Id;

                if (dockable.TabCustomizationSettings.Icon is IImage image)
                {
                    int dimensions = (int)Models.Commands.XAML.Menu.IconDimensions;
                    dockableItem.Icon = image.ToBitmap(new PixelSize(dimensions, dimensions));
                }

                dockablesItem.Menu.Items.Add(dockableItem);
            }
        }
    }
}
