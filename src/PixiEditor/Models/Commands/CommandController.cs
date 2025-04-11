using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PixiEditor.Exceptions;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Extensions.Common.Localization;
using PixiEditor.Models.AnalyticsAPI;
using PixiEditor.Models.Commands.Attributes.Commands;
using PixiEditor.Models.Commands.Attributes.Evaluators;
using PixiEditor.Models.Commands.CommandContext;
using PixiEditor.Models.Commands.Evaluators;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Handlers;
using PixiEditor.Models.Input;
using PixiEditor.Models.Structures;
using PixiEditor.OperatingSystem;
using Command = PixiEditor.Models.Commands.Commands.Command;
using CommandAttribute = PixiEditor.Models.Commands.Attributes.Commands.Command;

namespace PixiEditor.Models.Commands;

internal class CommandController
{
    private ShortcutFile shortcutFile;

    public static CommandController Current { get; private set; }

    public static string ShortcutsPath { get; private set; }

    public CommandCollection Commands { get; }

    public List<CommandGroup> CommandGroups { get; }

    public CommandLog.CommandLog Log { get; }

    public OneToManyDictionary<string, Command> FilterCommands { get; }

    public Dictionary<string, string> FilterSearchTerm { get; }

    public Dictionary<string, CanExecuteEvaluator> CanExecuteEvaluators { get; }

    public Dictionary<string, IconEvaluator> IconEvaluators { get; }

    private static readonly List<Command> objectsToInvokeOn = new();

    public CommandController()
    {
        Current ??= this;
        Log = new CommandLog.CommandLog();

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
                    ReplaceShortcut(Commands[command], shortcut.KeyCombination, false);
                }
            }
        }

        if (save)
        {
            shortcutFile.SaveShortcuts();
        }
    }

    private static List<Attributes.Commands.Command.GroupAttribute> FindCommandGroups(
        IEnumerable<Type> typesToSearchForAttributes)
    {
        List<Attributes.Commands.Command.GroupAttribute> result = new();

        foreach (var type in typesToSearchForAttributes)
        {
            foreach (var group in type.GetCustomAttributes<Attributes.Commands.Command.GroupAttribute>())
            {
                result.Add(group);
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
            File.Move(shortcutFile.Path, $"{shortcutFile.Path}.corrupted", true); // TODO: platform dependent
            shortcutFile = new ShortcutFile(ShortcutsPath, this);
            template = shortcutFile.LoadTemplate();
            NoticeDialog.Show("SHORTCUTS_CORRUPTED", "SHORTCUTS_CORRUPTED_TITLE");
        }

        var compiledCommandList = new CommandNameList();
        List<Attributes.Commands.Command.GroupAttribute> commandGroupsData =
            FindCommandGroups(compiledCommandList.Groups);
        OneToManyDictionary<string, Command>
            commands = new(); // internal name of the corr. group -> command in that group

        LoadEvaluators(serviceProvider, compiledCommandList);
        LoadCommands(serviceProvider, compiledCommandList, commandGroupsData, commands, template);
        LoadTools(serviceProvider, commandGroupsData, commands, template);

        var miscList = new List<Command>();

        foreach (var (groupInternalName, storedCommands) in commands)
        {
            var groupData = commandGroupsData.FirstOrDefault(group => group.InternalName == groupInternalName);
            if (groupData == default || groupData.InternalName == "PixiEditor.Links")
            {
                miscList.AddRange(storedCommands);
                continue;
            }

            LocalizedString groupDisplayName = groupData.DisplayName;
            CommandGroups.Add(new CommandGroup(groupDisplayName, storedCommands)
            {
                IsVisibleProperty = groupData.IsVisibleMenuProperty
            });
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
            if (evaluator.Methods.CanExecuteEvaluator.DependentOn != null &&
                evaluator.Methods.CanExecuteEvaluator.DependentOn.Contains(propertyName))
            {
                evaluator.OnCanExecuteChanged();
            }
        }
    }

    private void LoadTools(IServiceProvider serviceProvider,
        List<Attributes.Commands.Command.GroupAttribute> commandGroupsData,
        OneToManyDictionary<string, Command> commands,
        ShortcutsTemplate template)
    {
        IToolsHandler toolsHandler = serviceProvider.GetService<IToolsHandler>();
        foreach (var toolInstance in serviceProvider.GetServices<IToolHandler>())
        {
            var type = toolInstance.GetType();

            if (!type.IsAssignableTo(typeof(IToolHandler)))
                continue;

            var toolAttr = type.GetCustomAttribute<Attributes.Commands.Command.ToolAttribute>();
            if (toolAttr is null)
                continue;

            string internalName = $"PixiEditor.Tools.Select.{type.Name}";

            LocalizedString displayName = new("SELECT_TOOL", toolInstance.DisplayName);

            var command = new Command.ToolCommand(toolsHandler)
            {
                InternalName = internalName,
                DisplayName = displayName,
                Description = displayName,
                Icon = toolInstance.DefaultIcon,
                IconEvaluator = IconEvaluator.Default,
                TransientKey = toolAttr.Transient,
                TransientImmediate = toolAttr.TransientImmediate,
                DefaultShortcut = toolAttr.GetShortcut(),
                Shortcut = GetShortcut(internalName, toolAttr.GetShortcut(), template),
                ToolType = type,
            };

            Commands.Add(command);
            AddCommandToCommandsCollection(command, commandGroupsData, commands);
        }
    }

    private KeyCombination GetShortcut(string internalName, KeyCombination defaultShortcut,
        ShortcutsTemplate template) =>
        template.Shortcuts
            .FirstOrDefault(x => x.Commands.Contains(internalName), new Shortcut(defaultShortcut, (List<string>)null))
            .KeyCombination;

    private void AddCommandToCommandsCollection(Command command,
        List<Attributes.Commands.Command.GroupAttribute> commandGroupsData,
        OneToManyDictionary<string, Command> commands)
    {
        var group = commandGroupsData.FirstOrDefault(x => command.InternalName.StartsWith(x.InternalName));
        if (group == default)
            commands.Add("", command);
        else
            commands.Add(group.InternalName, command);
    }

    private void LoadCommands(IServiceProvider serviceProvider, CommandNameList compiledCommandList,
        List<Attributes.Commands.Command.GroupAttribute> commandGroupsData,
        OneToManyDictionary<string, Command> commands, ShortcutsTemplate template)
    {
        foreach (var type in compiledCommandList.Commands)
        {
            foreach (var methodNames in type.Value)
            {
                var name = methodNames.Item1;

                var methodInfo = type.Key.GetMethod(name, methodNames.Item2.ToArray());

                var commandAttrs = methodInfo.GetCustomAttributes<Attributes.Commands.Command.CommandAttribute>();
                var customOsShortcuts = methodInfo.GetCustomAttributes<CustomOsShortcutAttribute>();

                CustomOsShortcutAttribute? customOsShortcut =
                    customOsShortcuts.FirstOrDefault(x => string.Equals(x.ValidOs, IOperatingSystem.Current.Name,
                        StringComparison.InvariantCultureIgnoreCase));

                foreach (var attribute in commandAttrs)
                {
                    if (attribute is Attributes.Commands.Command.BasicAttribute basic)
                    {
                        var validCustomShortcut = customOsShortcut?.TargetCommand == basic.InternalName
                            ? customOsShortcut
                            : null;
                        AddCommand(methodInfo, serviceProvider.GetService(type.Key), attribute,
                            (isDebug, name, x, xCan, xIcon) => new Command.BasicCommand(x, xCan)
                            {
                                InternalName = name,
                                IsDebug = isDebug,
                                DisplayName = attribute.DisplayName,
                                Description = attribute.Description,
                                Icon = attribute.Icon,
                                IconEvaluator = xIcon,
                                DefaultShortcut = AdjustForOS(attribute.GetShortcut(), validCustomShortcut),
                                Shortcut =
                                    GetShortcut(name, AdjustForOS(attribute.GetShortcut(), validCustomShortcut),
                                        template),
                                Parameter = basic.Parameter,
                                MenuItemPath = basic.MenuItemPath,
                                MenuItemOrder = basic.MenuItemOrder,
                                ShortcutContexts = basic.ShortcutContexts
                            });
                    }
                    else if (attribute is Attributes.Commands.Command.FilterAttribute menu)
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

                        foreach (var menuCommand in commandAttrs.Where(x =>
                                     x is not Attributes.Commands.Command.FilterAttribute))
                        {
                            FilterCommands.Add(searchTerm, Commands[menuCommand.InternalName]);
                        }

                        if (hasFilter)
                            continue;

                        var validCustomShortcut = customOsShortcut?.TargetCommand == menu.InternalName
                            ? customOsShortcut
                            : null;

                        ISearchHandler searchHandler = serviceProvider.GetRequiredService<ISearchHandler>();

                        if (searchHandler is null)
                            continue;

                        var command =
                            new Command.BasicCommand(
                                ExecuteFilter,
                                CanExecuteEvaluator.AlwaysTrue)
                            {
                                InternalName = menu.InternalName,
                                DisplayName = menu.DisplayName,
                                Description = menu.DisplayName,
                                IconEvaluator = IconEvaluator.Default,
                                DefaultShortcut = AdjustForOS(menu.GetShortcut(), validCustomShortcut),
                                Shortcut = GetShortcut(name, AdjustForOS(attribute.GetShortcut(), validCustomShortcut),
                                    template)
                            };

                        Commands.Add(command);

                        AddCommandToCommandsCollection(command, commandGroupsData, commands);

                        void ExecuteFilter(object o)
                        {
                            if (attribute.AnalyticsTrack && o is CommandExecutionContext c)
                            {
                                Analytics.SendCommand(menu.InternalName, c.SourceInfo);
                            }

                            searchHandler.OpenSearchWindow($":{searchTerm}:");
                        }
                    }
                }
            }
        }

        TCommand AddCommand<TAttr, TCommand>(MethodInfo method, object instance, TAttr attribute,
            Func<bool, string, Action<object>, CanExecuteEvaluator, IconEvaluator, TCommand> commandFactory)
            where TAttr : Attributes.Commands.Command.CommandAttribute
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

            string name = attribute.InternalName;
            bool isDebug = attribute.InternalName.StartsWith("#DEBUG#");

            if (attribute.InternalName.StartsWith("#DEBUG#"))
            {
                name = name["#DEBUG#".Length..];
            }

            var command = commandFactory(
                isDebug,
                name,
                CommandAction,
                attribute.CanExecute != null
                    ? CanExecuteEvaluators[attribute.CanExecute]
                    : CanExecuteEvaluator.AlwaysTrue,
                attribute.IconEvaluator != null ? IconEvaluators[attribute.IconEvaluator] : IconEvaluator.Default);

            Commands.Add(command);
            AddCommandToCommandsCollection(command, commandGroupsData, commands);

            return command;

            void CommandAction(object x) =>
                CommandMethodInvoker(method, name, instance, x, parameters, attribute.AnalyticsTrack);
        }

        KeyCombination AdjustForOS(KeyCombination combination, CustomOsShortcutAttribute? customOsShortcut)
        {
            if (customOsShortcut != null)
            {
                return new KeyCombination(customOsShortcut.Key, customOsShortcut.Modifiers);
            }

            if (IOperatingSystem.Current.IsMacOs)
            {
                KeyCombination newCombination = combination;
                if (combination.Modifiers.HasFlag(KeyModifiers.Control))
                {
                    newCombination.Modifiers &= ~KeyModifiers.Control;
                    newCombination.Modifiers |= KeyModifiers.Meta;
                }

                if (combination.Key == Key.Delete)
                {
                    newCombination.Key = Key.Back;
                }

                return newCombination;
            }

            return combination;
        }
    }

    private static void CommandMethodInvoker(MethodInfo method, string name, object? instance, object parameter,
        ParameterInfo[] parameterInfos, bool isTracking)
    {
        var parameters = GetParameters(parameter, parameterInfos);
        AnalyticEvent? analytics = null;

        if (isTracking)
        {
            analytics = Analytics.SendCommand(name, (parameter as CommandExecutionContext)?.SourceInfo,
                expectingEndTime: true);
        }

        try
        {
            object result = method.Invoke(instance, parameters);
            if (result is Task task)
            {
                task.ContinueWith(ActionOnException, TaskContinuationOptions.OnlyOnFaulted);
                task.ContinueWith(ReportEndTime, TaskContinuationOptions.OnlyOnRanToCompletion);
            }
            else
            {
                analytics?.ReportEndTime();
            }
        }
        catch (TargetInvocationException e)
        {
            throw new CommandInvocationException(name, e);
        }

        return;

        static async void ActionOnException(Task faultedTask)
        {
            // since this method is "async void" and not "async Task", the runtime will propagate exceptions out if it
            // (instead of putting them into the returned task and forgetting about them)
            await faultedTask; // this instantly throws the exception from the already faulted task
        }

        ValueTask ReportEndTime(Task originalTask)
        {
            analytics?.ReportEndTime();
            return ValueTask.CompletedTask;
        }

        static object?[]? GetParameters(object parameter, ParameterInfo[] parameterInfos)
        {
            object?[]? parameters;

            if (parameterInfos.Length == 0)
            {
                parameters = null;
            }
            else if (parameter is CommandExecutionContext context)
            {
                if (parameterInfos[0].ParameterType == typeof(CommandExecutionContext))
                {
                    parameters = [context];
                }
                else
                {
                    parameters = [context.Parameter];
                }
            }
            else
            {
                parameters = [parameter];
            }

            return parameters;
        }
    }

    private void LoadEvaluators(IServiceProvider serviceProvider, CommandNameList compiledCommandList)
    {
        object CastParameter(object input, Type target)
        {
            var commandExecutionType = typeof(CommandExecutionContext);
            if (input is CommandExecutionContext context && !target.IsAssignableTo(commandExecutionType))
                input = context.Parameter;

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

                T evaluator = factory(x => Task.Run(async () => await func(x)).Result); //TODO: This is not truly async
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
                                        DependentOn = canExecuteAttribute.DependentOn
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

    private static bool IsAssignaleAsync<TAttr, T, TParameter>(MethodInfo method) where T : Evaluator<TParameter>, new()
        where TAttr : Evaluator.EvaluatorAttribute
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
    public void ReplaceShortcut(Command command, KeyCombination newShortcut, bool clear = true)
    {
        List<Command> toRemove = new List<Command>();
        foreach (Command other in Commands[newShortcut])
        {
            bool anyContextOverlap = (other.ShortcutContexts != null && other.ShortcutContexts
                                         .Any(x => command.ShortcutContexts != null &&
                                                   command.ShortcutContexts.Contains(x)))
                                     || other.ShortcutContexts == null && command.ShortcutContexts == null;
            if (anyContextOverlap && newShortcut == other.Shortcut && other.Shortcut != KeyCombination.None)
            {
                toRemove.Add(other);
            }
        }

        if (clear)
        {
            Commands.ClearShortcut(newShortcut);
        }

        foreach (var cmd in toRemove)
        {
            Commands.RemoveShortcut(cmd, cmd.Shortcut);
            cmd.Shortcut = KeyCombination.None;
        }

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

    public static void CanExecuteChanged(string commandPattern)
    {
        foreach (var command in Current.Commands.Where(x => x.InternalName.StartsWith(commandPattern)))
        {
            command.OnCanExecuteChanged();
        }
    }
}
