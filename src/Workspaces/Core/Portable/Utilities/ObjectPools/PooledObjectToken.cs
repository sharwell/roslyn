// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Roslyn.Utilities
{
    internal struct PooledObjectToken
    {
        public static readonly PooledObjectToken None = new PooledObjectToken(0);

        public readonly int Value;

        public PooledObjectToken(int value)
        {
            Value = value;
        }

        public static bool operator ==(PooledObjectToken a, PooledObjectToken b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(PooledObjectToken a, PooledObjectToken b)
        {
            return !a.Equals(b);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PooledObjectToken other))
                return false;

            return Equals(other);
        }

        public bool Equals(PooledObjectToken other)
        {
            return Value == other.Value;
        }

        public override int GetHashCode()
        {
            return Value;
        }
    }
}
