﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.VisualStudio.IntegrationTest.Utilities.InProcess2
{
    public class Debugger_InProc2 : InProcComponent2
    {
        /// <summary>
        /// HResult for "Operation Not Supported" when raising commands. 
        /// </summary>
        private const uint OperationNotSupportedHResult = 0x8971003c;

        /// <summary>
        /// Time to wait between retries if "Operation Not Supported" is thrown when raising a debugger stepping command.
        /// </summary>
        private static readonly TimeSpan DebuggerCommandRetryTimeout = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Time to wait before re-polling a delegate.
        /// </summary>
        private static readonly TimeSpan DefaultPollingInterCallSleep = TimeSpan.FromMilliseconds(250);

        public Debugger_InProc2(TestServices testServices)
            : base(testServices)
        {
        }

        private async Task<EnvDTE.Debugger> GetDebuggerAsync()
        {
            return (await GetDTEAsync()).Debugger;
        }

        public async Task SetBreakPointAsync(string fileName, int lineNumber, int columnIndex)
        {
            var debugger = await GetDebuggerAsync();

            // Need to increment the line number because editor line numbers starts from 0 but the debugger ones starts from 1.
            debugger.Breakpoints.Add(File: fileName, Line: lineNumber + 1, Column: columnIndex);
        }

        public async Task SetBreakPointAsync(string fileName, string text, int charsOffset = 0)
        {
            await TestServices.Editor.SelectTextInCurrentDocumentAsync(text);
            var lineNumber = await TestServices.Editor.GetLineAsync();
            var columnIndex = await TestServices.Editor.GetColumnAsync();

            await SetBreakPointAsync(fileName, lineNumber, columnIndex + charsOffset);
        }

        public async Task GoAsync(bool waitForBreakMode)
        {
            var debugger = await GetDebuggerAsync();

            debugger.Go(waitForBreakMode);

            // Yield to ensure main thread operations at higher priority than Background are completed before the test
            // continues.
            await Task.Yield();
        }

        public async Task StepOverAsync(bool waitForBreakOrEnd)
        {
            await WaitForRaiseDebuggerDteCommandAsync(async () =>
            {
                var debugger = await GetDebuggerAsync();
                debugger.StepOver(waitForBreakOrEnd);
            });
        }

        public async Task StopAsync(bool waitForDesignMode)
        {
            await ExecuteCommandAsync(WellKnownCommandNames.Debug_StopDebugging);
        }

        public async Task SetNextStatementAsync()
        {
            var debugger = await GetDebuggerAsync();
            debugger.SetNextStatement();
        }

        public async Task ExecuteStatementAsync(string statement)
        {
            var debugger = await GetDebuggerAsync();
            debugger.ExecuteStatement(statement);
        }

        public async Task<Common.Expression> GetExpressionAsync(string expressionText)
        {
            var debugger = await GetDebuggerAsync();
            return new Common.Expression(debugger.GetExpression(expressionText));
        }

        public async Task CheckExpressionAsync(string expressionText, string expectedType, string expectedValue)
        {
            var entry = await GetExpressionAsync(expressionText);
            Assert.Equal(expectedType, entry.Type);
            Assert.Equal(expectedValue, entry.Value);
        }

        /// <summary>
        /// Executes the specified action delegate and retries if Operation Not Supported is thrown.
        /// </summary>
        /// <param name="action">Action delegate to exectute.</param>
        private async Task WaitForRaiseDebuggerDteCommandAsync(Func<Task> action)
        {
            var actionSucceeded = false;

            Func<bool> predicate = delegate
            {
                try
                {
                    action();
                    actionSucceeded = true;
                }
                catch (COMException ex)
                {
                    if ((uint)ex.ErrorCode != OperationNotSupportedHResult)
                    {
                        var message = string.Format(
                            CultureInfo.InvariantCulture,
                            "Failed to raise debugger command, an unexpected '{0}' was thrown with the HResult of '{1}'.",
                            typeof(COMException),
                            ex.ErrorCode);

                        throw new Exception(message, ex);
                    }

                    actionSucceeded = false;
                }

                return actionSucceeded;
            };

            // Repeat the command if "Operation Not Supported" is thrown.
            if (!await TryWaitForAsync(DebuggerCommandRetryTimeout, predicate))
            {
                var message = string.Format(
                    CultureInfo.InvariantCulture,
                    "Failed to raise debugger command within '{0}' seconds.",
                    DebuggerCommandRetryTimeout.TotalSeconds);

                throw new Exception(message);
            }
        }

        /// <summary>
        /// Polls for the specified delegate to return true for the given timeout.
        /// </summary>
        /// <param name="timeout">Timeout to keep polling.</param>
        /// <param name="predicate">Delegate to invoke.</param>
        /// <returns>True if the delegate returned true when polled, otherwise false.</returns>
        public static async Task<bool> TryWaitForAsync(TimeSpan timeout, Func<bool> predicate)
        {
            return await TryWaitForAsync(timeout, DefaultPollingInterCallSleep, predicate);
        }

        /// <summary>
        /// Polls for the specified delegate to return true for the given timeout.
        /// </summary>
        /// <param name="timeout">Timeout to keep polling.</param>
        /// <param name="interval">Time to wait between polling.</param>
        /// <param name="predicate">Delegate to invoke.</param>
        /// <returns>
        /// True if the delegate returned true when polled, otherwise false.
        /// </returns>
        private static async Task<bool> TryWaitForAsync(TimeSpan timeout, TimeSpan interval, Func<bool> predicate)
        {
            var endTime = DateTime.UtcNow + timeout;
            var validationDelegateSuccess = false;

            while (DateTime.UtcNow < endTime)
            {
                // Note: we don't do this inline in the while() condition and return the result of 
                // (DateTime.Now < startTime + timeout) as this could lead to cases where the validation
                // delegate returned true, at the boundary of the valid time, and would return false
                // when hitting the return statement. 
                if (predicate())
                {
                    validationDelegateSuccess = true;
                    break;
                }

                await Task.Delay(interval);
            }

            return validationDelegateSuccess;
        }
    }
}
