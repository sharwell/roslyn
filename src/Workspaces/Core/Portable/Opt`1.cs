// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis
{
    /// <summary>
    /// Represents a reference which is allowed to have the value <see langword="null"/>. It is the equivalent of
    /// <see cref="Nullable{T}"/> for reference types: an instance either "does not have a value", or "has a non-null
    /// value".
    /// </summary>
    /// <remarks>
    /// <para>This structure may be used for passing optional arguments.</para>
    /// </remarks>
    /// <typeparam name="T">The type.</typeparam>
    internal readonly struct Opt<T>
        where T : class
    {
        public static readonly Opt<T> Missing;

        private readonly T _value;

        private Opt(T value)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public bool HasValue => _value != null;
        public T Value => _value ?? throw new InvalidOperationException();
        public T ValueOrDefault => _value;

        public static implicit operator Opt<T>(Opt.MissingValue missing) => Missing;

        public static implicit operator Opt<T>(T value) => FromNullable(value);
        public static explicit operator T(Opt<T> value) => value.Value;

        public static bool operator ==(Opt<T> x, Opt<T> y)
            => x._value == y._value;

        public static bool operator !=(Opt<T> x, Opt<T> y)
            => x._value != y._value;

        public static Opt<T> From(T value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return new Opt<T>(value);
        }

        public static Opt<T> FromNullable(T value)
            => new Opt<T>(value);

        public Opt<TOther> As<TOther>()
            where TOther : class
        {
            return new Opt<TOther>(_value as TOther);
        }

        public override int GetHashCode()
            => EqualityComparer<T>.Default.GetHashCode(_value);

        public override bool Equals(object obj)
        {
            if (obj is Opt<T> other)
            {
                return Equals(other);
            }

            return false;
        }

        public bool Equals(Opt<T> obj)
            => EqualityComparer<T>.Default.Equals(_value, obj._value);
    }
}
