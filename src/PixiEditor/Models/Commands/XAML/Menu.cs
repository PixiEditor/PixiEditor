﻿using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using PixiEditor.Helpers;
using PixiEditor.Models.DataHolders;

namespace PixiEditor.Models.Commands.XAML;

internal class Menu : System.Windows.Controls.Menu
{
    public static readonly DependencyProperty CommandNameProperty =
        DependencyProperty.RegisterAttached(
            "Command",
            typeof(string),
            typeof(Menu),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, CommandChanged)
        );

    public const double IconDimensions = 21;
    
    public static string GetCommand(UIElement target) => (string)target.GetValue(CommandNameProperty);

    public static void SetCommand(UIElement target, string value) => target.SetValue(CommandNameProperty, value);

    public static void CommandChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is not string value || sender is not MenuItem item)
        {
            throw new InvalidOperationException($"{nameof(Menu)}.Command only works for MenuItem's");
        }

        if (DesignerProperties.GetIsInDesignMode(sender as DependencyObject))
        {
            HandleDesignMode(item, value);
            return;
        }

        var command = CommandController.Current.Commands[value];

        var icon = new Image 
        { 
            Source = command.GetIcon(), 
            Width = IconDimensions, Height = IconDimensions,
            Opacity = command.CanExecute() ? 1 : 0.75
        };
        
        icon.IsVisibleChanged += (_, v) =>
        {
            if ((bool)v.NewValue)
            {
                icon.Opacity = command.CanExecute() ? 1 : 0.75;
            }
        };

        item.Command = Command.GetICommand(command, false);
        item.Icon = icon;
        item.SetBinding(MenuItem.InputGestureTextProperty, ShortcutBinding.GetBinding(command, null));
    }

    private static void HandleDesignMode(MenuItem item, string name)
    {
        var command = DesignCommandHelpers.GetCommandAttribute(name);
        item.InputGestureText = new KeyCombination(command.Key, command.Modifiers).ToString();
    }
}
