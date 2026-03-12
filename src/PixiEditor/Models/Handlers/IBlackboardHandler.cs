using System.Collections.ObjectModel;
using PixiEditor.ViewModels.Document.Blackboard;

namespace PixiEditor.Models.Handlers;

public interface IBlackboardHandler
{
    public ICollection<IVariableHandler> Variables { get; }
    public void AddVariableInternal(string name, Type type, object value, string? unit = null, double min = double.MinValue, double max = double.MaxValue);
    public IVariableHandler GetVariable(string name);
    public void SetVariableInternal(string name, object value);
    public void RemoveVariableInternal(string name);
    void RenameVariableInternal(string oldName, string newName);
}
