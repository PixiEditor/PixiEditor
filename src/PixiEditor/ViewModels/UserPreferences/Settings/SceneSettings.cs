using System.Windows.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using Drawie.Numerics;
using PixiEditor.Extensions.CommonApi.UserPreferences;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings.PixiEditor;
using PixiEditor.Helpers.Extensions;

namespace PixiEditor.ViewModels.UserPreferences.Settings;

internal class SceneSettings : SettingsGroup
{
    private bool autoScaleBackground = GetPreference(PreferencesConstants.AutoScaleBackground, PreferencesConstants.AutoScaleBackgroundDefault);
    public bool AutoScaleBackground
    {
        get => autoScaleBackground;
        set => RaiseAndUpdatePreference(ref autoScaleBackground, value);
    }

    private double customBackgroundScaleX = GetPreference(PreferencesConstants.CustomBackgroundScaleX, PreferencesConstants.CustomBackgroundScaleDefault);
    public double CustomBackgroundScaleX
    {
        get => customBackgroundScaleX;
        set => RaiseAndUpdatePreference(ref customBackgroundScaleX, value);
    }

    private double customBackgroundScaleY = GetPreference(PreferencesConstants.CustomBackgroundScaleY, PreferencesConstants.CustomBackgroundScaleDefault);
    public double CustomBackgroundScaleY
    {
        get => customBackgroundScaleY;
        set => RaiseAndUpdatePreference(ref customBackgroundScaleY, value);
    }

    private string _primaryBackgroundColorHex = GetPreference(PreferencesConstants.PrimaryBackgroundColor, PreferencesConstants.PrimaryBackgroundColorDefault);
    public string PrimaryBackgroundColorHex
    {
        get => _primaryBackgroundColorHex;
        set => RaiseAndUpdatePreference(ref _primaryBackgroundColorHex, value, PreferencesConstants.PrimaryBackgroundColor);
    }

    private string _secondaryBackgroundColorHex = GetPreference(PreferencesConstants.SecondaryBackgroundColor, PreferencesConstants.SecondaryBackgroundColorDefault);
    public string SecondaryBackgroundColorHex
    {
        get => _secondaryBackgroundColorHex;
        set => RaiseAndUpdatePreference(ref _secondaryBackgroundColorHex, value, PreferencesConstants.SecondaryBackgroundColor);
    }

    public bool SelectionTintingEnabled
    {
        get => PixiEditorSettings.Tools.SelectionTintingEnabled.Value;
        set => RaiseAndUpdatePreference(PixiEditorSettings.Tools.SelectionTintingEnabled, value);
    }

    public Color PrimaryBackgroundColor
    {
        get => Color.Parse(PrimaryBackgroundColorHex);
        set => PrimaryBackgroundColorHex = value.ToColor().ToRgbHex();
    }

    public Color SecondaryBackgroundColor
    {
        get => Color.Parse(SecondaryBackgroundColorHex);
        set => SecondaryBackgroundColorHex = value.ToColor().ToRgbHex();
    }

    public ICommand ResetBackgroundCommand { get; }

    public SceneSettings()
    {
        ResetBackgroundCommand = new RelayCommand(() =>
        {
            PrimaryBackgroundColorHex = PreferencesConstants.PrimaryBackgroundColorDefault;
            SecondaryBackgroundColorHex = PreferencesConstants.SecondaryBackgroundColorDefault;
        });
        
        SubscribeValueChanged(PixiEditorSettings.Tools.SelectionTintingEnabled, nameof(SelectionTintingEnabled));
    }
}
