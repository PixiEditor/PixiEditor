namespace PixiEditor.ChangeableDocument.Changeables.Graph;

public class SyncGroup
{
    public List<SyncedTypeInputProperty> InputProperties { get; } = new();
    public List<SyncedTypeOutputProperty> OutputProperties { get; } = new();

    public bool MatchToBaseType { get; set; } = true;

    public void AddInput(SyncedTypeInputProperty input)
    {
        InputProperties.Add(input);
        input.ConnectionChanged += OnInputConnectionChanged;
        input.ForceUpdateType(FindCommonBaseType(InputProperties));
    }

    public void AddOutput(SyncedTypeOutputProperty output)
    {
        OutputProperties.Add(output);
        output.ForceUpdateType(FindCommonBaseType(InputProperties));
    }

    private void OnInputConnectionChanged(SyncedTypeInputProperty input)
    {
        UpdateTypes();
    }

    public void UpdateTypes()
    {
        Type newType = FindCommonBaseType(InputProperties);

        foreach (var prop in InputProperties)
        {
            prop.ForceUpdateType(newType);
        }

        foreach (var prop in OutputProperties)
        {
            prop.ForceUpdateType(newType);
        }
    }

    private Type FindCommonBaseType(List<SyncedTypeInputProperty> inputProperties)
    {
        Type commonBaseType = null;
        foreach (var input in inputProperties)
        {
            if (input.InternalProperty.Connection == null)
            {
                continue;
            }

            Type inputType = input.InternalProperty.Connection.ValueType;
            if (commonBaseType == null)
            {
                commonBaseType = inputType;
            }
            else
            {
                while (inputType != null && !commonBaseType.IsAssignableTo(inputType))
                {
                    inputType = inputType.BaseType;
                }

                if (inputType == null)
                {
                    return typeof(object);
                }

                commonBaseType = inputType;
            }
        }

        if (commonBaseType == typeof(ValueType))
        {
            return typeof(object);
        }

        return commonBaseType ?? typeof(object);
    }

    public void RemoveInput(SyncedTypeInputProperty syncedTypeInputProperty)
    {
        InputProperties.Remove(syncedTypeInputProperty);
        syncedTypeInputProperty.ConnectionChanged -= OnInputConnectionChanged;
        syncedTypeInputProperty.StopListeningToConnectionChanges();

        UpdateTypes();
    }
}
