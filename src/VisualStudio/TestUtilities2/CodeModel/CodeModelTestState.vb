' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.
' See the LICENSE file in the project root for more information.

Imports Microsoft.CodeAnalysis.Editor.UnitTests.Workspaces
Imports Microsoft.CodeAnalysis
Imports Microsoft.VisualStudio.LanguageServices.Implementation.CodeModel
Imports Microsoft.VisualStudio.LanguageServices.Implementation.Interop

Namespace Microsoft.VisualStudio.LanguageServices.UnitTests.CodeModel
    Friend Class CodeModelTestState
        Implements IDisposable

        Public ReadOnly Workspace As TestWorkspace
        Private ReadOnly _visualStudioWorkspace As VisualStudioWorkspace
        Private ReadOnly _rootCodeModel As ComHandle(Of EnvDTE.CodeModel, RootCodeModel)
        Private ReadOnly _fileCodeModel As ComHandle(Of EnvDTE80.FileCodeModel2, FileCodeModel)
        Private ReadOnly _codeModelService As ICodeModelService

        Public Sub New(
            workspace As TestWorkspace,
            visualStudioWorkspace As VisualStudioWorkspace,
            rootCodeModel As ComHandle(Of EnvDTE.CodeModel, RootCodeModel),
            fileCodeModel As ComHandle(Of EnvDTE80.FileCodeModel2, FileCodeModel),
            codeModelService As ICodeModelService
        )

            If workspace Is Nothing Then
                Throw New ArgumentNullException(NameOf(workspace))
            End If

            If codeModelService Is Nothing Then
                Throw New ArgumentNullException(NameOf(codeModelService))
            End If

            Me.Workspace = workspace
            _visualStudioWorkspace = visualStudioWorkspace
            _rootCodeModel = rootCodeModel
            _fileCodeModel = fileCodeModel
            _codeModelService = codeModelService
        End Sub

        Public ReadOnly Property VisualStudioWorkspace As VisualStudioWorkspace
            Get
                Return _visualStudioWorkspace
            End Get
        End Property

        Public ReadOnly Property FileCodeModel As EnvDTE80.FileCodeModel2
            Get
                Return _fileCodeModel.Handle
            End Get
        End Property

        Public ReadOnly Property FileCodeModelObject As FileCodeModel
            Get
                Return _fileCodeModel.Object
            End Get
        End Property

        Public ReadOnly Property RootCodeModel As EnvDTE.CodeModel
            Get
                Return _rootCodeModel.Handle
            End Get
        End Property

        Public ReadOnly Property RootCodeModelObject As RootCodeModel
            Get
                Return _rootCodeModel.Object
            End Get
        End Property

        Public ReadOnly Property CodeModelService As ICodeModelService
            Get
                Return _codeModelService
            End Get
        End Property

        Private _disposedValue As Boolean ' To detect redundant calls

        Protected Overrides Sub Finalize()
            If Not Environment.HasShutdownStarted Then
                FailFast.Fail("TestWorkspaceAndFileModelCodel GC'd without call to Dispose()!")
            End If
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' We only use the finalizer to ensure Dispose was called explicitly. Suppress that call early to avoid
            ' crashing the process for an unrelated exception within the Dispose implementation.
            GC.SuppressFinalize(Me)

            If Not Me._disposedValue Then
                VisualStudioWorkspace.Dispose()
                Workspace.Dispose()
            End If

            Me._disposedValue = True
        End Sub

    End Class
End Namespace
