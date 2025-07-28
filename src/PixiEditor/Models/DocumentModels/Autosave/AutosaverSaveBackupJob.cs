using Avalonia.Threading;
using PixiEditor.Models.IO;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Models.DocumentModels.Autosave;

internal class AutosaverSaveBackupJob(DocumentViewModel documentToSave, int backupAttempt = 1) : IAutosaverJob
{
    public event Action? OnCompleted;
    public event Action<AutosaverSaveBackupJob, BackupAutosaveResult>? OnNonCompleted;
    public Exception? Exception { get; private set; }

    public AutosaveState CorrespondingState => AutosaveState.InProgress;

    private DispatcherTimer? waitingTimer;

    public void Start()
    {
        string filePath = documentToSave.AutosaveViewModel.AutosavePath;
        BackupAutosaveResult result = Autosave(filePath);
        if (result == BackupAutosaveResult.Success)
        {
            documentToSave.AutosaveViewModel.LastAutosavedPath = filePath;
            documentToSave.MarkAsAutosaved();
        }

        if (result == BackupAutosaveResult.Success)
            OnCompleted?.Invoke();
        else
            OnNonCompleted?.Invoke(this, result);
    }

    public void OnUpdateableChangeEnded() { }

    public void ForceStop()
    {
        waitingTimer!.Stop();
    }

    private BackupAutosaveResult Autosave(string filePath)
    {
        if (documentToSave.AllChangesSaved)
        {
            documentToSave.AutosaveViewModel.AddAutosaveHistoryEntry(AutosaveHistoryType.Periodic,
                AutosaveHistoryResult.NothingToSave);
            return BackupAutosaveResult.NothingToSave;
        }

        if (documentToSave.BlockingUpdateableChangeActive)
            return BackupAutosaveResult.BlockedByUpdateableChange;

        try
        {
            Directory.CreateDirectory(Directory.GetParent(filePath)!.FullName);

            ExportConfig config = new ExportConfig(documentToSave.SizeBindable);
            var result = Exporter.TrySave(documentToSave, filePath, config, null);

            if (result.ResultType == SaveResultType.Success)
            {
                documentToSave.MarkAsAutosaved();
                documentToSave.AutosaveViewModel.AddAutosaveHistoryEntry(AutosaveHistoryType.Periodic,
                    AutosaveHistoryResult.SavedBackup);
                return BackupAutosaveResult.Success;
            }

            Exception = new Exception($"Failed to autosave for the {backupAttempt}. time due to {result}");
            return BackupAutosaveResult.Error;
        }
        catch (Exception e)
        {
            Exception = e;
            return BackupAutosaveResult.Error;
        }
    }
}
