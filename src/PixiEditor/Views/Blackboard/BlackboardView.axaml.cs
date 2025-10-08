using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PixiEditor.Models.Blackboard;
using PixiEditor.ViewModels.Document.Blackboard;

namespace PixiEditor.Views.Blackboard;

internal partial class BlackboardView : UserControl
{
    public static readonly StyledProperty<ObservableCollection<VariableViewModel>> VariablesProperty = AvaloniaProperty.Register<BlackboardView, ObservableCollection<VariableViewModel>>(
        nameof(Variables));

    public ObservableCollection<VariableViewModel> Variables
    {
        get => GetValue(VariablesProperty);
        set => SetValue(VariablesProperty, value);
    }

    public static readonly StyledProperty<ICommand> AddVariableCommandProperty = AvaloniaProperty.Register<BlackboardView, ICommand>(
        nameof(AddVariableCommand));

    public ICommand AddVariableCommand
    {
        get => GetValue(AddVariableCommandProperty);
        set => SetValue(AddVariableCommandProperty, value);
    }

    public ObservableCollection<VariableDefinition> VariableOptions { get; } = new ObservableCollection<VariableDefinition>
    {
        new VariableDefinition("DECIMAL_NUMBER", typeof(double)) { Min = double.MinValue, Max = double.MaxValue },
        new VariableDefinition("SIZE", typeof(double), "px"),
        new VariableDefinition("PERCENTAGE", typeof(double), "%", 0, 100),
        new VariableDefinition("WHOLE_NUMBER", typeof(int)),
        new VariableDefinition("BOOLEAN", typeof(bool)),
    };

    public BlackboardView()
    {
        InitializeComponent();
    }
}
