using CodeLess.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace CodeLess.Interfaces
{
    public class InterfaceBuilder : TypeBuilder
    {
        private readonly StringBuilder methods = new (1000);

        private readonly StringBuilder properties = new (1000);

        private readonly StringBuilder events = new (500);

        private readonly StringBuilder usingStatic = new (100);

        private readonly StringBuilder classTrivia = new (100);

        internal override string BuildSource() =>
            $$"""
              #nullable enable
              {{Usings}}

              {{usingStatic}}

              {{NamespaceLeading}}
                {{classTrivia}}
                {{AccessModifier}} partial interface {{ClassName}}{{TypeGenericArguments}}
                {
                    {{events}}

                    {{properties}}

                    {{methods}}
                }
              {{NamespaceTrailing}}
              """;

        internal void SetOriginalTypeName(in GeneratorTypeInfo typeInfo)
        {
            // add the original type name as the static using in case it contains nested types or delegates

            if (typeInfo.Symbol.GetTypeMembers().Length > 0)
            {
                usingStatic.Append("using static ");
                usingStatic.Append(typeInfo.Symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
                usingStatic.AppendLine(";");
            }
        }

        internal void SetClassTrivia(in GeneratorTypeInfo typeInfo)
        {
            classTrivia.Clear();

            if (typeInfo.Syntax.HasLeadingTrivia)
                classTrivia.Append(typeInfo.Syntax.GetLeadingTrivia().ToFullString());
        }

        internal void SetInterfaceName(INamedTypeSymbol interfaceSymbol)
        {
            ClassName.Clear();
            // Append the interface name as is
            ClassName.Append(interfaceSymbol);
        }

        internal void TryAddMember(ISymbol symbol)
        {
            if (SymbolShouldBeIgnored(symbol))
                return;

            switch (symbol)
            {
                case IMethodSymbol method:
                    TryAddMethod(method);
                    break;
                case IPropertySymbol property:
                    TryAddProperty(property);
                    break;
                case IEventSymbol eventSymbol:
                    TryAddEvent(eventSymbol);
                    break;
            }
        }

        private void TryAddMethod(IMethodSymbol method)
        {
            if (method.MethodKind != MethodKind.Ordinary || !method.TryGetSyntax(out MethodDeclarationSyntax methodDeclarationSyntax))
                return;

            var stripped = methodDeclarationSyntax
                          .WithBody(null)                              // remove { … }
                          .WithExpressionBody(null)                    // remove => …;
                          .WithModifiers(FilterModifiers(methodDeclarationSyntax.Modifiers))
                          .WithSemicolonToken(                         // add a semicolon
                               SyntaxFactory.Token(SyntaxKind.SemicolonToken)
                                            .WithTriviaFrom(
                                                 methodDeclarationSyntax.Body?.CloseBraceToken ??    // steal the trivia
                                                 methodDeclarationSyntax.SemicolonToken));

            string signatureText = stripped.ToFullString();

            methods.AppendLine(signatureText);
            methods.AppendLine();
        }

        private void TryAddProperty(IPropertySymbol property)
        {
            if (property.IsIndexer && property.TryGetSyntax(out IndexerDeclarationSyntax indexer))
            {
                var accessorList = BuildAccessorList(
                    indexer,
                    indexer.ExpressionBody,
                    indexer.SemicolonToken
                );

                var ifaceIndexer = indexer
                                  .WithModifiers(FilterModifiers(indexer.Modifiers))
                                  .WithExpressionBody(null)
                                  .WithAccessorList(accessorList)
                                  .WithSemicolonToken(default)

                                   // preserve comments/trivia on the original declaration
                                  .WithLeadingTrivia(indexer.GetLeadingTrivia())
                                  .WithTrailingTrivia(indexer.GetTrailingTrivia());

                properties.AppendLine(ifaceIndexer.ToFullString());
                properties.AppendLine();
            }
            else if (property.TryGetSyntax(out PropertyDeclarationSyntax propDecl))
            {
                var accessorList = BuildAccessorList(
                    propDecl,
                    propDecl.ExpressionBody,
                    propDecl.SemicolonToken
                );

                var ifaceProp = propDecl
                               .WithModifiers(FilterModifiers(propDecl.Modifiers)) // strip modifiers
                               .WithExpressionBody(null)
                               .WithAccessorList(accessorList)
                               .WithSemicolonToken(default)

                                // preserve comments/trivia on the original declaration
                               .WithLeadingTrivia(propDecl.GetLeadingTrivia())
                               .WithTrailingTrivia(propDecl.GetTrailingTrivia());

                properties.AppendLine(ifaceProp.ToFullString());
                properties.AppendLine();
            }
        }

        /// <summary>
        /// Keep internal modifier if present
        /// </summary>
        private static SyntaxTokenList FilterModifiers(SyntaxTokenList originalModifiers)
        {
            // if modifier is public strip it, otherwise keep it
            var modifier = originalModifiers.FirstOrDefault(m => m.IsKind(SyntaxKind.InternalKeyword));
            return modifier == null ? [] : new SyntaxTokenList(modifier);
        }

        private static AccessorListSyntax BuildAccessorList(
            BasePropertyDeclarationSyntax orig,
            ArrowExpressionClauseSyntax? exprBody,
            SyntaxToken semicolonToken)
        {
            var original = orig.AccessorList?.Accessors
                ?? Enumerable.Empty<AccessorDeclarationSyntax>();

            // Keep only public/internal (drop private/protected)
            var visible = original
                .Where(acc => !acc.Modifiers.Any(m =>
                    m.IsKind(SyntaxKind.PrivateKeyword) ||
                    m.IsKind(SyntaxKind.ProtectedKeyword)
                ));

            var newAccessors = visible.Select(acc =>
            {
                var node = SyntaxFactory.AccessorDeclaration(acc.Kind())
                    .WithAttributeLists(acc.AttributeLists)
                    .WithModifiers(FilterModifiers(acc.Modifiers)) // strip modifiers
                    .WithKeyword(acc.Keyword)
                    .WithLeadingTrivia(acc.GetLeadingTrivia())
                    .WithTrailingTrivia(acc.GetTrailingTrivia());

                var semi = SyntaxFactory.Token(SyntaxKind.SemicolonToken)
                    .WithLeadingTrivia(acc.Body?.OpenBraceToken.LeadingTrivia
                        ?? acc.SemicolonToken.LeadingTrivia)
                    .WithTrailingTrivia(acc.Body?.CloseBraceToken.TrailingTrivia
                        ?? acc.SemicolonToken.TrailingTrivia);

                return node.WithSemicolonToken(semi);
            });

            // expression-bodied => implicit getter
            if (exprBody != null)
            {
                var getSemi = SyntaxFactory.Token(SyntaxKind.SemicolonToken)
                    .WithLeadingTrivia(semicolonToken.LeadingTrivia)
                    .WithTrailingTrivia(semicolonToken.TrailingTrivia);

                newAccessors = newAccessors.Append(
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithAttributeLists(default)
                        .WithModifiers(default)
                        .WithKeyword(SyntaxFactory.Token(SyntaxKind.GetKeyword))
                        .WithSemicolonToken(getSemi)
                        .WithLeadingTrivia(exprBody.GetLeadingTrivia())
                        .WithTrailingTrivia(exprBody.GetTrailingTrivia())
                );
            }

            var open = orig.AccessorList?.OpenBraceToken ?? SyntaxFactory.Token(SyntaxKind.OpenBraceToken);
            var close = orig.AccessorList?.CloseBraceToken ?? SyntaxFactory.Token(SyntaxKind.CloseBraceToken);

            return SyntaxFactory.AccessorList(
                SyntaxFactory.Token(SyntaxKind.OpenBraceToken)
                    .WithLeadingTrivia(open.LeadingTrivia)
                    .WithTrailingTrivia(open.TrailingTrivia),
                SyntaxFactory.List(newAccessors),
                SyntaxFactory.Token(SyntaxKind.CloseBraceToken)
                    .WithLeadingTrivia(close.LeadingTrivia)
                    .WithTrailingTrivia(close.TrailingTrivia)
            );


        }

        private void TryAddEvent(IEventSymbol ev)
        {
            EventFieldDeclarationSyntax? fieldDecl = null;
            MemberDeclarationSyntax? syntaxNode = null;

            // First, try to get a direct MemberDeclarationSyntax
            if (ev.TryGetSyntax(out MemberDeclarationSyntax declSyntax))
            {
                syntaxNode = declSyntax;
            }
            else
            {
                // Fall back to DeclaringSyntaxReferences for field-like events
                foreach (var reference in ev.DeclaringSyntaxReferences)
                {
                    var node = reference.GetSyntax();
                    if (node is EventFieldDeclarationSyntax efd)
                    {
                        fieldDecl = efd;
                        syntaxNode = efd;
                        break;
                    }
                    if (node is VariableDeclaratorSyntax varDecl
                        && varDecl.Parent?.Parent is EventFieldDeclarationSyntax parentEfd)
                    {
                        fieldDecl = parentEfd;
                        syntaxNode = parentEfd;
                        break;
                    }
                }
            }

            MemberDeclarationSyntax? ifaceEvent = null;
            if (syntaxNode is EventDeclarationSyntax decl)
            {
                ifaceEvent = decl
                            .WithAccessorList(null)
                            .WithModifiers(SyntaxFactory.TokenList())
                            .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)
                                                             .WithTrailingTrivia(SyntaxFactory.LineFeed));
            }
            else if (syntaxNode is EventFieldDeclarationSyntax field)
            {
                ifaceEvent = CreateInterfaceFieldEvent(field)
                    .WithModifiers(SyntaxFactory.TokenList());
            }

            if (ifaceEvent != null)
            {
                // preserve comments/trivia
                ifaceEvent = ifaceEvent
                    .WithLeadingTrivia(syntaxNode.GetLeadingTrivia())
                    .WithTrailingTrivia(syntaxNode.GetTrailingTrivia())
                    .NormalizeWhitespace();

                events.AppendLine(ifaceEvent.ToFullString());
                events.AppendLine();
            }
        }

        private static AccessorListSyntax BuildEventAccessorList(EventDeclarationSyntax orig)
        {
            var original = orig.AccessorList?.Accessors
                ?? Enumerable.Empty<AccessorDeclarationSyntax>();

            // Keep only public/internal (drop private/protected)
            var visible = original.Where(a => !a.Modifiers.Any(m =>
                m.IsKind(SyntaxKind.PrivateKeyword) ||
                m.IsKind(SyntaxKind.ProtectedKeyword)
            ));

            var built = visible.Select(a =>
            {
                var node = SyntaxFactory.AccessorDeclaration(a.Kind())
                    .WithAttributeLists(default)
                    .WithModifiers(default)
                    .WithKeyword(a.Keyword)
                    .WithLeadingTrivia(a.GetLeadingTrivia())
                    .WithTrailingTrivia(a.GetTrailingTrivia());

                var semi = SyntaxFactory.Token(SyntaxKind.SemicolonToken)
                    .WithLeadingTrivia(a.Body?.OpenBraceToken.LeadingTrivia
                        ?? a.SemicolonToken.LeadingTrivia)
                    .WithTrailingTrivia(a.Body?.CloseBraceToken.TrailingTrivia
                        ?? a.SemicolonToken.TrailingTrivia);

                return node.WithSemicolonToken(semi);
            });

            var open = orig.AccessorList?.OpenBraceToken ?? SyntaxFactory.Token(SyntaxKind.OpenBraceToken);
            var close = orig.AccessorList?.CloseBraceToken ?? SyntaxFactory.Token(SyntaxKind.CloseBraceToken);

            return SyntaxFactory.AccessorList(
                SyntaxFactory.Token(SyntaxKind.OpenBraceToken)
                    .WithLeadingTrivia(open.LeadingTrivia)
                    .WithTrailingTrivia(open.TrailingTrivia),
                SyntaxFactory.List(built),
                SyntaxFactory.Token(SyntaxKind.CloseBraceToken)
                    .WithLeadingTrivia(close.LeadingTrivia)
                    .WithTrailingTrivia(close.TrailingTrivia)
            );
        }

        private static EventFieldDeclarationSyntax CreateInterfaceFieldEvent(EventFieldDeclarationSyntax orig)
        {
            var vars = orig.Declaration.Variables.Select(v => v.WithInitializer(null));
            var decl = SyntaxFactory.VariableDeclaration(orig.Declaration.Type, SyntaxFactory.SeparatedList(vars));

            return SyntaxFactory.EventFieldDeclaration(
                    orig.AttributeLists,
                    orig.Modifiers,
                    decl
                )
                .WithSemicolonToken(orig.SemicolonToken)
                .WithLeadingTrivia(orig.GetLeadingTrivia())
                .WithTrailingTrivia(orig.GetTrailingTrivia());
        }

        private bool SymbolShouldBeIgnored(ISymbol symbol) =>
            symbol.TryGetAttribute(Consts.IGNORE_MEMBER_ATTRIBUTE_NAME, out _)
            || symbol.IsStatic
            || (symbol.DeclaredAccessibility != Accessibility.Internal && symbol.DeclaredAccessibility != Accessibility.Public);

        internal override void Clear()
        {
            methods.Clear();
            properties.Clear();
            events.Clear();
            usingStatic.Clear();
            classTrivia.Clear();
            base.Clear();
        }
    }
}
