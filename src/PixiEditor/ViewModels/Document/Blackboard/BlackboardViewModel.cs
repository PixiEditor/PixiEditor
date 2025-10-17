using System.Collections.ObjectModel;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables.Graph;
using PixiEditor.Models.Blackboard;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;

namespace PixiEditor.ViewModels.Document.Blackboard;

internal class BlackboardViewModel : ViewModelBase, IBlackboardHandler
{
    ICollection<IVariableHandler> IBlackboardHandler.Variables => Variables.Cast<IVariableHandler>().ToList();

    public ObservableCollection<VariableViewModel> Variables { get; } = new ObservableCollection<VariableViewModel>();

    private DocumentInternalParts internals;

    public BlackboardViewModel(DocumentInternalParts internals)
    {
        this.internals = internals;
    }

    internal BlackboardViewModel(DocumentInternalParts documentInternalParts, IReadOnlyBlackboard blackboard)
    {
        internals = documentInternalParts;
        foreach (var variable in blackboard.Variables)
        {
            Variables.Add(new VariableViewModel(variable.Value.Name, variable.Value.Type, variable.Value.Value,
                variable.Value.Unit, variable.Value.Min ?? double.MinValue, variable.Value.Max ?? double.MaxValue, internals));

        }
    }

    public void AddVariable(VariableDefinition definition)
    {
        if (definition == null)
        {
            return;
        }

        internals.ActionAccumulator.AddFinishedActions(
            new AddBlackboardVariable_Action(definition.UnderlyingType, definition.Min ?? double.NaN,
                definition.Max ?? double.NaN, definition.Unit));
    }

    public void AddVariableInternal(string name, Type type, object value, string? unit = null,
        double min = double.MinValue, double max = double.MaxValue)
    {
        Variables.Add(new VariableViewModel(name, type, value, unit, min, max, internals));
    }

    public IVariableHandler? GetVariable(string name)
    {
        return Variables.FirstOrDefault(v => v.Name == name);
    }

    public void SetVariableInternal(string name, object value)
    {
        VariableViewModel? variable = Variables.FirstOrDefault(v => v.Name == name);
        variable?.SetValueInternal(value);
    }

    public void RemoveVariableInternal(string name)
    {
        VariableViewModel? variable = Variables.FirstOrDefault(v => v.Name == name);
        if (variable != null)
            Variables.Remove(variable);
    }
}
