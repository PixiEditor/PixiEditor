using PixiEditor.Helpers;
using PixiEditor.ViewModels.Document;

namespace PixiEditor.Models.DocumentModels.Autosave;

internal class DocumentAutosaver : IDisposable
{
    public event EventHandler? JobChanged;

    public AutosaveStateData State
    {
        get
        {
            return new AutosaveStateData
            {
                AutosaveInterval = autosavePeriod,
                AutosaveLaunchDateTime = autosaveStartedAt,
                AutosaveState = currentJob?.CorrespondingState ?? AutosaveState.Idle,
                LastBackupAutosaveData =
                    lastBackupAutosaveResult is null
                        ? null
                        : new LastBackupAutosaveData
                        {
                            Time = lastBackupAutosaveDateTime.Value, SaveResult = lastBackupAutosaveResult.Value
                        },
                LastUserFileAutosaveData = lastUserFileAutosaveResult is null
                    ? null
                    : new LastUserFileAutosaveData
                    {
                        Time = lastUserFileAutosaveDateTime.Value, SaveResult = lastUserFileAutosaveResult.Value
                    }
            };
        }
    }

    private readonly DateTime autosaveStartedAt;

    private int backupSaveFailureCount = 0;
    private int userFileSaveFailureCount = 0;

    private IAutosaverJob? currentJob;

    private IAutosaverJob? CurrentJob
    {
        get => currentJob;
        set
        {
            currentJob = value;
            JobChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private readonly bool saveUserFile;
    private readonly DocumentViewModel document;
    private readonly TimeSpan autosavePeriod;

    private bool isDisposed = false;

    private UserFileAutosaveResult? lastUserFileAutosaveResult;
    private DateTime? lastUserFileAutosaveDateTime;
    private BackupAutosaveResult? lastBackupAutosaveResult;
    private DateTime? lastBackupAutosaveDateTime;

    public DocumentAutosaver(DocumentViewModel document, TimeSpan autosavePeriod, bool saveUserFile)
    {
        this.document = document;
        this.autosavePeriod = autosavePeriod;
        this.saveUserFile = saveUserFile;
        autosaveStartedAt = DateTime.Now;

        AutosaverWaitJob initialWaitJob = new(autosavePeriod);
        initialWaitJob.OnCompleted += OnWaitingCompleted;
        StartJob(initialWaitJob);
    }

    public void OnUpdateableChangeEnded() => CurrentJob?.OnUpdateableChangeEnded();

    private void OnWaitingCompleted()
    {
        if (isDisposed)
            return;
        InitiateSaving();
    }

    private void StartJob(IAutosaverJob job)
    {
        CurrentJob = job;
        job.Start();
    }

    private void WaitForNextSave()
    {
        AutosaverWaitJob waitJob = new(autosavePeriod);
        waitJob.OnCompleted += OnWaitingCompleted;
        StartJob(waitJob);
    }

    private void InitiateSaving()
    {
        AutosaverSaveBackupJob saveBackupJob = new(document, backupSaveFailureCount + 1);
        saveBackupJob.OnCompleted += OnBackupSavingCompleted;
        saveBackupJob.OnNonCompleted += OnBackupSavingNonCompleted;
        StartJob(saveBackupJob);
    }

    private void OnBackupSavingCompleted()
    {
        if (isDisposed)
            return;

        lastBackupAutosaveResult = BackupAutosaveResult.Success;
        lastBackupAutosaveDateTime = DateTime.Now;

        backupSaveFailureCount = 0;
        if (saveUserFile)
        {
            AutosaverSaveUserFileJob saveUserFileJob = new(document);
            saveUserFileJob.OnCompleted += OnUserFileSavingCompleted;
            saveUserFileJob.OnNonCompleted += OnUserFileSavingNonCompleted;
            StartJob(saveUserFileJob);
        }
        else
        {
            WaitForNextSave();
        }
    }

    private void OnUserFileSavingCompleted()
    {
        if (isDisposed)
            return;

        lastUserFileAutosaveResult = UserFileAutosaveResult.Success;
        lastUserFileAutosaveDateTime = DateTime.Now;

        userFileSaveFailureCount = 0;
        WaitForNextSave();
    }

    private void OnBackupSavingNonCompleted(AutosaverSaveBackupJob job, BackupAutosaveResult result)
    {
        if (isDisposed)
            return;

        lastBackupAutosaveResult = result;
        lastBackupAutosaveDateTime = DateTime.Now;

        switch (result)
        {
            case BackupAutosaveResult.Error:
                backupSaveFailureCount++;
                if (backupSaveFailureCount < 3)
                    CrashHelper.SendExceptionInfo(job.Exception);
                WaitForNextSave();
                break;

            case BackupAutosaveResult.NothingToSave:
                WaitForNextSave();
                break;

            case BackupAutosaveResult.BlockedByUpdateableChange:
                AutosaverAwaitUpdateableChangeEndJob waitJob = new();
                waitJob.OnCompleted += OnAwaitUpdateableChangeCompleted;
                StartJob(waitJob);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(result), result, null);
        }
    }

    private void OnAwaitUpdateableChangeCompleted()
    {
        if (isDisposed)
            return;

        InitiateSaving();
    }

    private void OnUserFileSavingNonCompleted(AutosaverSaveUserFileJob job, UserFileAutosaveResult result)
    {
        if (isDisposed)
            return;

        lastUserFileAutosaveResult = result;
        lastUserFileAutosaveDateTime = DateTime.Now;

        switch (result)
        {
            case UserFileAutosaveResult.NoUserFile:
            case UserFileAutosaveResult.ExceptionWhileSaving:
                userFileSaveFailureCount++;
                if (userFileSaveFailureCount < 3)
                    CrashHelper.SendExceptionInfo(job.Exception);
                WaitForNextSave();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(result), result, null);
        }
    }

    public void Dispose()
    {
        CurrentJob?.ForceStop();
        isDisposed = true;
    }
}
