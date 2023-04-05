using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PixiEditor.Helpers;
using PixiEditor.Localization;
using PixiEditor.Models.UserPreferences;

namespace PixiEditor.Views.Dialogs;

public partial class LocalizationDebugWindow : Window
{
    private LocalizationDataContext dataContext;

    public LocalizationDebugWindow()
    {
        InitializeComponent();
        DataContext = dataContext = new LocalizationDataContext(this);
    }

    private void ApiKeyChanged(object sender, TextChangedEventArgs e)
    {
        dataContext.LoggedIn = false;
        dataContext.StatusMessage = "Not logged in";
    }

    private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
    {
        SystemCommands.CloseWindow(this);
    }

    private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    private class LocalizationDataContext : NotifyableObject
    {
        private readonly LocalizationDebugWindow window;
        private string apiKey;
        private bool loggedIn;
        private LocalizedString statusMessage = "NOT_LOGGED_IN";
        private string selectedLanguage;

        public DebugViewModel DebugViewModel { get; } = ViewModelMain.Current.DebugSubViewModel;

        public string ApiKey
        {
            get => apiKey;
            set
            {
                if (SetProperty(ref apiKey, value))
                {
                    PreferencesSettings.Current.UpdateLocalPreference("POEditor_API_Key", apiKey);
                }
            }
        }

        public bool LoggedIn
        {
            get => loggedIn;
            set => SetProperty(ref loggedIn, value);
        }

        public LocalizedString StatusMessage
        {
            get => statusMessage;
            set => SetProperty(ref statusMessage, value);
        }

        public string SelectedLanguage
        {
            get => selectedLanguage;
            set => SetProperty(ref selectedLanguage, value);
        }

        public ObservableCollection<string> LanguageCodes { get; } = new();

        public RelayCommand LoadApiKeyCommand { get; }

        public RelayCommand SyncLanguageCommand { get; }

        public LocalizationDataContext(LocalizationDebugWindow window)
        {
            this.window = window;
            apiKey = PreferencesSettings.Current.GetLocalPreference<string>("POEditor_API_Key");
            LoadApiKeyCommand = new RelayCommand(LoadApiKey, _ => !string.IsNullOrWhiteSpace(apiKey));
            SyncLanguageCommand =
                new RelayCommand(SyncLanguage, _ => loggedIn && !string.IsNullOrWhiteSpace(SelectedLanguage) && SelectedLanguage != "Select your language");
        }

        private void LoadApiKey(object parameter)
        {
            LanguageCodes.Clear();
            Mouse.OverrideCursor = Cursors.Wait;

            Task.Run(async () =>
            {
                try
                {
                    var result = await CheckProjectByIdAsync(ApiKey);

                    window.Dispatcher.Invoke(() =>
                    {
                        LoggedIn = result.success;
                        StatusMessage = result.message;

                        if (result.languages != null)
                        {
                            foreach (string code in result.languages)
                            {
                                LanguageCodes.Add(code);
                            }
                        }

                        if (LoggedIn)
                        {
                            SelectedLanguage = "Select your language";
                        }
                    });
                }
                finally
                {
                    window.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
                }
            });
        }

        private void SyncLanguage(object parameter)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            
            Task.Run(async () =>
            {
                try
                {
                    var result = await DownloadLanguage(ApiKey, SelectedLanguage);

                    window.Dispatcher.Invoke(() =>
                    {
                        StatusMessage = result.status;
                        DebugViewModel.Owner.LocalizationProvider.LoadDebugKeys(result.response);
                    });
                }
                finally
                {
                    window.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
                }
            });
        }

        private static async Task<(bool success, LocalizedString message, string[] languages)> CheckProjectByIdAsync(string key)
        {
            try
            {
                using HttpClient httpClient = new HttpClient();

                // --- Check if user is part of project ---
                var response = await httpClient.PostAsync("https://api.poeditor.com/v2/projects/list",
                    new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("api_token", key) }));

                // Failed with an HTTP error code, according to API docs this should not be possible
                if (!response.IsSuccessStatusCode)
                {
                    return (false, new LocalizedString("HTTP_ERROR_MESSAGE", (int)response.StatusCode, response.StatusCode), null);
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();
                JObject root = JObject.Parse(jsonResponse);

                var rsp = root["response"];
                var rspCode = rsp["code"].Value<string>();

                // Failed with an error code from the POEditor API, alongside a message
                if (rspCode != "200")
                {
                    return (false, new LocalizedString("POE_EDITOR_ERROR", rspCode, rsp["message"].Value<string>()), null);
                }

                var projects = (JArray)root["result"]["projects"];

                // Check if user is part of project
                if (!projects.Any(x => x["id"].Value<int>() == 400351))
                {
                    return (false, new LocalizedString("LOGGED_IN_NO_PROJECT_ACCESS"), null);
                }

                // --- Fetch languages ---
                response = await httpClient.PostAsync("https://api.poeditor.com/v2/languages/list",
                    new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("api_token", key),
                        new KeyValuePair<string, string>("id", "400351")
                    }));

                // Failed with an HTTP error code, according to API docs this should not be possible
                if (!response.IsSuccessStatusCode)
                {
                    return (false, new LocalizedString("HTTP_ERROR_MESSAGE", (int)response.StatusCode, response.StatusCode), null);
                }

                jsonResponse = await response.Content.ReadAsStringAsync();
                root = JObject.Parse(jsonResponse);

                rsp = root["response"];
                rspCode = rsp["code"].Value<string>();

                // Failed with an error code from the POEditor API, alongside a message
                if (rspCode != "200")
                {
                    return (false, new LocalizedString("POE_EDITOR_ERROR", rspCode, rsp["message"].Value<string>()), null);
                }

                var languages = ((JArray)root["result"]["languages"]).Select(x => x["code"].Value<string>());

                return (true, new LocalizedString("LOGGED_IN"), languages.ToArray());
            }
            catch (Exception e)
            {
                return (false, new LocalizedString("EXCEPTION_ERROR", e.Message), null);
            }
        }

        private static async Task<(LocalizedString status, Dictionary<string, string> response)> DownloadLanguage(string key,
            string language)
        {
            try
            {
                using HttpClient httpClient = new HttpClient();

                var response = await httpClient.PostAsync(
                    "https://api.poeditor.com/v2/projects/export",
                    new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("api_token", key),
                        new KeyValuePair<string, string>("id", "400351"),
                        new KeyValuePair<string, string>("type", "key_value_json"),
                        new KeyValuePair<string, string>("language", language)
                    }));


                // Failed with an HTTP error code, according to API docs this should not be possible
                if (!response.IsSuccessStatusCode)
                {
                    return (new LocalizedString("HTTP_ERROR_MESSAGE", (int)response.StatusCode, response.StatusCode), null);
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();
                JObject root = JObject.Parse(jsonResponse);

                var rsp = root["response"];
                var rspCode = rsp["code"].Value<string>();

                // Failed with an error code from the POEditor API, alongside a message
                if (rspCode != "200")
                {
                    return (new LocalizedString("POE_EDITOR_ERROR", rspCode, rsp["message"].Value<string>()), null);
                }

                var url = root["result"]["url"].Value<string>();

                response = await httpClient.GetAsync(url);

                // Failed with an HTTP error code, according to API docs this should not be possible
                if (!response.IsSuccessStatusCode)
                {
                    return (new LocalizedString("HTTP_ERROR_MESSAGE", (int)response.StatusCode, response.StatusCode), null);
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var keys = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseJson);

                return (new LocalizedString("SYNCED_SUCCESSFULLY"), keys);
            }
            catch (Exception e)
            {
                return (new LocalizedString("EXCEPTION_ERROR", e.Message), null);
            }
        }
    }
}
