using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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

        context.RegisterSourceOutput(commandList.Collect(), AddCommands);
        context.RegisterSourceOutput(evaluatorList.Collect(), AddEvaluators);
        context.RegisterSourceOutput(groupList.Collect(), AddGroups);
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

    private void AddCommands(SourceProductionContext context, ImmutableArray<Command> methodNames)
    {
        List<string> createdClasses = new List<string>();
        SyntaxList<StatementSyntax> statements = new SyntaxList<StatementSyntax>();

        foreach (var methodName in methodNames)
        {
            if (!createdClasses.Contains(methodName.OwnerTypeName))
            {
                statements = statements.Add(SyntaxFactory.ParseStatement($"Commands.Add(typeof({methodName.OwnerTypeName}), new());"));
                createdClasses.Add(methodName.OwnerTypeName);
            }

            var parameters = string.Join(",", methodName.ParameterTypeNames);

            bool hasParameters = parameters.Length > 0;
            string paramString = hasParameters ? $"new Type[] {{ {parameters} }}" : "Array.Empty<Type>()";

            statements = statements.Add(SyntaxFactory.ParseStatement($"Commands[typeof({methodName.OwnerTypeName})].Add((\"{methodName.MethodName}\", {paramString}));"));
        }

        var method = SyntaxFactory
            .MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "AddCommands")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
            .WithBody(SyntaxFactory.Block(statements));

        var cDecl = SyntaxFactory
            .ClassDeclaration("CommandNameList")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.InternalKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword))
            .AddMembers(method);

        var nspace = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName("PixiEditor.Models.Commands"))
            .AddMembers(cDecl);

        context.AddSource("CommandNameList+Commands", nspace.NormalizeWhitespace().ToFullString());
    }

    private void AddEvaluators(SourceProductionContext context, ImmutableArray<Command> methodNames)
    {
        List<string> createdClasses = new List<string>();
        SyntaxList<StatementSyntax> statements = new SyntaxList<StatementSyntax>();

        foreach (var methodName in methodNames)
        {
            if (!createdClasses.Contains(methodName.OwnerTypeName))
            {
                statements = statements.Add(SyntaxFactory.ParseStatement($"Evaluators.Add(typeof({methodName.OwnerTypeName}), new());"));
                createdClasses.Add(methodName.OwnerTypeName);
            }

            if (methodName.ParameterTypeNames == null || !methodName.ParameterTypeNames.Any())
            {
                statements = statements.Add(SyntaxFactory.ParseStatement($"Evaluators[typeof({methodName.OwnerTypeName})].Add((\"{methodName.MethodName}\", Array.Empty<Type>()));"));
            }
            else
            {
                var parameters = string.Join(",", methodName.ParameterTypeNames);
                string paramString = parameters.Length > 0 ? $"new Type[] {{ {parameters} }}" : "Array.Empty<Type>()";
                statements = statements.Add(SyntaxFactory.ParseStatement($"Evaluators[typeof({methodName.OwnerTypeName})].Add((\"{methodName.MethodName}\", {paramString}));"));
            }
        }

        var method = SyntaxFactory
            .MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "AddEvaluators")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
            .WithBody(SyntaxFactory.Block(statements));

        var cDecl = SyntaxFactory
            .ClassDeclaration("CommandNameList")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.InternalKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword))
            .AddMembers(method);

        var nspace = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName("PixiEditor.Models.Commands"))
            .AddMembers(cDecl);

        context.AddSource("CommandNameList+Evaluators", nspace.NormalizeWhitespace().ToFullString());
    }

    private void AddGroups(SourceProductionContext context, ImmutableArray<string?> typeNames)
    {
        SyntaxList<StatementSyntax> statements = new SyntaxList<StatementSyntax>();

        foreach (var name in typeNames)
        {
            statements = statements.Add(SyntaxFactory.ParseStatement($"Groups.Add(typeof({name}));"));
        }

        var method = SyntaxFactory
        .MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "AddGroups")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
            .WithBody(SyntaxFactory.Block(statements));

        var cDecl = SyntaxFactory
            .ClassDeclaration("CommandNameList")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.InternalKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword))
            .AddMembers(method);

        var nspace = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName("PixiEditor.Models.Commands"))
            .AddMembers(cDecl);

        context.AddSource("CommandNameList+Groups", nspace.NormalizeWhitespace().ToFullString());
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
