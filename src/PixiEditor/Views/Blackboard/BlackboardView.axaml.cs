using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Numerics;
using Drawie.Numerics;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes;
using PixiEditor.ChangeableDocument.Changeables.Graph.Nodes.Shapes.Data;
using PixiEditor.Models.Blackboard;
using PixiEditor.Models.BrushEngine;
using PixiEditor.ViewModels.Document.Blackboard;

namespace PixiEditor.Views.Blackboard;

internal partial class BlackboardView : UserControl
{
    public static readonly StyledProperty<ObservableCollection<VariableViewModel>> VariablesProperty =
        AvaloniaProperty.Register<BlackboardView, ObservableCollection<VariableViewModel>>(
            nameof(Variables));

    public ObservableCollection<VariableViewModel> Variables
    {
        get => GetValue(VariablesProperty);
        set => SetValue(VariablesProperty, value);
    }

    public static readonly StyledProperty<ICommand> AddVariableCommandProperty =
        AvaloniaProperty.Register<BlackboardView, ICommand>(
            nameof(AddVariableCommand));

    public ICommand AddVariableCommand
    {
        get => GetValue(AddVariableCommandProperty);
        set => SetValue(AddVariableCommandProperty, value);
    }

    public static readonly StyledProperty<VariableDefinition> SelectedVariableOptionProperty =
        AvaloniaProperty.Register<BlackboardView, VariableDefinition>(
            nameof(SelectedVariableOption));

    public VariableDefinition SelectedVariableOption
    {
        get => GetValue(SelectedVariableOptionProperty);
        set => SetValue(SelectedVariableOptionProperty, value);
    }

    public ObservableCollection<VariableDefinition> VariableOptions { get; } =
        new ObservableCollection<VariableDefinition>
        {
            new VariableDefinition("DECIMAL_NUMBER", typeof(double)) { Min = double.MinValue, Max = double.MaxValue },
            new VariableDefinition("SIZE", typeof(double), "px"),
            new VariableDefinition("PERCENTAGE", typeof(double), "%", 0, 100),
            new VariableDefinition("WHOLE_NUMBER", typeof(int)),
            new VariableDefinition("VECTOR", typeof(VecD)), // TODO: Picker
            new VariableDefinition("WHOLE_NUM_VECTOR", typeof(VecI)), // TODO: Picker
            new VariableDefinition("TEXT", typeof(string)), // TODO: Picker
            new VariableDefinition("MATRIX", typeof(Matrix3X3)),
            new VariableDefinition("BOOLEAN", typeof(bool)),
            new VariableDefinition("BRUSH", typeof(Brush)),
            new VariableDefinition("PAINTABLE", typeof(Paintable)),
            new VariableDefinition("COLOR", typeof(Color)),
            new VariableDefinition("TEXTURE", typeof(Texture)),
            new VariableDefinition("PAINTER", typeof(Painter)),
            new VariableDefinition("VECTOR_PATH", typeof(PathVectorData)),
            new VariableDefinition("ANY", typeof(object)),
        };


    public BlackboardView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        if (SelectedVariableOption == null && VariableOptions.Count > 0)
            SelectedVariableOption = VariableOptions[0];
    }
}
