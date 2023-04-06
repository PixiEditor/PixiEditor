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
        dataContext.StatusMessage = "NOT_LOGGED_IN";
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
        private const int ProjectId = 400351;

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
                new RelayCommand(SyncLanguage, _ => loggedIn && !string.IsNullOrWhiteSpace(SelectedLanguage));
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

                        if (result.languages == null)
                        {
                            return;
                        }

                        foreach (string code in result.languages)
                        {
                            LanguageCodes.Add(code);
                        }
                    });
                }
                catch (Exception e)
                {
                    LoggedIn = false;
                    StatusMessage = new LocalizedString("EXCEPTION_ERROR", e.Message);
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
                catch (Exception e)
                {
                    StatusMessage = new LocalizedString("EXCEPTION_ERROR", e.Message);
                }
                finally
                {
                    window.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
                }
            });
        }

        private static async Task<(bool success, LocalizedString message, string[] languages)>
            CheckProjectByIdAsync(string key)
        {
            using HttpClient client = new HttpClient();

            // --- Check if user is part of project ---
            var response = await PostAsync(client, "https://api.poeditor.com/v2/projects/list", key);
            var rootError = await ParseResponseAsync(response);

            if (rootError.IsT1)
            {
                return Error(rootError.AsT1);
            }

            var projects = (JArray)rootError.AsT0["result"]["projects"];

            // Check if user is part of project
            if (!projects.Any(x => x["id"].Value<int>() == ProjectId))
            {
                return Error("LOGGED_IN_NO_PROJECT_ACCESS");
            }

            response = await PostAsync(client, "https://api.poeditor.com/v2/languages/list", key, ("id", ProjectId.ToString()));
            rootError = await ParseResponseAsync(response);

            if (rootError.IsT1)
            {
                return Error(rootError.AsT1);
            }

            var languages = ((JArray)rootError.AsT0["result"]["languages"]).Select(x => x["code"].Value<string>());

            return (true, new LocalizedString("LOGGED_IN"), languages.ToArray());

            (bool success, LocalizedString message, string[] languages) Error(LocalizedString message) =>
                (false, message, null);
        }

        private static async Task<(LocalizedString status, Dictionary<string, string> response)> DownloadLanguage(
            string key,
            string language)
        {
            using var client = new HttpClient();

            // Get Url to key_value_json in language
            var response = await PostAsync(
                client,
                "https://api.poeditor.com/v2/projects/export",
                key,
                ("id", ProjectId.ToString()), ("type", "key_value_json"), ("language", language));

            var rootError = await ParseResponseAsync(response);

            if (rootError.IsT1)
            {
                return Error(rootError.AsT1);
            }

            response = await client.GetAsync(rootError.AsT0["result"]["url"].Value<string>());

            // Failed with an HTTP error code, according to API docs this should not be possible
            if (!response.IsSuccessStatusCode)
            {
                return Error(new LocalizedString("HTTP_ERROR_MESSAGE", (int)response.StatusCode, response.StatusCode));
            }

            string responseJson = await response.Content.ReadAsStringAsync();
            var keys = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseJson);

            return (new LocalizedString("SYNCED_SUCCESSFULLY"), keys);

            (LocalizedString, Dictionary<string, string>) Error(LocalizedString message) => (message, null);
        }

        private static async Task<OneOf<JObject, LocalizedString>> ParseResponseAsync(HttpResponseMessage response)
        {
            // Failed with an HTTP error code, according to API docs this should not be possible
            if (!response.IsSuccessStatusCode)
            {
                return new LocalizedString("HTTP_ERROR_MESSAGE", (int)response.StatusCode, response.StatusCode);
            }

            string jsonResponse = await response.Content.ReadAsStringAsync();
            var root = JObject.Parse(jsonResponse);

            var rsp = root["response"];
            string rspCode = rsp["code"].Value<string>();

            // Failed with an error code from the POEditor API, alongside a message
            if (rspCode != "200")
            {
                return new LocalizedString("POE_EDITOR_ERROR", rspCode, rsp["message"].Value<string>());
            }

            return root;
        }

        private static Task<HttpResponseMessage> PostAsync(HttpClient client, string requestUri, string apiKey,
            params (string key, string value)[] body)
        {
            var bodyKeys = new List<KeyValuePair<string, string>>(
                body.Select(x => new KeyValuePair<string, string>(x.key, x.value))) { new("api_token", apiKey) };

            return client.PostAsync(requestUri, new FormUrlEncodedContent(bodyKeys));
        }
    }
}
