using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using PixiEditor.DrawingApi.Core.Numerics;

namespace PixiEditor.Views.UserControls;

public class NestedPopup : Popup
{
    public static DependencyProperty TopmostProperty = Window.TopmostProperty.AddOwner(
        typeof(NestedPopup),
        new FrameworkPropertyMetadata( false, OnTopmostChanged ) );

    public bool Topmost
    {
        get { return (bool)GetValue( TopmostProperty ); }
        set { SetValue( TopmostProperty, value ); }
    }

    private static void OnTopmostChanged( DependencyObject obj, DependencyPropertyChangedEventArgs e )
    {
        (obj as NestedPopup)?.UpdateWindow();
    }

    protected override void OnOpened( EventArgs e )
    {
        UpdateWindow();
    }

    private void UpdateWindow()
    {
        var hwnd = ((HwndSource)PresentationSource.FromVisual(this.Child)).Handle;
        RectI rect;

        if (GetWindowRect(hwnd, out rect))
        {
            SetWindowPos(hwnd, Topmost ? -1 : -2, rect.Left, rect.Top, (int)this.Width, (int)this.Height, 0);
        }
    }

    #region P/Invoke imports & definitions

    [DllImport( "user32.dll" )]
    [return: MarshalAs( UnmanagedType.Bool )]
    private static extern bool GetWindowRect(IntPtr hWnd, out RectI lpRect);

    [DllImport( "user32", EntryPoint = "SetWindowPos" )]
    private static extern int SetWindowPos( IntPtr hWnd, int hwndInsertAfter, int x, int y, int cx, int cy, int wFlags );

    #endregion
}
