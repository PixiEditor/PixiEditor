namespace PixiEditor.Models.IO;

public class ExportJob
{
    public int Progress { get; private set; }
    public string Status { get; private set; }
    public CancellationTokenSource CancellationTokenSource { get; set; }
    
    public event Action<int, string> ProgressChanged;
    public event Action Finished;
    public event Action Cancelled;
    
    public ExportJob()
    {
        CancellationTokenSource = new CancellationTokenSource();
    }
    
    public void Finish()
    {
        Finished?.Invoke();
    }
    
    public void Report(double progress, string status)
    {
        Progress = (int)Math.Clamp(Math.Round(progress * 100), 0, 100);
        Status = status;
        ProgressChanged?.Invoke(Progress, Status);
    }
}
