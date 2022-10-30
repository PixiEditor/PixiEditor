using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PixiEditorGen;

[Generator(LanguageNames.CSharp)]
public class CommandNameListGenerator : IIncrementalGenerator
{
    private const string Command = "PixiEditor.Models.Commands.Attributes.Commands";

    private const string Evaluators = "PixiEditor.Models.Commands.Attributes.Evaluators.Evaluator";
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var commandList = context.SyntaxProvider.CreateSyntaxProvider(
            (x, token) =>
        {
            return x is MethodDeclarationSyntax method && method.AttributeLists.Count > 0;
        }, static (context, cancelToken) =>
        {
            var method = (MethodDeclarationSyntax)context.Node;

            if (!HasCommandAttribute(method, context, cancelToken, Command))
                return (null, null, null);

            var symbol = context.SemanticModel.GetDeclaredSymbol(method, cancelToken);

            if (symbol is IMethodSymbol methodSymbol)
            {
                if (methodSymbol.ReceiverType == null)
                    return (null, null, null);
                
                return (methodSymbol.ReceiverType.ToDisplayString(), methodSymbol.Name, methodSymbol.Parameters.Select(x => x.ToDisplayString()));
            }
            else
            {
                return (null, null, null);
            }
        }).Where(x => x.Item1 != null);

        var evaluatorList = context.SyntaxProvider.CreateSyntaxProvider(
            (x, token) =>
            {
                return x is MethodDeclarationSyntax method && method.AttributeLists.Count > 0;
            }, static (context, cancelToken) =>
            {
                var method = (MethodDeclarationSyntax)context.Node;

                if (!HasCommandAttribute(method, context, cancelToken, Evaluators))
                    return (null, null, null);

                var symbol = context.SemanticModel.GetDeclaredSymbol(method, cancelToken);

                if (symbol is IMethodSymbol methodSymbol)
                {
                    return (methodSymbol.ReceiverType.ToDisplayString(), methodSymbol.Name, methodSymbol.Parameters.Select(x => x.ToDisplayString()));
                }
                else
                {
                    return (null, null, null);
                }
            }).Where(x => x.Item1 != null);
        
        var groupList = context.SyntaxProvider.CreateSyntaxProvider(
            (x, token) =>
            {
                return x is MethodDeclarationSyntax method && method.AttributeLists.Count > 0;
            }, static (context, cancelToken) =>
            {
                var method = (MethodDeclarationSyntax)context.Node;

                if (!HasCommandAttribute(method, context, cancelToken, Evaluators))
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
            }).Where(x => x != null);

        context.RegisterSourceOutput(commandList.Collect(), static (context, methodNames) =>
        {
            var code = new StringBuilder(
                @"namespace PixiEditor.Models.Commands;

internal partial class CommandNameList {
    partial void AddCommands() {");

            List<string> createdClasses = new List<string>();

            foreach (var method in methodNames)
            {
                if (!createdClasses.Contains(method.Item1))
                {
                    code.AppendLine($"      Commands.Add(typeof({method.Item1}), new());");
                    createdClasses.Add(method.Item1);
                }

                var parameters = string.Join(",", method.Item3.Select(x => $"typeof({x})"));
                
                code.AppendLine($"      Commands[typeof({method.Item1})].Add((\"{method.Item2}\", new Type[] {{ {parameters} }}));");
            }

            code.Append("   }\n}");

            context.AddSource("CommandNameList+Commands", code.ToString());
        });
        
        context.RegisterSourceOutput(evaluatorList.Collect(), static (context, methodNames) =>
        {
            var code = new StringBuilder(
                @"namespace PixiEditor.Models.Commands;

internal partial class CommandNameList {
    partial void AddEvaluators() {");

            List<string> createdClasses = new List<string>();

            foreach (var method in methodNames)
            {
                if (!createdClasses.Contains(method.Item1))
                {
                    code.AppendLine($"      Evaluators.Add(typeof({method.Item1}), new());");
                    createdClasses.Add(method.Item1);
                }

                if (method.Item3 == null || !method.Item3.Any())
                {
                    code.AppendLine($"      Evaluators[typeof({method.Item1})].Add((\"{method.Item2}\", Array.Empty<Type>()));");
                }
                else
                {
                    var parameters = string.Join(",", method.Item3.Select(x => $"typeof({x})"));
                
                    code.AppendLine($"      Evaluators[typeof({method.Item1})].Add((\"{method.Item2}\", new Type[] {{ {parameters} }}));");
                }
            }

            code.Append("   }\n}");

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
    
    private static bool HasCommandAttribute(MethodDeclarationSyntax method, GeneratorSyntaxContext context, CancellationToken token, string commandAttributeStart)
    {
        foreach (var attrList in method.AttributeLists)
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
}
