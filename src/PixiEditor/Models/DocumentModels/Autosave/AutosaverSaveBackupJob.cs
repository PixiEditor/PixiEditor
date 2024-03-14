using System.IO;
using System.Windows;
using System.Windows.Threading;
using PixiEditor.Models.DocumentModels.Autosave.Enums;
using PixiEditor.Models.IO;
using PixiEditor.ViewModels.SubViewModels.Document;
using PixiEditor.Views.UserControls;

namespace PixiEditor.Models.DocumentModels.Autosave;

#nullable enable
internal class AutosaverSaveBackupJob(DocumentViewModel documentToSave, int backupAttempt = 1) : IAutosaverJob
{
    public event EventHandler? OnCompleted;
    public event EventHandler<BackupAutosaveResult>? OnNonCompleted;
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
            OnCompleted?.Invoke(this, EventArgs.Empty);
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
            documentToSave.AutosaveViewModel.AddAutosaveHistoryEntry(AutosaveHistoryType.Periodic, AutosaveHistoryResult.NothingToSave);
            return BackupAutosaveResult.NothingToSave;
        }

        if (documentToSave.UpdateableChangeActive)
            return BackupAutosaveResult.BlockedByUpdateableChange;

        try
        {
            Directory.CreateDirectory(Directory.GetParent(filePath)!.FullName);
        
            var result = Exporter.TrySave(documentToSave, filePath);
        
            if (result == SaveResult.Success)
            {
                documentToSave.MarkAsAutosaved();
                documentToSave.AutosaveViewModel.AddAutosaveHistoryEntry(AutosaveHistoryType.Periodic, AutosaveHistoryResult.SavedBackup);
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
