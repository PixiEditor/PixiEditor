using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace PixiEditor.Helpers;

internal static class FocusHelper
{
    public static void MoveFocusToParent(FrameworkElement element)
    {
        FrameworkElement parent = (FrameworkElement)VisualTreeHelper.GetParent(element);

        while (parent is IInputElement elem && !elem.Focusable)
        {
            parent = (FrameworkElement)VisualTreeHelper.GetParent(parent);
        }

        DependencyObject scope = FocusManager.GetFocusScope(element);
        FocusManager.SetFocusedElement(scope, parent);
    }
}
