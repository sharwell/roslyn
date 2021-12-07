// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Microsoft.CodeAnalysis.Extensions
{
    internal static class IExtensionManagerExtensions
    {
        public static void PerformAction(this IExtensionManager extensionManager, object extension, Action action)
        {
            try
            {
                if (!extensionManager.IsDisabled(extension))
                {
                    action();
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e) when (extensionManager.CanHandleException(extension, e))
            {
                extensionManager.HandleException(extension, e);
            }
        }

        public static T PerformFunction<T>(
            this IExtensionManager extensionManager,
            object extension,
            Func<T> function,
            T defaultValue)
        {
            try
            {
                if (!extensionManager.IsDisabled(extension))
                {
                    return function();
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e) when (extensionManager.CanHandleException(extension, e))
            {
                extensionManager.HandleException(extension, e);
            }

            return defaultValue;
        }

        public static T PerformFunction<T, TArg>(
            this IExtensionManager extensionManager,
            object extension,
            Func<TArg, T> function,
            TArg arg,
            T defaultValue)
        {
            try
            {
                if (!extensionManager.IsDisabled(extension))
                {
                    return function(arg);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e) when (extensionManager.CanHandleException(extension, e))
            {
                extensionManager.HandleException(extension, e);
            }

            return defaultValue;
        }

        public static async Task PerformActionAsync(
            this IExtensionManager extensionManager,
            object extension,
            Func<Task?> function)
        {
            try
            {
                if (!extensionManager.IsDisabled(extension))
                {
                    var task = function() ?? Task.CompletedTask;
                    await task.ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e) when (extensionManager.CanHandleException(extension, e))
            {
                extensionManager.HandleException(extension, e);
            }
        }

        public static async Task<T> PerformFunctionAsync<T>(
            this IExtensionManager extensionManager,
            object extension,
            Func<Task<T>?> function,
            T defaultValue)
        {
            if (extensionManager.IsDisabled(extension))
            {
                return defaultValue;
            }

            try
            {
                var task = function();
                if (task != null)
                {
                    return await task.ConfigureAwait(false);
                }
            }
            catch (Exception e) when (!(e is OperationCanceledException) && extensionManager.CanHandleException(extension, e))
            {
                extensionManager.HandleException(extension, e);
            }

            return defaultValue;
        }

        public static Func<SyntaxNode, ImmutableArray<TExtension>> CreateNodeExtensionGetter<TExtension>(
            this IExtensionManager extensionManager, ImmutableArray<TExtension> extensions, Func<TExtension, ImmutableArray<Type>> nodeTypeGetter)
            where TExtension : notnull
        {
            var map = new ConcurrentDictionary<Type, ImmutableArray<TExtension>>();

            return n =>
            {
                var key = n.GetType();
                if (map.TryGetValue(key, out var cachedExtensions))
                    return cachedExtensions;

                return GetExtensionsSlow(extensionManager, extensions, nodeTypeGetter, map, key);
            };

            // Helper method to avoid capturing allocations on fast paths
            static ImmutableArray<TExtension> GetExtensionsSlow(IExtensionManager extensionManager, ImmutableArray<TExtension> extensions, Func<TExtension, ImmutableArray<Type>> nodeTypeGetter, ConcurrentDictionary<Type, ImmutableArray<TExtension>> map, Type key)
            {
                return map.GetOrAdd(key, GetExtensions);

                ImmutableArray<TExtension> GetExtensions(Type t1)
                {
                    return extensions.WhereAsArray(
                        static (e, arg) =>
                        {
                            var types = arg.extensionManager.PerformFunction(
                                e,
                                static arg => arg.nodeTypeGetter(arg.e),
                                arg: (arg.nodeTypeGetter, e),
                                defaultValue: ImmutableArray<Type>.Empty);
                            return types.IsEmpty || types.Any(static (t2, t1) => t1 == t2 || t1.GetTypeInfo().IsSubclassOf(t2), arg: arg.t1);
                        },
                        (extensionManager, nodeTypeGetter, t1));
                }
            }
        }

        public static Func<SyntaxToken, ImmutableArray<TExtension>> CreateTokenExtensionGetter<TExtension>(
            this IExtensionManager extensionManager, ImmutableArray<TExtension> extensions, Func<TExtension, ImmutableArray<int>> tokenKindGetter)
            where TExtension : notnull
        {
            var map = new ConcurrentDictionary<int, ImmutableArray<TExtension>>();

            return t =>
            {
                var key = t.RawKind;
                if (map.TryGetValue(key, out var cachedExtensions))
                    return cachedExtensions;

                return GetExtensionsSlow(extensionManager, extensions, tokenKindGetter, map, key);
            };

            // Helper method to avoid capturing allocations on fast paths
            static ImmutableArray<TExtension> GetExtensionsSlow(IExtensionManager extensionManager, ImmutableArray<TExtension> extensions, Func<TExtension, ImmutableArray<int>> tokenKindGetter, ConcurrentDictionary<int, ImmutableArray<TExtension>> map, int key)
            {
                return map.GetOrAdd(key, GetExtensions);

                ImmutableArray<TExtension> GetExtensions(int k)
                {
                    return extensions.WhereAsArray(
                        static (e, arg) =>
                        {
                            var kinds = arg.extensionManager.PerformFunction(
                                e,
                                static arg => arg.tokenKindGetter(arg.e),
                                arg: (arg.tokenKindGetter, e),
                                defaultValue: ImmutableArray<int>.Empty);
                            return kinds.IsEmpty || kinds.Contains(arg.k);
                        },
                        (extensionManager, tokenKindGetter, k));
                }
            }
        }
    }
}
