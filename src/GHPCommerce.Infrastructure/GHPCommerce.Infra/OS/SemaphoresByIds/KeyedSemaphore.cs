﻿using System;
using System.Threading;
using GHPCommerce.Infra.OS.KeyedSemaphores;

namespace GHPCommerce.Infra.OS.SemaphoresByIds
{
    /// <summary>
    ///     A wrapper around <see cref="System.Threading.SemaphoreSlim" /> that has a unique key.
    /// </summary>
    internal sealed class KeyedSemaphore<TKey>: IDisposable
    {
        private readonly KeyedSemaphoresCollection<TKey> _collection;
        
        /// <summary>
        ///     The key
        /// </summary>
        public readonly TKey Key;

        /// <summary>
        ///     The semaphore slim that will be used for locking
        /// </summary>
        public readonly SemaphoreSlim SemaphoreSlim;

        /// <summary>
        ///     The consumer counter
        /// </summary>
        public int Consumers;

        /// <summary>
        /// Initializes a new instance of a keyed semaphore
        /// </summary>
        /// <param name="key">The unique key of this semaphore</param>
        /// <param name="collection">The collection to which this keyed semaphore belongs</param>
        /// <param name="semaphoreSlim">The semaphore slim that will be used internally for locking purposes</param>
        /// <exception cref="ArgumentNullException">When key is null</exception>
        public KeyedSemaphore(TKey key, KeyedSemaphoresCollection<TKey> collection, SemaphoreSlim semaphoreSlim)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            _collection = collection ?? throw new ArgumentNullException(nameof(collection));
            SemaphoreSlim = semaphoreSlim;
            Consumers = 1;
        }

        public void Dispose()
        {
            _collection.Release(this);
        }
    }
}