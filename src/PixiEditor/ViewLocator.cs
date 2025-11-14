using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.ComponentModel;
using PixiDocks.Core.Docking;
using PixiEditor.ViewModels.Dock;
using PixiEditor.ViewModels.Nodes.Properties;
using PixiEditor.ViewModels.SubViewModels;
using PixiEditor.ViewModels.Tools.ToolSettings.Settings;
using PixiEditor.Views.Dock;
using PixiEditor.Views.Layers;
using PixiEditor.Views.Nodes.Properties;
using PixiEditor.Views.Tools.ToolSettings.Settings;

namespace PixiEditor;

public class ViewLocator : IDataTemplate
{
    public static Dictionary<Type, Type> ViewBindingsMap = new Dictionary<Type, Type>()
    {
        [typeof(ViewportWindowViewModel)] = typeof(DocumentTemplate),
        [typeof(LazyViewportWindowViewModel)] = typeof(LazyDocumentTemplate),
        [typeof(LayersDockViewModel)] = typeof(LayersManager),
        [typeof(SinglePropertyViewModel)] = typeof(DoublePropertyView),
        [typeof(PaintableSettingViewModel)] = typeof(ColorSettingView)
    };

    public Control Build(object? data)
    {
        Type dataType = data.GetType();
        var name = dataType.FullName.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type);
        }

        if (dataType.IsGenericType)
        {
            string nameWithoutGeneric = data.GetType().FullName.Split('`')[0];
            name = nameWithoutGeneric.Replace("ViewModel", "View");
            type = Type.GetType(name);
            
            if (type != null)
            {
                return (Control)Activator.CreateInstance(type);
            }
        }

        type = data?.GetType() ?? typeof(object);
        if (ViewBindingsMap.TryGetValue(type, out Type viewType))
        {
            var instance = Activator.CreateInstance(viewType);
            if (instance is not null)
            {
                return (Control)instance;
            }
            else
            {
                return new TextBlock { Text = "Create Instance Failed: " + viewType.FullName };
            }
        }

        throw new KeyNotFoundException($"View for {type.FullName} not found");
    }

    public bool Match(object? data)
    {
        return data is ObservableObject or IDockable;
    }
}
