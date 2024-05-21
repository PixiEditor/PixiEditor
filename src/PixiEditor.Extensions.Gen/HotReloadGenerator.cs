using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace PixiEditor.Extensions.Gen
{
    [Generator(LanguageNames.CSharp)]
    public class HotReloadGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            /*var provider = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: Helpers.IsLayoutElementOrStateType,
                    transform: static (context, cancelToken) =>
                    {
                        ClassDeclarationSyntax classDeclaration = (ClassDeclarationSyntax)context.Node;
                        return classDeclaration;
                    });

            context.RegisterSourceOutput(provider, Generate);*/

            /*var classProvider = context.SyntaxProvider
                .CreateSyntaxProvider(predicate: Helpers.IsLayoutElementOrStateType,
                    (ctx, _) => (ClassDeclarationSyntax)ctx.Node);*/

            var compilationProvider = context.CompilationProvider.Select((compilation, token) => Helpers.IsLayoutElementOrStateType(compilation, token));

            context.RegisterSourceOutput(compilationProvider, Generate);
        }

        private void Generate(SourceProductionContext context, bool arg2)
        {
            
        }

        private void Generate(SourceProductionContext context, ClassDeclarationSyntax syntax)
        {
            GenerateHotReloadCode(context, syntax.Identifier.Text);
        }

        private void GenerateHotReloadCode(SourceProductionContext context, string name)
        {
            var ns = " PixiEditor.Extensions.Gen";

            context.AddSource($"{ns}.{name}.prefs.cs", $@"//

            namespace {ns}
            {{
               partial class {name}
               {{
               }}
            }}
            ");
        }
    }
}
