using System.Collections.ObjectModel;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
        private PoeLanguage selectedLanguage;

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

        public PoeLanguage SelectedLanguage
        {
            get => selectedLanguage;
            set => SetProperty(ref selectedLanguage, value);
        }

        public ObservableCollection<PoeLanguage> LanguageCodes { get; } = new();

        public RelayCommand LoadApiKeyCommand { get; }

        public RelayCommand SyncLanguageCommand { get; }

        public LocalizationDataContext(LocalizationDebugWindow window)
        {
            this.window = window;
            apiKey = PreferencesSettings.Current.GetLocalPreference<string>("POEditor_API_Key");
            LoadApiKeyCommand = new RelayCommand(LoadApiKey, _ => !string.IsNullOrWhiteSpace(apiKey));
            SyncLanguageCommand =
                new RelayCommand(SyncLanguage, _ => loggedIn && SelectedLanguage != null);
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

                        foreach (var language in result.Output
                                     .OrderByDescending(x => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == x.Code || CultureInfo.InstalledUICulture.TwoLetterISOLanguageName == x.Code)
                                     .ThenByDescending(x => x.UpdateSortable))
                        {
                            language.LocalEquivalent = ILocalizationProvider.Current.LocalizationData.Languages
                                .OrderByDescending(x => language.Code == x.Code)
                                .FirstOrDefault(x => language.Code.StartsWith(x.Code));
                            
                            LanguageCodes.Add(language);
                        }

                        SelectedLanguage = LanguageCodes.FirstOrDefault();
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
                    var result = await DownloadLanguage(ApiKey, SelectedLanguage.Code);

                    window.Dispatcher.Invoke(() =>
                    {
                        StatusMessage = result.Message;
                        DebugViewModel.Owner.LocalizationProvider.LoadDebugKeys(result.Output, SelectedLanguage.IsRightToLeft);
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

        private static async Task<Result<PoeLanguage[]>>
            CheckProjectByIdAsync(string key)
        {
            using HttpClient client = new HttpClient();

            // --- Check if user is part of project ---
            var response = await PostAsync(client, "https://api.poeditor.com/v2/projects/list", key);
            var result = await ParseResponseAsync(response);

            if (!result.IsSuccess)
            {
                return result.As<PoeLanguage[]>();
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
                return result.As<PoeLanguage[]>();
            }

            var languages = result.Output["result"]["languages"].ToObject<PoeLanguage[]>();

            return Result.Success(new LocalizedString("LOGGED_IN"), languages);

            Result<PoeLanguage[]> Error(LocalizedString message) => Result.Error<PoeLanguage[]>(message);
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

        public class PoeLanguage
        {
            private static readonly SolidColorBrush LocalOlder = new(Colors.Red);
            private static readonly SolidColorBrush LocalMin = new(Colors.Orange);
            private static readonly SolidColorBrush Equal = new(Colors.Lime);
            private static readonly SolidColorBrush LocalNewer = new(Colors.DodgerBlue);
            private static readonly SolidColorBrush LocalMissing = new(Colors.Gray);
            
            public string Name { get; set; }

            public string Code { get; set; }

            public string Updated { get; set; }

            public DateTimeOffset UpdateSortable => string.IsNullOrWhiteSpace(Updated) ? DateTimeOffset.MinValue : DateTimeOffset.Parse(Updated, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

            public bool IsRightToLeft => Code is "ar" or "he" or "ku" or "fa" or "ur";

            public LanguageData LocalEquivalent { get; set; }

            public SolidColorBrush Comparison => (l: LocalEquivalent?.LastUpdated, r: UpdateSortable) switch
            {
                (null, _) => LocalMissing,
                { l.Ticks: 0 } => LocalMin,
                { l: var l, r: var r } when r > l.Value => LocalOlder,
                { l: var l, r: var r } when r == l.Value => Equal,
                { l: var l, r: var r } when r < l.Value => LocalNewer,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            public override string ToString() => $"{Name} ({Code}) {UpdateSortable.ToString(CultureInfo.InvariantCulture)}";
        }
    }
}
