' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.
' See the LICENSE file in the project root for more information.

Imports System.Runtime.InteropServices
Imports Microsoft.CodeAnalysis.Test.Utilities
Imports Microsoft.VisualStudio.Shell.Interop

Namespace Microsoft.VisualStudio.LanguageServices.UnitTests
    Friend NotInheritable Class StubComWrapperFactory
        Implements IComWrapperFactory

        Public Function CreateAggregatedObject(managedObject As Object) As Object Implements IComWrapperFactory.CreateAggregatedObject
            Dim wrapperUnknown = BlindAggregatorFactory.CreateWrapper()
            Try
                Dim innerUnknown = Marshal.CreateAggregatedObject(wrapperUnknown, managedObject)
                Try
                    Dim handle = GCHandle.Alloc(managedObject, GCHandleType.Normal)
                    Dim freeHandle = True
                    Try
#Disable Warning RS0042 ' Do not copy value
                        BlindAggregatorFactory.SetInnerObject(wrapperUnknown, innerUnknown, GCHandle.ToIntPtr(handle))
#Enable Warning RS0042 ' Do not copy value
                        freeHandle = False
                    Finally
                        If freeHandle Then handle.Free()
                    End Try

                    Dim wrapperRCW = Marshal.GetObjectForIUnknown(wrapperUnknown)
                    Return CType(wrapperRCW, IComWrapper)
                Finally
                    Marshal.Release(innerUnknown)
                End Try
            Finally
                Marshal.Release(wrapperUnknown)
            End Try
        End Function
    End Class
End Namespace
