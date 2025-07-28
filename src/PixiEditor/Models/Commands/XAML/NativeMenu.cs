using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.Helpers;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Commands.CommandContext;
using PixiEditor.Models.Commands.Evaluators;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Input;
using PixiEditor.ViewModels;
using PixiEditor.Views;

namespace PixiEditor.Models.Commands.XAML;

internal class NativeMenu : global::Avalonia.Controls.Menu
{
    public const double IconDimensions = 18;
    public const double IconFontSize = 18;

    public static readonly AttachedProperty<string> CommandProperty =
        AvaloniaProperty.RegisterAttached<NativeMenu, NativeMenuItem, string>(nameof(Command));

    public static readonly AttachedProperty<string> LocalizationKeyHeaderProperty =
        AvaloniaProperty.RegisterAttached<NativeMenu, NativeMenuItem, string>("LocalizationKeyHeader");

    public static void SetLocalizationKeyHeader(NativeMenuItem obj, string value) =>
        obj.SetValue(LocalizationKeyHeaderProperty, value);

    public static string GetLocalizationKeyHeader(NativeMenuItem obj) => obj.GetValue(LocalizationKeyHeaderProperty);

    static NativeMenu()
    {
        CommandProperty.Changed.Subscribe(CommandChanged);
    }

    public static string GetCommand(NativeMenuItem menu) => (string)menu.GetValue(CommandProperty);
    public static void SetCommand(NativeMenuItem menu, string value) => menu.SetValue(CommandProperty, value);

    public static async void CommandChanged(AvaloniaPropertyChangedEventArgs e) //TODO: Validate async void works
    {
        if (e.NewValue is not string value || e.Sender is not NativeMenuItem item)
        {
            throw new InvalidOperationException($"{nameof(NativeMenu)}.Command only works for NativeMenuItem's");
        }

        if (Design.IsDesignMode)
        {
            HandleDesignMode(item, value);
            return;
        }

        var command = CommandController.Current.Commands[value];

        Bitmap? bitmapIcon = command.GetIcon().ToBitmap(new PixelSize((int)IconDimensions, (int)IconDimensions));

        var iCommand = Command.GetICommand(command, new MenuSourceInfo(MenuType.Menu), false);

        RelayCommand<object> wrapper = new RelayCommand<object>(parameter =>
        {
            if (!ShortcutController.ShortcutExecutionBlocked)
            {
                if (iCommand.CanExecute(parameter))
                {
                    iCommand.Execute(parameter);
                }
            }
            else
            {
                var focusedElement = MainWindow.Current.FocusManager.GetFocusedElement();
                focusedElement?.RaiseEvent(new KeyEventArgs()
                {
                    Key = command.Shortcut.Key,
                    KeyModifiers = command.Shortcut.Modifiers,
                    KeyDeviceType = KeyDeviceType.Keyboard,
                    PhysicalKey = command.Shortcut.Key.ToPhysicalKey(),
                    Source = item,
                    RoutedEvent = InputElement.KeyDownEvent,
                    KeySymbol = command.Shortcut.Key.ToString()
                });
            }
        });

        item.Command = wrapper;
        item.Icon = bitmapIcon;
        // Setting gestures causes issues, since macos is not aware of active contexts and might bypass Command Controller blockers
        // Making a wrapper prevents this, but also doesn't pass key down events to the app. So that's why we invoke the event manually

        item.Bind(NativeMenuItem.GestureProperty, ShortcutBinding.GetBinding(command, null, true));
    }

    private static RawInputModifiers ToRawModifiers(KeyModifiers modifiers)
    {
        RawInputModifiers raw = 0;
        if (modifiers.HasFlag(KeyModifiers.Shift))
            raw |= RawInputModifiers.Shift;
        if (modifiers.HasFlag(KeyModifiers.Control))
            raw |= RawInputModifiers.Control;
        if (modifiers.HasFlag(KeyModifiers.Alt))
            raw |= RawInputModifiers.Alt;
        if (modifiers.HasFlag(KeyModifiers.Meta))
            raw |= RawInputModifiers.Meta;

        return raw;
    }

    private static void HandleDesignMode(NativeMenuItem item, string name)
    {
        var command = DesignCommandHelpers.GetCommandAttribute(name);
        item.Gesture = new KeyCombination(command.Key, command.Modifiers).ToKeyGesture();
    }

    private static Bitmap ImageToBitmap(IImage? image, int width, int height)
    {
        if (image is null)
        {
            return null;
        }

        RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(new PixelSize(width, height));
        var ctx = renderTargetBitmap.CreateDrawingContext();
        image.Draw(ctx, new Rect(0, 0, width, height), new Rect(0, 0, width, height));
        ctx.Dispose();

        return renderTargetBitmap;
    }
}
