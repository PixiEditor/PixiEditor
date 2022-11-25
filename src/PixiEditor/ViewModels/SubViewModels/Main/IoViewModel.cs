using System.Windows;
using System.Windows.Input;
using ChunkyImageLib.DataHolders;
using PixiEditor.DrawingApi.Core.Numerics;
using PixiEditor.Helpers;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Commands.Commands;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Events;
using PixiEditor.ViewModels.SubViewModels.Document;
using PixiEditor.ViewModels.SubViewModels.Tools;
using PixiEditor.ViewModels.SubViewModels.Tools.Tools;
using PixiEditor.Views;

namespace PixiEditor.ViewModels.SubViewModels.Main;
#nullable enable
internal class IoViewModel : SubViewModel<ViewModelMain>
{
    public RelayCommand MouseMoveCommand { get; set; }
    public RelayCommand MouseDownCommand { get; set; }
    public RelayCommand PreviewMouseMiddleButtonCommand { get; set; }
    public RelayCommand MouseUpCommand { get; set; }

    private MouseInputFilter mouseFilter = new();
    private KeyboardInputFilter keyboardFilter = new();

    public IoViewModel(ViewModelMain owner)
        : base(owner)
    {
        MouseDownCommand = new RelayCommand(mouseFilter.MouseDownInlet);
        MouseMoveCommand = new RelayCommand(mouseFilter.MouseMoveInlet);
        MouseUpCommand = new RelayCommand(mouseFilter.MouseUpInlet);
        PreviewMouseMiddleButtonCommand = new RelayCommand(OnPreviewMiddleMouseButton);
        GlobalMouseHook.OnMouseUp += mouseFilter.MouseUpInlet;

        InputManager.Current.PreProcessInput += Current_PreProcessInput;

        mouseFilter.OnMouseDown += OnMouseDown;
        mouseFilter.OnMouseMove += OnMouseMove;
        mouseFilter.OnMouseUp += OnMouseUp;

        keyboardFilter.OnAnyKeyDown += OnKeyDown;
        keyboardFilter.OnAnyKeyUp += OnKeyUp;

        keyboardFilter.OnConvertedKeyDown += OnConvertedKeyDown;
        keyboardFilter.OnConvertedKeyUp += OnConvertedKeyDown;

        Application.Current.Deactivated += keyboardFilter.DeactivatedInlet;
        Application.Current.Deactivated += mouseFilter.DeactivatedInlet;
    }

    private void OnConvertedKeyDown(object? sender, FilteredKeyEventArgs args)
    {
        Owner.DocumentManagerSubViewModel.ActiveDocument?.EventInlet.OnConvertedKeyDown(args);
        Owner.ToolsSubViewModel.ConvertedKeyDownInlet(args);
    }

    private void OnConvertedKeyUp(object? sender, FilteredKeyEventArgs args)
    {
        Owner.DocumentManagerSubViewModel.ActiveDocument?.EventInlet.OnConvertedKeyUp(args);
        Owner.ToolsSubViewModel.ConvertedKeyUpInlet(args);
    }

    private void Current_PreProcessInput(object sender, PreProcessInputEventArgs e)
    {
        if (e is { StagingItem: { Input: { } } })
        {
            InputEventArgs inputEvent = e.StagingItem.Input;

            if (inputEvent is KeyboardEventArgs)
            {
                KeyboardEventArgs k = (KeyboardEventArgs)inputEvent;
                RoutedEvent r = k.RoutedEvent;
                KeyEventArgs? keyEvent = k as KeyEventArgs;

                if (keyEvent is null && keyEvent?.InputSource?.RootVisual != MainWindow.Current)
                    return;
                if (r == Keyboard.KeyDownEvent)
                {
                    keyboardFilter.KeyDownInlet(keyEvent);
                }

                if (r == Keyboard.KeyUpEvent)
                {
                    keyboardFilter.KeyUpInlet(keyEvent);
                }
            }
        }
    }

    private void OnKeyDown(object? sender, FilteredKeyEventArgs args)
    {
        ProcessShortcutDown(args.IsRepeat, args.Key);
        Owner.DocumentManagerSubViewModel.ActiveDocument?.EventInlet.OnKeyDown(args.Key);
    }

    private void HandleTransientKey(Key transientKey)
    {
        if (ShortcutController.ShortcutExecutionBlocked)
        {
            return;
        }

        var tool = GetTransientTool(transientKey);

        if (tool is not null)
        {
            Owner.ToolsSubViewModel.SetActiveTool(tool.ToolType, true);
        }
    }

    private static Command.ToolCommand? GetTransientTool(Key transientKey)
    {
        Command.ToolCommand? tool = CommandController.Current.Commands
            .OfType<Command.ToolCommand?>()
            .FirstOrDefault(x => x != null && x.TransientKey == transientKey);
        return tool;
    }

    private void ProcessShortcutDown(bool isRepeat, Key key)
    {
        HandleTransientKey(key);
        if (isRepeat && Owner.ShortcutController.LastCommands != null &&
            Owner.ShortcutController.LastCommands.Any(x => x is Command.ToolCommand))
        {
            Owner.ToolsSubViewModel.HandleToolRepeatShortcutDown();
        }

        Owner.ShortcutController.KeyPressed(key, Keyboard.Modifiers);
    }

    private void OnKeyUp(object? sender, FilteredKeyEventArgs args)
    {
        ProcessShortcutUp(new(args.Key, args.Modifiers));

        Owner.DocumentManagerSubViewModel.ActiveDocument?.EventInlet.OnKeyUp(args.Key);
    }

    private void ProcessShortcutUp(KeyCombination shortcut)
    {
        var transientTool = GetTransientTool(shortcut.Key);

        if (Owner.ShortcutController.LastCommands != null &&
            Owner.ShortcutController.LastCommands.Any(x => x.Shortcut == shortcut) || transientTool is not null)
        {
            Owner.ToolsSubViewModel.HandleToolShortcutUp();
        }
    }

    private void OnMouseDown(object? sender, MouseOnCanvasEventArgs args)
    {
        if (args.Button == MouseButton.Left)
        {
            DocumentManagerViewModel docManager = Owner.DocumentManagerSubViewModel;
            DocumentViewModel? activeDocument = docManager.ActiveDocument;
            if (activeDocument == null)
                return;

            activeDocument.EventInlet.OnCanvasLeftMouseButtonDown(args.PositionOnCanvas);
            Owner.ToolsSubViewModel.LeftMouseButtonDownInlet(args.PositionOnCanvas);
        }
    }

    private void OnPreviewMiddleMouseButton(object sender)
    {
        Owner.ToolsSubViewModel.SetActiveTool<MoveViewportToolViewModel>(true);
    }

    private void OnMouseMove(object? sender, VecD pos)
    {
        DocumentViewModel? activeDocument = Owner.DocumentManagerSubViewModel.ActiveDocument;
        if (activeDocument is null)
            return;
        activeDocument.EventInlet.OnCanvasMouseMove(pos);
    }

    private void OnMouseUp(object? sender, MouseButton button)
    {
        if (Owner.DocumentManagerSubViewModel.ActiveDocument is null)
            return;
        if (button == MouseButton.Left)
        {
            Owner.DocumentManagerSubViewModel.ActiveDocument.EventInlet.OnCanvasLeftMouseButtonUp();
        }
        else if (button == MouseButton.Middle)
        {
            Owner.ToolsSubViewModel.RestorePreviousTool();
        }
    }
}
