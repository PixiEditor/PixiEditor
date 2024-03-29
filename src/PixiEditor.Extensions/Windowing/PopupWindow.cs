﻿namespace PixiEditor.Extensions.Windowing;

public class PopupWindow : IPopupWindow
{
    public string UniqueId => _underlyingWindow.UniqueId;

    private IPopupWindow _underlyingWindow;

    public PopupWindow(IPopupWindow basicPopup)
    {
        _underlyingWindow = basicPopup;
    }

    public string Title
    {
        get => _underlyingWindow.Title;
        set => _underlyingWindow.Title = value;
    }

    public void Show() => _underlyingWindow.Show();
    public void Close() => _underlyingWindow.Close();

    public bool? ShowDialog() => _underlyingWindow.ShowDialog();
    public double Width
    {
        get => _underlyingWindow.Width;
        set => _underlyingWindow.Width = value;
    }
    public double Height
    {
        get => _underlyingWindow.Height;
        set => _underlyingWindow.Height = value;
    }
}
