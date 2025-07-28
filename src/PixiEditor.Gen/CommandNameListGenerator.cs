using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PixiEditorGen;

[Generator(LanguageNames.CSharp)]
public class CommandNameListGenerator : IIncrementalGenerator
{
    private const string Commands = "PixiEditor.Models.Commands.Attributes.Commands";

    private const string Evaluators = "PixiEditor.Models.Commands.Attributes.Evaluators";

    private const string Groups = "PixiEditor.Models.Commands.Attributes.Commands";

    private const string InternalNameAttribute = "PixiEditor.Models.Commands.Attributes.InternalNameAttribute";

    private static DiagnosticDescriptor commandDuplicate = new("Pixi01", "Command/Evaluator duplicate", "{0} with name '{1}' is defined multiple times", "PixiEditor.Commands", DiagnosticSeverity.Error, true);
    
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var commandList = CreateSyntaxProvider<CommandMethod>(context, Commands).Where(x => x != null);
        var evaluatorList = CreateSyntaxProvider<CommandMethod>(context, Evaluators).Where(x => x != null);
        var groupList = CreateSyntaxProvider<GroupType>(context, Groups).Where(x => x != null);

        context.RegisterSourceOutput(commandList.Collect(), (context, commands) => AddSource(context, commands, "Commands"));
        context.RegisterSourceOutput(evaluatorList.Collect(), (context, evaluators) => AddSource(context, evaluators, "Evaluators"));
        context.RegisterSourceOutput(groupList.Collect(), AddGroupsSource);
    }

    private IncrementalValuesProvider<T?> CreateSyntaxProvider<T>(IncrementalGeneratorInitializationContext context, string className) where T : CommandMember<T>
    {
        return context.SyntaxProvider.CreateSyntaxProvider(
            (x, token) =>
            {
                if (typeof(T) == typeof(CommandMethod))
                {
                    return x is MethodDeclarationSyntax { AttributeLists.Count: > 0 };
                }

                return x is TypeDeclarationSyntax { AttributeLists.Count: > 0 };
            }, (context, cancelToken) =>
            {
                var member = (MemberDeclarationSyntax)context.Node;

                var attributes = GetCommandAttributes(member, context, cancelToken, className);
                if (attributes.Count == 0)
                    return null;

                var symbol = context.SemanticModel.GetDeclaredSymbol(member, cancelToken);

                if (symbol is IMethodSymbol methodSymbol && typeof(T) == typeof(CommandMethod))
                {
                    if (methodSymbol.ReceiverType == null)
                        return null;

                    return (T)(object)new CommandMethod(attributes, methodSymbol);
                }
                else if (symbol is ITypeSymbol typeSymbol && typeof(T) == typeof(GroupType))
                {
                    return (T)(object)new GroupType(typeSymbol);
                }
                else
                {
                    return null;
                }
            });
    }

    private void AddSource(SourceProductionContext context, ImmutableArray<CommandMethod> methodNames, string name)
    {
        if (ReportDuplicateDefinitions(context, methodNames, name))
            return;
        
        var createdClasses = new List<string>();
        var statements = new SyntaxList<StatementSyntax>();

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

    private bool ReportDuplicateDefinitions(SourceProductionContext context, ImmutableArray<CommandMethod> methodNames, string name)
    {
        var hasDuplicate = false;
        var allAttributes = methodNames.SelectMany(x => x.Attributes).ToArray();
        
        foreach (var attribute in allAttributes)
        {
            if (!allAttributes.Any(x => x != attribute && x.InternalName == attribute.InternalName))
            {
                continue;
            }

            context.ReportDiagnostic(Diagnostic.Create(commandDuplicate, attribute.InternalNameArgument?.GetLocation(), name.TrimEnd('s'), attribute.InternalName));
            hasDuplicate = true;
        }

        return hasDuplicate;
    }

    private void AddGroupsSource(SourceProductionContext context, ImmutableArray<GroupType> groups)
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

    private static List<CommandAttribute> GetCommandAttributes(MemberDeclarationSyntax declaration, GeneratorSyntaxContext context, CancellationToken token, string commandAttributeStart)
    {
        var list = new List<CommandAttribute>();
        
        foreach (var attribute in declaration.AttributeLists.SelectMany(attrList => attrList.Attributes))
        {
            token.ThrowIfCancellationRequested();
            var symbol = context.SemanticModel.GetSymbolInfo(attribute, token);
            if (symbol.Symbol is not IMethodSymbol methodSymbol)
                continue;
            if (!methodSymbol.ContainingType.ToDisplayString()
                    .StartsWith(commandAttributeStart))
                continue;

            var target = -1;
                
            for (var i = 0; i < methodSymbol.Parameters.Length; i++)
            {
                var parameter = methodSymbol.Parameters[i];
                if (parameter.GetAttributes().Any(x => x.AttributeClass?.ToDisplayString() == InternalNameAttribute))
                {
                    target = i;
                    break;
                }
            }

            if (target != -1)
            {
                var argument = attribute.ArgumentList?.Arguments[target];

                if (argument?.Expression is LiteralExpressionSyntax literal)
                {
                    list.Add(new CommandAttribute(argument, literal.Token.ValueText));
                }
                else
                {
                    list.Add(new CommandAttribute(argument, null));
                }
            }
        }

        return list;
    }

    class CommandMember<TSelf> where TSelf : CommandMember<TSelf>
    {
        public string OwnerTypeName { get; }

        public CommandMember(string ownerTypeName)
        {
            OwnerTypeName = ownerTypeName;
        }
    }

    class CommandMethod : CommandMember<CommandMethod>
    {
        public string MethodName { get; }

        public string[] ParameterTypeNames { get; }
        
        public List<CommandAttribute> Attributes { get; }

        public CommandMethod(List<CommandAttribute> attributes, IMethodSymbol symbol) : base(symbol.ContainingType.ToDisplayString())
        {
            Attributes = attributes;
            MethodName = symbol.Name;
            ParameterTypeNames = symbol.Parameters.Select(x => $"typeof({x.Type.ToDisplayString()})").ToArray();
        }
    }

    class CommandAttribute
    {
        public string? InternalName { get; }

        public AttributeArgumentSyntax? InternalNameArgument { get; }
        
        public CommandAttribute(AttributeArgumentSyntax? internalNameArgument, string? internalName)
        {
            InternalNameArgument = internalNameArgument;
            InternalName = internalName;
        }
    }

    class GroupType : CommandMember<GroupType>
    {
        public GroupType(ITypeSymbol symbol) : base(symbol.ToDisplayString())
        { }
    }
}
