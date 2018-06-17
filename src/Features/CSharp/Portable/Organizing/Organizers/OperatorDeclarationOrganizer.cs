﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Organizing.Organizers;

namespace Microsoft.CodeAnalysis.CSharp.Organizing.Organizers
{
    [ExportSyntaxNodeOrganizer(LanguageNames.CSharp), Shared]
    internal class OperatorDeclarationOrganizer : AbstractSyntaxNodeOrganizer<OperatorDeclarationSyntax>
    {
        protected override OperatorDeclarationSyntax Organize(
            OperatorDeclarationSyntax syntax,
            OptionSet optionSet,
            CancellationToken cancellationToken)
        {
            return syntax.Update(syntax.AttributeLists,
                ModifiersOrganizer.ForCodeStyle(optionSet).Organize(syntax.Modifiers),
                syntax.ReturnType,
                syntax.OperatorKeyword,
                syntax.OperatorToken,
                syntax.ParameterList,
                syntax.Body,
                syntax.SemicolonToken);
        }
    }
}
