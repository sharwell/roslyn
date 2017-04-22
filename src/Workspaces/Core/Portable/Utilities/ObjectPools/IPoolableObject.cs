// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Roslyn.Utilities
{
    internal interface IPoolableObject
    {
        /// <summary>
        /// Gets the current token assigned to the poolable object. Tokens are used to detect certain misuse patterns
        /// prior to corrupting program state.
        /// </summary>
        PooledObjectToken Token
        {
            get;
        }

        /// <summary>
        /// Initializes the poolable object for use.
        /// </summary>
        /// <remarks>
        /// <para>If this object supports tokens, this method changes the object token assigned to the instance.</para>
        /// </remarks>
        void Initialize();

        /// <summary>
        /// Prepares the poolable object for return to a shared pool.
        /// </summary>
        /// <remarks>
        /// <para>If this object supports tokens, this method changes the object token assigned to the instance.</para>
        /// </remarks>
        /// <returns>
        /// <see langword="true"/> if the object can be returned to an object pool; otherwise, <see langword="false"/>
        /// if the instance is not suitable for reuse and should be discarded.
        /// </returns>
        bool Release();
    }
}
