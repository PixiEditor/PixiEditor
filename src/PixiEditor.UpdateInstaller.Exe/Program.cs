using System.Diagnostics;
using System.Text;
using PixiEditor.UpdateInstaller.ViewModels;

UpdateController controller = new UpdateController();
StringBuilder log = new StringBuilder();
bool startAfterUpdate = false;
string logDirectory = Path.Combine(Path.GetTempPath(), "PixiEditor");

foreach (string arg in args)
{
    if (arg == "--startOnSuccess")
    {
        startAfterUpdate = true;
        log.AppendLine($"{DateTime.Now}: Found --startOnSuccess argument, will start PixiEditor after update.");
        break;
    }
}

try
{
    log.AppendLine($"{DateTime.Now}: Starting update installation...");
    controller.InstallUpdate(log);
    log.AppendLine($"{DateTime.Now}: Update installation completed successfully.");
}
catch (Exception ex)
{
    log.AppendLine($"{DateTime.Now}: Error during update installation: {ex.Message}");
    string errorLogPath = Path.Combine(logDirectory, "ErrorLog.txt");
    File.AppendAllText(errorLogPath, $"Error PixiEditor.UpdateInstaller: {DateTime.Now}\n{ex.Message}\n{ex.StackTrace}\n-----\n");
}
finally
{
    if (startAfterUpdate)
    {
        log.AppendLine($"{DateTime.Now}: Starting PixiEditor after update.");
        if (OperatingSystem.IsMacOS())
        {
            StartPixiEditorOnMacOS(controller);
        }
        else
        {
            string binaryName = OperatingSystem.IsWindows() ? "PixiEditor.exe" : "PixiEditor";
            var files = Directory.GetFiles(controller.UpdateDirectory, binaryName);
            if (files.Length > 0)
            {
                string pixiEditorExecutablePath = files[0];
                Process.Start(pixiEditorExecutablePath);
            }
            else
            {
                binaryName = OperatingSystem.IsWindows() ? "PixiEditor.Desktop.exe" : "PixiEditor.Desktop";
                files = Directory.GetFiles(controller.UpdateDirectory, binaryName);
                if (files.Length > 0)
                {
                    string pixiEditorExecutablePath = files[0];
                    Process.Start(pixiEditorExecutablePath);
                }
                else
                {
                    log.AppendLine("PixiEditor executable not found.");
                }
            }
        }
    }
    
    try
    {
        string updateLogPath = Path.Combine(logDirectory, "UpdateLog.txt");
        File.WriteAllText(updateLogPath, log.ToString());
    }
    catch
    {
        // probably permissions or disk full, the best we can do is to ignore this
    }
}

void StartPixiEditorOnMacOS(UpdateController controller)
{
    string pixiEditorExecutablePath = Path.Combine(controller.UpdateDirectory, "PixiEditor.app");
    if (Directory.Exists(pixiEditorExecutablePath))
    {
        log.AppendLine($"{DateTime.Now}: Starting PixiEditor with open -a PixiEditor");
        Process.Start(new ProcessStartInfo
        {
            FileName = "open",
            Arguments = $"-a PixiEditor",
        });
    }
    else
    {
        log.AppendLine($"{DateTime.Now}: PixiEditor.app not found at {pixiEditorExecutablePath}");
    }
}
