using CommunityToolkit.Mvvm.ComponentModel;

namespace PixiEditor.ViewModels.Document.Properties;

internal abstract class PropertyViewModel : ObservableObject
{
    public string Name
    {
        get;
    }

    public string Label
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool IsReadOnly
    {
        get;
        set => SetProperty(ref field, value);
    }

    public PropertyViewModel(string name)
    {
        Name = name;
    }
}

internal abstract class PropertyViewModel<T> : PropertyViewModel
{
    public T Value
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                SyncBack?.Invoke(value, MergeChanges ? UpdateState.Updating : UpdateState.Finish);
            }
        }
    }

    public bool MergeChanges
    {
        get;
        set
        {
            if (SetProperty(ref field, value) && !value)
            {
                SyncBack?.Invoke(Value, UpdateState.Finish);
            }
        }
    }

    public Action<T, UpdateState> SyncBack
    {
        get;
    }


    public PropertyViewModel(string name, Action<T, UpdateState> syncBack) : base(name)
    {
        SyncBack = syncBack;
    }
}

public enum UpdateState
{
    Updating,
    Finish
}
