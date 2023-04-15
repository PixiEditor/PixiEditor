using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PixiEditorGen;

[Generator(LanguageNames.CSharp)]
public class CommandNameListGenerator : IIncrementalGenerator
{
    private const string Commands = "PixiEditor.Models.Commands.Attributes.Commands";

    private const string Evaluators = "PixiEditor.Models.Commands.Attributes.Evaluators.Evaluator";

    private const string Groups = "PixiEditor.Models.Commands.Attributes.Commands.Command.GroupAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var commandList = CreateSyntaxProvider(context, Commands).Where(x => !x.IsNone);
        var evaluatorList = CreateSyntaxProvider(context, Evaluators).Where(x => !x.IsNone);
        var groupList = CreateGroupSyntaxProvider(context).Where(x => x != null);

        context.RegisterSourceOutput(commandList.Collect(), static (context, methodNames) =>
        {
            var code = new StringBuilder(
                """
                namespace PixiEditor.Models.Commands;

                internal partial class CommandNameList {
                    partial void AddCommands() {
                """);

            List<string> createdClasses = new List<string>();

            foreach (var method in methodNames)
            {
                if (!createdClasses.Contains(method.OwnerTypeName))
                {
                    code.AppendLine($"      Commands.Add(typeof({method.OwnerTypeName}), new());");
                    createdClasses.Add(method.OwnerTypeName);
                }

                var parameters = string.Join(",", method.ParameterTypeNames);

                bool hasParameters = parameters.Length > 0;
                string paramString = hasParameters ? $"new Type[] {{ {parameters} }}" : "Array.Empty<Type>()";

                code.AppendLine($"      Commands[typeof({method.OwnerTypeName})].Add((\"{method.MethodName}\", {paramString}));");
            }

            code.Append("   }\n}");

            context.AddSource("CommandNameList+Commands", code.ToString());
        });

        context.RegisterSourceOutput(evaluatorList.Collect(), static (context, methodNames) =>
        {
            var code = new StringBuilder(
                """
                namespace PixiEditor.Models.Commands;

                internal partial class CommandNameList {
                    partial void AddEvaluators() {
                """);

            List<string> createdClasses = new List<string>();

            foreach (var method in methodNames)
            {
                if (!createdClasses.Contains(method.OwnerTypeName))
                {
                    code.AppendLine($"      Evaluators.Add(typeof({method.OwnerTypeName}), new());");
                    createdClasses.Add(method.OwnerTypeName);
                }

                if (method.ParameterTypeNames == null || !method.ParameterTypeNames.Any())
                {
                    code.AppendLine($"      Evaluators[typeof({method.OwnerTypeName})].Add((\"{method.MethodName}\", Array.Empty<Type>()));");
                }
                else
                {
                    var parameters = string.Join(",", method.ParameterTypeNames);
                    string paramString = parameters.Length > 0 ? $"new Type[] {{ {parameters} }}" : "Array.Empty<Type>()";
                    code.AppendLine($"      Evaluators[typeof({method.OwnerTypeName})].Add((\"{method.MethodName}\", {paramString}));");
                }
            }

            code.Append("   }\n}");

            File.WriteAllText(@"C:\Users\phili\Documents\Evals.txt", code.ToString());

            context.AddSource("CommandNameList+Evaluators", code.ToString());
        });

        context.RegisterSourceOutput(groupList.Collect(), static (context, typeNames) =>
        {
            var code = new StringBuilder(
                @"namespace PixiEditor.Models.Commands;

internal partial class CommandNameList {
    partial void AddGroups() {");

            foreach (var name in typeNames)
            {
                code.AppendLine($"      Groups.Add(typeof({name}));");
            }

            code.Append("   }\n}");

            context.AddSource("CommandNameList+Groups", code.ToString());
        });
    }

    private IncrementalValuesProvider<Command> CreateSyntaxProvider(IncrementalGeneratorInitializationContext context, string className)
    {
        return context.SyntaxProvider.CreateSyntaxProvider(
            (x, token) =>
            {
                return x is MethodDeclarationSyntax method && method.AttributeLists.Count > 0;
            }, (context, cancelToken) =>
            {
                var method = (MethodDeclarationSyntax)context.Node;

                if (!HasCommandAttribute(method, context, cancelToken, className))
                    return Command.None;

                var symbol = context.SemanticModel.GetDeclaredSymbol(method, cancelToken);

                if (symbol is IMethodSymbol methodSymbol)
                {
                    if (methodSymbol.ReceiverType == null)
                        return Command.None;

                    return new Command(methodSymbol);
                }
                else
                {
                    return Command.None;
                }
            });
    }

    private IncrementalValuesProvider<string?> CreateGroupSyntaxProvider(IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider.CreateSyntaxProvider(
            (x, token) =>
            {
                return x is TypeDeclarationSyntax type && type.AttributeLists.Count > 0;
            }, static (context, cancelToken) =>
            {
                var method = (TypeDeclarationSyntax)context.Node;

                if (!HasCommandAttribute(method, context, cancelToken, Groups))
                    return null;

                var symbol = context.SemanticModel.GetDeclaredSymbol(method, cancelToken);

                if (symbol is ITypeSymbol methodSymbol)
                {
                    return methodSymbol.ToDisplayString();
                }
                else
                {
                    return null;
                }
            });
    }

    private static bool HasCommandAttribute(MemberDeclarationSyntax declaration, GeneratorSyntaxContext context, CancellationToken token, string commandAttributeStart)
    {
        foreach (var attrList in declaration.AttributeLists)
        {
            foreach (var attribute in attrList.Attributes)
            {
                token.ThrowIfCancellationRequested();
                var symbol = context.SemanticModel.GetSymbolInfo(attribute, token);
                if (symbol.Symbol is not IMethodSymbol methodSymbol)
                    continue;
                if (!methodSymbol.ContainingType.ToDisplayString()
                    .StartsWith(commandAttributeStart))
                    continue;
                return true;
            }
        }

        return false;
    }

    readonly struct Command
    {
        public string OwnerTypeName { get; }

        public string MethodName { get; }

        public string[] ParameterTypeNames { get; }

        public Command(IMethodSymbol symbol)
        {
            OwnerTypeName = symbol.ContainingType.ToDisplayString();
            MethodName = symbol.Name;
            ParameterTypeNames = symbol.Parameters.Select(x => $"typeof({x.Type.ToDisplayString()})").ToArray();
        }

        public bool IsNone => OwnerTypeName == null;

        public static Command None => default;
    }
}
