using PixiEditor.Helpers;
using PixiEditor.Models;
using PixiEditor.Models.UserPreferences;
using PixiEditor.SDK;
using PixiEditor.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class ExtensionViewModel : SubViewModel<ViewModelMain>
    {
        internal SDKManager SDKManager { get; set; }

        public RelayCommand OpenExtensionsWindowCommand { get; set; }

        public ReadOnlyCollection<Extension> Extensions => new ReadOnlyCollection<Extension>(SDKManager.Extensions);

        public ExtensionViewModel(ViewModelMain owner)
            : base(owner)
        {
            SDKManager = new SDKManager();

            Preferences.Init(SavePreference, LoadPreference);

            // Responsible for parsing .pixi, .png, .jpeg
            SDKManager.AddBaseExtension(new BaseExtension());

            SDKManager.LoadExtensions(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PixiEditor", "Extensions"));
            SDKManager.SetupExtensions();

            OpenExtensionsWindowCommand = new RelayCommand(OpenExtensionsWindow);
        }

        public void OpenExtensionsWindow() => OpenExtensionsWindow(null);

        private void OpenExtensionsWindow(object param)
        {
            ExtensionWindow window = new ExtensionWindow(this);
            window.Show();
        }

        private void SavePreference(Extension ext, string name, object obj, PreferenceStorageLocation location)
        {
            Dictionary<string, object> extensionSettings;

            if (location == PreferenceStorageLocation.Local)
            {
                extensionSettings = IPreferences.Current.GetLocalPreference<Dictionary<string, object>>(ext.Name, new());
            }
            else
            {
                extensionSettings = IPreferences.Current.GetPreference<Dictionary<string, object>>(ext.Name, new());
            }

            if (!extensionSettings.ContainsKey(name))
            {
                extensionSettings.Add(name, obj);
            }
            else
            {
                extensionSettings[name] = obj;
            }

            if (location == PreferenceStorageLocation.Local)
            {
                IPreferences.Current.UpdateLocalPreference(ext.Name, extensionSettings);
            }
            else
            {
                IPreferences.Current.UpdatePreference(ext.Name, extensionSettings);
            }
        }

        private object LoadPreference(Extension ext, string name, PreferenceStorageLocation location)
        {
            Dictionary<string, object> extensionSettings;

            if (location == PreferenceStorageLocation.Local)
            {
                extensionSettings = IPreferences.Current.GetLocalPreference<Dictionary<string, object>>(ext.Name, new());
            }
            else
            {
                extensionSettings = IPreferences.Current.GetPreference<Dictionary<string, object>>(ext.Name, new());
            }

            // KeyNotFoundException is handled by the SDK's Preferences
            return extensionSettings[name];
        }
    }
}
