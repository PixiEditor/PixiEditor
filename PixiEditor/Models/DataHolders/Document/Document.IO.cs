using System.Collections.ObjectModel;
using PixiEditor.Models.IO;

namespace PixiEditor.Models.DataHolders
{
    public partial class Document
    {
        private string documentFilePath = string.Empty;

        public string DocumentFilePath
        {
            get => documentFilePath;
            set
            {
                documentFilePath = value;
                RaisePropertyChanged(nameof(DocumentFilePath));
                RaisePropertyChanged(nameof(Name));
                UpdateRecentlyOpened(value);
            }
        }

        private bool changesSaved = true;

        public bool ChangesSaved
        {
            get => changesSaved;
            set
            {
                changesSaved = value;
                RaisePropertyChanged(nameof(ChangesSaved));
                RaisePropertyChanged(nameof(Name)); // This updates name so it shows asterisk if unsaved
            }
        }

        public void SaveWithDialog()
        {
            bool savedSuccessfully = Exporter.SaveAsEditableFileWithDialog(this, out string path);
            DocumentFilePath = path;
            ChangesSaved = savedSuccessfully;
        }

        public void Save()
        {
            Save(DocumentFilePath);
        }

        public void Save(string path)
        {
            DocumentFilePath = Exporter.SaveAsEditableFile(this, path);
            ChangesSaved = true;
        }

        private void UpdateRecentlyOpened(string newPath)
        {
            ObservableCollection<string> recentlyOpened = XamlAccesibleViewModel.FileSubViewModel.RecentlyOpened;

            if (!recentlyOpened.Contains(newPath))
            {
                recentlyOpened.Insert(0, newPath);
            }

            if (recentlyOpened.Count > 5)
            {
                for (int i = 4; i < recentlyOpened.Count; i++)
                {
                    recentlyOpened.RemoveAt(i);
                }
            }

            UserPreferences.PreferencesSettings.UpdatePreference("RecentlyOpened", recentlyOpened);
        }

    }
}