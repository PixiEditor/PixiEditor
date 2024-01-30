using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using PixiEditor.Extensions.CommonApi.LayoutBuilding;

namespace PixiEditor.Extensions.LayoutBuilding.Elements;

public class Center : SingleChildLayoutElement
{
    private Panel panel;
    public Center(ILayoutElement<Control> child = null)
    {
        Child = child;
        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(panel == null)
        {
            return;
        }

        if (e.PropertyName == nameof(Child))
        {
            panel.Children.Clear();
            if (Child != null)
            {
                panel.Children.Add(Child.BuildNative());
            }
        }
    }

    public override Control BuildNative()
    {
        panel = new Panel()
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };

        if (Child != null)
        {
            Control child = Child.BuildNative();
            panel.Children.Add(child);
        }

        return panel;
    }
}
