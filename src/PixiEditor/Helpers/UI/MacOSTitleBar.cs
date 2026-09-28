using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace PixiEditor.Helpers.UI;

// Taken from https://github.com/avaloniaui/avalonia/issues/21119
public static class MacOSTitleBar
{
    // ── attached property ──────────────────────────────────

    public static readonly AttachedProperty<bool> IsThickProperty =
        AvaloniaProperty.RegisterAttached<Window, bool>(
            "IsThick", typeof(MacOSTitleBar), defaultValue: false);

    public static bool GetIsThick(Window window) => window.GetValue(IsThickProperty);
    public static void SetIsThick(Window window, bool value) => window.SetValue(IsThickProperty, value);

    static MacOSTitleBar()
    {
        IsThickProperty.Changed.AddClassHandler<Window>(OnIsThickChanged);
    }

    // ── per-window state ───────────────────────────────────
    // ConditionalWeakTable ensures state is garbage-collected alongside the
    // Window instance, preventing leaks when windows are closed.

    private static readonly ConditionalWeakTable<Window, ThickTitleBarState> States = new();

    private static void OnIsThickChanged(Window window, AvaloniaPropertyChangedEventArgs args)
    {
        // no-op on non-macOS — safe to leave the property in cross-platform XAML
        if (!System.OperatingSystem.IsMacOS())
            return;

        if (args.NewValue is true)
        {
            var state = new ThickTitleBarState(window);
            States.AddOrUpdate(window, state);
            state.Attach();
        }
        else
        {
            if (States.TryGetValue(window, out var state))
            {
                state.Detach();
                States.Remove(window);
            }
        }
    }

    // ── per-window lifecycle ───────────────────────────────

    private sealed class ThickTitleBarState
    {
        private readonly Window _window;
        private IntPtr _nsWindow;
        private IntPtr _nsToolbar;
        private IntPtr _selSetToolbar;
        private IntPtr _selSetTitlebarAppearsTransparent;
        private IntPtr _selSetTitleVisibility;
        private bool _attached;

        public ThickTitleBarState(Window window) => _window = window;

        public void Attach()
        {
            if (_attached)
                return;

            // the NSWindow handle is only available after the native window is
            // created — defer setup if the window hasn't opened yet
            if (_window.IsLoaded)
                Setup();
            else
                _window.Opened += OnOpened;
        }

        public void Detach()
        {
            if (!_attached)
                return;

            _window.Opened -= OnOpened;
            _window.PropertyChanged -= OnWindowPropertyChanged;

            // remove the toolbar to restore the default thin title bar
            if (_nsWindow != IntPtr.Zero && _selSetToolbar != IntPtr.Zero)
                objc_msgSend_arg(_nsWindow, _selSetToolbar, IntPtr.Zero);

            _attached = false;
        }

        private void OnOpened(object? sender, EventArgs e)
        {
            _window.Opened -= OnOpened;
            Setup();
        }

        private void Setup()
        {
            try
            {
                // 1. get the native NSWindow handle from Avalonia's platform interop.
                //    this is the actual Cocoa window object that AppKit manages.
                _nsWindow = _window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
                if (_nsWindow == IntPtr.Zero)
                    return;

                // 2. cache ObjC selectors used during setup and fullscreen transitions.
                //    sel_registerName returns a stable pointer for the lifetime of the process.
                _selSetToolbar = sel_registerName("setToolbar:");
                _selSetTitlebarAppearsTransparent = sel_registerName("setTitlebarAppearsTransparent:");
                _selSetTitleVisibility = sel_registerName("setTitleVisibility:");

                // 3. create an empty NSToolbar. assigning ANY toolbar to an NSWindow causes
                //    macOS to switch to the "unified toolbar" layout with a taller title bar.
                //    the toolbar itself has no items — it's purely a trigger for the height.
                _nsToolbar = objc_msgSend_ret(
                    objc_getClass("NSToolbar"), sel_registerName("alloc"));

                _nsToolbar = objc_msgSend_arg_ret(
                    _nsToolbar,
                    sel_registerName("initWithIdentifier:"),
                    CreateNSString("filamentTitleBar"));

                if (_nsToolbar == IntPtr.Zero)
                    return;

                // 4. hide the baseline separator line between the toolbar and content.
                //    deprecated since macOS 11 (Big Sur) where Apple unified the toolbar,
                //    but still functions as a harmless no-op on modern versions.
                objc_msgSend_bool(_nsToolbar, sel_registerName("setShowsBaselineSeparator:"), false);

                // 5. assign the toolbar to the window — this is what triggers the height increase
                objc_msgSend_arg(_nsWindow, _selSetToolbar, _nsToolbar);

                // 6. hide the window title text (NSWindowTitleHidden = 1).
                //    with the extended client area, we render our own title bar content
                //    in Avalonia, so the native title label is unnecessary clutter.
                objc_msgSend_long(_nsWindow, _selSetTitleVisibility, 1);

                // 7. subscribe to WindowState changes to handle fullscreen transitions.
                //    the toolbar must be removed during fullscreen to prevent an opaque
                //    gray bar, and restored on exit to bring back the tall title bar.
                _window.PropertyChanged += OnWindowPropertyChanged;

                _attached = true;
                Trace.TraceInformation("macOS thick title bar: toolbar assigned to NSWindow");
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("macOS thick title bar setup failed: {0}", ex.Message);
            }
        }

        private void OnWindowPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property != Window.WindowStateProperty)
                return;

            var newState = e.GetNewValue<WindowState>();

            // use Background priority so this runs AFTER Avalonia's own fullscreen
            // handlers, which set titlebarAppearsTransparent = NO during transitions.
            // Background is lower priority than Normal/Send, so our override wins.
            Dispatcher.UIThread.Post(() =>
            {
                try
                {
                    if (newState == WindowState.FullScreen)
                    {
                        // remove the toolbar entirely — if we leave it attached, macOS
                        // renders it as an opaque gray bar at the top of the fullscreen
                        // space. without the toolbar, the app content fills the screen.
                        objc_msgSend_arg(_nsWindow, _selSetToolbar, IntPtr.Zero);

                        // Avalonia's native layer sets titlebarAppearsTransparent = NO
                        // on fullscreen entry. override it back to YES so the title bar
                        // region (now toolbar-less) blends with our Avalonia content.
                        objc_msgSend_bool(_nsWindow, _selSetTitlebarAppearsTransparent, true);

                        // ensure title text stays hidden
                        objc_msgSend_long(_nsWindow, _selSetTitleVisibility, 1);
                    }
                    else
                    {
                        // re-attach the toolbar to restore the thick title bar.
                        // this is needed after exiting fullscreen — macOS reverts to
                        // the thin default title bar when no toolbar is assigned.
                        objc_msgSend_arg(_nsWindow, _selSetToolbar, _nsToolbar);

                        // same transparent titlebar override for the exit transition
                        objc_msgSend_bool(_nsWindow, _selSetTitlebarAppearsTransparent, true);

                        // ensure title text stays hidden
                        objc_msgSend_long(_nsWindow, _selSetTitleVisibility, 1);
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceWarning("macOS thick title bar fullscreen toggle failed: {0}", ex.Message);
                }
            }, DispatcherPriority.Background);
        }
    }

    // ── ObjC helpers ───────────────────────────────────────

    private static IntPtr CreateNSString(string str)
    {
        var nsString = objc_msgSend_ret(objc_getClass("NSString"), sel_registerName("alloc"));
        var utf8Ptr = Marshal.StringToCoTaskMemUTF8(str);
        try
        {
            return objc_msgSend_arg_ret(nsString, sel_registerName("initWithUTF8String:"), utf8Ptr);
        }
        finally
        {
            Marshal.FreeCoTaskMem(utf8Ptr);
        }
    }

    // ── P/Invoke: ObjC runtime ─────────────────────────────
    // These are thin wrappers around the Objective-C runtime's messaging
    // functions. Each variant handles a different return/argument signature
    // because objc_msgSend uses the C calling convention and the marshaller
    // needs to know the exact types at compile time.

    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern IntPtr objc_getClass(string name);

    [DllImport("/usr/lib/libobjc.A.dylib")]
    private static extern IntPtr sel_registerName(string name);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_ret(IntPtr receiver, IntPtr selector);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_arg(IntPtr receiver, IntPtr selector, IntPtr arg);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern IntPtr objc_msgSend_arg_ret(IntPtr receiver, IntPtr selector, IntPtr arg);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_bool(IntPtr receiver, IntPtr selector, [MarshalAs(UnmanagedType.I1)] bool arg);

    [DllImport("/usr/lib/libobjc.A.dylib", EntryPoint = "objc_msgSend")]
    private static extern void objc_msgSend_long(IntPtr receiver, IntPtr selector, long arg);
}
