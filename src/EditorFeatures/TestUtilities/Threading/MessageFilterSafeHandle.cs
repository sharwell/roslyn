// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using IMessageFilter = Microsoft.VisualStudio.OLE.Interop.IMessageFilter;

namespace Roslyn.Test.Utilities
{
    internal sealed class MessageFilterSafeHandle : SafeHandleMinusOneIsInvalid
    {
        private readonly IntPtr _oldFilter;

        private MessageFilterSafeHandle(IntPtr handle)
            : base(true)
        {
            SetHandle(handle);

            try
            {
                if (CoRegisterMessageFilter(handle, out _oldFilter) != 0)
                {
                    throw new InvalidOperationException("Failed to register a new message filter");
                }
            }
            catch
            {
                Marshal.Release(handle);
                SetHandleAsInvalid();
                throw;
            }
        }

        [DllImport("ole32", SetLastError = true)]
        private static extern int CoRegisterMessageFilter(IntPtr messageFilter, out IntPtr oldMessageFilter);

        public static MessageFilterSafeHandle Register<T>(T messageFilter)
            where T : IMessageFilter
        {
            var handle = Marshal.GetComInterfaceForObject<T, IMessageFilter>(messageFilter);
            return new MessageFilterSafeHandle(handle);
        }

        protected override bool ReleaseHandle()
        {
            if (CoRegisterMessageFilter(_oldFilter, out _) == 0)
            {
                // Release twice:
                // 1. The call to 'GetComInterfaceForObject'
                Marshal.Release(handle);

                // 2. The call to 'CoRegisterMessageFilter'
                Marshal.Release(handle);
            }

            return true;
        }
    }
}
