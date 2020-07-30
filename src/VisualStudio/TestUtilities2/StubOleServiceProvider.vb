' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.
' See the LICENSE file in the project root for more information.

Imports System.Composition
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Threading
Imports Microsoft.CodeAnalysis.Editor.Shared.Utilities
Imports Microsoft.CodeAnalysis.Host.Mef
Imports Microsoft.Internal.VisualStudio.Shell
Imports Microsoft.VisualStudio.Shell
Imports Microsoft.VisualStudio.Shell.Interop
Imports IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider
Imports WrapperPolicy = Microsoft.VisualStudio.LanguageServices.Implementation.Interop.WrapperPolicy

Namespace Microsoft.VisualStudio.LanguageServices.UnitTests
    <Export(GetType(IOleServiceProvider))>
    <[Shared]>
    <PartNotDiscoverable>
    Friend NotInheritable Class StubOleServiceProvider
        Implements SVsServiceProvider, IServiceProvider, IOleServiceProvider, IDisposable

        Private ReadOnly _shell As StubShell
        Private ReadOnly _taskSchedulerService As StubTaskSchedulerService
        Private ReadOnly _asyncServiceProvider As StubAsyncServiceProvider
        Private ReadOnly _localRegistry As StubLocalRegistry
        Private _disposed As Boolean

        <ImportingConstructor>
        <Obsolete(MefConstruction.ImportingConstructorMessage, True)>
        Public Sub New(threadingContext As IThreadingContext)
            _shell = New StubShell()
            _taskSchedulerService = New StubTaskSchedulerService(threadingContext)
            _asyncServiceProvider = New StubAsyncServiceProvider()
            _localRegistry = New StubLocalRegistry()
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            _disposed = True

            ' Set known cache fields to null
            GetType(ThreadHelper).GetField("_joinableTaskContextCache", BindingFlags.NonPublic Or BindingFlags.Static).SetValue(Nothing, value:=Nothing)
            GetType(MpfHelpers).GetField("vsTaskSchedulerService", BindingFlags.NonPublic Or BindingFlags.Static).SetValue(Nothing, value:=Nothing)
            GetType(MpfHelpers).GetField("shuttingDownTokenSource", BindingFlags.NonPublic Or BindingFlags.Static).SetValue(Nothing, value:=New CancellationTokenSource())
            GetType(ServiceProvider).GetField("globalProvider", BindingFlags.NonPublic Or BindingFlags.Static).SetValue(Nothing, value:=Nothing)
            GetType(ServiceProvider).GetField("asyncServiceProvider", BindingFlags.NonPublic Or BindingFlags.Static).SetValue(Nothing, value:=Nothing)
            WrapperPolicy.TestAccessor.ComWrapperFactory = Nothing
        End Sub

        Public Function GetService(serviceType As Type) As Object Implements IServiceProvider.GetService
            If serviceType = GetType(SVsTaskSchedulerService) Then
                Return _taskSchedulerService
            ElseIf serviceType = GetType(SVsShell) Then
                Return _shell
            ElseIf serviceType = GetType(SAsyncServiceProvider) Then
                Return _asyncServiceProvider
            ElseIf serviceType = GetType(SLocalRegistry) Then
                Return _localRegistry
            ElseIf serviceType.GUID = GetType(SVsExecutionContextTracker).GUID Then
                Return Nothing
            End If

            Throw New NotImplementedException()
        End Function

        Public Function QueryService(ByRef guidService As Guid, ByRef riid As Guid, ByRef ppvObject As IntPtr) As Integer Implements IOleServiceProvider.QueryService
            If _disposed Then
                Throw New InvalidOperationException()
            End If

            Dim obj As Object
            If guidService = GetType(SVsTaskSchedulerService).GUID Then
                obj = GetService(GetType(SVsTaskSchedulerService))
            ElseIf guidService = GetType(SVsShell).GUID Then
                obj = GetService(GetType(SVsShell))
            ElseIf guidService = GetType(SAsyncServiceProvider).GUID Then
                obj = GetService(GetType(SAsyncServiceProvider))
            ElseIf guidService = GetType(SLocalRegistry).GUID Then
                obj = GetService(GetType(SLocalRegistry))
            ElseIf guidService = GetType(SVsExecutionContextTracker).GUID Then
                ppvObject = IntPtr.Zero
                Return VSConstants.E_NOTIMPL
            Else
                Throw New NotImplementedException()
            End If

            Dim unknown = Marshal.GetIUnknownForObject(obj)
            Try
                Return Marshal.QueryInterface(unknown, riid, ppvObject)
            Finally
                Marshal.Release(unknown)
            End Try
        End Function

        <Guid("4C2E7029-D0BC-3F57-BE15-6AD5D43A7832")>
        Private Interface SVsExecutionContextTracker
        End Interface
    End Class
End Namespace
