using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stylus
{
    public static class Util
    {
        public static Dictionary<int, (SyntaxKind, Accessibility)> _memberOrder = new Dictionary<int, (SyntaxKind, Accessibility)>()
        {
            { 1, (SyntaxKind.StaticKeyword, Accessibility.NotApplicable)},
            { 2, (SyntaxKind.FieldDeclaration, Accessibility.Private)},
            { 3, (SyntaxKind.FieldDeclaration, Accessibility.Protected)},
            { 4, (SyntaxKind.FieldDeclaration, Accessibility.Public)},
            { 5, (SyntaxKind.ConstructorDeclaration, Accessibility.NotApplicable)},
            { 6, (SyntaxKind.MethodDeclaration, Accessibility.Public)},
            { 7, (SyntaxKind.MethodDeclaration, Accessibility.Protected)},
            { 8, (SyntaxKind.MethodDeclaration, Accessibility.Private)},
        };

        public static void TryGetAccessModifier(SyntaxNode node, out Accessibility modifier)
        {
            var nodeKind = node.Kind();
            if (nodeKind == SyntaxKind.StaticKeyword || nodeKind == SyntaxKind.ConstructorDeclaration)
            {
                modifier = Accessibility.NotApplicable;
                return;
            }
            if (nodeKind == SyntaxKind.FieldDeclaration)
            {
                Enum.TryParse((node as FieldDeclarationSyntax).Modifiers.FirstOrDefault().ToString(), true, out modifier);
                return;
            }
            if (nodeKind == SyntaxKind.PropertyDeclaration)
            {
                Enum.TryParse((node as PropertyDeclarationSyntax).Modifiers.FirstOrDefault().ToString(), true, out modifier);
                return;
            }
            if (nodeKind == SyntaxKind.MethodDeclaration)
            {
                Enum.TryParse((node as MethodDeclarationSyntax).Modifiers.FirstOrDefault().ToString(), true, out modifier);
                return;
            }
            modifier = Accessibility.NotApplicable;
        }

    }
}
