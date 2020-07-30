' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.
' See the LICENSE file in the project root for more information.

Imports Microsoft.VisualStudio.Shell
Imports IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider

Namespace Microsoft.VisualStudio.LanguageServices.UnitTests
    Friend NotInheritable Class StubAsyncServiceProvider
        Implements IAsyncServiceProvider2, IOleServiceProvider

        Public Function GetServiceAsync(serviceType As Type, swallowExceptions As Boolean) As Task(Of Object) Implements IAsyncServiceProvider2.GetServiceAsync
            Throw New NotImplementedException()
        End Function

        Public Function GetServiceAsync(serviceType As Type) As Task(Of Object) Implements IAsyncServiceProvider.GetServiceAsync
            Throw New NotImplementedException()
        End Function

        Public Function QueryService(ByRef guidService As Guid, ByRef riid As Guid, ByRef ppvObject As IntPtr) As Integer Implements IOleServiceProvider.QueryService
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
