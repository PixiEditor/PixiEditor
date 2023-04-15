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
        var commandList = CreateSyntaxProvider<Command>(context, Commands).Where(x => x != null);
        var evaluatorList = CreateSyntaxProvider<Command>(context, Evaluators).Where(x => x != null);
        var groupList = CreateSyntaxProvider<Group>(context, Groups).Where(x => x != null);

        context.RegisterSourceOutput(commandList.Collect(), (context, commands) => AddSource(context, commands, "Commands"));
        context.RegisterSourceOutput(evaluatorList.Collect(), (context, evaluators) => AddSource(context, evaluators, "Evaluators"));
        context.RegisterSourceOutput(groupList.Collect(), AddGroupsSource);
    }

    private IncrementalValuesProvider<T?> CreateSyntaxProvider<T>(IncrementalGeneratorInitializationContext context, string className) where T : CommandMember<T>
    {
        return context.SyntaxProvider.CreateSyntaxProvider(
            (x, token) =>
            {
                if (typeof(T) == typeof(Command))
                {
                    return x is MethodDeclarationSyntax method && method.AttributeLists.Count > 0;
                }
                else
                {
                    return x is TypeDeclarationSyntax type && type.AttributeLists.Count > 0;
                }
            }, (context, cancelToken) =>
            {
                var member = (MemberDeclarationSyntax)context.Node;

                if (!HasCommandAttribute(member, context, cancelToken, className))
                    return null;

                var symbol = context.SemanticModel.GetDeclaredSymbol(member, cancelToken);

                if (symbol is IMethodSymbol methodSymbol && typeof(T) == typeof(Command))
                {
                    if (methodSymbol.ReceiverType == null)
                        return null;

                    return (T)(object)new Command(methodSymbol);
                }
                else if (symbol is ITypeSymbol typeSymbol && typeof(T) == typeof(Group))
                {
                    return (T)(object)new Group(typeSymbol);
                }
                else
                {
                    return null;
                }
            });
    }

    private void AddSource(SourceProductionContext context, ImmutableArray<Command> methodNames, string name)
    {
        List<string> createdClasses = new List<string>();
        SyntaxList<StatementSyntax> statements = new SyntaxList<StatementSyntax>();

        foreach (var methodName in methodNames)
        {
            if (!createdClasses.Contains(methodName.OwnerTypeName))
            {
                statements = statements.Add(SyntaxFactory.ParseStatement($"{name}.Add(typeof({methodName.OwnerTypeName}), new());"));
                createdClasses.Add(methodName.OwnerTypeName);
            }

            var parameters = string.Join(",", methodName.ParameterTypeNames);
            string paramString = parameters.Length > 0 ? $"new Type[] {{ {parameters} }}" : "Array.Empty<Type>()";

            statements = statements.Add(SyntaxFactory.ParseStatement($"{name}[typeof({methodName.OwnerTypeName})].Add((\"{methodName.MethodName}\", {paramString}));"));
        }

        // partial void Add$name$()
        var method = SyntaxFactory
            .MethodDeclaration(SyntaxFactory.ParseTypeName("void"), $"Add{name}")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
            .WithBody(SyntaxFactory.Block(statements));

        // internal partial class CommandNameList
        var cDecl = SyntaxFactory
            .ClassDeclaration("CommandNameList")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.InternalKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword))
            .AddMembers(method);

        // namespace PixiEditor.Models.Commands
        var nspace = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName("PixiEditor.Models.Commands"))
            .AddMembers(cDecl);

        context.AddSource($"CommandNameList+{name}", nspace.NormalizeWhitespace().ToFullString());
    }

    private void AddGroupsSource(SourceProductionContext context, ImmutableArray<Group> groups)
    {
        SyntaxList<StatementSyntax> statements = new SyntaxList<StatementSyntax>();

        foreach (var group in groups)
        {
            statements = statements.Add(SyntaxFactory.ParseStatement($"Groups.Add(typeof({group.OwnerTypeName}));"));
        }

        // partial void AddGroups()
        var method = SyntaxFactory
            .MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "AddGroups")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
            .WithBody(SyntaxFactory.Block(statements));

        // internal partial class CommandNameList
        var cDecl = SyntaxFactory
            .ClassDeclaration("CommandNameList")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.InternalKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword))
            .AddMembers(method);

        // namespace PixiEditor.Models.Commands
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

    class CommandMember<TSelf> where TSelf : CommandMember<TSelf>
    {
        public string OwnerTypeName { get; }

        public CommandMember(string ownerTypeName)
        {
            OwnerTypeName = ownerTypeName;
        }
    }

    class Command : CommandMember<Command>
    {
        public string MethodName { get; }

        public string[] ParameterTypeNames { get; }

        public Command(IMethodSymbol symbol) : base(symbol.ContainingType.ToDisplayString())
        {
            MethodName = symbol.Name;
            ParameterTypeNames = symbol.Parameters.Select(x => $"typeof({x.Type.ToDisplayString()})").ToArray();
        }
    }

    class Group : CommandMember<Group>
    {
        public Group(ITypeSymbol symbol) : base(symbol.ToDisplayString())
        { }
    }
}
