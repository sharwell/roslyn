﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.VisualStudio.Shell;

[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\Microsoft.VisualStudio.IntegrationTest.Setup.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\Microsoft.VisualStudio.IntegrationTest.Utilities.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\Roslyn.Hosting.Diagnostics.dll")]

[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\Microsoft.Diagnostics.Runtime.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\xunit.abstractions.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\xunit.core.dll")]
[assembly: ProvideCodeBase(CodeBase = @"$PackageFolder$\xunit.execution.desktop.dll")]
