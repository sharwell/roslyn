// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.LanguageServer.Handler.Diagnostics.DiagnosticSources;
using Microsoft.CodeAnalysis.Options;
using Roslyn.LanguageServer.Protocol;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.LanguageServer.Handler.Diagnostics;

internal abstract class AbstractWorkspacePullDiagnosticsHandler<TDiagnosticsParams, TReport, TReturn>
    : AbstractPullDiagnosticHandler<TDiagnosticsParams, TReport, TReturn>, IDisposable
    where TDiagnosticsParams : IPartialResultParams<TReport>
{
    private readonly LspWorkspaceRegistrationService _workspaceRegistrationService;
    private readonly LspWorkspaceManager _workspaceManager;
    protected readonly IDiagnosticSourceManager DiagnosticSourceManager;

    /// <summary>
    /// Stores the LSP changed state on a per category basis.  This ensures that requests for different categories
    /// are 'walled off' from each other and only reset state for their own category.
    /// </summary>
    private ImmutableDictionary<string, CancellationSeries> _categoryToLspChanged = ImmutableDictionary<string, CancellationSeries>.Empty;

    protected AbstractWorkspacePullDiagnosticsHandler(
        LspWorkspaceManager workspaceManager,
        LspWorkspaceRegistrationService registrationService,
        IDiagnosticAnalyzerService diagnosticAnalyzerService,
        IDiagnosticSourceManager diagnosticSourceManager,
        IDiagnosticsRefresher diagnosticRefresher,
        IGlobalOptionService globalOptions) : base(diagnosticAnalyzerService, diagnosticRefresher, globalOptions)
    {
        DiagnosticSourceManager = diagnosticSourceManager;
        _workspaceManager = workspaceManager;
        _workspaceRegistrationService = registrationService;

        _workspaceRegistrationService.LspSolutionChanged += OnLspSolutionChanged;
        _workspaceManager.LspTextChanged += OnLspTextChanged;
    }

    public void Dispose()
    {
        _workspaceManager.LspTextChanged -= OnLspTextChanged;
        _workspaceRegistrationService.LspSolutionChanged -= OnLspSolutionChanged;
    }

    protected override ValueTask<ImmutableArray<IDiagnosticSource>> GetOrderedDiagnosticSourcesAsync(TDiagnosticsParams diagnosticsParams, string? requestDiagnosticCategory, RequestContext context, CancellationToken cancellationToken)
    {
        if (context.ServerKind == WellKnownLspServerKinds.RazorLspServer)
        {
            // If we're being called from razor, we do not support WorkspaceDiagnostics at all.  For razor, workspace
            // diagnostics will be handled by razor itself, which will operate by calling into Roslyn and asking for
            // document-diagnostics instead.
            return new([]);
        }

        return DiagnosticSourceManager.CreateWorkspaceDiagnosticSourcesAsync(context, requestDiagnosticCategory, cancellationToken);
    }

    private void OnLspSolutionChanged(object? sender, WorkspaceChangeEventArgs e)
    {
        UpdateLspChanged();
    }

    private void OnLspTextChanged(object? sender, EventArgs e)
    {
        UpdateLspChanged();
    }

    private void UpdateLspChanged()
    {
        // Loop through our map of source -> has changed and mark them as all having changed.
        foreach (var (_, cancellationSeries) in _categoryToLspChanged)
        {
            _ = cancellationSeries.CreateNext();
        }
    }

    protected override CancellationToken GetWaitForChangesCancellationToken(string? category, CancellationToken cancellationToken)
    {
        var series = ImmutableInterlocked.GetOrAdd(ref _categoryToLspChanged, category ?? "", static _ => new CancellationSeries());
        return series.CreateNext(cancellationToken);
    }

    internal abstract TestAccessor GetTestAccessor();

    internal readonly struct TestAccessor(AbstractWorkspacePullDiagnosticsHandler<TDiagnosticsParams, TReport, TReturn> handler)
    {
        public void TriggerConnectionClose() => handler.UpdateLspChanged();
    }
}
