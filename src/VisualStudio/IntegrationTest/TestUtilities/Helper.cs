﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UIAutomationClient;

namespace Microsoft.VisualStudio.IntegrationTest.Utilities
{
    public static class Helper
    {
        /// <summary>
        /// A long timeout used to avoid hangs in tests, where a test failure manifests as an operation never occurring.
        /// </summary>
        public static readonly TimeSpan HangMitigatingTimeout = TimeSpan.FromMinutes(1);

        private static IUIAutomation2 _automation;

        public static IUIAutomation2 Automation
        {
            get
            {
                if (_automation == null)
                {
                    Interlocked.CompareExchange(ref _automation, new CUIAutomation8(), null);
                }

                return _automation;
            }
        }

        /// <summary>
        /// This method will retry the asynchronous action represented by <paramref name="action"/>,
        /// waiting for <paramref name="delay"/> time after each retry. If a given retry returns a value 
        /// other than the default value of <typeparamref name="T"/>, this value is returned.
        /// </summary>
        /// <param name="action">the asynchronous action to retry</param>
        /// <param name="delay">the amount of time to wait between retries</param>
        /// <typeparam name="T">type of return value</typeparam>
        /// <returns>the return value of <paramref name="action"/></returns>
        public static Task<T> RetryAsync<T>(Func<Task<T>> action, TimeSpan delay)
        {
            return RetryAsyncHelper(async () =>
            {
                try
                {
                    return await action();
                }
                catch (COMException)
                {
                    // Devenv can throw COMExceptions if it's busy when we make DTE calls.
                    return default;
                }
            },
                delay);
        }

        private static async Task<T> RetryAsyncHelper<T>(Func<Task<T>> action, TimeSpan delay)
        {
            while (true)
            {
                var retval = await action().ConfigureAwait(true);
                if (!Equals(default(T), retval))
                {
                    return retval;
                }

                await Task.Delay(delay).ConfigureAwait(true);
            }
        }
    }
}
