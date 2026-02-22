using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Text;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables.Brushes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.Models.BrushEngine;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;
using PixiEditor.Parser.Graph;
using PixiEditor.ViewModels.BrushSystem;
using PixiEditor.ViewModels.Tools.Tools;
using PixiEditor.ViewModels.Tools.ToolSettings.Settings;
using PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;

namespace PixiEditor.ViewModels.Document.Blackboard;

internal class VariableViewModel : ViewModelBase, IVariableHandler
{
    private Type type;
    private object value;
    private string name;
    private bool isExposed = true;

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
    public ICommand RemoveCommand { get; }

    public bool IsExposedBindable
    {
        get => isExposed;
        set
        {
            if (value != IsExposedBindable)
            {
                internals.ActionAccumulator.AddFinishedActions(
                    new SetBlackboardVariableExposed_Action(Name, value));
            }
        }
    }

    public VariableViewModel(string name, Type type, object value, string? unit, double min, double max,
        DocumentInternalParts internals)
    {
        this.type = type;
        if (type == typeof(object))
        {
            this.type = value?.GetType() ?? typeof(object);
        }

        this.name = name;
        this.value = value;
        this.internals = internals;

        SettingView = CreateSettingFromType(this.type, unit, min, max);

        SettingView.Label = name;
        SettingView.Value = value;

        SettingView.ValueChanged += (sender, args) =>
        {
            if (suppressValueChange)
                return;

            if (SettingView.MergeChanges)
            {
                var adjustedValue = AdjustValueForBlackboard(SettingView.Value);
                internals.ActionAccumulator.AddActions(
                    new SetBlackboardVariable_Action(Name, adjustedValue, adjustedValue?.GetType() ?? typeof(object), min, max, unit, IsExposedBindable));
            }
            else
            {
                var adjustedValue = AdjustValueForBlackboard(SettingView.Value);
                internals.ActionAccumulator.AddFinishedActions(
                    new SetBlackboardVariable_Action(Name, adjustedValue, adjustedValue?.GetType() ?? typeof(object), min, max, unit, IsExposedBindable),
                    new EndSetBlackboardVariable_Action());
            }
        };

        SettingView.MergeChangesEnded += () =>
        {
            internals.ActionAccumulator.AddFinishedActions(
                new EndSetBlackboardVariable_Action());
        };

        RemoveCommand = new RelayCommand(() =>
        {
            internals.ActionAccumulator.AddFinishedActions(
                new RemoveBlackboardVariable_Action(Name));
        });
    }

    protected virtual object AdjustValueForBlackboard(object value)
    {
        return value is BrushViewModel brushVm ? brushVm.Brush : value;
    }

    private Setting CreateSettingFromType(Type type, string? unit, double min, double max)
    {
        min = double.IsNaN(min) ? double.MinValue : min;
        max = double.IsNaN(max) ? double.MaxValue : max;

        if (type == typeof(bool))
        {
            return new BoolSettingViewModel("Variable", false, "Variable");
        }

        if (type == typeof(int))
        {
            int intMin = (int)Math.Max(int.MinValue, Math.Ceiling(min));
            int intMax = (int)Math.Min(int.MaxValue, Math.Floor(max));

            if (unit == null)
            {
                return new FloatSettingViewModel("Variable", 0, "Variable")
                {
                    DecimalPlaces = 0, Min = intMin, Max = intMax
                };
            }

            return new SizeSettingViewModel("Variable", "Variable")
            {
                DecimalPlaces = 0, Min = min, Max = max, Unit = unit
            };
        }

        if (type == typeof(double) || type == typeof(float))
        {
            if (unit == null)
            {
                return new FloatSettingViewModel("Variable", 0, "Variable") { Min = (float)min, Max = (float)max };
            }

            return new SizeSettingViewModel("Variable", "Variable") { Min = min, Max = max, Unit = unit };
        }

        if (type.IsAssignableTo(typeof(Brush)))
        {
            return new BrushSettingViewModel("Variable", "Variable");
        }

        if (type == typeof(Paintable))
        {
            return new PaintableSettingViewModel("Variable", "Variable");
        }
        
        if (type == typeof(Color))
        {
            return new ColorSettingViewModel("Variable", "Variable") { AllowGradient = false };
        }

        if (type.IsAssignableTo(typeof(Texture)))
        {
            return new TextureSettingViewModel("Variable", "Variable");
        }

        if(type == typeof(string))
        {
            return new StringSettingViewModel("Variable", "Variable");
        }

        if (type == (typeof(FontFamilyName)))
        {
            return new FontFamilySettingViewModel("Variable", "Variable");
        }

        return new GenericSettingViewModel("Variable");
    }

    public void SetValueInternal(object newValue)
    {
        if(SettingView is BrushSettingViewModel brushSetting && newValue is Brush brush)
        {
            newValue = new BrushViewModel(brush);
        }

        this.value = newValue;
        suppressValueChange = true;

        SettingView.Value = value;

        suppressValueChange = false;
    }

    internal void SetNameInternal(string newName)
    {
        name = newName;

        SettingView.Label = newName;
        OnPropertyChanged(nameof(Name));
    }

    internal void SetIsExposedInternal(bool infoValue)
    {
        isExposed = infoValue;
        OnPropertyChanged(nameof(IsExposedBindable));
    }
}
