namespace PixiEditor.ViewModels.UserPreferences.Settings;

internal class DiscordSettings : SettingsGroup
{
    private bool enableRichPresence = GetPreference(nameof(EnableRichPresence), true);

    public bool EnableRichPresence
    {
        get => enableRichPresence;
        set
        {
            enableRichPresence = value;
            RaiseAndUpdatePreference(nameof(EnableRichPresence), value);
        }
    }

    private bool showDocumentName = GetPreference(nameof(ShowDocumentName), false);

    public bool ShowDocumentName
    {
        get => showDocumentName;
        set
        {
            showDocumentName = value;
            RaiseAndUpdatePreference(nameof(ShowDocumentName), value);
            OnPropertyChanged(nameof(DetailPreview));
        }
    }

    private bool showDocumentSize = GetPreference(nameof(ShowDocumentSize), true);

    public bool ShowDocumentSize
    {
        get => showDocumentSize;
        set
        {
            showDocumentSize = value;
            RaiseAndUpdatePreference(nameof(ShowDocumentSize), value);
            OnPropertyChanged(nameof(StatePreview));
        }
    }

    private bool showLayerCount = GetPreference(nameof(ShowLayerCount), true);

    public bool ShowLayerCount
    {
        get => showLayerCount;
        set
        {
            showLayerCount = value;
            RaiseAndUpdatePreference(nameof(ShowLayerCount), value);
            OnPropertyChanged(nameof(StatePreview));
        }
    }

    public string DetailPreview
    {
        get
        {
            return ShowDocumentName ? $"Editing coolPixelArt.pixi" : "Editing an image";
        }
    }

    public string StatePreview
    {
        get
        {
            string state = string.Empty;

            if (ShowDocumentSize)
            {
                state = "16x16";
            }

            if (ShowDocumentSize && ShowLayerCount)
            {
                state += ", ";
            }

            if (ShowLayerCount)
            {
                state += $"2 Layers";
            }

            return state;
        }
    }
}
