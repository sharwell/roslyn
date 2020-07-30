' Licensed to the .NET Foundation under one or more agreements.
' The .NET Foundation licenses this file to you under the MIT license.
' See the LICENSE file in the project root for more information.

Imports Microsoft.CodeAnalysis.Editor.Shared.Utilities
Imports Microsoft.VisualStudio.Shell.Interop

Namespace Microsoft.VisualStudio.LanguageServices.UnitTests
    Friend NotInheritable Class StubTaskSchedulerService
        Implements IVsTaskSchedulerService, IVsTaskSchedulerService2

        Private ReadOnly _threadingContext As IThreadingContext

        Public Sub New(threadingContext As IThreadingContext)
            _threadingContext = threadingContext
        End Sub

        Public Function CreateTask(context As UInteger, pTaskBody As IVsTaskBody) As IVsTask Implements IVsTaskSchedulerService.CreateTask
            Throw New NotImplementedException()
        End Function

        Public Function CreateTaskEx(context As UInteger, options As UInteger, pTaskBody As IVsTaskBody, pAsyncState As Object) As IVsTask Implements IVsTaskSchedulerService.CreateTaskEx
            Throw New NotImplementedException()
        End Function

        Public Function ContinueWhenAllCompleted(context As UInteger, dwTasks As UInteger, pDependentTasks() As IVsTask, pTaskBody As IVsTaskBody) As IVsTask Implements IVsTaskSchedulerService.ContinueWhenAllCompleted
            Throw New NotImplementedException()
        End Function

        Public Function ContinueWhenAllCompletedEx(context As UInteger, dwTasks As UInteger, pDependentTasks() As IVsTask, options As UInteger, pTaskBody As IVsTaskBody, pAsyncState As Object) As IVsTask Implements IVsTaskSchedulerService.ContinueWhenAllCompletedEx
            Throw New NotImplementedException()
        End Function

        Public Function CreateTaskCompletionSource() As IVsTaskCompletionSource Implements IVsTaskSchedulerService.CreateTaskCompletionSource
            Throw New NotImplementedException()
        End Function

        Public Function CreateTaskCompletionSourceEx(options As UInteger, AsyncState As Object) As IVsTaskCompletionSource Implements IVsTaskSchedulerService.CreateTaskCompletionSourceEx
            Throw New NotImplementedException()
        End Function

        Public Function GetAsyncTaskContext() As Object Implements IVsTaskSchedulerService2.GetAsyncTaskContext
            Return _threadingContext.JoinableTaskContext
        End Function

        Public Function GetTaskScheduler(context As UInteger) As Object Implements IVsTaskSchedulerService2.GetTaskScheduler
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
