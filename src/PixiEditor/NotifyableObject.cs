using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using PixiEditor.Exceptions;
using PixiEditor.Helpers;

namespace PixiEditor;

[Serializable]
internal class NotifyableObject : INotifyPropertyChanged
{
    private static Thread uiThread;
    
    [field: NonSerialized]
    public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

    public void AddPropertyChangedCallback(string propertyName, Action action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(propertyName));
        }

        if (string.IsNullOrWhiteSpace(propertyName))
        {
            PropertyChanged += (_, _) => action();
            return;
        }

        PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == propertyName)
            {
                action();
            }
        };
    }

    public void RaisePropertyChanged(string property)
    {
        ReportNonUiThread(property);
        
        if (property != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }

    protected bool SetProperty<T>(ref T backingStore, T value, Action beforeChange = null, [CallerMemberName] string propertyName = "") =>
        SetProperty(ref backingStore, value, out _, beforeChange, propertyName);

    protected bool SetProperty<T>(ref T backingStore, T value, out T oldValue, Action beforeChange = null, [CallerMemberName] string propertyName = "")
    {
        ReportNonUiThread(propertyName);
        
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
        {
            oldValue = backingStore;
            return false;
        }

        beforeChange?.Invoke();
        oldValue = backingStore;
        backingStore = value;
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }

    private static void ReportNonUiThread(string property, [CallerMemberName] string methodName = "")
    {
        uiThread ??= Application.Current.Dispatcher.Thread;
        var currentThread = Thread.CurrentThread;

        if (currentThread == uiThread)
        {
            return;
        }

        var exception = new CustomStackTraceException($"Do not call {methodName}() from a thread other than the UI thread. Calling for property '{property ?? "{ property name null }"}'. (Calling from '{currentThread.Name ?? "{ name null }"}' @{currentThread.ManagedThreadId})");
        
#if DEBUG
        throw exception;
#else
        exception.GenerateStackTrace();
        Task.Run(() => CrashHelper.SendExceptionInfoToWebhook(exception));
#endif
    }
}
