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
    private T _value;
    public T Value
    {
        get => _value;
        set
        {
            if (SetProperty(ref _value, value))
            {
                SyncBack?.Invoke(value, MergeChanges ? UpdateState.Updating : UpdateState.OneShot);
            }
        }
    }

    public bool MergeChanges
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                if (!value)
                {
                    SyncBack?.Invoke(Value, UpdateState.Finish);
                }
            }
        }
    }

    public Action<T, UpdateState> SyncBack
    {
        get;
    }


    public void SetValueWithoutSyncBack(T value)
    {
        _value = value;
        OnPropertyChanged(nameof(Value));
    }

    public PropertyViewModel(string name, Action<T, UpdateState> syncBack, T defaultValue) : base(name)
    {
        SyncBack = syncBack;
        _value = defaultValue;
    }
}

public enum UpdateState
{
    OneShot,
    Updating,
    Finish
}
