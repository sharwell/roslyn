﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.Utilities
{
    /// <summary>
    /// Provides helpers for asynchronous lazy initialization similar to <see cref="LazyInitializer"/>.
    /// </summary>
    internal static class AsyncLazyInitializer
    {
        public delegate ref T Accessor<T>();

        public delegate ref T Accessor<T, TState>(TState state);

        /// <summary>
        /// Initializes a target reference type by using a specified asynchronous function if it hasn't already been
        /// initialized.
        /// </summary>
        /// <typeparam name="T">The reference type of the reference to be initialized.</typeparam>
        /// <param name="targetAccessor">An accessor that provides a reference of type <typeparamref name="T"/> to initialize if it hasn't already been initialized.</param>
        /// <param name="valueFactory">The function that is called to initialize the reference.</param>
        /// <returns>The initialized value of type <typeparamref name="T"/>.</returns>
        /// <exception cref="NullReferenceException">
        /// <para>If <paramref name="targetAccessor"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If the target value is not already initialized and <paramref name="valueFactory"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException"><paramref name="valueFactory"/> returned <see langword="null"/>.</exception>
        public static ValueTask<T> EnsureInitializedAsync<T>(Accessor<T> targetAccessor, Func<ValueTask<T>> valueFactory)
            where T : class
        {
            // Fast path
            var value = Volatile.Read(ref targetAccessor());
            if (!(value is null))
            {
                return new ValueTask<T>(value);
            }

            return EnsureInitializedCoreAsync(targetAccessor, valueFactory);
        }

        /// <summary>
        /// Initializes a target reference type by using a specified asynchronous function if it hasn't already been
        /// initialized.
        /// </summary>
        /// <typeparam name="T">The reference type of the reference to be initialized.</typeparam>
        /// <typeparam name="TState">The type of the state object to pass to the accessor and value factory.</typeparam>
        /// <param name="targetAccessor">An accessor that provides a reference of type <typeparamref name="T"/> to initialize if it hasn't already been initialized.</param>
        /// <param name="valueFactory">The function that is called to initialize the reference.</param>
        /// <param name="state">The state object to pass to the accessor and value factory.</param>
        /// <returns>The initialized value of type <typeparamref name="T"/>.</returns>
        /// <exception cref="NullReferenceException">
        /// <para>If <paramref name="targetAccessor"/> is <see langword="null"/>.</para>
        /// <para>-or-</para>
        /// <para>If the target value is not already initialized and <paramref name="valueFactory"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="InvalidOperationException"><paramref name="valueFactory"/> returned <see langword="null"/>.</exception>
        public static ValueTask<T> EnsureInitializedAsync<T, TState>(Accessor<T, TState> targetAccessor, Func<TState, ValueTask<T>> valueFactory, TState state)
            where T : class
        {
            // Fast path
            var value = Volatile.Read(ref targetAccessor(state));
            if (!(value is null))
            {
                return new ValueTask<T>(value);
            }

            return EnsureInitializedCoreAsync(targetAccessor, valueFactory, state);
        }

        /// <summary>
        /// Initializes a target reference type by using a specified asynchronous function (slow path).
        /// </summary>
        /// <typeparam name="T">The reference type of the reference to be initialized.</typeparam>
        /// <param name="targetAccessor">An accessor that provides a reference of type <typeparamref name="T"/> to initialize.</param>
        /// <param name="valueFactory">The function that is called to initialize the reference.</param>
        /// <returns>The initialized value of type <typeparamref name="T"/>.</returns>
        private static async ValueTask<T> EnsureInitializedCoreAsync<T>(Accessor<T> targetAccessor, Func<ValueTask<T>> valueFactory)
            where T : class
        {
            var value = await valueFactory().ConfigureAwait(false);
            if (value is null)
            {
                throw new InvalidOperationException(WorkspacesResources.AsyncLazy_StaticInit_InvalidOperation);
            }

            return Interlocked.CompareExchange(ref targetAccessor(), value, null) ?? value;
        }

        /// <summary>
        /// Initializes a target reference type by using a specified asynchronous function (slow path).
        /// </summary>
        /// <typeparam name="T">The reference type of the reference to be initialized.</typeparam>
        /// <typeparam name="TState">The type of the state object to pass to the accessor and value factory.</typeparam>
        /// <param name="targetAccessor">An accessor that provides a reference of type <typeparamref name="T"/> to initialize.</param>
        /// <param name="valueFactory">The function that is called to initialize the reference.</param>
        /// <param name="state">The state object to pass to the accessor and value factory.</param>
        /// <returns>The initialized value of type <typeparamref name="T"/>.</returns>
        private static async ValueTask<T> EnsureInitializedCoreAsync<T, TState>(Accessor<T, TState> targetAccessor, Func<TState, ValueTask<T>> valueFactory, TState state)
            where T : class
        {
            var value = await valueFactory(state).ConfigureAwait(false);
            if (value is null)
            {
                throw new InvalidOperationException(WorkspacesResources.AsyncLazy_StaticInit_InvalidOperation);
            }

            return Interlocked.CompareExchange(ref targetAccessor(state), value, null) ?? value;
        }
    }
}
