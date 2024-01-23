﻿using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using PixiEditor.AvaloniaUI.Models.Commands;
using PixiEditor.AvaloniaUI.Models.Commands.Commands;
using PixiEditor.AvaloniaUI.Models.Input;
using PixiEditor.AvaloniaUI.ViewModels;

namespace PixiEditor.AvaloniaUI.Views.Dialogs;

internal partial class ShortcutsPopup : PixiEditorPopup
{
    public static readonly StyledProperty<CommandController> ControllerProperty = AvaloniaProperty.Register<ShortcutsPopup, CommandController>(
        nameof(Controller));

    public static readonly StyledProperty<bool> IsTopmostProperty = AvaloniaProperty.Register<ShortcutsPopup, bool>(
        "IsTopmost");

    public bool IsTopmost
    {
        get => GetValue(IsTopmostProperty);
        set => SetValue(IsTopmostProperty, value);
    }

    public CommandController Controller
    {
        get => GetValue(ControllerProperty);
        set => SetValue(ControllerProperty, value);
    }

    Command settingsCommand;

    public ShortcutsPopup(CommandController controller)
    {
        DataContext = this;
        InitializeComponent();
        Controller = controller;
        settingsCommand = Controller.Commands["PixiEditor.Window.OpenSettingsWindow"];
    }

    private void ShortcutPopup_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (settingsCommand.Shortcut != new KeyCombination(e.Key, e.KeyModifiers))
        {
            return;
        }

        settingsCommand.Methods.Execute("Keybinds");
    }
}
