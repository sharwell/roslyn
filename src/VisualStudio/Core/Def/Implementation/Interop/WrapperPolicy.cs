// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.Interop
{
    internal static class WrapperPolicy
    {
        /// <summary>
        /// Factory object for creating IComWrapper instances.
        /// </summary>
        private static IComWrapperFactory s_comWrapperFactory;

        private static IComWrapperFactory ComWrapperFactory
        {
            get
            {
                var factory = s_comWrapperFactory;
                if (factory is null)
                {
                    factory = PackageUtilities.CreateInstance(typeof(IComWrapperFactory).GUID) as IComWrapperFactory;
                    factory = Interlocked.CompareExchange(ref s_comWrapperFactory, factory, null) ?? factory;
                }

                Assumes.Present(factory);
                return factory;
            }
        }

        internal static object CreateAggregatedObject(object managedObject) => ComWrapperFactory.CreateAggregatedObject(managedObject);

        /// <summary>
        /// Return the RCW for the native IComWrapper instance aggregating "managedObject"
        /// if there is one. Return "null" if "managedObject" is not aggregated.
        /// </summary>
        internal static IComWrapper TryGetWrapper(object managedObject)
        {
            // Note: this method should be "return managedObject" once we can get rid of this while IComWrapper
            // business.

            // This force the CLR to retrieve the "outer" object of "managedObject"
            // if "managedObject" has been aggregated
            var ptr = Marshal.GetIUnknownForObject(managedObject);
            try
            {
                // This asks the CLR to return the RCW corresponding to the
                // aggregator object.
                var wrapper = Marshal.GetObjectForIUnknown(ptr);

                // The aggregator (if there is one) implement IComWrapper!
                return wrapper as IComWrapper;
            }
            finally
            {
                Marshal.Release(ptr);
            }
        }

        internal readonly struct TestAccessor
        {
            public static ref IComWrapperFactory ComWrapperFactory => ref s_comWrapperFactory;
        }
    }
}
