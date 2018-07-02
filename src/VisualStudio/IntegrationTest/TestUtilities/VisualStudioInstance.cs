﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Serialization.Formatters;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio.IntegrationTest.Utilities.Harness;
using Microsoft.VisualStudio.IntegrationTest.Utilities.InProcess;
using Microsoft.VisualStudio.IntegrationTest.Utilities.Input;
using Microsoft.VisualStudio.IntegrationTest.Utilities.OutOfProcess;

using Process = System.Diagnostics.Process;

namespace Microsoft.VisualStudio.IntegrationTest.Utilities
{
    public class VisualStudioInstance
    {
        private readonly IntegrationService _integrationService;
        private readonly IpcChannel _integrationServiceChannel;
        private readonly VisualStudio_InProc _inProc;

        public ChangeSignatureDialog_OutOfProc ChangeSignatureDialog { get; }

        public ObjectBrowserWindow_OutOfProc ObjectBrowserWindow { get; }

        public Debugger_OutOfProc Debugger { get; }

        public Dialog_OutOfProc Dialog { get; }

        public Editor_OutOfProc Editor { get; }

        public EncapsulateField_OutOfProc EncapsulateField { get; }

        public ExtractInterfaceDialog_OutOfProc ExtractInterfaceDialog { get; }

        public FindReferencesWindow_OutOfProc FindReferencesWindow { get; }

        public GenerateTypeDialog_OutOfProc GenerateTypeDialog { get; }

        public ImmediateWindow_OutOfProc ImmediateWindow { get; }

        public InlineRenameDialog_OutOfProc InlineRenameDialog { get; set; }

        public LocalsWindow_OutOfProc LocalsWindow { get; set; }

        public PreviewChangesDialog_OutOfProc PreviewChangesDialog { get; }

        public SendKeys SendKeys { get; }

        public Shell_OutOfProc Shell { get; }

        public SolutionExplorer_OutOfProc SolutionExplorer { get; }

        public VisualStudioWorkspace_OutOfProc Workspace { get; }

        public TestInvoker_OutOfProc TestInvoker { get; }

        internal DTE Dte { get; }

        internal Process HostProcess { get; }

        /// <summary>
        /// The set of Visual Studio packages that are installed into this instance.
        /// </summary>
        public ImmutableHashSet<string> SupportedPackageIds { get; }

        /// <summary>
        /// The path to the root of this installed version of Visual Studio. This is the folder that contains
        /// Common7\IDE.
        /// </summary>
        public string InstallationPath { get; }

        public VisualStudioInstance(Process hostProcess, DTE dte, ImmutableHashSet<string> supportedPackageIds, string installationPath)
        {
            HostProcess = hostProcess;
            Dte = dte;
            SupportedPackageIds = supportedPackageIds;
            InstallationPath = installationPath;

            if (System.Diagnostics.Debugger.IsAttached)
            {
                // If a Visual Studio debugger is attached to the test process, attach it to the instance running
                // integration tests as well.
                var debuggerHostDte = GetDebuggerHostDte();
                int targetProcessId = Process.GetCurrentProcess().Id;
                var localProcess = debuggerHostDte?.Debugger.LocalProcesses.OfType<EnvDTE80.Process2>().FirstOrDefault(p => p.ProcessID == hostProcess.Id);
                if (localProcess != null)
                {
                    //debuggerHostDte.Debugger.DetachAll();
                    localProcess.Attach2("Managed");
                }
            }

            StartRemoteIntegrationService(dte);

            string portName = $"IPC channel client for {HostProcess.Id}; {Guid.NewGuid():b}";
            _integrationServiceChannel = new IpcChannel(
                new Hashtable
                {
                    { "name", portName },
                    { "portName", portName },
                },
                new BinaryClientFormatterSinkProvider(),
                new BinaryServerFormatterSinkProvider { TypeFilterLevel = TypeFilterLevel.Full });

            ChannelServices.RegisterChannel(_integrationServiceChannel, ensureSecurity: true);

            // Connect to a 'well defined, shouldn't conflict' IPC channel
            _integrationService = IntegrationService.GetInstanceFromHostProcess(hostProcess);
            _integrationService.AddAssemblyResolver(new CrossProcessAssemblyResolver());

            // Create marshal-by-ref object that runs in host-process.
            _inProc = ExecuteInHostProcess<VisualStudio_InProc>(
                type: typeof(VisualStudio_InProc),
                methodName: nameof(VisualStudio_InProc.Create)
            );

            // There is a lot of VS initialization code that goes on, so we want to wait for that to 'settle' before
            // we start executing any actual code.
            _inProc.WaitForSystemIdle();

            ChangeSignatureDialog = new ChangeSignatureDialog_OutOfProc(this);
            ObjectBrowserWindow = new ObjectBrowserWindow_OutOfProc(this);
            Debugger = new Debugger_OutOfProc(this);
            Dialog = new Dialog_OutOfProc(this);
            Editor = new Editor_OutOfProc(this);
            EncapsulateField = new EncapsulateField_OutOfProc(this);
            ExtractInterfaceDialog = new ExtractInterfaceDialog_OutOfProc(this);
            FindReferencesWindow = new FindReferencesWindow_OutOfProc(this);
            GenerateTypeDialog = new GenerateTypeDialog_OutOfProc(this);
            InlineRenameDialog = new InlineRenameDialog_OutOfProc(this);
            ImmediateWindow = new ImmediateWindow_OutOfProc(this);
            LocalsWindow = new LocalsWindow_OutOfProc(this);
            PreviewChangesDialog = new PreviewChangesDialog_OutOfProc(this);
            Shell = new Shell_OutOfProc(this);
            SolutionExplorer = new SolutionExplorer_OutOfProc(this);
            Workspace = new VisualStudioWorkspace_OutOfProc(this);
            TestInvoker = new TestInvoker_OutOfProc(this);

            SendKeys = new SendKeys(this);

            // Ensure we are in a known 'good' state by cleaning up anything changed by the previous instance
            CleanUp();
        }

        public void ExecuteInHostProcess(Type type, string methodName)
        {
            var result = _integrationService.Execute(type.Assembly.Location, type.FullName, methodName);

            if (result != null)
            {
                throw new InvalidOperationException("The specified call was not expected to return a value.");
            }
        }

        public T ExecuteInHostProcess<T>(Type type, string methodName)
        {
            var objectUri = _integrationService.Execute(type.Assembly.Location, type.FullName, methodName) ?? throw new InvalidOperationException("The specified call was expected to return a value.");
            return (T)Activator.GetObject(typeof(T), $"{_integrationService.BaseUri}/{objectUri}");
        }

        public void ActivateMainWindow(bool skipAttachingThreads = false)
            => _inProc.ActivateMainWindow(skipAttachingThreads);

        public void WaitForApplicationIdle(CancellationToken cancellationToken)
        {
            var task = Task.Factory.StartNew(() => _inProc.WaitForApplicationIdle(), cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            task.Wait(cancellationToken);
        }

        public void ExecuteCommand(string commandName, string argument = "")
            => _inProc.ExecuteCommand(commandName, argument);

        public bool IsCommandAvailable(string commandName)
            => _inProc.IsCommandAvailable(commandName);

        public string[] GetAvailableCommands()
            => _inProc.GetAvailableCommands();

        public int ErrorListErrorCount
            => _inProc.GetErrorListErrorCount();

        public void WaitForNoErrorsInErrorList()
            => _inProc.WaitForNoErrorsInErrorList();

        public bool IsRunning => !HostProcess.HasExited;

        public void CleanUp()
        {
            Workspace.CleanUpWaitingService();
            Workspace.CleanUpWorkspace();
            SolutionExplorer.CleanUpOpenSolution();

            // Close any windows leftover from previous (failed) tests
            ObjectBrowserWindow.CloseWindow();
            ChangeSignatureDialog.CloseWindow();
            GenerateTypeDialog.CloseWindow();
            ExtractInterfaceDialog.CloseWindow();
        }

        public void Close(bool exitHostProcess = true)
        {
            if (!IsRunning)
            {
                return;
            }

            CleanUp();

            CloseRemotingService();

            if (exitHostProcess)
            {
                CloseHostProcess();
            }
        }

        private static DTE GetDebuggerHostDte()
        {
            var currentProcessId = Process.GetCurrentProcess().Id;
            foreach (var process in Process.GetProcessesByName("devenv"))
            {
                var dte = IntegrationHelper.TryLocateDteForProcess(process);
                if (dte?.Debugger?.DebuggedProcesses?.OfType<EnvDTE.Process>().Any(p => p.ProcessID == currentProcessId) ?? false)
                {
                    return dte;
                }
            }

            return null;
        }

        private void CloseHostProcess()
        {
            _inProc.Quit();
            if (!HostProcess.WaitForExit(milliseconds: 10000))
            {
                IntegrationHelper.KillProcess(HostProcess);
            }
        }

        private void CloseRemotingService()
        {
            try
            {
                StopRemoteIntegrationService();
            }
            finally
            {
                if (_integrationServiceChannel != null)
                {
                    if (ChannelServices.RegisteredChannels.Contains(_integrationServiceChannel))
                    {
                        ChannelServices.UnregisterChannel(_integrationServiceChannel);
                    }

                    _integrationServiceChannel.StopListening(null);
                }
            }
        }

        private void StartRemoteIntegrationService(DTE dte)
        {
            // We use DTE over RPC to start the integration service. All other DTE calls should happen in the host process.

            if (dte.Commands.Item(WellKnownCommandNames.Test_IntegrationTestService_Start).IsAvailable)
            {
                dte.ExecuteCommand(WellKnownCommandNames.Test_IntegrationTestService_Start);
            }
        }

        private void StopRemoteIntegrationService()
        {
            if (_inProc.IsCommandAvailable(WellKnownCommandNames.Test_IntegrationTestService_Stop))
            {
                _inProc.ExecuteCommand(WellKnownCommandNames.Test_IntegrationTestService_Stop);
            }
        }

        public TelemetryVerifier EnableTestTelemetryChannel()
        {
            _inProc.EnableTestTelemetryChannel();
            return new TelemetryVerifier(this);
        }

        private void DisableTestTelemetryChannel()
            => _inProc.DisableTestTelemetryChannel();

        private void WaitForTelemetryEvents(string[] names)
            => _inProc.WaitForTelemetryEvents(names);

        public class TelemetryVerifier : IDisposable
        {
            internal VisualStudioInstance _instance;

            public TelemetryVerifier(VisualStudioInstance instance)
            {
                _instance = instance;
            }

            public void Dispose() => _instance.DisableTestTelemetryChannel();

            /// <summary>
            /// Asserts that a telemetry event of the given name was fired. Does not
            /// do any additional validation (of performance numbers, etc).
            /// </summary>
            /// <param name="expectedEventNames"></param>
            public void VerifyFired(params string[] expectedEventNames)
            {
                _instance.WaitForTelemetryEvents(expectedEventNames);
            }
        }
    }
}
