using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Models.Commands.Attributes;
using PixiEditor.Models.Commands.Evaluators;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Tools;
using System.IO;
using System.Reflection;
using System.Windows.Media;
using CommandAttribute = PixiEditor.Models.Commands.Attributes.Command;

namespace PixiEditor.Models.Commands
{
    public class CommandController
    {
        private readonly ShortcutFile shortcutFile;

        public static CommandController Current { get; private set; }
        
        public static string ShortcutsPath { get; private set; }

        public CommandCollection Commands { get; }

        public List<CommandGroup> CommandGroups { get; }

        public Dictionary<string, CanExecuteEvaluator> CanExecuteEvaluators { get; }

        public Dictionary<string, IconEvaluator> IconEvaluators { get; }

        public CommandController(IServiceProvider services)
        {
            Current ??= this;

            ShortcutsPath = Path.Join(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "PixiEditor",
                "shortcuts.json");
            
            shortcutFile = new(ShortcutsPath, this);

            Commands = new();
            CommandGroups = new();
            CanExecuteEvaluators = new();
            IconEvaluators = new();
        }

        public void Import(IEnumerable<KeyValuePair<KeyCombination, IEnumerable<string>>> shortcuts, bool save = true)
        {
            foreach (var shortcut in shortcuts)
            {
                foreach (var command in shortcut.Value)
                {
                    ReplaceShortcut(Commands[command], shortcut.Key);
                }
            }
            
            if (save)
            {
                shortcutFile.SaveShortcuts();
            }
        }
        
        private static List<(string internalName, string displayName)> FindCommandGroups(Type[] typesToSearchForAttributes)
        {
            List<(string internalName, string displayName)> result = new();

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
            KeyValuePair<KeyCombination, IEnumerable<string>>[] shortcuts = shortcutFile.LoadShortcuts()?.ToArray()
                ?? Array.Empty<KeyValuePair<KeyCombination, IEnumerable<string>>>();

            Type[] allTypesInPixiEditorAssembly = typeof(CommandController).Assembly.GetTypes();

            List<(string internalName, string displayName)> commandGroupsData = FindCommandGroups(allTypesInPixiEditorAssembly);
            OneToManyDictionary<string, Command> commands = new(); // internal name of the corr. group -> command in that group

            // Find evaluators
            ForEachMethod(allTypesInPixiEditorAssembly, serviceProvider, (methodInfo, maybeServiceInstance) =>
            {
                var evaluatorAttrs = methodInfo.GetCustomAttributes<Evaluator.EvaluatorAttribute>();
                foreach (var attribute in evaluatorAttrs)
                {
                    switch (attribute)
                    {
                        case Evaluator.CanExecuteAttribute canExecuteAttribute:
                            {
                                var getRequiredEvaluatorsObjectsOfCurrentEvaluator =
                                    (CommandController controller) =>
                                        canExecuteAttribute.NamesOfRequiredCanExecuteEvaluators.Select(x => controller.CanExecuteEvaluators[x]);

                                AddEvaluatorFactory<Evaluator.CanExecuteAttribute, CanExecuteEvaluator, bool>(
                                    methodInfo,
                                    maybeServiceInstance,
                                    canExecuteAttribute,
                                    CanExecuteEvaluators,
                                    evaluateFunction => new CanExecuteEvaluator()
                                    {
                                        Name = attribute.Name,
                                        Evaluate = evaluateFunctionArgument =>
                                            evaluateFunction.Invoke(evaluateFunctionArgument) &&
                                            getRequiredEvaluatorsObjectsOfCurrentEvaluator.Invoke(this).All(requiredEvaluator =>
                                                requiredEvaluator.CallEvaluate(null, evaluateFunctionArgument))
                                    });
                                break;
                            }
                        case Evaluator.IconAttribute icon:
                            AddEvaluator<Evaluator.IconAttribute, IconEvaluator, ImageSource>(methodInfo, maybeServiceInstance, icon, IconEvaluators);
                            break;
                    }
                }
            });

            // Find basic commands
            ForEachMethod(allTypesInPixiEditorAssembly, serviceProvider, (methodInfo, maybeServiceInstance) =>
            {
                var commandAttrs = methodInfo.GetCustomAttributes<CommandAttribute.CommandAttribute>();

                foreach (var attribute in commandAttrs)
                {
                    if (attribute is CommandAttribute.BasicAttribute basic)
                    {
                        AddCommand(methodInfo, maybeServiceInstance, attribute, (isDebug, name, x, xCan, xIcon) => new Command.BasicCommand(x, xCan)
                        {
                            InternalName = name,
                            IsDebug = isDebug,
                            DisplayName = attribute.DisplayName,
                            Description = attribute.Description,
                            IconPath = attribute.IconPath,
                            IconEvaluator = xIcon,
                            DefaultShortcut = attribute.GetShortcut(),
                            Shortcut = GetShortcut(name, attribute.GetShortcut()),
                            Parameter = basic.Parameter,
                        });
                    }
                }
            });

            // Find tool commands
            foreach (var type in allTypesInPixiEditorAssembly)
            {
                if (!type.IsAssignableTo(typeof(Tool)))
                    continue;

                var toolAttr = type.GetCustomAttribute<CommandAttribute.ToolAttribute>();
                if (toolAttr is null)
                    continue;

                Tool toolInstance = serviceProvider.GetServices<Tool>().First(x => x.GetType() == type);
                string internalName = $"PixiEditor.Tools.Select.{type.Name}";

                var command = new Command.ToolCommand()
                {
                    InternalName = internalName,
                    DisplayName = $"Select {toolInstance.DisplayName} Tool",
                    Description = $"Select {toolInstance.DisplayName} Tool",
                    IconPath = $"@{toolInstance.ImagePath}",
                    IconEvaluator = IconEvaluator.Default,
                    TransientKey = toolAttr.Transient,
                    DefaultShortcut = toolAttr.GetShortcut(),
                    Shortcut = GetShortcut(internalName, toolAttr.GetShortcut()),
                    ToolType = type,
                };

                Commands.Add(command);
                AddCommandToCommandsCollection(command);
            }

            // save all commands into CommandGroups
            foreach (var (groupInternalName, storedCommands) in commands)
            {
                var groupData = commandGroupsData.Where(group => group.internalName == groupInternalName).FirstOrDefault();
                string groupDisplayName;
                if (groupData == default)
                    groupDisplayName = "Misc";
                else
                    groupDisplayName = groupData.displayName;
                CommandGroups.Add(new(groupDisplayName, storedCommands));
            }

            KeyCombination GetShortcut(string internalName, KeyCombination defaultShortcut) 
                => shortcuts.FirstOrDefault(x => x.Value.Contains(internalName), new(defaultShortcut, null)).Key;

            void AddCommandToCommandsCollection(Command command)
            {
                (string internalName, string displayName) group = commandGroupsData.FirstOrDefault(x => command.InternalName.StartsWith(x.internalName));
                if (group == default)
                    commands.Add("", command);
                else
                    commands.Add(group.internalName, command);
            }

            void AddEvaluator<TAttr, T, TParameter>(MethodInfo method, object instance, TAttr attribute, IDictionary<string, T> evaluators)
                where T : Evaluator<TParameter>, new()
                where TAttr : Evaluator.EvaluatorAttribute
                => AddEvaluatorFactory<TAttr, T, TParameter>(method, instance, attribute, evaluators, x => new T() { Name = attribute.Name, Evaluate = x });

            void AddEvaluatorFactory<TAttr, T, TParameter>(MethodInfo method, object serviceInstance, TAttr attribute, IDictionary<string, T> evaluators, Func<Func<object, TParameter>, T> factory)
                where T : Evaluator<TParameter>, new()
                where TAttr : Evaluator.EvaluatorAttribute
            {
                if (method.ReturnType != typeof(TParameter))
                {
                    throw new Exception($"Invalid return type for the CanExecute evaluator '{attribute.Name}' at {method.ReflectedType.FullName}.{method.Name}\nExpected '{typeof(TParameter).FullName}'");
                }
                else if (method.GetParameters().Length > 1)
                {
                    throw new Exception($"Too many parameters for the CanExecute evaluator '{attribute.Name}' at {method.ReflectedType.FullName}.{method.Name}");
                }
                else if (!method.IsStatic && serviceInstance is null)
                {
                    throw new Exception($"No type instance for the CanExecute evaluator '{attribute.Name}' at {method.ReflectedType.FullName}.{method.Name} found");
                }

                var parameters = method.GetParameters();

                Func<object, TParameter> func;

                if (parameters.Length == 1)
                {
                    func = x => (TParameter)method.Invoke(serviceInstance, new[] { CastParameter(x, parameters[0].ParameterType) });
                }
                else
                {
                    func = x => (TParameter)method.Invoke(serviceInstance, null);
                }

                T evaluator = factory(func);

                evaluators.Add(evaluator.Name, evaluator);
            }

            object CastParameter(object input, Type target)
            {
                if (target == typeof(object) || target == input.GetType())
                    return input;
                return Convert.ChangeType(input, target);
            }

            TCommand AddCommand<TAttr, TCommand>(MethodInfo method, object instance, TAttr attribute, Func<bool, string, Action<object>, CanExecuteEvaluator, IconEvaluator, TCommand> commandFactory)
                where TAttr : CommandAttribute.CommandAttribute
                where TCommand : Command
            {
                if (method != null)
                {
                    if (method.GetParameters().Length > 1)
                    {
                        throw new Exception($"Too many parameters for the CanExecute evaluator '{attribute.InternalName}' at {method.ReflectedType.FullName}.{method.Name}");
                    }
                    else if (!method.IsStatic && instance is null)
                    {
                        throw new Exception($"No type instance for the CanExecute evaluator '{attribute.InternalName}' at {method.ReflectedType.FullName}.{method.Name} found");
                    }
                }

                var parameters = method?.GetParameters();

                Action<object> action;

                if (parameters == null || parameters.Length != 1)
                {
                    action = x => method.Invoke(instance, null);
                }
                else
                {
                    action = x => method.Invoke(instance, new[] { x });
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
                AddCommandToCommandsCollection(command);

                return command;
            }
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
}
