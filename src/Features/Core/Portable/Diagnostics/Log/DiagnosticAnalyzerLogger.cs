﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

#nullable enable

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis.Diagnostics.Telemetry;
using Microsoft.CodeAnalysis.Internal.Log;

namespace Microsoft.CodeAnalysis.Diagnostics.Log
{
    internal sealed class DiagnosticAnalyzerLogger
    {
        private const string Id = nameof(Id);
        private const string AnalyzerCount = nameof(AnalyzerCount);
        private const string AnalyzerName = "Analyzer.Name";
        private const string AnalyzerHashCode = "Analyzer.NameHashCode";
        private const string AnalyzerCrashCount = "Analyzer.CrashCount";
        private const string AnalyzerException = "Analyzer.Exception";
        private const string AnalyzerExceptionHashCode = "Analyzer.ExceptionHashCode";

        private static readonly ConditionalWeakTable<DiagnosticAnalyzer, StrongBox<bool>> s_telemetryCache = new ConditionalWeakTable<DiagnosticAnalyzer, StrongBox<bool>>();

        private static string ComputeSha256Hash(string name)
        {
            using var sha256 = SHA256.Create();
            return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(name)));
        }

        public static void LogWorkspaceAnalyzers(ImmutableArray<AnalyzerReference> analyzers)
        {
            Logger.Log(FunctionId.DiagnosticAnalyzerService_Analyzers, KeyValueLogMessage.Create(m =>
            {
                m[AnalyzerCount] = analyzers.Length;
            }));
        }

        public static void LogAnalyzerCrashCount(DiagnosticAnalyzer? analyzer, Exception? ex, LogAggregator? logAggregator)
        {
            if (logAggregator == null || analyzer == null || ex == null || ex is OperationCanceledException)
            {
                return;
            }

            // TODO: once we create description manager, pass that into here.
            var telemetry = AllowsTelemetry(analyzer, null);
            logAggregator.IncreaseCount((telemetry, analyzer.GetType(), ex.GetType()));
        }

        public static void LogAnalyzerCrashCountSummary(int correlationId, LogAggregator logAggregator)
        {
            if (logAggregator == null)
            {
                return;
            }

            foreach (var analyzerCrash in logAggregator)
            {
                Logger.Log(FunctionId.DiagnosticAnalyzerDriver_AnalyzerCrash, KeyValueLogMessage.Create(m =>
                {
                    var key = (ValueTuple<bool, Type, Type>)analyzerCrash.Key;
                    var telemetry = key.Item1;
                    m[Id] = correlationId;

                    // we log analyzer name and exception as it is, if telemetry is allowed
                    if (telemetry)
                    {
                        m[AnalyzerName] = key.Item2.FullName;
                        m[AnalyzerCrashCount] = analyzerCrash.Value.GetCount();
                        m[AnalyzerException] = key.Item3.FullName;
                    }
                    else
                    {
                        var analyzerName = key.Item2.FullName;
                        var exceptionName = key.Item3.FullName;

                        m[AnalyzerHashCode] = ComputeSha256Hash(analyzerName);
                        m[AnalyzerCrashCount] = analyzerCrash.Value.GetCount();
                        m[AnalyzerExceptionHashCode] = ComputeSha256Hash(exceptionName);
                    }
                }));
            }
        }

        public static void LogAnalyzerTypeCountSummary(int correlationId, DiagnosticLogAggregator logAggregator)
        {
            if (logAggregator == null)
            {
                return;
            }

            foreach (var kvp in logAggregator.AnalyzerInfoMap)
            {
                Logger.Log(FunctionId.DiagnosticAnalyzerDriver_AnalyzerTypeCount, KeyValueLogMessage.Create(m =>
                {
                    m[Id] = correlationId;

                    var analyzerInfo = kvp.Value;
                    var hasTelemetry = analyzerInfo.Telemetry;

                    // we log analyzer name as it is, if telemetry is allowed
                    if (hasTelemetry)
                    {
                        m[AnalyzerName] = analyzerInfo.CLRType.FullName;
                    }
                    else
                    {
                        // if it is from third party, we use hashcode
                        m[AnalyzerHashCode] = ComputeSha256Hash(analyzerInfo.CLRType.FullName);
                    }

                    for (var i = 0; i < analyzerInfo.Counts.Length; i++)
                    {
                        m[DiagnosticLogAggregator.AnalyzerTypes[i]] = analyzerInfo.Counts[i];
                    }
                }));
            }
        }

        public static bool AllowsTelemetry(DiagnosticAnalyzer analyzer, IDiagnosticAnalyzerService? analyzerService = null)
        {
            if (s_telemetryCache.TryGetValue(analyzer, out var value))
            {
                return value.Value;
            }

            return s_telemetryCache.GetValue(analyzer, a => new StrongBox<bool>(CheckTelemetry(a, analyzerService))).Value;
        }

        private static bool CheckTelemetry(DiagnosticAnalyzer analyzer, IDiagnosticAnalyzerService? analyzerService)
        {
            if (analyzer.IsCompilerAnalyzer())
            {
                return true;
            }

            if (analyzer is IBuiltInAnalyzer)
            {
                // if it is builtin analyzer, telemetry is always allowed
                return true;
            }

            ImmutableArray<DiagnosticDescriptor> diagDescriptors;
            try
            {
                // SupportedDiagnostics is potentially user code and can throw an exception.
                diagDescriptors = analyzerService != null ? analyzerService.GetDiagnosticDescriptors(analyzer) : analyzer.SupportedDiagnostics;
            }
            catch (Exception)
            {
                return false;
            }

            if (diagDescriptors == null)
            {
                return false;
            }

            // find if the first diagnostic in this analyzer allows telemetry
            var diagnostic = diagDescriptors.Length > 0 ? diagDescriptors[0] : null;
            return diagnostic == null ? false : diagnostic.CustomTags.Any(t => t == WellKnownDiagnosticTags.Telemetry);
        }
    }
}
