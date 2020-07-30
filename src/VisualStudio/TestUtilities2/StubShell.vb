﻿' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.
' See the LICENSE file in the project root for more information.

Imports Microsoft.VisualStudio.Imaging.Interop
Imports Microsoft.VisualStudio.Shell.Interop

Namespace Microsoft.VisualStudio.LanguageServices.UnitTests
    Friend NotInheritable Class StubShell
        Implements IVsShell, IVsShell2, IVsShell3, IVsShell4, IVsShell5, IVsShell6, IVsShell7

        Private _shellPropertyChangeId As Integer
        Private ReadOnly _shellPropertyChanges As New Dictionary(Of UInteger, IVsShellPropertyEvents)()

        Public Function GetPackageEnum(ByRef ppenum As IEnumPackages) As Integer Implements IVsShell.GetPackageEnum
            Throw New NotImplementedException()
        End Function

        Public Function GetProperty(propid As Integer, ByRef pvar As Object) As Integer Implements IVsShell.GetProperty
            If propid = __VSSPROPID.VSSPROPID_IsInCommandLineMode Then
                pvar = False
                Return VSConstants.S_OK
            ElseIf propid = __VSSPROPID6.VSSPROPID_ShutdownStarted Then
                pvar = False
                Return VSConstants.S_OK
            End If

            Throw New NotImplementedException()
        End Function

        Public Function SetProperty(propid As Integer, var As Object) As Integer Implements IVsShell.SetProperty
            Throw New NotImplementedException()
        End Function

        Public Function AdviseBroadcastMessages(pSink As IVsBroadcastMessageEvents, ByRef pdwCookie As UInteger) As Integer Implements IVsShell.AdviseBroadcastMessages
            Throw New NotImplementedException()
        End Function

        Public Function UnadviseBroadcastMessages(dwCookie As UInteger) As Integer Implements IVsShell.UnadviseBroadcastMessages
            Throw New NotImplementedException()
        End Function

        Public Function AdviseShellPropertyChanges(pSink As IVsShellPropertyEvents, ByRef pdwCookie As UInteger) As Integer Implements IVsShell.AdviseShellPropertyChanges
            If pSink Is Nothing Then
                Throw New ArgumentNullException(NameOf(pSink))
            End If

            SyncLock _shellPropertyChanges
                _shellPropertyChangeId += 1
                pdwCookie = CUInt(_shellPropertyChangeId)
                _shellPropertyChanges.Add(pdwCookie, pSink)
            End SyncLock

            Return VSConstants.S_OK
        End Function

        Public Function UnadviseShellPropertyChanges(dwCookie As UInteger) As Integer Implements IVsShell.UnadviseShellPropertyChanges
            SyncLock _shellPropertyChanges
                If Not _shellPropertyChanges.Remove(dwCookie) Then
                    Throw New InvalidOperationException()
                End If
            End SyncLock

            Return VSConstants.S_OK
        End Function

        Public Function LoadPackage(ByRef guidPackage As Guid, ByRef ppPackage As IVsPackage) As Integer Implements IVsShell.LoadPackage
            Throw New NotImplementedException()
        End Function

        Public Function LoadPackageString(ByRef guidPackage As Guid, resid As UInteger, ByRef pbstrOut As String) As Integer Implements IVsShell.LoadPackageString
            Throw New NotImplementedException()
        End Function

        Public Function LoadUILibrary(ByRef guidPackage As Guid, dwExFlags As UInteger, ByRef phinstOut As UInteger) As Integer Implements IVsShell.LoadUILibrary
            Throw New NotImplementedException()
        End Function

        Public Function IsPackageInstalled(ByRef guidPackage As Guid, ByRef pfInstalled As Integer) As Integer Implements IVsShell.IsPackageInstalled
            Throw New NotImplementedException()
        End Function

        Public Function IsPackageLoaded(ByRef guidPackage As Guid, ByRef ppPackage As IVsPackage) As Integer Implements IVsShell.IsPackageLoaded
            Throw New NotImplementedException()
        End Function

        Public Function LoadPackageStringWithLCID(ByRef guidPackage As Guid, resid As UInteger, lcid As UInteger, ByRef pbstrOut As String) As Integer Implements IVsShell2.LoadPackageStringWithLCID
            Throw New NotImplementedException()
        End Function

        Public Function RestartElevated() As Integer Implements IVsShell3.RestartElevated
            Throw New NotImplementedException()
        End Function

        Public Function IsRunningElevated(ByRef pElevated As Boolean) As Integer Implements IVsShell3.IsRunningElevated
            Throw New NotImplementedException()
        End Function

        Public Function Restart(rtRestartMode As UInteger) As Integer Implements IVsShell4.Restart
            Throw New NotImplementedException()
        End Function

        Public Function LoadPackageWithContext(ByRef packageGuid As Guid, reason As Integer, ByRef context As Guid) As IVsPackage Implements IVsShell5.LoadPackageWithContext
            Throw New NotImplementedException()
        End Function

        Public Function CreatePackageExtension(ByRef Package As Guid, ByRef extensionPoint As Guid, ByRef instance As Guid) As Object Implements IVsShell5.CreatePackageExtension
            Throw New NotImplementedException()
        End Function

        Private Function IVsShell6_LoadPackageWithContext(ByRef packageGuid As Guid, reason As Integer, ByRef context As Guid) As IVsPackage Implements IVsShell6.LoadPackageWithContext
            Throw New NotImplementedException()
        End Function

        Private Function IVsShell6_CreatePackageExtension(ByRef Package As Guid, ByRef extensionPoint As Guid, ByRef instance As Guid) As Object Implements IVsShell6.CreatePackageExtension
            Throw New NotImplementedException()
        End Function

        Public Function AdvisePackageLoadEvents(eventSink As IVsPackageLoadEvents) As UInteger Implements IVsShell6.AdvisePackageLoadEvents
            Throw New NotImplementedException()
        End Function

        Public Sub UnadvisePackageLoadEvents(cookie As UInteger) Implements IVsShell6.UnadvisePackageLoadEvents
            Throw New NotImplementedException()
        End Sub

        Public Sub NotifyExtensionSettingsChanged() Implements IVsShell6.NotifyExtensionSettingsChanged
            Throw New NotImplementedException()
        End Sub

        Public Function LoadPackageAsync(ByRef guidPackage As Guid) As IVsTask Implements IVsShell7.LoadPackageAsync
            Throw New NotImplementedException()
        End Function

        Public Function LoadPackageWithContextAsync(ByRef guidPackage As Guid, reason As Integer, ByRef context As Guid) As IVsTask Implements IVsShell7.LoadPackageWithContextAsync
            Throw New NotImplementedException()
        End Function

        Public ReadOnly Property SccGlyphImageListImageMoniker As ImageMoniker Implements IVsShell7.SccGlyphImageListImageMoniker
            Get
                Throw New NotImplementedException()
            End Get
        End Property
    End Class
End Namespace
