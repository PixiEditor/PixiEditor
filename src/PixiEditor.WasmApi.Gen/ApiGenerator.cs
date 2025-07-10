using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PixiEditor.WasmApi.Gen;

[Generator(LanguageNames.CSharp)]
public class ApiGenerator : IIncrementalGenerator
{
    private const string FullyQualifiedApiFunctionAttributeName =
        "PixiEditor.Extensions.WasmRuntime.ApiFunctionAttribute";

    private const string ApiFunctionAttributeName = "ApiFunctionAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var methods = context.SyntaxProvider.ForAttributeWithMetadataName(
                FullyQualifiedApiFunctionAttributeName,
                (_, _) => true,
                GetApiFunctionMethodOrNull)
            .Where(x => x is not null)
            .Collect();

        context.RegisterSourceOutput(methods, GenerateLinkerCode);
    }

    private void GenerateLinkerCode(SourceProductionContext ctx, ImmutableArray<(IMethodSymbol methodSymbol, SemanticModel SemanticModel)?> symbols)
    {
        List<StatementSyntax> linkingMethodsCode = new List<StatementSyntax>();

        foreach (var symbol in symbols)
        {
            if(!symbol.HasValue) continue;
            if (symbol.Value.methodSymbol == null) continue;

            linkingMethodsCode.Add(GenerateLinkingCodeForMethod(symbol.Value));
        }

        // partial void LinkApiFunctions()
        var methodDeclaration = SyntaxFactory
            .MethodDeclaration(SyntaxFactory.ParseTypeName("void"), $"LinkApiFunctions")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
            .WithBody(SyntaxFactory.Block(linkingMethodsCode));

        // internal partial class WasmExtensionInstance
        var cDecl = SyntaxFactory
            .ClassDeclaration("WasmExtensionInstance")
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword))
            .AddMembers(methodDeclaration);

        // namespace PixiEditor.Extensions.WasmRuntime
        var nspace = SyntaxFactory
            .NamespaceDeclaration(SyntaxFactory.ParseName("PixiEditor.Extensions.WasmRuntime"))
            .AddMembers(cDecl);

        ctx.AddSource($"WasmExtensionInstance+ApiFunctions", nspace.NormalizeWhitespace().ToFullString());
    }

    private StatementSyntax GenerateLinkingCodeForMethod((IMethodSymbol methodSymbol, SemanticModel SemanticModel) symbol)
    {
        string name = $"{symbol.methodSymbol.GetAttributes()[0].ConstructorArguments[0].ToCSharpString()}";

        ImmutableArray<IParameterSymbol> arguments = symbol.methodSymbol.Parameters;

        List<string> convertedParams = new List<string>();
        foreach (var argSymbol in arguments)
        {
            convertedParams.AddRange(TypeConversionTable.ConvertTypeToFunctionParams(argSymbol));
        }

        ParameterListSyntax paramList = SyntaxFactory.ParseParameterList(string.Join(",", convertedParams));

        SyntaxList<StatementSyntax> statements = new SyntaxList<StatementSyntax>();

        SyntaxList<StatementSyntax> variableStatements = BuildVariableStatements(arguments);


        statements = statements.AddRange(variableStatements);
        statements = statements.AddRange(BuildFunctionBody(symbol));

        BlockSyntax body = SyntaxFactory.Block(statements);

        var parameters = SyntaxFactory.ParameterList(paramList.Parameters);

        var define = SyntaxFactory.ParseStatement(
            $"Linker.DefineFunction(\"env\", {name}, {parameters.ToFullString()} => \n{body.ToFullString()});");

        return define;
    }

    private SyntaxList<StatementSyntax> BuildVariableStatements(ImmutableArray<IParameterSymbol> arguments)
    {
        SyntaxList<StatementSyntax> syntaxes = new SyntaxList<StatementSyntax>();

        foreach (var argSymbol in arguments)
        {
            if (!TypeConversionTable.IsValuePassableType(argSymbol.Type, out _))
            {
                string lowerType = argSymbol.Type.Name;
                bool isLengthType = TypeConversionTable.IsLengthType(argSymbol);
                string paramsString = isLengthType
                    ? $"{argSymbol.Name}Pointer, {argSymbol.Name}Length"
                    : $"{argSymbol.Name}Pointer";
                syntaxes = syntaxes.Add(SyntaxFactory.ParseStatement(
                    $"{argSymbol.Type.ToDisplayString()} {argSymbol.Name} = WasmMemoryUtility.Get{lowerType}({paramsString});"));
                continue;
            }

            if (TypeConversionTable.RequiresConversion(argSymbol.Type))
            {
                string lowerType = argSymbol.Type.Name;
                string paramsString = $"{argSymbol.Name}Raw";
                
                syntaxes = syntaxes.Add(SyntaxFactory.ParseStatement(
                    $"{argSymbol.Type.ToDisplayString()} {argSymbol.Name} = WasmMemoryUtility.Convert{lowerType}({paramsString});"));
            }
        }

        return syntaxes;
    }

    private SyntaxList<StatementSyntax> BuildFunctionBody((IMethodSymbol methodSymbol, SemanticModel SemanticModel) method)
    {
        SyntaxList<StatementSyntax> syntaxes = new SyntaxList<StatementSyntax>();
        MethodBodyRewriter rewriter = new MethodBodyRewriter(method.SemanticModel);
        foreach (SyntaxReference? reference in method.methodSymbol.DeclaringSyntaxReferences)
        {
            SyntaxNode? node = reference.GetSyntax();

            if (node is not MethodDeclarationSyntax methodDeclaration)
                continue;

            var statements = methodDeclaration.Body!.Statements;
            foreach (var statement in statements)
            {
                if(statement is not ReturnStatementSyntax returnStatementSyntax)
                {
                    var newStatement = (StatementSyntax)rewriter.Visit(statement);
                    syntaxes = syntaxes.Add(newStatement);
                }
                else
                {
                    var returnType = method.methodSymbol.ReturnType.TypeKind == TypeKind.Array ? "BytesWithEncodedLength" : method.methodSymbol.ReturnType.Name;
                    string statementString =
                        $"return WasmMemoryUtility.Write{returnType}({returnStatementSyntax.Expression.ToFullString()});";

                    if (TypeConversionTable.IsValuePassableType(method.methodSymbol.ReturnType, out _))
                    {
                        statementString = $"return {returnStatementSyntax.Expression.ToFullString()};";

                        if (TypeConversionTable.RequiresConversion(method.methodSymbol.ReturnType))
                        {
                            statementString =
                                $"return WasmMemoryUtility.Convert{returnType}({returnStatementSyntax.Expression.ToFullString()});";
                        }
                    }

                    syntaxes = syntaxes.Add(SyntaxFactory.ParseStatement(statementString));
                }
            }
        }

        return syntaxes;
    }

    private static (IMethodSymbol methodSymbol, SemanticModel SemanticModel)? GetApiFunctionMethodOrNull(GeneratorAttributeSyntaxContext context,
        CancellationToken cancelToken)
    {
        if (context.TargetSymbol is not IMethodSymbol methodSymbol)
            return null;

        return (methodSymbol, context.SemanticModel);
    }
}
