// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Editor.UnitTests;
using Microsoft.CodeAnalysis.Editor.UnitTests.Workspaces;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.FindSymbols.SymbolTree;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Storage;
using Microsoft.CodeAnalysis.Test.Utilities;
using Microsoft.VisualStudio.Composition;

namespace IdeBenchmarks
{
    [ShortRunJob]
    public class PersistentStorageBenchmarks
    {
        private readonly UseExportProviderAttribute _useExportProviderAttribute = new UseExportProviderAttribute();
        private ComposableCatalog _catalog;
        private IExportProviderFactory _exportProviderFactory;

        [Params(0, 1000, 10000)]
        public int ClassCount { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            _catalog = TestExportProvider.EntireAssemblyCatalogWithCSharpAndVisualBasic.WithPart(typeof(PersistentStorageLocationService));
            _exportProviderFactory = ExportProviderCache.GetOrCreateExportProviderFactory(_catalog);
        }

        [IterationSetup]
        public void IterationSetup()
            => _useExportProviderAttribute.Before(null);

        [IterationCleanup]
        public void IterationCleanup()
            => _useExportProviderAttribute.After(null);

        [Benchmark]
        public object TestProject()
        {
            var files = CreateTestInput(fileCount: ClassCount);
            return WriteStorageAsync(files, parseOptions: null).Result;
        }

        private static XElement CreateTestInput(int fileCount)
        {
            var documents = new XElement[fileCount];
            for (var i = 0; i < fileCount; i++)
            {
                documents[i] = new XElement("Document",
                    new XAttribute("FilePath", $"Class{i}.cs"),
                    new XText($@"
class Class{i}
{{
}}
"));
            }

            return new XElement(
                "Workspace",
                new XAttribute("FilePath", "SolutionPath.sln"),
                new XElement(
                    "Project",
                    new XAttribute("AssemblyName", "CSharpAssembly"),
                    new XAttribute("Language", LanguageNames.CSharp),
                    new XAttribute("CommonReferences", "true"),
                    documents));
        }

        private async Task<SymbolTreeInfo> WriteStorageAsync(XElement workspaceElement, ParseOptions parseOptions)
        {
            using (var workspace = TestWorkspace.Create(workspaceElement.ToString(), exportProvider: _exportProviderFactory.CreateExportProvider()))
            {
                workspace.Options = workspace.Options.WithChangedOption(StorageOptions.SolutionSizeThreshold, -1);

                var persistentStorageService = workspace.Services.GetRequiredService<IPersistentStorageService>();
                var persistentStorage = persistentStorageService.GetStorage(workspace.CurrentSolution);
                if (persistentStorage is NoOpPersistentStorage)
                {
                    throw new InvalidOperationException("Benchmark is not configured to use persistent storage.");
                }

                var symbolTreeInfoCacheService = workspace.Services.GetRequiredService<ISymbolTreeInfoCacheService>();
                var symbolTreeInfo = await symbolTreeInfoCacheService.TryGetSourceSymbolTreeInfoAsync(workspace.CurrentSolution.Projects.Single(), CancellationToken.None);
                if (symbolTreeInfo is null)
                {
                    throw new InvalidOperationException("Benchmark failed to calculate symbol tree info.");
                }

                return symbolTreeInfo;
            }
        }

        [ExportWorkspaceService(typeof(IPersistentStorageLocationService), ServiceLayer.Host)]
        [Shared]
        [PartNotDiscoverable]
        private class PersistentStorageLocationService : IPersistentStorageLocationService
        {
            private readonly string _storageLocation = Path.Combine(TempRoot.Root, Path.GetRandomFileName());

            [ImportingConstructor]
            [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
            public PersistentStorageLocationService()
            {
            }

            event EventHandler<PersistentStorageLocationChangingEventArgs> IPersistentStorageLocationService.StorageLocationChanging
            {
                add { }
                remove { }
            }

            public bool IsSupported(Workspace workspace)
            {
                return workspace.CurrentSolution.FilePath != null;
            }

            public string TryGetStorageLocation(SolutionId solutionId)
            {
                return _storageLocation;
            }
        }
    }
}
