using Avalonia.Threading;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Models.DocumentModels.Autosave;

internal class AutosaverSaveUserFileJob(DocumentViewModel document) : IAutosaverJob
{
    public event Action OnCompleted;
    public event Action<AutosaverSaveUserFileJob, UserFileAutosaveResult>? OnNonCompleted;
    public Exception? Exception { get; private set; }

    public AutosaveState CorrespondingState => AutosaveState.InProgress;

    private DispatcherTimer? waitingTimer;

    public async void Start()
    {
        UserFileAutosaveResult result;
        try
        {
            result = await Task.Run(Copy);
        }
        catch (Exception e)
        {
            result = UserFileAutosaveResult.ExceptionWhileSaving;
            Exception = e;
        }

        if (result == UserFileAutosaveResult.Success)
        {
            document.MarkAsSaved();
        }

        if (result == UserFileAutosaveResult.Success)
            OnCompleted?.Invoke();
        else
            OnNonCompleted?.Invoke(this, result);

        UserFileAutosaveResult Copy()
        {
            try
            {
                string path = document.FullFilePath;
                if (!File.Exists(path))
                    return UserFileAutosaveResult.NoUserFile;

                File.Copy(document.AutosaveViewModel.LastAutosavedPath, path, true);

                document.MarkAsSaved();
                document.AutosaveViewModel.AddAutosaveHistoryEntry(AutosaveHistoryType.Periodic, AutosaveHistoryResult.SavedUserFile);
                return UserFileAutosaveResult.Success;
            }
            catch (Exception e) when (e is UnauthorizedAccessException or DirectoryNotFoundException)
            {
                return UserFileAutosaveResult.NoUserFile;
            }
            catch (Exception e)
            {
                Exception = e;
                return UserFileAutosaveResult.ExceptionWhileSaving;
            }
        }
    }

    public void OnUpdateableChangeEnded() { }
    public void ForceStop()
    {
        waitingTimer!.Stop();
    }
}
