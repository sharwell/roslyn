// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.Host;

namespace Microsoft.CodeAnalysis.Editor.UnitTests.Workspaces
{
    internal partial class TestHostSolution
    {
        public readonly SolutionId Id;
        public readonly VersionStamp Version;
        public readonly string FilePath;
        public readonly IEnumerable<TestHostProject> Projects;

        public TestHostSolution(
            HostLanguageServices languageServiceProvider,
            CompilationOptions compilationOptions,
            ParseOptions parseOptions,
            params MetadataReference[] references)
            : this(workspaceElementOpt: null, new TestHostProject(languageServiceProvider, compilationOptions, parseOptions, references))
        {
        }

        public TestHostSolution(XElement workspaceElementOpt, params TestHostProject[] projects)
        {
            if (workspaceElementOpt != null && workspaceElementOpt.Name != TestWorkspace.WorkspaceElementName)
            {
                throw new ArgumentException("Invalid workspace element", nameof(workspaceElementOpt));
            }

            this.Id = SolutionId.CreateNewId();
            this.Version = VersionStamp.Create();
            this.FilePath = workspaceElementOpt?.Attribute(TestWorkspace.FilePathAttributeName).Value;
            this.Projects = projects;

            foreach (var project in projects)
            {
                project.SetSolution(this);
            }
        }
    }
}
