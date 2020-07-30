' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.
' See the LICENSE file in the project root for more information.

Imports System.Runtime.InteropServices
Imports Microsoft.VisualStudio.OLE.Interop
Imports Microsoft.VisualStudio.Shell.Interop

Namespace Microsoft.VisualStudio.LanguageServices.UnitTests
    Friend NotInheritable Class StubLocalRegistry
        Implements ILocalRegistry, ILocalRegistry2, ILocalRegistry3, ILocalRegistry4, ILocalRegistry5

        Public Function CreateInstance(clsid As Guid, punkOuter As Object, ByRef riid As Guid, dwFlags As UInteger, ByRef ppvObj As IntPtr) As Integer Implements ILocalRegistry.CreateInstance
            If punkOuter IsNot Nothing Then
                Throw New NotSupportedException()
            End If

            If dwFlags <> CLSCTX.CLSCTX_INPROC_SERVER Then
                Throw New NotSupportedException()
            End If

            Dim obj As Object
            If clsid = GetType(IComWrapperFactory).GUID Then
                obj = New StubComWrapperFactory()
            Else
                Throw New NotImplementedException()
            End If

            Dim unknown = Marshal.GetIUnknownForObject(obj)
            Try
                Return Marshal.QueryInterface(unknown, riid, ppvObj)
            Finally
                Marshal.Release(unknown)
            End Try
        End Function

        Public Function GetTypeLibOfClsid(clsid As Guid, ByRef pptLib As ITypeLib) As Integer Implements ILocalRegistry.GetTypeLibOfClsid
            Throw New NotImplementedException()
        End Function

        Public Function GetClassObjectOfClsid(ByRef clsid As Guid, dwFlags As UInteger, lpReserved As IntPtr, ByRef riid As Guid, ByRef ppvClassObject As IntPtr) As Integer Implements ILocalRegistry.GetClassObjectOfClsid
            Throw New NotImplementedException()
        End Function

        Private Function ILocalRegistry2_CreateInstance(clsid As Guid, punkOuter As Object, ByRef riid As Guid, dwFlags As UInteger, ByRef ppvObj As IntPtr) As Integer Implements ILocalRegistry2.CreateInstance
            Return CreateInstance(clsid, punkOuter, riid, dwFlags, ppvObj)
        End Function

        Private Function ILocalRegistry2_GetTypeLibOfClsid(clsid As Guid, ByRef pptLib As ITypeLib) As Integer Implements ILocalRegistry2.GetTypeLibOfClsid
            Return GetTypeLibOfClsid(clsid, pptLib)
        End Function

        Private Function ILocalRegistry2_GetClassObjectOfClsid(ByRef clsid As Guid, dwFlags As UInteger, lpReserved As IntPtr, ByRef riid As Guid, ppvClassObject As IntPtr) As Integer Implements ILocalRegistry2.GetClassObjectOfClsid
            Return GetClassObjectOfClsid(clsid, dwFlags, lpReserved, riid, ppvClassObject)
        End Function

        Public Function GetLocalRegistryRoot(ByRef pbstrRoot As String) As Integer Implements ILocalRegistry2.GetLocalRegistryRoot
            Throw New NotImplementedException()
        End Function

        Private Function ILocalRegistry3_CreateInstance(clsid As Guid, punkOuter As Object, ByRef riid As Guid, dwFlags As UInteger, ByRef ppvObj As IntPtr) As Integer Implements ILocalRegistry3.CreateInstance
            Return CreateInstance(clsid, punkOuter, riid, dwFlags, ppvObj)
        End Function

        Private Function ILocalRegistry3_GetTypeLibOfClsid(clsid As Guid, ByRef pptLib As ITypeLib) As Integer Implements ILocalRegistry3.GetTypeLibOfClsid
            Return GetTypeLibOfClsid(clsid, pptLib)
        End Function

        Private Function ILocalRegistry3_GetClassObjectOfClsid(ByRef clsid As Guid, dwFlags As UInteger, lpReserved As IntPtr, ByRef riid As Guid, ppvClassObject As IntPtr) As Integer Implements ILocalRegistry3.GetClassObjectOfClsid
            Return GetClassObjectOfClsid(clsid, dwFlags, lpReserved, riid, ppvClassObject)
        End Function

        Private Function ILocalRegistry3_GetLocalRegistryRoot(ByRef pbstrRoot As String) As Integer Implements ILocalRegistry3.GetLocalRegistryRoot
            Return GetLocalRegistryRoot(pbstrRoot)
        End Function

        Public Function CreateManagedInstance(codeBase As String, assemblyName As String, typeName As String, ByRef riid As Guid, ByRef ppvObj As IntPtr) As Integer Implements ILocalRegistry3.CreateManagedInstance
            Throw New NotImplementedException()
        End Function

        Public Function GetClassObjectOfManagedClass(codeBase As String, assemblyName As String, typeName As String, ByRef riid As Guid, ByRef ppvClassObject As IntPtr) As Integer Implements ILocalRegistry3.GetClassObjectOfManagedClass
            Throw New NotImplementedException()
        End Function

        Public Function RegisterClassObject(ByRef rclsid As Guid, ByRef pdwCookie As UInteger) As Integer Implements ILocalRegistry4.RegisterClassObject
            Throw New NotImplementedException()
        End Function

        Public Function RevokeClassObject(dwCookie As UInteger) As Integer Implements ILocalRegistry4.RevokeClassObject
            Throw New NotImplementedException()
        End Function

        Public Function RegisterInterface(ByRef riid As Guid) As Integer Implements ILocalRegistry4.RegisterInterface
            Throw New NotImplementedException()
        End Function

        Public Function GetLocalRegistryRootEx(dwRegType As UInteger, ByRef pdwRegRootHandle As UInteger, ByRef pbstrRoot As String) As Integer Implements ILocalRegistry4.GetLocalRegistryRootEx
            Throw New NotImplementedException()
        End Function

        Public Function CreateAggregatedManagedInstance(codeBase As String, AssemblyName As String, TypeName As String, pUnkOuter As IntPtr, ByRef riid As Guid, ByRef ppvObj As IntPtr) As Integer Implements ILocalRegistry5.CreateAggregatedManagedInstance
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
