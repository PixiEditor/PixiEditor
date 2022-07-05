using System.Windows;
using System.Windows.Input;
using PixiEditor.Helpers;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Tools;
using PixiEditor.Models.Tools.Tools;

namespace PixiEditor.ViewModels.SubViewModels.Main;

public class IoViewModel : SubViewModel<ViewModelMain>
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
        var key = args.Key;
        if (key == Key.System)
            key = args.SystemKey;

        ProcessShortcutDown(args.IsRepeat, key);

        if (Owner.BitmapManager.ActiveDocument != null)
        {
            Owner.BitmapManager.InputTarget.OnKeyDown(key);
        }

        HandleTransientKey(args, true);
    }

    private void HandleTransientKey(KeyEventArgs args, bool state)
    {
        if (ShortcutController.ShortcutExecutionBlocked)
        {
            return;
        }

        var controller = Owner.ShortcutController;

        Key finalKey = args.Key;
        if (finalKey == Key.System)
        {
            finalKey = args.SystemKey;
        }

        Command.ToolCommand tool = CommandController.Current.Commands
            .Select(x => x as Command.ToolCommand)
            .FirstOrDefault(x => x != null && x.TransientKey == finalKey);

        if (tool != null)
        {
            ChangeToolState(tool.ToolType, state);
        }
    }

    private void ProcessShortcutDown(bool isRepeat, Key key)
    {
        if (isRepeat && !restoreToolOnKeyUp && Owner.ShortcutController.LastCommands != null &&
            Owner.ShortcutController.LastCommands.Any(x => x is Command.ToolCommand))
        {
            restoreToolOnKeyUp = true;
            ShortcutController.BlockShortcutExection("ShortcutDown");
        }

        Owner.ShortcutController.KeyPressed(key, Keyboard.Modifiers);

        //public void KeyPressed(Key key, ModifierKeys modifiers)
        //{
        //    if (!ShortcutExecutionBlocked)
        //    {
        //        Shortcut[] shortcuts = ShortcutGroups.SelectMany(x => x.Shortcuts).ToList().FindAll(x => x.ShortcutKey == key).ToArray();
        //        if (shortcuts.Length < 1)
        //        {
        //            return;
        //        }

        //        shortcuts = shortcuts.OrderByDescending(x => x.Modifier).ToArray();
        //        for (int i = 0; i < shortcuts.Length; i++)
        //        {
        //            if (modifiers.HasFlag(shortcuts[i].Modifier))
        //            {
        //                shortcuts[i].Execute();
        //                LastShortcut = shortcuts[i];
        //                break;
        //            }
        //        }
        //    }
        //}
    }

    private void OnKeyUp(KeyEventArgs args)
    {
        var key = args.Key;
        if (key == Key.System)
            key = args.SystemKey;

        ProcessShortcutUp(new(key, args.KeyboardDevice.Modifiers));

        if (Owner.BitmapManager.ActiveDocument != null)
            Owner.BitmapManager.InputTarget.OnKeyUp(key);

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

    private void OnMouseDown(object sender, MouseButton button)
    {
        /*
        if (button == MouseButton.Left)
        {
            BitmapManager bitmapManager = Owner.BitmapManager;
            var activeDocument = bitmapManager.ActiveDocument;
            if (activeDocument == null)
                return;

            bitmapManager.InputTarget.OnLeftMouseButtonDown(activeDocument.MouseXOnCanvas, activeDocument.MouseYOnCanvas);
        }
        */
    }

    private void OnPreviewMiddleMouseButton(object sender)
    {
        ChangeToolState<MoveViewportTool>(true);
    }

    private void ChangeToolState<T>(bool setOn)
        where T : Tool
    {
        ChangeToolState(typeof(T), setOn);
    }

    private void ChangeToolState(Type type, bool setOn)
    {
        if (setOn)
        {
            var transientToolIsActive = Owner.ToolsSubViewModel.ActiveTool.GetType() == type;
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

    private void OnMouseMove(object sender, EventArgs args)
    {
        /*
        var activeDocument = Owner.BitmapManager.ActiveDocument;
        if (activeDocument == null)
            return;
        Owner.BitmapManager.InputTarget.OnMouseMove(activeDocument.MouseXOnCanvas, activeDocument.MouseYOnCanvas);*/
    }

    private void OnMouseUp(object sender, MouseButton button)
    {
        if (Owner.BitmapManager.ActiveDocument == null)
            return;
        if (button == MouseButton.Left)
        {
            Owner.BitmapManager.InputTarget.OnLeftMouseButtonUp();
        }
        else if (button == MouseButton.Middle)
        {
            ChangeToolState<MoveViewportTool>(false);
        }
    }
}
