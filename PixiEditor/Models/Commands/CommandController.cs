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

        public CommandCollection Commands { get; }

        public List<CommandGroup> CommandGroups { get; }

        public Dictionary<string, FactoryEvaluator> FactoryEvaluators { get; }

        public Dictionary<string, CanExecuteEvaluator> CanExecuteEvaluators { get; }

        public Dictionary<string, IconEvaluator> IconEvaluators { get; }

        public CommandController(IServiceProvider services)
        {
            Current ??= this;

            shortcutFile =
                new(Path.Join(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "PixiEditor",
                        "shortcuts.json"),
                    this);

            Commands = new();
            CommandGroups = new();
            FactoryEvaluators = new();
            CanExecuteEvaluators = new();
            IconEvaluators = new();

            Init(services);
        }

        public void Init(IServiceProvider services)
        {
            KeyValuePair<KeyCombination, IEnumerable<string>>[] shortcuts = shortcutFile.GetShortcuts()?.ToArray()
                ?? Array.Empty<KeyValuePair<KeyCombination, IEnumerable<string>>>();

            var types = typeof(CommandController).Assembly.GetTypes();

            EnumerableDictionary<string, string> groups = new();
            EnumerableDictionary<string, Command> commandGroups = new();

            foreach (var type in types)
            {
                object instanceType = null;

                foreach (var group in type.GetCustomAttributes<CommandAttribute.GroupAttribute>())
                {
                    groups.Add(group.Display, group.Name);
                }

                var methods = type.GetMethods();

                foreach (var method in methods)
                {
                    var evaluatorAttrs = method.GetCustomAttributes<Evaluator.EvaluatorAttribute>();

                    if (instanceType is null && evaluatorAttrs.Any())
                    {
                        instanceType = services.GetService(type);
                    }

                    foreach (var attribute in evaluatorAttrs)
                    {
                        if (attribute is Evaluator.CanExecuteAttribute canExecute)
                        {
                            var required = (CommandController controller) => canExecute.Requires.Select(x => controller.CanExecuteEvaluators[x]);
                            AddEvaluatorFactory<Evaluator.CanExecuteAttribute, CanExecuteEvaluator, bool>(
                                method,
                                instanceType,
                                canExecute,
                                CanExecuteEvaluators,
                                x => new CanExecuteEvaluator()
                                {
                                    Name = attribute.Name, 
                                    Evaluate = y => x.Invoke(y) && required.Invoke(this).All(z => z.EvaluateEvaluator(null, y))
                                });
                        }
                        else if (attribute is Evaluator.FactoryAttribute factory)
                        {
                            AddEvaluator<Evaluator.FactoryAttribute, FactoryEvaluator, object>(method, instanceType, factory, FactoryEvaluators);
                        }
                        else if (attribute is Evaluator.IconAttribute icon)
                        {
                            AddEvaluator<Evaluator.IconAttribute, IconEvaluator, ImageSource>(method, instanceType, icon, IconEvaluators);
                        }
                    }
                }
            }

            foreach (var type in types)
            {
                object instanceType = null;
                var methods = type.GetMethods();

                foreach (var method in methods)
                {
                    var commandAttrs = method.GetCustomAttributes<CommandAttribute.CommandAttribute>();

                    if (instanceType is null && commandAttrs.Any())
                    {
                        instanceType = services.GetService(type);
                    }

                    foreach (var attribute in commandAttrs)
                    {
                        if (attribute is CommandAttribute.BasicAttribute basic)
                        {
                            AddCommand(method, instanceType, attribute, (isDebug, name, x, xCan, xIcon) => new Command.BasicCommand(x, xCan)
                            {
                                Name = name,
                                IsDebug = isDebug,
                                Display = attribute.Display,
                                Description = attribute.Description,
                                IconPath = attribute.Icon,
                                IconEvaluator = xIcon,
                                DefaultShortcut = attribute.GetShortcut(),
                                Shortcut = GetShortcut(name, attribute.GetShortcut()),
                                Parameter = basic.Parameter,
                            });
                        }
                    }
                }

                if (type.IsAssignableTo(typeof(Tool)))
                {
                    var toolAttr = type.GetCustomAttribute<CommandAttribute.ToolAttribute>();

                    if (toolAttr != null)
                    {
                        var tool = services.GetServices<Tool>().First(x => x.GetType() == type);
                        string name = $"PixiEditor.Tools.Select.{type.Name}";

                        var command = new Command.ToolCommand()
                        {
                            Name = name,
                            Display = $"Select {tool.DisplayName} Tool",
                            Description = $"Select {tool.DisplayName} Tool",
                            IconPath = $"@{tool.ImagePath}",
                            IconEvaluator = IconEvaluator.Default,
                            TransientKey = toolAttr.Transient,
                            DefaultShortcut = toolAttr.GetShortcut(),
                            Shortcut = GetShortcut(name, toolAttr.GetShortcut()),
                            ToolType = type,
                        };

                        Commands.Add(command);
                        AddCommandToGroup(command);
                    }
                }
            }

            foreach (var commands in commandGroups)
            {
                CommandGroups.Add(new(commands.Key, commands.Value));
            }

            KeyCombination GetShortcut(string name, KeyCombination defaultShortcut) => shortcuts.FirstOrDefault(x => x.Value.Contains(name), new(defaultShortcut, null)).Key;

            void AddCommandToGroup(Command command)
            {
                var display = groups.FirstOrDefault(x => x.Value.Any(x => command.Name.StartsWith(x))).Key;
                if (display == null)
                {
                    display = "Misc";
                }
                commandGroups.Add(display, command);
            }

            void AddEvaluator<TAttr, T, TParameter>(MethodInfo method, object instance, TAttr attribute, IDictionary<string, T> evaluators)
                where T : Evaluator<TParameter>, new()
                where TAttr : Evaluator.EvaluatorAttribute
                => AddEvaluatorFactory<TAttr, T, TParameter>(method, instance, attribute, evaluators, x => new T() { Name = attribute.Name, Evaluate = x });

            void AddEvaluatorFactory<TAttr, T, TParameter>(MethodInfo method, object instance, TAttr attribute, IDictionary<string, T> evaluators, Func<Func<object, TParameter>, T> factory)
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
                else if (!method.IsStatic && instance is null)
                {
                    throw new Exception($"No type instance for the CanExecute evaluator '{attribute.Name}' at {method.ReflectedType.FullName}.{method.Name} found");
                }

                var parameters = method.GetParameters();

                Func<object, TParameter> func;

                if (parameters.Length == 1)
                {
                    func = x => (TParameter)method.Invoke(instance, new[] { Convert.ChangeType(x, parameters[0].ParameterType) });
                }
                else
                {
                    func = x => (TParameter)method.Invoke(instance, null);
                }

                T evaluator = factory(func);

                evaluators.Add(evaluator.Name, evaluator);
            }

            TCommand AddCommand<TAttr, TCommand>(MethodInfo method, object instance, TAttr attribute, Func<bool, string, Action<object>, CanExecuteEvaluator, IconEvaluator, TCommand> commandFactory)
                where TAttr : CommandAttribute.CommandAttribute
                where TCommand : Command
            {
                if (method != null)
                {
                    if (method.GetParameters().Length > 1)
                    {
                        throw new Exception($"Too many parameters for the CanExecute evaluator '{attribute.Name}' at {method.ReflectedType.FullName}.{method.Name}");
                    }
                    else if (!method.IsStatic && instance is null)
                    {
                        throw new Exception($"No type instance for the CanExecute evaluator '{attribute.Name}' at {method.ReflectedType.FullName}.{method.Name} found");
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

                string name = attribute.Name;
                bool isDebug = attribute.Name.StartsWith("#DEBUG#");

                if (attribute.Name.StartsWith("#DEBUG#"))
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
                AddCommandToGroup(command);

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
        /// Delets all shortcuts of <paramref name="newShortcut"/> and adds <paramref name="command"/>
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
            Commands.ClearShortcuts();
        }
    }
}
