﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable enable

#if !NETCOREAPP

using Microsoft.CodeAnalysis.Editor;

namespace Microsoft.CodeAnalysis.ExternalAccess.VSTypeScript.Api
{
    internal static class VSTypeScriptPredefinedCommandHandlerNames
    {
        public const string SignatureHelpAfterCompletion = PredefinedCommandHandlerNames.SignatureHelpAfterCompletion;
    }
}

#endif
