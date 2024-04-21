using System.IO;
using System.Text;
using System.Web;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PixiEditor.AvaloniaUI.Models.ExceptionHandling;
using PixiEditor.AvaloniaUI.ViewModels;
using PixiEditor.OperatingSystem;

namespace PixiEditor.AvaloniaUI.Views.Dialogs;

internal partial class SendCrashReportDialog : PixiEditorPopup
{
    const string DiscordInviteLink = "https://discord.gg/eh8gx6vNEp";

    private readonly CrashReport report;

    public SendCrashReportDialog(CrashReport report)
    {
        this.report = report;
        InitializeComponent();
    }

    private void OpenInExplorer(object sender, RoutedEventArgs e)
    {
        string tempPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PixiEditor",
            "crash_logs",
            "to-copy");

        DirectoryInfo info = Directory.CreateDirectory(tempPath);

        foreach (var file in info.EnumerateFiles())
        {
            file.Delete();
        }

        File.Copy(report.FilePath, Path.Combine(tempPath, Path.GetFileName(report.FilePath)), true);

        IOperatingSystem.Current.ProcessUtility.ShellExecute(tempPath);
    }

    private void OpenHyperlink(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var tag = button.Tag as string;

        string body =
            HttpUtility.UrlEncode($"** IMPORTANT: Drop the \"{Path.GetFileName(report.FilePath)}\" file in here **");

        var result = tag switch
        {
            "github" => GetGitHubLink(),
            "discord" => DiscordInviteLink,
            "email" => GetMailtoLink(),
            _ => throw new NotImplementedException()
        };

        OpenInExplorer(null, null);
        IOperatingSystem.Current.ProcessUtility.ShellExecute(result);

        string GetGitHubLink()
        {
            StringBuilder builder = new();

            builder.Append("https://github.com/PixiEditor/PixiEditor/issues/new?title=");
            builder.Append(HttpUtility.UrlEncode($"Crash Report"));
            builder.Append("&body=");
            builder.Append(body);

            return builder.ToString();
        }

        string GetMailtoLink()
        {
            StringBuilder builder = new();

            builder.Append("mailto:pixieditorproject@gmail.com?subject=");
            builder.Append(HttpUtility.UrlEncode($"Crash Report"));
            builder.Append("&body=");
            builder.Append(body);

            return builder.ToString();
        }
    }
}
