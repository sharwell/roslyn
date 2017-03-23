// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Test.Utilities;
using Roslyn.Test.Utilities;

namespace Microsoft.CodeAnalysis.Test.Utilities.CodeRuntime
{
    public sealed class DesktopRuntimeEnvironmentFactory : IRuntimeEnvironmentFactory
    {
        public IRuntimeEnvironment Create(IEnumerable<ModuleData> additionalDependencies)
        {
            return new DesktopRuntimeEnvironment(additionalDependencies);
        }
    }
}
