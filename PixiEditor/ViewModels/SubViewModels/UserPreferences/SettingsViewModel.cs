using System;
using System.Configuration;
using PixiEditor.Models.UserPreferences;

namespace PixiEditor.ViewModels.SubViewModels.UserPreferences
{
    public class SettingsViewModel : SubViewModel<SettingsWindowViewModel>
    {
        private bool showNewFilePopupOnStartup = PreferencesSettings.GetPreference("ShowNewFilePopupOnStartup", true);

        public bool ShowNewFilePopupOnStartup
        {
            get => showNewFilePopupOnStartup;
            set
            {
                showNewFilePopupOnStartup = value;
                string name = nameof(ShowNewFilePopupOnStartup);
                RaiseAndUpdatePreference(name, value);
            }
        }

        private bool checkUpdatesOnStartup = PreferencesSettings.GetPreference("CheckUpdatesOnStartup", true);

        public bool CheckUpdatesOnStartup
        {
            get => checkUpdatesOnStartup;
            set
            {
                checkUpdatesOnStartup = value;
                string name = nameof(CheckUpdatesOnStartup);
                RaiseAndUpdatePreference(name, value);
            }
        }

        private long defaultNewFileWidth = (int)PreferencesSettings.GetPreference("DefaultNewFileWidth", 16L);

        public long DefaultNewFileWidth
        {
            get => defaultNewFileWidth;
            set
            {
                defaultNewFileWidth = value;
                string name = nameof(DefaultNewFileWidth);
                RaiseAndUpdatePreference(name, value);
            }
        }

        private long defaultNewFileHeight = (int)PreferencesSettings.GetPreference("DefaultNewFileHeight", 16L);

        public long DefaultNewFileHeight
        {
            get => defaultNewFileHeight;
            set
            {
                defaultNewFileHeight = value;
                string name = nameof(DefaultNewFileHeight);
                RaiseAndUpdatePreference(name, value);
            }
        }

        public class DiscordSettings : SettingsGroup
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

            private bool showDocumentName = GetPreference(nameof(ShowDocumentName), true);

            public bool ShowDocumentName
            {
                get => showDocumentName;
                set
                {
                    showDocumentName = value;
                    RaiseAndUpdatePreference(nameof(ShowDocumentName), value);
                    RaisePropertyChanged(nameof(DetailPreview));
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
                    RaisePropertyChanged(nameof(StatePreview));
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
                    RaisePropertyChanged(nameof(StatePreview));
                }
            }

            public string DetailPreview
            {
                get
                {
                    return ShowDocumentName ? $"Editing coolPixelArt.pixi" : "Editing something (incognito)";
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
                        state += "2 Layers";
                    }

                    return state;
                }
            }
        }

        public DiscordSettings Discord { get; set; } = new DiscordSettings();

        public void RaiseAndUpdatePreference<T>(string name, T value)
        {
            RaisePropertyChanged(name);
            PreferencesSettings.UpdatePreference(name, value);
        }

        public SettingsViewModel(SettingsWindowViewModel owner)
            : base(owner)
        {
        }
    }
}