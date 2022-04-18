using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using PixiEditor.ChangeableDocument.Enums;

namespace PixiEditorPrototype.CustomControls;
internal class BlendModeComboBox : ComboBox
{
    public static readonly DependencyProperty SelectedBlendModeProperty = DependencyProperty.Register(
        nameof(SelectedBlendMode), typeof(BlendMode), typeof(BlendModeComboBox),
        new PropertyMetadata(BlendMode.Normal, OnBlendModeChange));

    public BlendMode SelectedBlendMode
    {
        get { return (BlendMode)GetValue(SelectedBlendModeProperty); }
        set { SetValue(SelectedBlendModeProperty, value); }
    }

    private bool ignoreDepPropChange = false;
    private bool ignoreSelectionChange = false;

    public BlendModeComboBox()
    {
        AddItems();
        SelectionChanged += OnSelectionChange;
    }

    private void OnSelectionChange(object sender, SelectionChangedEventArgs e)
    {
        if (ignoreSelectionChange || e.AddedItems.Count == 0 || e.AddedItems[0] is not ComboBoxItem item || item.Tag is not BlendMode mode)
            return;
        ignoreDepPropChange = true;
        SelectedBlendMode = mode;
        ignoreDepPropChange = false;
    }

    private static void OnBlendModeChange(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        var combobox = (BlendModeComboBox)obj;
        if (combobox.ignoreDepPropChange)
            return;
        foreach (var item in combobox.Items)
        {
            if (item is not ComboBoxItem cbItem)
                continue;
            if ((BlendMode)cbItem.Tag == (BlendMode)args.NewValue)
            {
                combobox.ignoreSelectionChange = true;
                combobox.SelectedItem = item;
                combobox.ignoreSelectionChange = false;
                break;
            }
        }
    }

    private void AddItems()
    {
        var items = new List<UIElement>() {
            new ComboBoxItem() { Content = "Normal", Tag = BlendMode.Normal },
            new Separator(),
            new ComboBoxItem() { Content = "Darken", Tag = BlendMode.Darken },
            new ComboBoxItem() { Content = "Multiply", Tag = BlendMode.Multiply },
            new ComboBoxItem() { Content = "Color Burn", Tag = BlendMode.ColorBurn },
            new Separator(),
            new ComboBoxItem() { Content = "Lighten", Tag = BlendMode.Lighten },
            new ComboBoxItem() { Content = "Screen", Tag = BlendMode.Screen },
            new ComboBoxItem() { Content = "Color Dodge", Tag = BlendMode.ColorDodge },
            new ComboBoxItem() { Content = "Linear Dodge (Add)", Tag = BlendMode.LinearDodge },
            new Separator(),
            new ComboBoxItem() { Content = "Overlay", Tag = BlendMode.Overlay },
            new ComboBoxItem() { Content = "Soft Light", Tag = BlendMode.SoftLight },
            new ComboBoxItem() { Content = "Hard Light", Tag = BlendMode.HardLight },
            new Separator(),
            new ComboBoxItem() { Content = "Difference", Tag = BlendMode.Difference },
            new ComboBoxItem() { Content = "Exclusion", Tag = BlendMode.Exclusion },
            new Separator(),
            new ComboBoxItem() { Content = "Hue", Tag = BlendMode.Hue },
            new ComboBoxItem() { Content = "Saturation", Tag = BlendMode.Saturation },
            new ComboBoxItem() { Content = "Luminosity", Tag = BlendMode.Luminosity },
            new ComboBoxItem() { Content = "Color", Tag = BlendMode.Color }
        };
        foreach (var item in items)
        {
            Items.Add(item);
        }
        SelectedIndex = 0;
    }
}
