﻿using System.Windows;
using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.Helpers;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Events;
using PixiEditor.ViewModels.SubViewModels.Document;
using PixiEditor.ViewModels.SubViewModels.Tools;
using PixiEditor.ViewModels.SubViewModels.Tools.Tools;
using PixiEditor.Views;

namespace PixiEditor.ViewModels.SubViewModels.Main;

internal class IoViewModel : SubViewModel<ViewModelMain>
{
    public RelayCommand MouseMoveCommand { get; set; }
    public RelayCommand MouseDownCommand { get; set; }
    public RelayCommand PreviewMouseMiddleButtonCommand { get; set; }
    public RelayCommand MouseUpCommand { get; set; }

    private bool restoreToolOnKeyUp = false;

    private MouseInputFilter filter = new();

    public IoViewModel(ViewModelMain owner)
        : base(owner)
    {
        MouseDownCommand = new RelayCommand(filter.MouseDown);
        MouseMoveCommand = new RelayCommand(filter.MouseMove);
        MouseUpCommand = new RelayCommand(filter.MouseUp);
        PreviewMouseMiddleButtonCommand = new RelayCommand(OnPreviewMiddleMouseButton);
        GlobalMouseHook.OnMouseUp += filter.MouseUp;

        InputManager.Current.PreProcessInput += Current_PreProcessInput;

        filter.OnMouseDown += OnMouseDown;
        filter.OnMouseMove += OnMouseMove;
        filter.OnMouseUp += OnMouseUp;
    }

    private void Current_PreProcessInput(object sender, PreProcessInputEventArgs e)
    {
        if (e != null && e.StagingItem != null && e.StagingItem.Input != null)
        {
            InputEventArgs inputEvent = e.StagingItem.Input;

            if (inputEvent is KeyboardEventArgs)
            {
                KeyboardEventArgs k = inputEvent as KeyboardEventArgs;
                RoutedEvent r = k.RoutedEvent;
                KeyEventArgs keyEvent = k as KeyEventArgs;

                if (keyEvent != null && keyEvent?.InputSource?.RootVisual != MainWindow.Current) return;
                if (r == Keyboard.KeyDownEvent)
                {
                    OnKeyDown(keyEvent);
                }

                if (r == Keyboard.KeyUpEvent)
                {
                    OnKeyUp(keyEvent);
                }
            }
        }
    }

    private void OnKeyDown(KeyEventArgs args)
    {
        Key key = args.Key;
        if (key == Key.System)
            key = args.SystemKey;

        ProcessShortcutDown(args.IsRepeat, key);

        if (Owner.DocumentManagerSubViewModel.ActiveDocument != null)
            Owner.DocumentManagerSubViewModel.ActiveDocument.OnKeyDown(key);
        Owner.ToolsSubViewModel.OnKeyDown(key);

        HandleTransientKey(args, true);
    }

    private void HandleTransientKey(KeyEventArgs args, bool state)
    {
        if (ShortcutController.ShortcutExecutionBlocked)
        {
            return;
        }

        ShortcutController controller = Owner.ShortcutController;

        Key finalKey = args.Key;
        if (finalKey == Key.System)
        {
            finalKey = args.SystemKey;
        }

        Models.Commands.Commands.Command.ToolCommand tool = CommandController.Current.Commands
            .Select(x => x as Models.Commands.Commands.Command.ToolCommand)
            .FirstOrDefault(x => x != null && x.TransientKey == finalKey);

        if (tool != null)
        {
            ChangeToolState(tool.ToolType, state);
        }
    }

    private void ProcessShortcutDown(bool isRepeat, Key key)
    {
        if (isRepeat && !restoreToolOnKeyUp && Owner.ShortcutController.LastCommands != null &&
            Owner.ShortcutController.LastCommands.Any(x => x is Models.Commands.Commands.Command.ToolCommand))
        {
            restoreToolOnKeyUp = true;
            ShortcutController.BlockShortcutExecution("ShortcutDown");
        }

        Owner.ShortcutController.KeyPressed(key, Keyboard.Modifiers);
    }

    private void OnKeyUp(KeyEventArgs args)
    {
        Key key = args.Key;
        if (key == Key.System)
            key = args.SystemKey;

        ProcessShortcutUp(new(key, args.KeyboardDevice.Modifiers));

        if (Owner.DocumentManagerSubViewModel.ActiveDocument is not null)
            Owner.DocumentManagerSubViewModel.ActiveDocument.OnKeyUp(key);
        Owner.ToolsSubViewModel.OnKeyUp(key);

        HandleTransientKey(args, false);
    }

    private void ProcessShortcutUp(KeyCombination shortcut)
    {
        if (restoreToolOnKeyUp && Owner.ShortcutController.LastCommands != null &&
            Owner.ShortcutController.LastCommands.Any(x => x.Shortcut == shortcut))
        {
            restoreToolOnKeyUp = false;
            Owner.ToolsSubViewModel.SetActiveTool(Owner.ToolsSubViewModel.LastActionTool);
            ShortcutController.UnblockShortcutExecution("ShortcutDown");
        }
    }

    private void OnMouseDown(object sender, MouseOnCanvasEventArgs args)
    {
        if (args.Button == MouseButton.Left)
        {
            DocumentManagerViewModel docManager = Owner.DocumentManagerSubViewModel;
            DocumentViewModel activeDocument = docManager.ActiveDocument;
            if (activeDocument == null)
                return;

            //docManager.InputTarget.OnLeftMouseButtonDown(activeDocument.MouseXOnCanvas, activeDocument.MouseYOnCanvas);
            docManager.ActiveDocument.OnCanvasLeftMouseButtonDown(args.PositionOnCanvas);
            Owner.ToolsSubViewModel.OnLeftMouseButtonDown(args.PositionOnCanvas);
        }
    }

    private void OnPreviewMiddleMouseButton(object sender)
    {
        ChangeToolState<MoveViewportToolViewModel>(true);
    }

    private void ChangeToolState<T>(bool setOn)
        where T : ToolViewModel
    {
        ChangeToolState(typeof(T), setOn);
    }

    private void ChangeToolState(Type type, bool setOn)
    {
        if (setOn)
        {
            bool transientToolIsActive = Owner.ToolsSubViewModel.ActiveTool.GetType() == type;
            if (!transientToolIsActive)
            {
                Owner.ToolsSubViewModel.SetActiveTool(type);
                Owner.ToolsSubViewModel.ActiveToolIsTransient = true;
            }
        }
        else if (Owner.ToolsSubViewModel.LastActionTool != null && Owner.ToolsSubViewModel.ActiveToolIsTransient)
        {
            Owner.ToolsSubViewModel.SetActiveTool(Owner.ToolsSubViewModel.LastActionTool);
            restoreToolOnKeyUp = false;
            ShortcutController.UnblockShortcutExecution("ShortcutDown");
        }
    }

    private void OnMouseMove(object sender, VecD pos)
    {
        DocumentViewModel activeDocument = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (activeDocument is null)
            return;
        //Owner.DocumentManagerSubViewModel.InputTarget.OnMouseMove(activeDocument.MouseXOnCanvas, activeDocument.MouseYOnCanvas);
        activeDocument.OnCanvasMouseMove(pos);
    }

    private void OnMouseUp(object sender, MouseButton button)
    {
        if (Owner.DocumentManagerSubViewModel.ActiveDocument is null)
            return;
        if (button == MouseButton.Left)
        {
            //Owner.BitmapManager.InputTarget.OnLeftMouseButtonUp();
            Owner.DocumentManagerSubViewModel.ActiveDocument.OnCanvasLeftMouseButtonUp();
        }
        else if (button == MouseButton.Middle)
        {
            ChangeToolState<MoveViewportToolViewModel>(false);
        }
    }
}