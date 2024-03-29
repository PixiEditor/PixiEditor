﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PixiEditor.ChangeableDocument.Gen
{
    [Generator]
    public class ChangeActionGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // find constructors with the attribute
            var constructorSymbolProvider = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: Helpers.IsConstructorWithAttribute,
                transform: static (context, cancelToken) =>
                {
                    var constructor = (ConstructorDeclarationSyntax)context.Node;
                    if (!Helpers.MethodHasAttribute(
                        context,
                        cancelToken,
                        constructor,
                        new NamespacedType("GenerateMakeChangeActionAttribute", "PixiEditor.ChangeableDocument.Actions.Attributes")
                        ))
                        return null;

                    var constructorSymbol = context.SemanticModel.GetDeclaredSymbol(constructor, cancelToken);
                    if (constructorSymbol is not IMethodSymbol methodConstructorSymbol ||
                        methodConstructorSymbol.Kind != SymbolKind.Method)
                        return null;
                    return methodConstructorSymbol;
                }
            ).Where(a => a is not null);

            // generate action source code
            var actionSourceCodeProvider = constructorSymbolProvider.Select(
                static (constructor, _) =>
                {
                    var info = Helpers.ExtractMethodInfo(constructor!);
                    return new NamedSourceCode(info.ContainingClass.NameWithNamespace + "MakeChangeAction", Helpers.CreateMakeChangeAction(info));
                }
            );

            // add the source code into compiler input
            context.RegisterSourceOutput(actionSourceCodeProvider, static (context, namedCode) =>
            {
                context.AddSource(namedCode.Name, namedCode.Code);
            });
        }
    }
}
