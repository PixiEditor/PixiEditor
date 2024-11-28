using Avalonia.Controls;
using Avalonia.Threading;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.IO;

namespace PixiEditor.Views.Dialogs;

internal class ProgressDialog : CustomDialog
{
    public ExportJob Job { get; }

    private ProgressPopup popup;

    public ProgressDialog(ExportJob job, Window ownerWindow) : base(ownerWindow)
    {
        Job = job;
        Job.ProgressChanged += (progress, status) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                popup.Progress = progress;
                popup.Status = status;
            });
        };

        Job.Finished += () =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                popup.Close();
            });
        };

        Job.Cancelled += () =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                popup.Close();
            });
        };
    }

    public override async Task<bool> ShowDialog()
    {
        popup = new ProgressPopup();
        popup.CancellationToken = Job.CancellationTokenSource;
        return await popup.ShowDialog<bool>(OwnerWindow);
    }
}
