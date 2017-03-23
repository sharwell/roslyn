// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Test.Utilities;
using Roslyn.Test.Utilities;
using static Microsoft.CodeAnalysis.Test.Utilities.CommonTestBase;

namespace Microsoft.CodeAnalysis.CSharp.UnitTests
{
    internal static class WinRTUtil
    {
        internal static CompilationVerifier CompileAndVerifyOnWin8Only(
            this CSharpTestBase testBase,
            string source,
            MetadataReference[] additionalRefs = null,
            string expectedOutput = null)
        {
            var isWin8 = OSVersion.IsWin8;
            return testBase.CompileAndVerifyWinRt(
                source,
                additionalRefs: additionalRefs,
                expectedOutput: isWin8 ? expectedOutput : null,
                verify: isWin8);
        }

    }
}
