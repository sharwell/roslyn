﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Composition;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace Microsoft.CodeAnalysis.Test.Utilities
{
    [Export(typeof(IVsEditorAdaptersFactoryService))]
    [Shared]
    [PartNotDiscoverable]
    internal sealed class FakeVsEditorAdaptersFactoryService : IVsEditorAdaptersFactoryService
    {
        [ImportingConstructor]
        [Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
        public FakeVsEditorAdaptersFactoryService()
        {
        }

        public IVsCodeWindow CreateVsCodeWindowAdapter(IOleServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }

        public IVsTextBuffer CreateVsTextBufferAdapter(IOleServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }

        public IVsTextBuffer CreateVsTextBufferAdapter(IOleServiceProvider serviceProvider, IContentType contentType)
        {
            throw new NotImplementedException();
        }

        public IVsTextBuffer CreateVsTextBufferAdapterForSecondaryBuffer(IOleServiceProvider serviceProvider, ITextBuffer secondaryBuffer)
        {
            throw new NotImplementedException();
        }

        public IVsTextBufferCoordinator CreateVsTextBufferCoordinatorAdapter()
        {
            throw new NotImplementedException();
        }

        public IVsTextView CreateVsTextViewAdapter(IOleServiceProvider serviceProvider)
        {
            throw new NotImplementedException();
        }

        public IVsTextView CreateVsTextViewAdapter(IOleServiceProvider serviceProvider, ITextViewRoleSet roles)
        {
            throw new NotImplementedException();
        }

        public IVsTextBuffer GetBufferAdapter(ITextBuffer textBuffer)
        {
            throw new NotImplementedException();
        }

        public ITextBuffer GetDataBuffer(IVsTextBuffer bufferAdapter)
        {
            throw new NotImplementedException();
        }

        public ITextBuffer GetDocumentBuffer(IVsTextBuffer bufferAdapter)
        {
            throw new NotImplementedException();
        }

        public IVsTextView GetViewAdapter(ITextView textView)
        {
            throw new NotImplementedException();
        }

        public IWpfTextView GetWpfTextView(IVsTextView viewAdapter)
        {
            throw new NotImplementedException();
        }

        public IWpfTextViewHost GetWpfTextViewHost(IVsTextView viewAdapter)
        {
            throw new NotImplementedException();
        }

        public void SetDataBuffer(IVsTextBuffer bufferAdapter, ITextBuffer dataBuffer)
        {
            throw new NotImplementedException();
        }
    }
}
