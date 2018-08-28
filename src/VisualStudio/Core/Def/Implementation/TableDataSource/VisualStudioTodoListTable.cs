// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using Microsoft.CodeAnalysis.Editor;
using Microsoft.VisualStudio.LanguageServices.Implementation.LanguageService;
using Microsoft.VisualStudio.Shell.TableManager;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.TableDataSource
{
    using Workspace = Microsoft.CodeAnalysis.Workspace;

    [Export(typeof(VisualStudioTodoListTable))]
    [PreloadServices]
    [Shared]
    internal class VisualStudioTodoListTable : VisualStudioBaseTodoListTable, IPreloadService
    {
        internal const string IdentifierString = nameof(VisualStudioTodoListTable);

        [ImportingConstructor]
        public VisualStudioTodoListTable(VisualStudioWorkspace workspace, ITodoListProvider todoListProvider, ITableManagerProvider provider) :
            base(workspace, todoListProvider, IdentifierString, provider)
        {
            ConnectWorkspaceEvents();
        }

        // only for test
        public VisualStudioTodoListTable(Workspace workspace, ITodoListProvider todoListProvider, ITableManagerProvider provider) :
            base(workspace, todoListProvider, IdentifierString, provider)
        {
        }
    }
}
