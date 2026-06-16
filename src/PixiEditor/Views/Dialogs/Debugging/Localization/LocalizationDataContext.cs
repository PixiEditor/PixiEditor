using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Extensions.CommonApi.UserPreferences.Settings.PixiEditor;
using PixiEditor.Helpers;
using PixiEditor.Models.Dialogs;
using PixiEditor.OperatingSystem;
using PixiEditor.UI.Common.Localization;
using PixiEditor.ViewModels;
using PixiEditor.ViewModels.SubViewModels;

namespace PixiEditor.Views.Dialogs.Debugging.Localization;

internal class LocalizationDataContext : PixiObservableObject
{
    private const int ProjectId = 400351;

    private Dispatcher dispatcher;
    private string apiKey;
    private bool loggedIn;
    private LocalizedString statusMessage = "Not logged in";
    private PoeLanguage selectedLanguage;
    private bool previouslySelectedLanguageRTL = false;
    public IReadOnlyDictionary<string, string>? previouslySelectedLanguageKeys = null;
    
    public DebugViewModel DebugViewModel { get; } = ViewModelMain.Current.DebugSubViewModel;

    public string ApiKey
    {
        get => apiKey;
        set
        {
            if (SetProperty(ref apiKey, value))
            {
                PixiEditorSettings.Debug.PoEditorApiKey.Value = value;
                LoadApiKeyCommand.NotifyCanExecuteChanged();
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

    public PoeLanguage? SelectedLanguage
    {
        get => selectedLanguage;
        set
        {
            if (SetProperty(ref selectedLanguage, value))
            {
                DownloadApplyLanguageCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public IReadOnlyDictionary<string, string>? PreviouslySelectedLanguageKeys
    {
        get => previouslySelectedLanguageKeys;
        private set => SetProperty(ref previouslySelectedLanguageKeys, value);
    }

    public ObservableCollection<PoeLanguage> LanguageCodes { get; } = new();

    public RelayCommand LoadApiKeyCommand { get; }

    public RelayCommand DownloadApplyLanguageCommand { get; }
    
    public RelayCommand QuickSwitchToPreviousLanguageCommand { get; }

    public AsyncRelayCommand CopySelectedPOEditorUpdatedDateCommand { get; }
    
    public AsyncRelayCommand CopySelectedBuiltinUpdatedDateCommand { get; }
    
    public RelayCommand UpdateLanguageJsonInSourceCodeCommand { get; }

    public LocalizationDataContext()
    {
        dispatcher = Dispatcher.UIThread;
        apiKey = PixiEditorSettings.Debug.PoEditorApiKey.Value;
        LoadApiKeyCommand = new RelayCommand(LoadApiKey, () => !string.IsNullOrWhiteSpace(apiKey));
        DownloadApplyLanguageCommand =
            new RelayCommand(DownloadApplyLanguage, () => loggedIn && SelectedLanguage != null);
        CopySelectedPOEditorUpdatedDateCommand = new AsyncRelayCommand(CopySelectedPOEditorUpdatedDateAsync);
        CopySelectedBuiltinUpdatedDateCommand = new AsyncRelayCommand(CopySelectedBuiltinUpdatedDateAsync);
        UpdateLanguageJsonInSourceCodeCommand = new RelayCommand(UpdateLanguageJsonInSourceCode);
        QuickSwitchToPreviousLanguageCommand = new RelayCommand(QuickSwitchToPreviousLanguage);
    }

    private async Task CopySelectedBuiltinUpdatedDateAsync()
    {
        await Application.Current.ForDesktopMainWindowAsync(async x =>
            await x.Clipboard.SetTextAsync(
                SelectedLanguage.BuiltinEquivalent.LastUpdatedUTC.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)));
    }
    
    private async Task CopySelectedPOEditorUpdatedDateAsync()
    {
        await Application.Current.ForDesktopMainWindowAsync(async x =>
            await x.Clipboard.SetTextAsync(
                SelectedLanguage.LastUpdatedUTC.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)));
    }

    private void LoadApiKey()
    {
        LanguageCodes.Clear();

        Task.Run(async () =>
        {
            try
            {
                var result = await CheckProjectByIdAsync(ApiKey);

                dispatcher.Invoke(() =>
                {
                    LoggedIn = result.IsSuccess;
                    StatusMessage = result.Message;

                    if (!result.IsSuccess)
                    {
                        return;
                    }

                    foreach (var language in result.Output
                                 .OrderByDescending(x =>
                                     CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == x.Code ||
                                     CultureInfo.InstalledUICulture.TwoLetterISOLanguageName == x.Code)
                                 .ThenByDescending(x => x.LastUpdatedUTC))
                    {
                        language.BuiltinEquivalent = ILocalizationProvider.Current.LocalizationData.Languages
                            .OrderByDescending(x => language.Code == x.Code)
                            .FirstOrDefault(x => language.Code.StartsWith(x.Code));

                        LanguageCodes.Add(language);
                    }
                });
            }
            catch (Exception e)
            {
                LoggedIn = false;
                StatusMessage = new LocalizedString("EXCEPTION_ERROR", e.Message);
            }
        });
    }

    private void QuickSwitchToPreviousLanguage()
    {
        if (PreviouslySelectedLanguageKeys == null) 
            return;
        
        var switchingTo = PreviouslySelectedLanguageKeys.ToDictionary();
        bool switchingToRTL = previouslySelectedLanguageRTL;
        
        PreviouslySelectedLanguageKeys = DebugViewModel.Owner.LocalizationProvider.CurrentLanguage.Locale;
        previouslySelectedLanguageRTL = DebugViewModel.Owner.LocalizationProvider.CurrentLanguage.FlowDirection == FlowDirection.RightToLeft;

        dispatcher.Invoke(() =>
        {
            DebugViewModel.Owner.LocalizationProvider.LoadDebugKeys(switchingTo,
                switchingToRTL);
        });
    }

    private void DownloadApplyLanguage()
    {
        Task.Run(async () =>
        {
            try
            {
                var result = await DownloadLanguage(ApiKey, SelectedLanguage.Code);
                if (result.IsSuccess) 
                {
                    PreviouslySelectedLanguageKeys = DebugViewModel.Owner.LocalizationProvider.CurrentLanguage.Locale;
                    previouslySelectedLanguageRTL = DebugViewModel.Owner.LocalizationProvider.CurrentLanguage.FlowDirection == FlowDirection.RightToLeft;
                }

                dispatcher.Invoke(() =>
                {
                    StatusMessage = result.Message;
                    DebugViewModel.Owner.LocalizationProvider.LoadDebugKeys(result.Output,
                        SelectedLanguage.IsRightToLeft);
                });
            }
            catch (Exception e)
            {
                StatusMessage = new LocalizedString("EXCEPTION_ERROR", e.Message);
            }
        });
    }

    private void UpdateLanguageJsonInSourceCode()
    {
        if (!GetPixiEditorProjectRoot(out var localizationRoot))
        {
            return;
        }
        
        var dataPath = Path.Combine(localizationRoot, "LocalizationData.json");

        if (!File.Exists(dataPath))
        {
            NoticeDialog.Show("Localization data path not found", "ERROR");
        }

        string code = SelectedLanguage.Code;
        
        if (!GetLanguageFile(code, localizationRoot, out string languagePath))
        {
            return;
        }

        Task.Run(async () => await UpdateSourceAsync(code, languagePath, dataPath));
    }

    private async Task UpdateSourceAsync(string code, string path, string dataPath)
    {
        // Fetch latest data to make sure data is up to date
        var languages = await CheckProjectByIdAsync(apiKey);
        
        string downloadFailedErrorMessage = "Downloading language failed.\nAPI Key might have been overused.";
        if (!languages.IsSuccess)
        {
            dispatcher.Invoke(() =>
            {
                NoticeDialog.Show(downloadFailedErrorMessage + "Error:" + languages.Message, "ERROR");
            });
        }

        var language = languages.Output.First(x => x.Code == code);
        
        try
        {
            var languageData = await DownloadLanguage(apiKey, code);

            if (!languageData.IsSuccess)
            {
                dispatcher.Invoke(() =>
                {
                    NoticeDialog.Show(downloadFailedErrorMessage + "Error:" + languages.Message, "ERROR");
                });
            }
            
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(languageData.Output, JsonOptions.CasesInsensitiveIndented));
        }
        catch (Exception e)
        {
            dispatcher.Invoke(() =>
            {
                NoticeDialog.Show("Downloading language failed.\nAPI Key might have been overused.", "ERROR");
            });
        }

        dispatcher.Invoke(() =>
        {
            Application.Current.ForDesktopMainWindow(x => x.Clipboard.SetTextAsync(language.LastUpdatedUTC.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)));
            
            var dialog = new OptionsDialog<string>("SUCCESS", "Do you want to open the LocalizationData.json?\nThe updated date has been put in the clipboard.\nNote that changes wont be applied until a restart", MainWindow.Current);
            dialog["VS Code"] = _ => IOperatingSystem.Current.ProcessUtility.ShellExecute($"vscode://file/{dataPath}");
            dialog[new LocalizedString("DEFAULT")] = _ => IOperatingSystem.Current.ProcessUtility.ShellExecute(dataPath);
            dialog[new LocalizedString("CANCEL")] = null;

            dialog.ShowDialog();
        });
    }

    private static bool GetLanguageFile(string code, string root, [NotNullWhen(true)] out string? languagePath)
    {
        root = Path.Combine(root, "Languages");

        languagePath = null;
        string file;
        
        if (code.Length == 2)
        {
            file = Path.Combine(root, $"{code}.json");
            languagePath = file;
            
            if (!File.Exists(file))
            {
                File.Create(file);
            }

            return true;
        }
        
        file = Path.Combine(root, $"{code}.json");
        
        if (File.Exists(file))
        {
            languagePath = file;
            return true;
        }
        
        string file2 = Path.Combine(root, $"{code[..2]}.json");
        
        if (File.Exists(file2))
        {
            languagePath = file2;
            return true;
        }

        NoticeDialog.Show($"Language file not found.\\nLooking for {Path.GetFileName(file)} or {Path.GetFileName(file2)}", "ERROR");
        return false;
    }

    private static bool GetPixiEditorProjectRoot([NotNullWhen(true)] out string? root)
    {
        const string solutionFileName = "PixiEditor.sln";
        root = Directory.GetCurrentDirectory();

        while (root != null)
        {
            string[] files = Directory.GetFiles(root, solutionFileName, SearchOption.TopDirectoryOnly);

            if (files.Length > 0)
            {
                // Found the file in the current directory
                break;
            }

            // Move up to the parent directory
            root = Directory.GetParent(root)?.FullName;
        }

        if (!Directory.Exists(root))
        {
            NoticeDialog.Show($"PixiEditor solution root not found.\nLooking for {solutionFileName}", "ERROR");
            return false;
        }
        
        root = Path.Combine(root, "PixiEditor");
        const string projectFileName = "PixiEditor.csproj";
        if (!File.Exists(Path.Combine(root, projectFileName)))
        {
            NoticeDialog.Show($"PixiEditor project root not found.\nLooking for {projectFileName}", "ERROR");
        }
        
        root = Path.Combine(root, "Data", "Localization");
        
        if (!Directory.Exists(root))
        {
            NoticeDialog.Show("Localization folder not found.\nLooking for /Data/Localization", "ERROR");
            return false;
        }

        return true;
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

        var projects = result.Output.RootElement.GetProperty("result").GetProperty("projects").EnumerateArray();

        // Check if user is part of project
        if (projects.All(x => x.GetProperty("id").GetInt32() != ProjectId))
        {
            return Error("The given API key does not allow access to the PixiEditor project on POEditor");
        }

        response = await PostAsync(client, "https://api.poeditor.com/v2/languages/list", key,
            ("id", ProjectId.ToString()));
        result = await ParseResponseAsync(response);

        if (!result.IsSuccess)
        {
            return result.As<PoeLanguage[]>();
        }

        var languages = result.Output.RootElement.GetProperty("result")
            .GetProperty("languages")
            .EnumerateArray()
            .Select(x => x.Deserialize<PoeLanguage>(JsonOptions.CasesInsensitive))
            .ToArray();

        return Result.Success("Logged in", languages);

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

        response = await client.GetAsync(result.Output.RootElement.GetProperty("result").GetProperty("url").GetString());

        // Failed with an HTTP error code, according to API docs this should not be possible
        if (!response.IsSuccessStatusCode)
        {
            return Error($"HTTP Error: {(int)response.StatusCode} {response.StatusCode}");
        }

        string responseJson = await response.Content.ReadAsStringAsync();
        var keys = JsonSerializer.Deserialize<Dictionary<string, string>>(responseJson);

        return Result.Success("Synced successfully", keys);

        Result<Dictionary<string, string>> Error(LocalizedString message) =>
            Result.Error<Dictionary<string, string>>(message);
    }

    private static async Task<Result<JsonDocument>> ParseResponseAsync(HttpResponseMessage response)
    {
        // Failed with an HTTP error code, according to API docs this should not be possible
        if (!response.IsSuccessStatusCode)
        {
            return Error($"HTTP Error: {(int)response.StatusCode} {response.StatusCode}");
        }

        string jsonResponse = await response.Content.ReadAsStringAsync();
        var root = JsonDocument.Parse(jsonResponse);

        var rsp = root.RootElement.GetProperty("response");
        string rspCode = rsp.GetProperty("code").GetString();

        // Failed with an error code from the POEditor API, alongside a message
        if (rspCode != "200")
        {
            return Error($"POEditor Error: {rspCode} {rsp.GetProperty("message").GetString()}");
        }

        return Result.Success(root);

        Result<JsonDocument> Error(string key, params object[] param) =>
            Result.Error<JsonDocument>(new LocalizedString(key, param));
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
