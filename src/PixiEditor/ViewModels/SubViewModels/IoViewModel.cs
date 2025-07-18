using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using PixiDocks.Avalonia.Controls;
using PixiEditor.Models.Preferences;
using Drawie.Backend.Core.Numerics;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings.PixiEditor;
using PixiEditor.Models.AnalyticsAPI;
using PixiEditor.Models.Commands;
using PixiEditor.Models.Commands.Commands;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Controllers.InputDevice;
using PixiEditor.Models.Events;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Input;
using Drawie.Numerics;
using PixiEditor.Models.DocumentModels.UpdateableChangeExecutors.Features;
using PixiEditor.ViewModels.Document;
using PixiEditor.ViewModels.Tools.Tools;
using PixiEditor.Views;

namespace PixiEditor.ViewModels.SubViewModels;
#nullable enable
internal class IoViewModel : SubViewModel<ViewModelMain>
{
    private double? previousEraseSize;
    private bool hadSharedToolbar;
    private bool? drawingWithRight;
    private bool startedWithEraser;

    private Key? queuedTransientKey;

    public RelayCommand<MouseOnCanvasEventArgs> MouseMoveCommand { get; set; }
    public RelayCommand<MouseOnCanvasEventArgs> MouseDownCommand { get; set; }
    public RelayCommand PreviewMouseMiddleButtonCommand { get; set; }
    public RelayCommand<MouseOnCanvasEventArgs> MouseUpCommand { get; set; }
    public RelayCommand<ScrollOnCanvasEventArgs> MouseWheelCommand { get; set; }

    private MouseInputFilter mouseFilter = new();
    private KeyboardInputFilter keyboardFilter = new();

    public IoViewModel(ViewModelMain owner)
        : base(owner)
    {
        MouseDownCommand = new RelayCommand<MouseOnCanvasEventArgs>(mouseFilter.MouseDownInlet);
        MouseMoveCommand = new RelayCommand<MouseOnCanvasEventArgs>(mouseFilter.MouseMoveInlet);
        MouseUpCommand = new RelayCommand<MouseOnCanvasEventArgs>(mouseFilter.MouseUpInlet);
        MouseWheelCommand = new RelayCommand<ScrollOnCanvasEventArgs>(mouseFilter.MouseWheelInlet);
        PreviewMouseMiddleButtonCommand = new RelayCommand(OnMiddleMouseButton);
        Owner.LayoutSubViewModel.LayoutManager.WindowFloated += OnLayoutManagerOnWindowFloated;
        // TODO: Implement mouse capturing
        //GlobalMouseHook.Instance.OnMouseUp += mouseFilter.MouseUpInlet;

        mouseFilter.OnMouseDown += OnMouseDown;
        mouseFilter.OnMouseMove += OnMouseMove;
        mouseFilter.OnMouseUp += OnMouseUp;
        mouseFilter.OnMouseWheel += HandleMouseWheel;

        keyboardFilter.OnAnyKeyDown += OnKeyDown;
        keyboardFilter.OnAnyKeyUp += OnKeyUp;

        keyboardFilter.OnConvertedKeyDown += OnConvertedKeyDown;
        keyboardFilter.OnConvertedKeyUp += OnConvertedKeyDown;
        
        Owner.AttachedToWindow += AttachWindowEvents;
    }

    public void AttachWindowEvents(MainWindow mainWindow)
    {
        mainWindow.KeyDown += MainWindowKeyDown;
        mainWindow.KeyUp += MainWindowKeyUp;

        mainWindow.Deactivated += keyboardFilter.DeactivatedInlet;
        mainWindow.Deactivated += mouseFilter.DeactivatedInlet;
    }

    private void OnLayoutManagerOnWindowFloated(HostWindow window)
    {
        window.KeyDown += MainWindowKeyDown;
        window.KeyUp += MainWindowKeyUp;

        window.Deactivated += keyboardFilter.DeactivatedInlet;
        window.Deactivated += mouseFilter.DeactivatedInlet;

        window.Closing += HostWindowOnClosing;
    }

    private void HostWindowOnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (sender is not HostWindow hostWindow)
        {
            return;
        }

        hostWindow.Closing -= HostWindowOnClosing;
        hostWindow.Deactivated -= keyboardFilter.DeactivatedInlet;
        hostWindow.Deactivated -= mouseFilter.DeactivatedInlet;
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

    private void MainWindowKeyDown(object? sender, KeyEventArgs e)
    {
        keyboardFilter.KeyDownInlet(e);
    }

    private void MainWindowKeyUp(object? sender, KeyEventArgs e)
    {
        keyboardFilter.KeyUpInlet(e);
    }

    private void OnKeyDown(object? sender, FilteredKeyEventArgs args)
    {
        if (args.Key == Key.None)
            return;

        ProcessShortcutDown(args.IsRepeat, args.Key, args.Modifiers);
        Owner.DocumentManagerSubViewModel.ActiveDocument?.EventInlet.OnKeyDown(args.Key);
    }

    private bool HandleTransientKey(Key transientKey, bool executeOnlyImmediate)
    {
        if (ShortcutController.ShortcutExecutionBlocked)
        {
            return false;
        }

        var tool = GetTransientTool(transientKey);

        if (tool is null)
        {
            return false;
        }

        if (!tool.TransientImmediate && executeOnlyImmediate)
        {
            return false;
        }

        Owner.ToolsSubViewModel.SetActiveTool(tool.ToolType, true);

        return true;
    }

    private static Command.ToolCommand? GetTransientTool(Key transientKey)
    {
        Command.ToolCommand? tool = CommandController.Current.Commands
            .OfType<Command.ToolCommand?>()
            .FirstOrDefault(x => x != null && x.TransientKey == transientKey);
        return tool;
    }

    private void ProcessShortcutDown(bool isRepeat, Key key, KeyModifiers argsModifiers)
    {
        if (argsModifiers == KeyModifiers.None && !isRepeat)
        {
            if (!HandleTransientKey(key, true))
            {
                queuedTransientKey = key;
            }
        }
        else
        {
            queuedTransientKey = null;
        }

        if (isRepeat && Owner.ShortcutController.LastCommands != null &&
            Owner.ShortcutController.LastCommands.Any(x =>
                x is Command.ToolCommand cmd && cmd.Shortcut == new KeyCombination(key, argsModifiers)))
        {
            Owner.ToolsSubViewModel.HandleToolRepeatShortcutDown();
        }

        Owner.ShortcutController.KeyPressed(isRepeat, key, argsModifiers);
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

        if (shortcut.Key == queuedTransientKey)
        {
            queuedTransientKey = null;
        }

        ShortcutController.UnblockShortcutExecution("ShortcutDown");
    }

    private void OnMouseDown(object? sender, MouseOnCanvasEventArgs args)
    {
        if (args.Button == MouseButton.Left && queuedTransientKey != null)
        {
            HandleTransientKey(queuedTransientKey.Value, false);
            queuedTransientKey = null;
        }

        if (drawingWithRight != null || args.Button is not (MouseButton.Left or MouseButton.Right))
            return;

        if (args.Button == MouseButton.Right && !HandleRightMouseDown())
            return;

        var docManager = Owner.DocumentManagerSubViewModel;
        var activeDocument = docManager.ActiveDocument;
        if (activeDocument == null)
            return;

        drawingWithRight = args.Button == MouseButton.Right;
        activeDocument.EventInlet.OnCanvasLeftMouseButtonDown(args);
        if (args.Handled) return;

        Owner.ToolsSubViewModel.UseToolEventInlet(args.PositionOnCanvas, args.Button);

        if (args.Button == MouseButton.Right)
        {
            HandleRightSwapColor();
        }

        Analytics.SendUseTool(Owner.ToolsSubViewModel.ActiveTool, args.PositionOnCanvas, activeDocument.SizeBindable);
    }

    private bool HandleRightMouseDown()
    {
        var tools = Owner.ToolsSubViewModel;

        startedWithEraser = tools.ActiveTool is EraserToolViewModel;

        switch (tools.RightClickMode)
        {
            case RightClickMode.SecondaryColor when tools.ActiveTool.UsesColor:
                if (Owner.DocumentManagerSubViewModel.ActiveDocument.IsChangeFeatureActive<IDelayedColorSwapFeature>())
                {
                    return true;
                }

                Owner.ColorsSubViewModel.SwapColors(true);
                return true;
            case RightClickMode.ColorPicker when tools.ActiveTool is not ColorPickerToolViewModel:
                HandleRightMouseColorPickerDown(tools);
                return true;
            case RightClickMode.Erase when tools.ActiveTool is ColorPickerToolViewModel:
                Owner.ColorsSubViewModel.SwapColors(true);
                return true;
            case RightClickMode.Erase when tools.ActiveTool.IsErasable:
            {
                HandleRightMouseEraseDown(tools);
                return true;
            }
            case RightClickMode.SecondaryColor when tools.ActiveTool is BrightnessToolViewModel:
                return true;
            case RightClickMode.ContextMenu:
            default:
                return false;
        }
    }

    private void HandleRightSwapColor()
    {
        if (Owner.DocumentManagerSubViewModel.ActiveDocument is null)
            return;

        if (Owner.ColorsSubViewModel.ColorsTempSwapped)
            return;

        var tools = Owner.ToolsSubViewModel;

        if (tools is { RightClickMode: RightClickMode.SecondaryColor, ActiveTool.UsesColor: true })
        {
            Owner.ColorsSubViewModel.SwapColors(true);
        }
    }

    private void HandleRightMouseEraseDown(IToolsHandler tools)
    {
        EraserToolViewModel? eraserTool = tools.GetTool<EraserToolViewModel>();
        if (eraserTool == null)
        {
            return;
        }

        var currentToolSize = tools.ActiveTool.Toolbar.Settings.FirstOrDefault(x => x.Name == "ToolSize");
        hadSharedToolbar = tools.EnableSharedToolbar;
        if (currentToolSize != null)
        {
            tools.EnableSharedToolbar = false;

            var toolSize = eraserTool.Toolbar.Settings.First(x => x.Name == "ToolSize");
            previousEraseSize = (double)toolSize.Value;
            toolSize.Value = tools.ActiveTool is PenToolViewModel { PixelPerfectEnabled: true }
                ? 1d
                : currentToolSize.Value;
        }
        else
        {
            previousEraseSize = null;
        }

        tools.SetActiveTool<EraserToolViewModel>(true);
    }

    private void HandleRightMouseColorPickerDown(IToolsHandler tools)
    {
        ColorPickerToolViewModel? colorPickerTool = tools.GetTool<ColorPickerToolViewModel>();
        if (colorPickerTool == null)
        {
            return;
        }

        tools.SetActiveTool<ColorPickerToolViewModel>(true);
    }

    private void OnMiddleMouseButton()
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

    private void OnMouseUp(object? sender, MouseOnCanvasEventArgs args)
    {
        var button = args.Button;
        bool toLeftRightClick = drawingWithRight == null ||
                                (button == MouseButton.Left && drawingWithRight.Value) ||
                                (button == MouseButton.Right && !drawingWithRight.Value);

        if (toLeftRightClick && button != MouseButton.Middle)
            return;

        if (Owner.DocumentManagerSubViewModel.ActiveDocument is null)
            return;
        var tools = Owner.ToolsSubViewModel;

        var rightCanUp = (button == MouseButton.Right) &&
                         tools.RightClickMode is RightClickMode.Erase or RightClickMode.SecondaryColor
                             or RightClickMode.ColorPicker;

        if (button == MouseButton.Left || rightCanUp)
        {
            Owner.DocumentManagerSubViewModel.ActiveDocument.EventInlet
                .OnCanvasLeftMouseButtonUp(args.PositionOnCanvas);
        }

        drawingWithRight = null;

        HandleRightMouseUp(button, tools);
    }

    private void HandleRightMouseUp(MouseButton button, IToolsHandler tools)
    {
        switch (button)
        {
            case MouseButton.Middle:
                tools.RestorePreviousTool();
                break;
            case MouseButton.Right when Owner.ColorsSubViewModel.ColorsTempSwapped &&
                                        (tools.RightClickMode == RightClickMode.SecondaryColor ||
                                         tools is
                                         {
                                             ActiveTool: ColorPickerToolViewModel, RightClickMode: RightClickMode.Erase
                                         }
                                        ):

                if (!Owner.DocumentManagerSubViewModel.ActiveDocument.BlockingUpdateableChangeActive)
                {
                    Owner.ColorsSubViewModel.SwapColors(null);
                }
                else
                {
                    Owner.DocumentManagerSubViewModel.ActiveDocument.ToolSessionFinished +=
                        ToolSessionFinished;
                }

                break;
            case MouseButton.Right when tools.RightClickMode is RightClickMode.Erase or RightClickMode.ColorPicker:
                HandleRightMouseEraseUp(tools);
                break;
        }
    }

    private void HandleMouseWheel(object sender, ScrollOnCanvasEventArgs args)
    {
        if (Owner.DocumentManagerSubViewModel.ActiveDocument is null)
            return;

        if (args.KeyModifiers == KeyModifiers.Control)
        {
            var delta = args.Delta;

            Owner.ToolsSubViewModel.ChangeToolSize(delta.Y);
            args.Handled = true;
        }
    }

    private void ToolSessionFinished()
    {
        Owner.ColorsSubViewModel.SwapColors(null);
        Owner.DocumentManagerSubViewModel.ActiveDocument.ToolSessionFinished -= ToolSessionFinished;
    }

    private void HandleRightMouseEraseUp(IToolsHandler tools)
    {
        if (startedWithEraser)
        {
            return;
        }

        tools.EnableSharedToolbar = hadSharedToolbar;
        if (previousEraseSize != null)
        {
            EraserToolViewModel? eraserTool = tools.GetTool<EraserToolViewModel>();
            if (eraserTool != null)
            {
                eraserTool.Toolbar.Settings.First(x => x.Name == "ToolSize").Value =
                    previousEraseSize.Value;
            }
        }

        tools.RestorePreviousTool();
    }
}
