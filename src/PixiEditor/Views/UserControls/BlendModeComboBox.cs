using System.Windows;
using System.Windows.Controls;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Extensions.UI;
using PixiEditor.Helpers.Extensions;

namespace PixiEditor.Views.UserControls;
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
        var items = new List<UIElement>() 
        {
            new ComboBoxItem() { Content = BlendMode.Normal.LocalizedKeys(), Tag = BlendMode.Normal },
            new Separator(),
            new ComboBoxItem() { Content = BlendMode.Darken.LocalizedKeys(), Tag = BlendMode.Darken },
            new ComboBoxItem() { Content = BlendMode.Multiply.LocalizedKeys(), Tag = BlendMode.Multiply },
            new ComboBoxItem() { Content = BlendMode.ColorBurn.LocalizedKeys(), Tag = BlendMode.ColorBurn },
            new Separator(),
            new ComboBoxItem() { Content = BlendMode.Lighten.LocalizedKeys(), Tag = BlendMode.Lighten },
            new ComboBoxItem() { Content = BlendMode.Screen.LocalizedKeys(), Tag = BlendMode.Screen },
            new ComboBoxItem() { Content = BlendMode.ColorDodge.LocalizedKeys(), Tag = BlendMode.ColorDodge },
            new ComboBoxItem() { Content = BlendMode.LinearDodge.LocalizedKeys(), Tag = BlendMode.LinearDodge },
            new Separator(),
            new ComboBoxItem() { Content = BlendMode.Overlay.LocalizedKeys(), Tag = BlendMode.Overlay },
            new ComboBoxItem() { Content = BlendMode.SoftLight.LocalizedKeys(), Tag = BlendMode.SoftLight },
            new ComboBoxItem() { Content = BlendMode.HardLight.LocalizedKeys(), Tag = BlendMode.HardLight },
            new Separator(),
            new ComboBoxItem() { Content = BlendMode.Difference.LocalizedKeys(), Tag = BlendMode.Difference },
            new ComboBoxItem() { Content = BlendMode.Exclusion.LocalizedKeys(), Tag = BlendMode.Exclusion },
            new Separator(),
            new ComboBoxItem() { Content = BlendMode.Hue.LocalizedKeys(), Tag = BlendMode.Hue },
            new ComboBoxItem() { Content = BlendMode.Saturation.LocalizedKeys(), Tag = BlendMode.Saturation },
            new ComboBoxItem() { Content = BlendMode.Luminosity.LocalizedKeys(), Tag = BlendMode.Luminosity },
            new ComboBoxItem() { Content = BlendMode.Color.LocalizedKeys(), Tag = BlendMode.Color }
        };
        foreach (var item in items)
        {
            if (item is ComboBoxItem)
            {
                Translator.SetKey((ComboBoxItem)item, ((ComboBoxItem)item).Content.ToString());
            }
            Items.Add(item);
        }
        SelectedIndex = 0;
    }
}
