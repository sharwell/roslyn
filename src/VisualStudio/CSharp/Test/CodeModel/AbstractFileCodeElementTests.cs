// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using EnvDTE;
using Microsoft.CodeAnalysis.Editor.UnitTests.Workspaces;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.VisualStudio.LanguageServices.Implementation.Interop;
using Microsoft.VisualStudio.LanguageServices.UnitTests.CodeModel.Mocks;
using Roslyn.Test.Utilities;
using SyntaxNodeKey = Microsoft.VisualStudio.LanguageServices.Implementation.CodeModel.SyntaxNodeKey;

namespace Microsoft.VisualStudio.LanguageServices.CSharp.UnitTests.CodeModel
{
    /// <summary>
    /// Base class of a all test-containing classes. Automatically creates a FileCodeModel for testing with the given
    /// file.
    /// </summary>
    [UseExportProvider]
    public abstract class AbstractFileCodeElementTests : IDisposable
    {
        private readonly string _contents;
        private Tuple<TestWorkspace, MockVisualStudioWorkspace, FileCodeModel> _workspaceAndCodeModel;

        public AbstractFileCodeElementTests(string contents)
        {
            _contents = contents;
        }

        internal Tuple<TestWorkspace, MockVisualStudioWorkspace, FileCodeModel> WorkspaceAndCodeModel
        {
            get
            {
                return _workspaceAndCodeModel ?? (_workspaceAndCodeModel = CreateWorkspaceAndFileCodeModelAsync(_contents));
            }
        }

        protected TestWorkspace GetWorkspace()
        {
            return WorkspaceAndCodeModel.Item1;
        }

        private protected MockVisualStudioWorkspace GetVisualStudioWorkspace()
            => WorkspaceAndCodeModel.Item2;

        protected FileCodeModel GetCodeModel()
        {
            return WorkspaceAndCodeModel.Item3;
        }

        protected Microsoft.CodeAnalysis.Solution GetCurrentSolution()
            => GetWorkspace().CurrentSolution;

        protected Microsoft.CodeAnalysis.Project GetCurrentProject()
            => GetCurrentSolution().Projects.Single();

        protected Microsoft.CodeAnalysis.Document GetCurrentDocument()
            => GetCurrentProject().Documents.Single();

        private protected static Tuple<TestWorkspace, MockVisualStudioWorkspace, EnvDTE.FileCodeModel> CreateWorkspaceAndFileCodeModelAsync(string file)
            => FileCodeModelTestHelpers.CreateWorkspaceAndFileCodeModel(file);

        protected CodeElement GetCodeElement(params object[] path)
        {
            WpfTestRunner.RequireWpfFact($"Tests create {nameof(CodeElement)}s which use the affinitized {nameof(CleanableWeakComHandleTable<SyntaxNodeKey, CodeElement>)}");

            if (path.Length == 0)
            {
                throw new ArgumentException("path must be non-empty.", nameof(path));
            }

            var codeElement = (GetCodeModel()).CodeElements.Item(path[0]);

            foreach (var pathElement in path.Skip(1))
            {
                codeElement = codeElement.Children.Item(pathElement);
            }

            return codeElement;
        }

        [SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "The pattern from CA1816 is not followed by this project.")]
        public void Dispose()
        {
            GetVisualStudioWorkspace().Dispose();
            GetWorkspace().Dispose();
        }

        /// <summary>
        /// Returns the current text of the test buffer.
        /// </summary>
        protected string GetFileText()
        {
            return (GetWorkspace()).Documents.Single().GetTextBuffer().CurrentSnapshot.GetText();
        }
    }
}
