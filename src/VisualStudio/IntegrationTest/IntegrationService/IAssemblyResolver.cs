﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Reflection;

namespace Microsoft.VisualStudio.IntegrationTest.Utilities
{
    public interface IAssemblyResolver
    {
        string TryResolveAssembly(AssemblyName assemblyName);
    }
}
