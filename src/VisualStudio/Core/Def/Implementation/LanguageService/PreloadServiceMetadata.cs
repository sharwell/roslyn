// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Roslyn.Utilities;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.LanguageService
{
    internal class PreloadServiceMetadata
    {
        public PreloadServiceMetadata(IDictionary<string, object> data)
        {
            PreloadedServices = ImmutableArray.CreateRange((IEnumerable<Type>)data.GetValueOrDefault(nameof(PreloadServicesAttribute.PreloadedServiceTypes)));
        }

        public ImmutableArray<Type> PreloadedServices
        {
            get;
        }
    }
}
