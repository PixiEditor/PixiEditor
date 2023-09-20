using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PixiEditor.AvaloniaUI.Helpers.Extensions;
using PixiEditor.AvaloniaUI.Models.Commands.Attributes.Evaluators;
using PixiEditor.AvaloniaUI.Models.Commands.Commands;
using PixiEditor.AvaloniaUI.Models.Commands.Evaluators;
using PixiEditor.AvaloniaUI.Models.Dialogs;
using PixiEditor.AvaloniaUI.Models.Handlers;
using PixiEditor.AvaloniaUI.Models.Input;
using PixiEditor.AvaloniaUI.Models.Structures;
using PixiEditor.Extensions.Common.Localization;
using CommandAttribute = PixiEditor.AvaloniaUI.Models.Commands.Attributes.Commands.Command;

namespace PixiEditor.AvaloniaUI.Models.Commands;

internal class CommandController
{
    private ShortcutFile shortcutFile;

    public static CommandController Current { get; private set; }

    public static string ShortcutsPath { get; private set; }

    public CommandCollection Commands { get; }

    public List<CommandGroup> CommandGroups { get; }

    public OneToManyDictionary<string, Command> FilterCommands { get; }
    
    public Dictionary<string, string> FilterSearchTerm { get; }

    public Dictionary<string, CanExecuteEvaluator> CanExecuteEvaluators { get; }

    public Dictionary<string, IconEvaluator> IconEvaluators { get; }

    private static readonly List<Command> objectsToInvokeOn = new();
    
    public CommandController()
    {
        Current ??= this;

        ShortcutsPath = Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "PixiEditor",
            "shortcuts.json");

        shortcutFile = new(ShortcutsPath, this);

        FilterCommands = new();
        FilterSearchTerm = new();
        Commands = new();
        CommandGroups = new();
        CanExecuteEvaluators = new();
        IconEvaluators = new();
    }

    public void Import(List<Shortcut> shortcuts, bool save = true)
    {
        foreach (var shortcut in shortcuts)
        {
            foreach (var command in shortcut.Commands)
            {
                if (Commands.ContainsKey(command))
                {
                    ReplaceShortcut(Commands[command], shortcut.KeyCombination);
                }
            }
        }

        if (save)
        {
            shortcutFile.SaveShortcuts();
        }
    }

    private static List<(string internalName, LocalizedString displayName)> FindCommandGroups(IEnumerable<Type> typesToSearchForAttributes)
    {
        List<(string internalName, LocalizedString displayName)> result = new();

        foreach (var type in typesToSearchForAttributes)
        {
            foreach (var group in type.GetCustomAttributes<CommandAttribute.GroupAttribute>())
            {
                result.Add((group.InternalName, group.DisplayName));
            }
        }

        return result;
    }

    private static void ForEachMethod
        (Type[] typesToSearchForMethods, IServiceProvider serviceProvider, Action<MethodInfo, object> action)
    {
        foreach (var type in typesToSearchForMethods)
        {
            object serviceInstance = serviceProvider.GetService(type);
            var methods = type.GetMethods();
            foreach (var method in methods)
            {
                action(method, serviceInstance);
            }
        }
    }

    public void Init(IServiceProvider serviceProvider)
    {
        ShortcutsTemplate template = new();
        try
        {
            template = shortcutFile.LoadTemplate();
        }
        catch (JsonException)
        {
            File.Move(shortcutFile.Path, $"{shortcutFile.Path}.corrupted", true);
            shortcutFile = new ShortcutFile(ShortcutsPath, this);
            template = shortcutFile.LoadTemplate();
            NoticeDialog.Show("SHORTCUTS_CORRUPTED", "SHORTCUTS_CORRUPTED_TITLE");
        }
        var compiledCommandList = new CommandNameList();
        List<(string internalName, LocalizedString displayName)> commandGroupsData = FindCommandGroups(compiledCommandList.Groups);
        OneToManyDictionary<string, Command> commands = new(); // internal name of the corr. group -> command in that group

        LoadEvaluators(serviceProvider, compiledCommandList);
        LoadCommands(serviceProvider, compiledCommandList, commandGroupsData, commands, template);
        LoadTools(serviceProvider, commandGroupsData, commands, template);
        
        var miscList = new List<Command>();

        foreach (var (groupInternalName, storedCommands) in commands)
        {
            var groupData = commandGroupsData.FirstOrDefault(group => group.internalName == groupInternalName);
            if (groupData == default || groupData.internalName == "PixiEditor.Links")
            {
                miscList.AddRange(storedCommands);
                continue;
            }

            LocalizedString groupDisplayName = groupData.displayName;
            CommandGroups.Add(new CommandGroup(groupDisplayName, storedCommands));
        }
        
        CommandGroups.Add(new CommandGroup("MISC", miscList));
    }

    public static void ListenForCanExecuteChanged(Command command)
    {
        objectsToInvokeOn.Add(command);
    }

    public static void StopListeningForCanExecuteChanged(Command handler)
    {
        objectsToInvokeOn.Remove(handler);
    }

    public void NotifyPropertyChanged(string? propertyName)
    {
        foreach (var evaluator in objectsToInvokeOn)
        {
            //TODO: Check if performance is better with or without this
            /*if (evaluator.Methods.CanExecuteEvaluator.DependentOn != null && evaluator.Methods.CanExecuteEvaluator.DependentOn.Contains(propertyName))*/
            {
                evaluator.OnCanExecuteChanged();
            }
        }
    }

    private void LoadTools(IServiceProvider serviceProvider, List<(string internalName, LocalizedString displayName)> commandGroupsData, OneToManyDictionary<string, Command> commands,
        ShortcutsTemplate template)
    {
        IToolsHandler toolsHandler = serviceProvider.GetService<IToolsHandler>();
        foreach (var toolInstance in serviceProvider.GetServices<IToolHandler>())
        {
            var type = toolInstance.GetType();

            if (!type.IsAssignableTo(typeof(IToolHandler)))
                continue;

            var toolAttr = type.GetCustomAttribute<CommandAttribute.ToolAttribute>();
            if (toolAttr is null)
                continue;

            string internalName = $"PixiEditor.Tools.Select.{type.Name}";

            LocalizedString displayName = new("SELECT_TOOL", toolInstance.DisplayName);

            var command = new Command.ToolCommand(toolsHandler)
            {
                InternalName = internalName,
                DisplayName = displayName,
                Description = displayName,
                IconPath = $"@{toolInstance.ImagePath}",
                IconEvaluator = IconEvaluator.Default,
                TransientKey = toolAttr.Transient,
                DefaultShortcut = toolAttr.GetShortcut(),
                Shortcut = GetShortcut(internalName, toolAttr.GetShortcut(), template),
                ToolType = type,
            };

            Commands.Add(command);
            AddCommandToCommandsCollection(command, commandGroupsData, commands);
        }
    }

    private KeyCombination GetShortcut(string internalName, KeyCombination defaultShortcut, ShortcutsTemplate template) =>
        template.Shortcuts
            .FirstOrDefault(x => x.Commands.Contains(internalName), new Shortcut(defaultShortcut, (List<string>)null))
            .KeyCombination;

    private void AddCommandToCommandsCollection(Command command, List<(string internalName, LocalizedString displayName)> commandGroupsData, OneToManyDictionary<string, Command> commands)
    {
        (string internalName, string displayName) group = commandGroupsData.FirstOrDefault(x => command.InternalName.StartsWith(x.internalName));
        if (group == default)
            commands.Add("", command);
        else
            commands.Add(group.internalName, command);
    }

    private void LoadCommands(IServiceProvider serviceProvider, CommandNameList compiledCommandList, List<(string internalName, LocalizedString displayName)> commandGroupsData, OneToManyDictionary<string, Command> commands, ShortcutsTemplate template)
    {
        foreach (var type in compiledCommandList.Commands)
        {
            foreach (var methodNames in type.Value)
            {
                var name = methodNames.Item1;

                var methodInfo = type.Key.GetMethod(name, methodNames.Item2.ToArray());

                var commandAttrs = methodInfo.GetCustomAttributes<CommandAttribute.CommandAttribute>();

                foreach (var attribute in commandAttrs)
                {
                    if (attribute is CommandAttribute.BasicAttribute basic)
                    {
                        AddCommand(methodInfo, serviceProvider.GetService(type.Key), attribute,
                            (isDebug, name, x, xCan, xIcon) => new Command.BasicCommand(x, xCan)
                            {
                                InternalName = name,
                                IsDebug = isDebug,
                                DisplayName = attribute.DisplayName,
                                Description = attribute.Description,
                                IconPath = attribute.IconPath,
                                IconEvaluator = xIcon,
                                DefaultShortcut = attribute.GetShortcut(),
                                Shortcut = GetShortcut(name, attribute.GetShortcut(), template),
                                Parameter = basic.Parameter,
                            });
                    }
                    else if (attribute is CommandAttribute.FilterAttribute menu)
                    {
                        string searchTerm = menu.SearchTerm;
                        
                        if (searchTerm == null)
                        {
                            searchTerm = FilterSearchTerm[menu.InternalName];
                        }
                        else
                        {
                            FilterSearchTerm.Add(menu.InternalName, menu.SearchTerm);
                        }

                        bool hasFilter = FilterCommands.ContainsKey(searchTerm);
                        
                        foreach (var menuCommand in commandAttrs.Where(x => x is not CommandAttribute.FilterAttribute))
                        {
                            FilterCommands.Add(searchTerm, Commands[menuCommand.InternalName]);
                        }

                        if (hasFilter)
                            continue;

                        ISearchHandler searchHandler = serviceProvider.GetRequiredService<ISearchHandler>();

                        if (searchHandler is null)
                            continue;

                        var command =
                            new Command.BasicCommand(
                                _ => searchHandler.OpenSearchWindow($":{searchTerm}:"),
                                CanExecuteEvaluator.AlwaysTrue)
                            {
                                InternalName = menu.InternalName,
                                DisplayName = menu.DisplayName,
                                Description = menu.DisplayName,
                                IconEvaluator = IconEvaluator.Default,
                                DefaultShortcut = menu.GetShortcut(),
                                Shortcut = GetShortcut(name, attribute.GetShortcut(), template)
                            };
                        
                        Commands.Add(command);

                        AddCommandToCommandsCollection(command, commandGroupsData, commands);
                    }
                }
            }
        }
        
        TCommand AddCommand<TAttr, TCommand>(MethodInfo method, object instance, TAttr attribute,
            Func<bool, string, Action<object>, CanExecuteEvaluator, IconEvaluator, TCommand> commandFactory)
            where TAttr : CommandAttribute.CommandAttribute
            where TCommand : Command
        {
            if (method != null)
            {
                if (method.GetParameters().Length > 1)
                {
                    throw new Exception(
                        $"Too many parameters for the CanExecute evaluator '{attribute.InternalName}' at {method.ReflectedType.FullName}.{method.Name}");
                }
                else if (!method.IsStatic && instance is null)
                {
                    throw new Exception(
                        $"No type instance for the CanExecute evaluator '{attribute.InternalName}' at {method.ReflectedType.FullName}.{method.Name} found");
                }
            }

            var parameters = method?.GetParameters();

            async void ActionOnException(Task faultedTask)
            {
                // since this method is "async void" and not "async Task", the runtime will propagate exceptions out if it
                // (instead of putting them into the returned task and forgetting about them)
                await faultedTask; // this instantly throws the exception from the already faulted task
            }

            Action<object> action;
            if (parameters is not { Length: 1 })
            {
                action = x =>
                {
                    object result = method.Invoke(instance, null);
                    if (result is Task task)
                        task.ContinueWith(ActionOnException, TaskContinuationOptions.OnlyOnFaulted);
                };
            }
            else
            {
                action = x =>
                {
                    object result = method.Invoke(instance, new[] { x });
                    if (result is Task task)
                        task.ContinueWith(ActionOnException, TaskContinuationOptions.OnlyOnFaulted);
                };
            }

            string name = attribute.InternalName;
            bool isDebug = attribute.InternalName.StartsWith("#DEBUG#");

            if (attribute.InternalName.StartsWith("#DEBUG#"))
            {
                name = name["#DEBUG#".Length..];
            }

            var command = commandFactory(
                isDebug,
                name,
                action,
                attribute.CanExecute != null ? CanExecuteEvaluators[attribute.CanExecute] : CanExecuteEvaluator.AlwaysTrue,
                attribute.IconEvaluator != null ? IconEvaluators[attribute.IconEvaluator] : IconEvaluator.Default);

            Commands.Add(command);
            AddCommandToCommandsCollection(command, commandGroupsData, commands);

            return command;
        }
    }

    private void LoadEvaluators(IServiceProvider serviceProvider, CommandNameList compiledCommandList)
    {
        object CastParameter(object input, Type target)
        {
            if (target == typeof(object) || target == input?.GetType())
                return input;
            return Convert.ChangeType(input, target);
        }

        void AddEvaluatorFactory<TAttr, T, TParameter>(MethodInfo method, object serviceInstance, TAttr attribute,
            IDictionary<string, T> evaluators, Func<Func<object, TParameter>, T> factory)
            where T : Evaluator<TParameter>, new()
            where TAttr : Evaluator.EvaluatorAttribute
        {
            bool isAssignableAsync = IsAssignaleAsync<TAttr, T, TParameter>(method);
            if (!method.ReturnType.IsAssignableFrom(typeof(TParameter)) && !isAssignableAsync)
            {
                throw new Exception(
                    $"Invalid return type for the CanExecute evaluator '{attribute.Name}' at {method.ReflectedType.FullName}.{method.Name}\nExpected '{typeof(TParameter).FullName}'");
            }
            else if (method.GetParameters().Length > 1)
            {
                throw new Exception(
                    $"Too many parameters for the CanExecute evaluator '{attribute.Name}' at {method.ReflectedType.FullName}.{method.Name}");
            }
            else if (!method.IsStatic && serviceInstance is null)
            {
                throw new Exception(
                    $"No type instance for the CanExecute evaluator '{attribute.Name}' at {method.ReflectedType.FullName}.{method.Name} found");
            }

            var parameters = method.GetParameters();

            if (!isAssignableAsync)
            {
                Func<object, TParameter> func;

                if (parameters.Length == 1)
                {
                    func = x => (TParameter)method.Invoke(serviceInstance,
                        new[] { CastParameter(x, parameters[0].ParameterType) });
                }
                else
                {
                    func = x => (TParameter)method.Invoke(serviceInstance, null);
                }

                T evaluator = factory(func);
                evaluators.Add(evaluator.Name, evaluator);
            }
            else
            {
                Func<object, Task<TParameter>> func;
                if (parameters.Length == 1)
                {
                    func = async x => await method.InvokeAsync<TParameter>(serviceInstance,
                        new[] { CastParameter(x, parameters[0].ParameterType) });
                }
                else
                {
                    func = async x => await method.InvokeAsync<TParameter>(serviceInstance, null);
                }

                T evaluator = factory(x => Task.Run(async () => await func(x)).Result);//TODO: This is not truly async
                evaluators.Add(evaluator.Name, evaluator);
            }
        }

        void AddEvaluator<TAttr, T, TParameter>(MethodInfo method, object instance, TAttr attribute,
            IDictionary<string, T> evaluators)
            where T : Evaluator<TParameter>, new()
            where TAttr : Evaluator.EvaluatorAttribute
            => AddEvaluatorFactory<TAttr, T, TParameter>(method, instance, attribute, evaluators,
                x => new T() { Name = attribute.Name, Evaluate = x });

        {
            foreach (var type in compiledCommandList.Evaluators)
            {
                foreach (var methodNames in type.Value)
                {
                    var name = methodNames.Item1;

                    var methodInfo = type.Key.GetMethod(name, methodNames.Item2.ToArray());

                    var commandAttrs = methodInfo.GetCustomAttributes<Evaluator.EvaluatorAttribute>();

                    foreach (var attribute in commandAttrs)
                    {
                        switch (attribute)
                        {
                            case Evaluator.CanExecuteAttribute canExecuteAttribute:
                            {
                                AddEvaluatorFactory<Evaluator.CanExecuteAttribute, CanExecuteEvaluator, bool>(
                                    methodInfo,
                                    serviceProvider.GetService(type.Key),
                                    canExecuteAttribute,
                                    CanExecuteEvaluators,
                                    evaluateFunction => new CanExecuteEvaluator()
                                    {
                                        Name = attribute.Name,
                                        Evaluate = evaluateFunction.Invoke,
                                        /*DependentOn = canExecuteAttribute.DependentOn*/
                                    });
                                break;
                            }
                            case Evaluator.IconAttribute icon:
                                AddEvaluator<Evaluator.IconAttribute, IconEvaluator, IImage>(methodInfo,
                                    serviceProvider.GetService(type.Key), icon, IconEvaluators);
                                break;
                        }
                    }
                }
            }
        }
    }

    private static bool IsAssignaleAsync<TAttr, T, TParameter>(MethodInfo method) where T : Evaluator<TParameter>, new() where TAttr : Evaluator.EvaluatorAttribute
    {
        if (method.ReturnType.IsAssignableTo(typeof(Task)))
        {
            return method.ReturnType.GenericTypeArguments.Length == 0 ||
                   method.ReturnType.GenericTypeArguments[0].IsAssignableFrom(typeof(TParameter));
        }

        return false;
    }

    /// <summary>
    /// Removes the old shortcut to this command and adds the new one
    /// </summary>
    public void UpdateShortcut(Command command, KeyCombination newShortcut)
    {
        Commands.RemoveShortcut(command, command.Shortcut);
        Commands.AddShortcut(command, newShortcut);
        command.Shortcut = newShortcut;
        shortcutFile.SaveShortcuts();
    }

    /// <summary>
    /// Deletes all shortcuts of <paramref name="newShortcut"/> and adds <paramref name="command"/>
    /// </summary>
    public void ReplaceShortcut(Command command, KeyCombination newShortcut)
    {
        foreach (Command other in Commands[newShortcut])
        {
            other.Shortcut = KeyCombination.None;
        }

        Commands.ClearShortcut(newShortcut);
        Commands.RemoveShortcut(command, command.Shortcut);
        Commands.AddShortcut(command, newShortcut);
        command.Shortcut = newShortcut;
        shortcutFile.SaveShortcuts();
    }

    public void ResetShortcuts()
    {
        File.Copy(ShortcutsPath, Path.ChangeExtension(ShortcutsPath, ".json.bak"), true);

        Commands.ClearShortcuts();

        foreach (var command in Commands)
        {
            Commands.RemoveShortcut(command, command.Shortcut);
            Commands.AddShortcut(command, command.DefaultShortcut);
            command.Shortcut = command.DefaultShortcut;
        }

        shortcutFile.SaveShortcuts();
    }
}
