﻿using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;

namespace PixiEditor.OperatingSystem;

public interface IOperatingSystem
{
    public static IOperatingSystem Current { get; protected set; }
    public string Name { get; }

    public virtual string AnalyticsName => Environment.OSVersion.ToString();
    public string AnalyticsId { get; }

    public IInputKeys InputKeys { get; }
    public IProcessUtility ProcessUtility { get; }

    public static void RegisterOS(IOperatingSystem operatingSystem)
    {
        if (Current != null)
        {
            throw new InvalidOperationException("Current operating system is already set");
        }

        Current = operatingSystem;
    }

    public void OpenUri(string uri);
    public void OpenFolder(string path);
    public bool HandleNewInstance(Dispatcher? dispatcher, Action<string> openInExistingAction, IApplicationLifetime lifetime);
}
