using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Text;
using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.Changeables.Brushes;
using PixiEditor.ChangeableDocument.Changeables.Interfaces;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.ExtensionServices;
using PixiEditor.Models.Handlers;
using PixiEditor.Parser.Graph;
using PixiEditor.ViewModels.BrushSystem;
using PixiEditor.ViewModels.Nodes.Properties;
using PixiEditor.ViewModels.Tools.Tools;
using PixiEditor.ViewModels.Tools.ToolSettings.Settings;
using PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;
using Brush = PixiEditor.Models.BrushEngine.Brush;
using Color = Drawie.Backend.Core.ColorsImpl.Color;
using IBrush = Avalonia.Media.IBrush;

namespace PixiEditor.ViewModels.Document.Blackboard;

internal class VariableViewModel : ViewModelBase, IVariableHandler
{
    private Type type;
    private object value;
    private string name;
    private bool isExposed = true;
    private string? unit;
    private double min;
    private double max;

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
        this.unit = unit;
        this.min = min;
        this.max = max;

        SettingView = CreateSettingView(name, value, unit, min, max, internals);

        RemoveCommand = new RelayCommand(() =>
        {
            internals.ActionAccumulator.AddFinishedActions(
                new RemoveBlackboardVariable_Action(Name));
        });
    }

    private Setting CreateSettingView(string name, object? value, string? unit, double min, double max,
        DocumentInternalParts internals)
    {
        var view = CreateSettingFromType(this.type, unit, min, max, name);

        view.Label = view.HasIcon ? null : name;
        view.Value = AdjustValueForSetting(value);

        view.ValueChanged += (sender, args) =>
        {
            if (suppressValueChange)
                return;

            if (view.MergeChanges)
            {
                var adjustedValue = AdjustValueForBlackboard(view.Value);
                internals.ActionAccumulator.AddActions(
                    new SetBlackboardVariable_Action(Name, adjustedValue, adjustedValue?.GetType() ?? typeof(object),
                        min, max, unit, IsExposedBindable));
            }
            else
            {
                var adjustedValue = AdjustValueForBlackboard(view.Value);
                internals.ActionAccumulator.AddFinishedActions(
                    new SetBlackboardVariable_Action(Name, adjustedValue, adjustedValue?.GetType() ?? typeof(object),
                        min, max, unit, IsExposedBindable),
                    new EndSetBlackboardVariable_Action());
            }
        };

        view.MergeChangesEnded += () =>
        {
            internals.ActionAccumulator.AddFinishedActions(
                new EndSetBlackboardVariable_Action());
        };

        return view;
    }

    protected object AdjustValueForBlackboard(object value)
    {
        if (value is IBrush avaloniaBrush)
        {
            if (avaloniaBrush is SolidColorBrush solidColorBrush && Type == typeof(Color))
            {
                return solidColorBrush.Color.ToColor();
            }

            return avaloniaBrush.ToPaintable();
        }

        return value is BrushViewModel brushVm ? brushVm.Brush : value;
    }

    protected object AdjustValueForSetting(object value)
    {
        if (value is Brush brush)
        {
            return new BrushViewModel(brush);
        }

        if (value is Color color)
        {
            return new SolidColorBrush(color.ToOpaqueMediaColor());
        }

        return value;
    }

    private Setting CreateSettingFromType(Type type, string? unit, double min, double max, string name)
    {
        min = double.IsNaN(min) ? double.MinValue : min;
        max = double.IsNaN(max) ? double.MaxValue : max;

        if (type == typeof(bool))
        {
            return new BoolSettingViewModel(name, false, name);
        }

        if (type == typeof(int))
        {
            int intMin = (int)Math.Max(int.MinValue, Math.Ceiling(min));
            int intMax = (int)Math.Min(int.MaxValue, Math.Floor(max));

            if (unit == null)
            {
                return new FloatSettingViewModel(name, 0, name) { DecimalPlaces = 0, Min = intMin, Max = intMax };
            }

            return new SizeSettingViewModel(name, name) { DecimalPlaces = 0, Min = min, Max = max, Unit = unit };
        }

        if (type == typeof(double) || type == typeof(float))
        {
            if (unit == null)
            {
                return new FloatSettingViewModel(name, 0, name) { Min = (float)min, Max = (float)max };
            }

            return new SizeSettingViewModel(name, name) { Min = min, Max = max, Unit = unit };
        }

        if (type.IsAssignableTo(typeof(Brush)))
        {
            return new BrushSettingViewModel(name, name);
        }

        if (type.IsAssignableTo(typeof(DocumentReference)))
        {
            return new DocumentReferenceSettingViewModel(name);
        }

        if (type == typeof(Paintable))
        {
            return new PaintableSettingViewModel(name, name);
        }

        if (type == typeof(Color))
        {
            return new ColorSettingViewModel(name, name) { AllowGradient = false };
        }

        if (type.IsAssignableTo(typeof(Texture)))
        {
            return new TextureSettingViewModel(name, name);
        }

        if (type == typeof(string))
        {
            return new StringSettingViewModel(name, name);
        }

        if (type == (typeof(FontFamilyName)))
        {
            return new FontFamilySettingViewModel(name, name);
        }

        return new GenericSettingViewModel(name);
    }

    public void SetValueInternal(object newValue)
    {
        this.value = AdjustValueForSetting(newValue);
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
