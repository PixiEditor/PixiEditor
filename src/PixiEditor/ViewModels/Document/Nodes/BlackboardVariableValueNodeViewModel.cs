using System.Collections.ObjectModel;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ViewModels.Nodes;
using PixiEditor.ViewModels.Nodes.Properties;

namespace PixiEditor.ViewModels.Document.Nodes;

[NodeViewModel("BLACKBOARD_VARIABLE_VALUE", null, PixiPerfectIcons.Variable)]
internal class BlackboardVariableValueNodeViewModel : NodeViewModel<BlackboardVariableValueNode>
{
    public override void OnInitialized()
    {
        Document.NodeGraph.Blackboard.VariableAdded += BlackboardOnVariableAdded;
        Document.NodeGraph.Blackboard.VariableRemoved += BlackboardOnVariableRemoved;
        Document.NodeGraph.Blackboard.VariableRenamed += BlackboardOnVariableRenamed;

        if (InputPropertyMap[BlackboardVariableValueNode.NameProperty] is StringPropertyViewModel
            stringPropertyViewModel)
        {
            stringPropertyViewModel.AvailableOptions =
                new ObservableCollection<string>(Internals.Tracker.Document.Blackboard.Variables.Keys);
        }
    }

    private void BlackboardOnVariableAdded(string obj)
    {
        if (InputPropertyMap[BlackboardVariableValueNode.NameProperty] is StringPropertyViewModel
            stringPropertyViewModel)
        {
            if (!stringPropertyViewModel.AvailableOptions!.Contains(obj))
            {
                stringPropertyViewModel.AvailableOptions?.Add(obj);
            }
        }
    }

    private void BlackboardOnVariableRemoved(string obj)
    {
        if (InputPropertyMap[BlackboardVariableValueNode.NameProperty] is StringPropertyViewModel
            stringPropertyViewModel)
        {
            stringPropertyViewModel.AvailableOptions?.Remove(obj);
        }
    }

    private void BlackboardOnVariableRenamed(string oldName, string newName)
    {
        if (InputPropertyMap[BlackboardVariableValueNode.NameProperty] is StringPropertyViewModel
            stringPropertyViewModel)
        {
            var options = stringPropertyViewModel.AvailableOptions;
            if (options == null)
                return;

            var index = options.IndexOf(oldName);
            if (index >= 0)
            {
                options[index] = newName;
            }
        }
    }
}
