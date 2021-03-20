using System.Collections.ObjectModel;
using System.Linq;
using PixiEditor.Models.IO;
using PixiEditor.Models.UserPreferences;

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
            RecentlyOpenedCollection recentlyOpened = XamlAccesibleViewModel.FileSubViewModel.RecentlyOpened;

            if (!recentlyOpened.Contains(newPath))
            {
                recentlyOpened.Insert(0, newPath);
            }
            else
            {
                int index = recentlyOpened.IndexOf(newPath);
                recentlyOpened.Move(index, 0);
            }

            if (recentlyOpened.Count > IPreferences.Current.GetPreference("maxOpenedRecently", 10))
            {
                for (int i = 4; i < recentlyOpened.Count; i++)
                {
                    recentlyOpened.RemoveAt(i);
                }
            }

            IPreferences.Current.UpdateLocalPreference("RecentlyOpened", recentlyOpened.Select(x => x.FilePath));

            XamlAccesibleViewModel.FileSubViewModel.HasRecent = true;
        }
    }
}