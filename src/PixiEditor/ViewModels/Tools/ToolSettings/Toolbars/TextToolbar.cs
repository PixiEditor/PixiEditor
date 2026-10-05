using System.Collections.ObjectModel;
using System.Text;
using Drawie.Backend.Core.Text;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.Helpers.Converters;
using PixiEditor.Helpers.Decorators;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Handlers.Toolbars;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels.Tools.ToolSettings.Settings;

namespace PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;

internal class TextToolbar : FillableShapeToolbar, ITextToolbar
{
    public FontFamilyName FontFamily
    {
        get
        {
            return GetSetting<FontFamilySettingViewModel>(nameof(FontFamily)).Value;
        }
        set
        {
            int index = Array.IndexOf(FontLibrary.AllFonts, value);
            if (index == -1)
            {
                try
                {
                    using Font font = Font.FromFontFamily(value);
                    if (font != null && FontLibrary.TryAddCustomFont(value))
                    {
                        index = Array.IndexOf(FontLibrary.AllFonts, value);
                    }

                    if (index == -1)
                    {
                        index = 0;
                    }
                }
                catch
                {
                    index = 0;
                }
            }

            GetSetting<FontFamilySettingViewModel>(nameof(FontFamily)).FontIndex = index;
        }
    }

    public FontStyle FontStyle
    {
        get
        {
            return GetSetting<ListSettingViewModel<FontStyle>>(nameof(FontStyle)).Value;
        }
        set
        {
            GetSetting<ListSettingViewModel<FontStyle>>(nameof(FontStyle)).Value = value;
        }
    }

    public double FontSize
    {
        get
        {
            return GetSetting<SizeSettingViewModel>(nameof(FontSize)).Value;
        }
        set
        {
            GetSetting<SizeSettingViewModel>(nameof(FontSize)).Value = value;
        }
    }

    public double Spacing
    {
        get
        {
            return GetSetting<SizeSettingViewModel>(nameof(Spacing)).Value;
        }
        set
        {
            GetSetting<SizeSettingViewModel>(nameof(Spacing)).Value = value;
        }
    }

    public TextAlign Alignment
    {
        get
        {
            return (TextAlign)GetSetting<EnumSettingViewModel<Align>>(nameof(Alignment)).Value;
        }
        set
        {
            GetSetting<EnumSettingViewModel<Align>>(nameof(Alignment)).Value = (Align)value;
        }
    }

    public bool ForceLowDpiRendering
    {
        get
        {
            return GetSetting<BoolSettingViewModel>(nameof(ForceLowDpiRendering)).Value;
        }
        set
        {
            GetSetting<BoolSettingViewModel>(nameof(ForceLowDpiRendering)).Value = value;
        }
    }

    public bool Bold
    {
        get
        {
            return GetSetting<BoolSettingViewModel>(nameof(Bold)).Value;
        }
        set
        {
            GetSetting<BoolSettingViewModel>(nameof(Bold)).Value = value;
        }
    }

    public bool Italic
    {
        get
        {
            return GetSetting<BoolSettingViewModel>(nameof(Italic)).Value;
        }
        set
        {
            GetSetting<BoolSettingViewModel>(nameof(Italic)).Value = value;
        }
    }

    public TextToolbar()
    {
        var fontFamilySetting = new FontFamilySettingViewModel(nameof(FontFamily), "");
        AddSetting(fontFamilySetting);

        FontFamily = FontLibrary.DefaultFontFamily;


        var styleSetting =
            new ListSettingViewModel<FontStyle>(nameof(FontStyle), "FONT_STYLE_LABEL",
                new[] { Drawie.Backend.Core.Text.FontStyle.Normal })
            {
                PickerType = ListSettingPickerType.ComboBox,
                TextFormatter = new InlineTextFormatter<FontStyle>(FormatStyleText)
            };

        AddSetting(styleSetting);

        var sizeSetting =
            new SizeSettingViewModel(nameof(FontSize), "FONT_SIZE_LABEL", unit: new LocalizedString("UNIT_PT"))
            {
                Value = 12
            };
        AddSetting(sizeSetting);
        var spacingSetting =
            new SizeSettingViewModel(nameof(Spacing), unit: new LocalizedString("UNIT_PT"))
            {
                Tooltip = "SPACING_LABEL", Icon = PixiPerfectIcons.LineHeight
            };
        spacingSetting.Value = 12;

        sizeSetting.ValueChanged += (sender, args) =>
        {
            double delta = args.NewValue - args.OldValue;
            spacingSetting.Value += delta;
        };

        AddSetting(spacingSetting);

        AddSetting(new BoolSettingViewModel(nameof(Bold)) { Icon = PixiPerfectIcons.Bold, Tooltip = "BOLD_TOOLTIP" });

        AddSetting(new BoolSettingViewModel(nameof(Italic))
        {
            Icon = PixiPerfectIcons.Italic, Tooltip = "ITALIC_TOOLTIP"
        });

        AddSetting(new EnumSettingViewModel<Align>(nameof(Alignment), "")
        {
            Tooltip = "TEXT_ALIGN_TOOLTIP", PickerType = ListSettingPickerType.IconButtons
        });

        AddSetting(new BoolSettingViewModel(nameof(ForceLowDpiRendering), "__force_low_dpi_rendering")
        {
            IsExposed = false, Value = false
        });
    }

    private string FormatStyleText(FontStyle? x)
    {
        var localizedWeight =
            new LocalizedString(x.Weight.ToString().Replace(" ", "_").ToUpperInvariant() + "_FONT_STYLE_WEIGHT");
        var localizedSlant = new LocalizedString(x.Slant.ToString().Replace(" ", "_").ToUpperInvariant() + "_FONT_STYLE_SLANT");

        StringBuilder builder = new StringBuilder();
        builder.Append(localizedWeight);
        if (x.Slant != FontStyleSlant.Upright)
        {
            builder.Append(' ');
            builder.Append(localizedSlant);
        }

        return builder.ToString();
    }

    public FontData ConstructFont()
    {
        FontData font = FontData.CreateDefault();
        if (!string.IsNullOrEmpty(FontFamily.Name))
        {
            font.Family = FontFamily;
        }

        font.Size = (float)FontSize;
        font.Edging = AntiAliasing ? FontEdging.AntiAlias : FontEdging.Alias;
        font.Weight = FontStyle?.Weight ?? FontStyleWeight.Normal;
        font.Slant = FontStyle?.Slant ?? FontStyleSlant.Upright;

        return font;
    }

    public void UpdateFontStyles()
    {
        GetSetting<ListSettingViewModel<FontStyle>>(nameof(FontStyle)).Values = new ObservableCollection<FontStyle>(FontLibrary.GetAvailableFontStyles(FontFamily.Name));
    }
}

enum Align
{
    [IconName(PixiPerfectIcons.AlignLeft)] Left,

    [IconName(PixiPerfectIcons.AlignStretch)]
    Center,

    [IconName(PixiPerfectIcons.AlignRight)]
    Right
}
