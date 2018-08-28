// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Composition;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.LanguageService
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class PreloadServicesAttribute : ExportAttribute
    {
        public PreloadServicesAttribute(params Type[] preloadedServiceTypes)
            : base(typeof(IPreloadService))
        {
            PreloadedServiceTypes = preloadedServiceTypes ?? Type.EmptyTypes;
        }

        public Type[] PreloadedServiceTypes
        {
            get;
        }
    }
}
