// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis
{
    internal static class Opt
    {
        /// <summary>
        /// An optional reference which does not have a value. This is equivalent to the <see langword="null"/> literal
        /// when working with <see cref="Opt{T}"/>.
        /// </summary>
        public static readonly MissingValue Missing;

        public static Opt<T> From<T>(T value)
            where T : class
        {
            return Opt<T>.From(value);
        }

        public static Opt<T> FromNullable<T>(T value)
            where T : class
        {
            return Opt<T>.FromNullable(value);
        }

        public readonly struct MissingValue
        {
        }
    }
}
