using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using PixiEditor.Helpers.Extensions;
using PixiEditor.ChangeableDocument.Enums;
using PixiEditor.Extensions.UI;
using PixiEditor.UI.Common.Localization;

namespace PixiEditor.Views.Input;
internal class BlendModeComboBox : ComboBox
{
    public static readonly StyledProperty<BlendMode> SelectedBlendModeProperty =
        AvaloniaProperty.Register<BlendModeComboBox, BlendMode>(
            nameof(SelectedBlendMode),
            BlendMode.Normal);

    public BlendMode SelectedBlendMode
    {
        get { return GetValue(SelectedBlendModeProperty); }
        set { SetValue(SelectedBlendModeProperty, value); }
    }

    private bool ignoreDepPropChange = false;
    private bool ignoreSelectionChange = false;

    static BlendModeComboBox()
    {
        SelectedBlendModeProperty.Changed.Subscribe(OnBlendModeChange);
    }

    public BlendModeComboBox()
    {
        ItemsPanel = new FuncTemplate<Panel>(() => new StackPanel() { Orientation = Orientation.Vertical });
        AddItems();
        SelectionChanged += OnSelectionChange;
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return item is Separator ? new Separator() : base.CreateContainerForItemOverride(item, index, recycleKey);
    }

    private void OnSelectionChange(object sender, SelectionChangedEventArgs e)
    {
        if (ignoreSelectionChange || e.AddedItems.Count == 0 || e.AddedItems[0] is not ComboBoxItem item || item.Tag is not BlendMode mode)
            return;
        ignoreDepPropChange = true;
        SelectedBlendMode = mode;
        ignoreDepPropChange = false;
    }

    private static void OnBlendModeChange(AvaloniaPropertyChangedEventArgs<BlendMode> args)
    {
        var combobox = (BlendModeComboBox)args.Sender;
        if (combobox.ignoreDepPropChange)
            return;
        foreach (var item in combobox.Items)
        {
            if (item is not ComboBoxItem cbItem)
                continue;
            if ((BlendMode)cbItem.Tag == args.NewValue.Value)
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
        var items = new List<AvaloniaObject>()
        {
            new ComboBoxItem() { Content = BlendMode.Normal.LocalizedKeys(), Tag = BlendMode.Normal },
            new ComboBoxItem() { Content = BlendMode.Erase.LocalizedKeys(), Tag = BlendMode.Erase },
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
            if (item is ComboBoxItem boxItem)
            {
                Translator.SetKey(boxItem, boxItem.Content.ToString());
            }

            Items.Add(item);
        }
        SelectedIndex = 0;
    }
}
