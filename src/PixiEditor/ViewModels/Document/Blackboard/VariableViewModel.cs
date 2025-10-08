using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;
using PixiEditor.ViewModels.Tools.ToolSettings.Settings;
using PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;

namespace PixiEditor.ViewModels.Document.Blackboard;

internal class VariableViewModel : ViewModelBase, IVariableHandler
{
    private Type type;
    private object value;
    private string name;

    public Type Type
    {
        get => type;
    }

    public object Value
    {
        get => value;
    }

    public string Name
    {
        get => name;
        set
        {
            internals.ActionAccumulator.AddFinishedActions(
                new RenameBlackboardVariable_Action(name, value));
        }
    }

    public Setting SettingView { get; }

    private bool suppressValueChange;

    private DocumentInternalParts internals { get; }

    public VariableViewModel(string name, Type type, object value, DocumentInternalParts internals)
    {
        this.type = type;
        this.name = name;
        this.value = value;
        this.internals = internals;

        SettingView = CreateSettingFromType(type);

        SettingView.Label = name;
        SettingView.Value = value;

        SettingView.ValueChanged += (sender, args) =>
        {
            if (suppressValueChange)
                return;

            internals.ActionAccumulator.AddFinishedActions(
                new SetBlackboardVariable_Action(name, SettingView.Value));
        };
    }

    private Setting CreateSettingFromType(Type type)
    {
        if (type == typeof(bool))
        {
            return new BoolSettingViewModel("Variable", false, "Variable");
        }
        else if (type == typeof(int))
        {
            return new SizeSettingViewModel("Variable", "Variable") { DecimalPlaces = 0 };
        }
        else if (type == typeof(double))
        {
            return new SizeSettingViewModel("Variable", "Variable");
        }
        else if (type == typeof(float))
        {
            return new FloatSettingViewModel("Variable", 0, "Variable");
        }
        else
        {
            throw new NotSupportedException($"Type {type} is not supported for VariableViewModel.");
        }
    }

    public void SetValueInternal(object newValue)
    {
        this.value = newValue;
        suppressValueChange = true;

        SettingView.Value = value;

        suppressValueChange = false;
    }

    public void SetNameInternal(string newName)
    {
        name = newName;

        SettingView.Label = newName;
        OnPropertyChanged(nameof(Name));
    }
}
