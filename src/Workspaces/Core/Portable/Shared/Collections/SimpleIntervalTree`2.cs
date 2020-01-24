// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Shared.Collections
{
    internal class SimpleIntervalTree<T, TIntrospector> : IntervalTree<T, TIntrospector>
        where TIntrospector : struct, IIntervalIntrospector<T>
    {
        public SimpleIntervalTree(in TIntrospector introspector, IEnumerable<T> values)
            : base(introspector, values)
        {
        }

        /// <summary>
        /// Warning.  Mutates the tree in place.
        /// </summary>
        /// <param name="value"></param>
        public void AddIntervalInPlace(T value)
        {
            var newNode = new Node(value);
            this.root = Insert(root, newNode, in Introspector);
        }

        //protected int MaxEndValue(Node node)
        //    => GetEnd(node.MaxEndNode.Value, in _introspector);
    }
}
