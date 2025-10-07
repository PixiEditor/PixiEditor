using System.Collections.ObjectModel;
using PixiEditor.ChangeableDocument.Actions.Generated;
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

    public void AddVariable(Type type)
    {
        internals.ActionAccumulator.AddFinishedActions(new AddBlackboardVariable_Action(type));
    }

    public void AddVariableInternal(string name, Type type, object value)
    {
        Variables.Add(new VariableViewModel(name, type, value, internals));
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
}
