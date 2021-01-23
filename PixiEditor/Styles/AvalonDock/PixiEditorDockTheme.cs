using System;
using AvalonDock.Themes;

namespace PixiEditor.Styles.AvalonDock
{
    public class PixiEditorDockTheme : Theme
    {
        public override Uri GetResourceUri()
        {
            return new Uri("/PixiEditor;component/Styles/AvalonDock/PixiEditorDockTheme.xaml", UriKind.Relative);
        }
    }
}