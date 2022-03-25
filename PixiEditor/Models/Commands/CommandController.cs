using PixiEditor.Models.Commands.Attributes;
using PixiEditor.Models.Commands.Evaluators;
using System.Reflection;
using CommandAttribute = PixiEditor.Models.Commands.Attributes.Command;

namespace PixiEditor.Models.Commands
{
    public class CommandController
    {
        public CommandCollection Commands { get; set; }

        public Dictionary<string, FactoryEvaluator> FactoryEvaluators { get; set; }

        public Dictionary<string, CanExecuteEvaluator> CanExecuteEvaluators { get; set; }

        public CommandController(IServiceProvider services)
        {
            Commands = new();
            FactoryEvaluators = new();
            CanExecuteEvaluators = new();

            Init(services);
        }

        public void Init(IServiceProvider services)
        {
            var types = typeof(CommandController).Assembly.GetTypes();

            foreach (var type in types)
            {
                object instanceType = null;
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
                            AddEvaluator<Evaluator.CanExecuteAttribute, CanExecuteEvaluator, bool>(method, instanceType, canExecute, CanExecuteEvaluators);
                        }
                        else if (attribute is Evaluator.FactoryAttribute factory)
                        {
                            AddEvaluator<Evaluator.FactoryAttribute, FactoryEvaluator, object>(method, instanceType, factory, FactoryEvaluators);
                        }
                    }
                }

                foreach (var method in methods)
                {
                    var commandAttrs = method.GetCustomAttributes<CommandAttribute.BasicAttribute>();

                    if (instanceType is null && commandAttrs.Any())
                    {
                        instanceType = services.GetService(type);
                    }

                    foreach (var attribute in commandAttrs)
                    {
                        AddCommand(method, instanceType, attribute, (x, xCan) => new Command.BasicCommand
                        {
                            Name = attribute.Name,
                            Display = attribute.Display,
                            Description = attribute.Description,
                            Methods = new(x, xCan),
                            DefaultShortcut = attribute.GetShortcut(),
                            Parameter = attribute.Parameter,
                            Shortcut = attribute.GetShortcut(),
                        });
                    }
                }
            }

            void AddEvaluator<TAttr, T, TParameter>(MethodInfo method, object instance, TAttr attribute, IDictionary<string, T> evaluators)
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

                T evaluator = new()
                {
                    Name = attribute.Name,
                    Evaluate = func
                };

                evaluators.Add(evaluator.Name, evaluator);
            }

            void AddCommand<TAttr, TCommand>(MethodInfo method, object instance, TAttr attribute, Func<Action<object>, Predicate<object>, TCommand> commandFactory)
                where TAttr : CommandAttribute.CommandAttribute
                where TCommand : Command
            {if (method.GetParameters().Length > 1)
                {
                    throw new Exception($"Too many parameters for the CanExecute evaluator '{attribute.Name}' at {method.ReflectedType.FullName}.{method.Name}");
                }
                else if (!method.IsStatic && instance is null)
                {
                    throw new Exception($"No type instance for the CanExecute evaluator '{attribute.Name}' at {method.ReflectedType.FullName}.{method.Name} found");
                }

                var parameters = method.GetParameters();

                Action<object> action;

                if (parameters.Length == 1)
                {
                    action = x => method.Invoke(instance, new[] { x });
                }
                else
                {
                    action = x => method.Invoke(instance, null);
                }

                Commands.Add(commandFactory(action, x => CanExecuteEvaluators[attribute.CanExecute].Evaluate(x)));
            }
        }
    }
}
