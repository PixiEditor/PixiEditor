using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Threading;
using PixiEditor.OperatingSystem;

namespace PixiEditor.Linux;

public sealed class LinuxOperatingSystem : IOperatingSystem
{
    public string Name { get; } = "Linux";
    public string AnalyticsId => "Linux";
    public string AnalyticsName => LinuxOSInformation.FromReleaseFile().ToString();
    public IInputKeys InputKeys { get; }
    public IProcessUtility ProcessUtility { get; }
    
    public void OpenUri(string uri)
    {
        throw new NotImplementedException();
    }

    public void OpenFolder(string path)
    {
        throw new NotImplementedException();
    }

    public bool HandleNewInstance(Dispatcher? dispatcher, Action<string> openInExistingAction, IApplicationLifetime lifetime)
    {
        return true;
    }

    class LinuxOSInformation
    {
        const string FilePath = "/etc/os-release";
        
        private LinuxOSInformation(string? name, string? version, bool available)
        {
            Name = name;
            Version = version;
            Available = available;
        }

        public static LinuxOSInformation FromReleaseFile()
        {
            if (!File.Exists(FilePath))
            {
                return new LinuxOSInformation(null, null, false);
            }
            
            var lines = File.ReadAllLines(FilePath).Select<string, (string Key, string Value)>(x =>
            {
                var separatorIndex = x.IndexOf('=');
                return (x[..separatorIndex], x[(separatorIndex + 1)..]);
            }).ToList();
            
            var name = lines.FirstOrDefault(x => x.Key == "NAME").Value.Trim('"');
            var version = lines.FirstOrDefault(x => x.Key == "VERSION").Value.Trim('"');
            
            return new LinuxOSInformation(name, version, true);
        }
        
        public bool Available { get; }
        
        public string? Name { get; private set; }
        
        public string? Version { get; private set; }

        public override string ToString() => $"{Name} {Version}";
    }
}
