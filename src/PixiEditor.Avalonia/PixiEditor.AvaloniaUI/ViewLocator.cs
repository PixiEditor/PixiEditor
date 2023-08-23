using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.Core;
using PixiEditor.AvaloniaUI.ViewModels.Dock;
using PixiEditor.AvaloniaUI.Views.Dock;

namespace PixiEditor.AvaloniaUI;

public class ViewLocator : IDataTemplate
{
    public static Dictionary<Type, Type> ViewBindingsMap = new Dictionary<Type, Type>()
    {
        [typeof(DockDocumentViewModel)] = typeof(DocumentTemplate)
    };

    public Control Build(object? data)
    {
        Type type = data?.GetType() ?? typeof(object);
        if (ViewBindingsMap.TryGetValue(type, out Type viewType))
        {
            var instance = Activator.CreateInstance(viewType);
            if (instance is { })
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
