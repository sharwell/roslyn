// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using Microsoft.CodeAnalysis.Test.Utilities;
using IMessageFilter = Microsoft.VisualStudio.OLE.Interop.IMessageFilter;
using INTERFACEINFO = Microsoft.VisualStudio.OLE.Interop.INTERFACEINFO;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using PENDINGMSG = Microsoft.VisualStudio.OLE.Interop.PENDINGMSG;
using SERVERCALL = Microsoft.VisualStudio.OLE.Interop.SERVERCALL;

namespace Roslyn.Test.Utilities
{
    public sealed class MessageFilter : IMessageFilter, IOleServiceProvider, IDisposable
    {
        private const uint CancelCall = ~0U;
        private const int E_NOTIMPL = -2147467263;

        private readonly MessageFilterSafeHandle _messageFilterRegistration;
        private readonly TimeSpan _timeout;
        private readonly TimeSpan _retryDelay;

        public MessageFilter()
            : this(timeout: TimeSpan.FromSeconds(60), retryDelay: TimeSpan.FromMilliseconds(150))
        {
        }

        public MessageFilter(TimeSpan timeout, TimeSpan retryDelay)
        {
            _timeout = timeout;
            _retryDelay = retryDelay;
            _messageFilterRegistration = MessageFilterSafeHandle.Register(this);
        }

        public uint HandleInComingCall(uint dwCallType, IntPtr htaskCaller, uint dwTickCount, INTERFACEINFO[] lpInterfaceInfo)
        {
            return (uint)SERVERCALL.SERVERCALL_ISHANDLED;
        }

        public uint RetryRejectedCall(IntPtr htaskCallee, uint dwTickCount, uint dwRejectType)
        {
            if ((SERVERCALL)dwRejectType != SERVERCALL.SERVERCALL_RETRYLATER
                && (SERVERCALL)dwRejectType != SERVERCALL.SERVERCALL_REJECTED)
            {
                return CancelCall;
            }

            if (dwTickCount >= _timeout.TotalMilliseconds)
            {
                return CancelCall;
            }

            return (uint)_retryDelay.TotalMilliseconds;
        }

        public uint MessagePending(IntPtr htaskCallee, uint dwTickCount, uint dwPendingType)
        {
            return (uint)PENDINGMSG.PENDINGMSG_WAITDEFPROCESS;
        }

        public void Dispose()
        {
            _messageFilterRegistration.Dispose();
        }

        int IOleServiceProvider.QueryService(ref Guid guidService, ref Guid riid, out IntPtr ppvObject)
        {
            var exportProvider = ExportProviderCache.LocalExportProviderForCleanup;
            var serviceProvider = exportProvider?.GetExportedValues<IOleServiceProvider>().SingleOrDefault();
            if (serviceProvider is object)
            {
                return serviceProvider.QueryService(ref guidService, ref riid, out ppvObject);
            }

            ppvObject = IntPtr.Zero;
            return E_NOTIMPL;
        }
    }
}
