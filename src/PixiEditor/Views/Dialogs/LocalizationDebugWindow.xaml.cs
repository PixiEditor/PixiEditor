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
                        LoggedIn = result.IsSuccess;
                        StatusMessage = result.Message;

                        if (!result.IsSuccess)
                        {
                            return;
                        }

                        foreach (string code in result.Output)
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
                        StatusMessage = result.Message;
                        DebugViewModel.Owner.LocalizationProvider.LoadDebugKeys(result.Output);
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

        private static async Task<Result<string[]>>
            CheckProjectByIdAsync(string key)
        {
            using HttpClient client = new HttpClient();

            // --- Check if user is part of project ---
            var response = await PostAsync(client, "https://api.poeditor.com/v2/projects/list", key);
            var result = await ParseResponseAsync(response);

            if (!result.IsSuccess)
            {
                return result.As<string[]>();
            }

            var projects = (JArray)result.Output["result"]["projects"];

            // Check if user is part of project
            if (!projects.Any(x => x["id"].Value<int>() == ProjectId))
            {
                return Error("LOGGED_IN_NO_PROJECT_ACCESS");
            }

            response = await PostAsync(client, "https://api.poeditor.com/v2/languages/list", key, ("id", ProjectId.ToString()));
            result = await ParseResponseAsync(response);

            if (!result.IsSuccess)
            {
                return result.As<string[]>();
            }

            var languages = ((JArray)result.Output["result"]["languages"]).Select(x => x["code"].Value<string>());

            return Result.Success(new LocalizedString("LOGGED_IN"), languages.ToArray());

            Result<string[]> Error(LocalizedString message) => Result.Error<string[]>(message);
        }

        private static async Task<Result<Dictionary<string, string>>> DownloadLanguage(
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

            var result = await ParseResponseAsync(response);

            if (!result.IsSuccess)
            {
                return result.As<Dictionary<string, string>>();
            }

            response = await client.GetAsync(result.Output["result"]["url"].Value<string>());

            // Failed with an HTTP error code, according to API docs this should not be possible
            if (!response.IsSuccessStatusCode)
            {
                return Error(new LocalizedString("HTTP_ERROR_MESSAGE", (int)response.StatusCode, response.StatusCode));
            }

            string responseJson = await response.Content.ReadAsStringAsync();
            var keys = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseJson);

            return Result.Success("SYNCED_SUCCESSFULLY", keys);

            Result<Dictionary<string, string>> Error(LocalizedString message) => Result.Error<Dictionary<string, string>>(message);
        }

        private static async Task<Result<JObject>> ParseResponseAsync(HttpResponseMessage response)
        {
            // Failed with an HTTP error code, according to API docs this should not be possible
            if (!response.IsSuccessStatusCode)
            {
                return Error("HTTP_ERROR_MESSAGE", (int)response.StatusCode, response.StatusCode);
            }

            string jsonResponse = await response.Content.ReadAsStringAsync();
            var root = JObject.Parse(jsonResponse);

            var rsp = root["response"];
            string rspCode = rsp["code"].Value<string>();

            // Failed with an error code from the POEditor API, alongside a message
            if (rspCode != "200")
            {
                return Error("POE_EDITOR_ERROR", rspCode, rsp["message"].Value<string>());
            }

            return Result.Success(root);
            
            Result<JObject> Error(string key, params object[] param) => Result.Error<JObject>(new LocalizedString(key, param));
        }

        private static Task<HttpResponseMessage> PostAsync(HttpClient client, string requestUri, string apiKey,
            params (string key, string value)[] body)
        {
            var bodyKeys = new List<KeyValuePair<string, string>>(
                body.Select(x => new KeyValuePair<string, string>(x.key, x.value))) { new("api_token", apiKey) };

            return client.PostAsync(requestUri, new FormUrlEncodedContent(bodyKeys));
        }

        private struct Result
        {
            public static Result<T> Error<T>(LocalizedString message) => new(false, message, default);

            public static Result<T> Success<T>(LocalizedString message, T output) => new(true, message, output);
            
            public static Result<T> Success<T>(T output) => new(true, null, output);
        }

        private record struct Result<T>(bool IsSuccess, LocalizedString Message, T Output)
        {
            public Result<TOther> As<TOther>()
            {
                if (IsSuccess)
                {
                    throw new ArgumentException("Result can't be a success");
                }

                return new Result<TOther>(false, Message, default);
            }
        }
    }
}
