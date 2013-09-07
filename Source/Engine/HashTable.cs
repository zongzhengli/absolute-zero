using System;
using System.Collections.Generic;

namespace AbsoluteZero {

    /// <summary>
    /// The transposition table component of the Absolute Zero chess engine. 
    /// </summary>
    partial class Zero {

        /// <summary>
        /// Defines a transposition hash table. 
        /// </summary>
        private class HashTable {

            /// <summary>
            /// The collection of hash entries. 
            /// </summary>
            private HashEntry[] _entries;

            /// <summary>
            /// The value used to index into the array storing the HashEntrys.
            /// </summary>
            private UInt64 _indexer;

            /// <summary>
            /// The number of entries stored in the HashTable. 
            /// </summary>
            public Int32 Count = 0;

            /// <summary>
            /// The number of entries that can be stored in the HashTable. 
            /// </summary>
            public Int32 Capacity;

            /// <summary>
            /// Constructs a HashTable of the given size in megabytes. 
            /// </summary>
            /// <param name="megabytes">The size of the new HashTable in megabytes.</param>
            public HashTable(Int32 megabytes) {
                Capacity = (megabytes << 20) / HashEntry.Size;
                _indexer = (UInt64)Capacity;
                _entries = new HashEntry[Capacity];
            }

            /// <summary>
            /// Returns the entry in the HashTable for the given key. An invalid entry 
            /// may be returned, and its associated key will differ from the given key. 
            /// </summary>
            /// <param name="key">The key to find the entry for.</param>
            /// <returns>The entry in the HashTable for the given key.</returns>
            public HashEntry Probe(UInt64 key) {
                return _entries[key % _indexer];
            }
            
            /// <summary>
            /// Stores the given entry in the HashTable. 
            /// </summary>
            /// <param name="entry">The entry to store.</param>
            public void Store(HashEntry entry) {
                UInt64 index = entry.Key % _indexer;
                if (_entries[index].Misc == 0)
                    Count++;
                _entries[index] = entry;
            }

            /// <summary>
            /// Clears the HashTable of all entries. 
            /// </summary>
            public void Clear() {
                _entries = new HashEntry[Capacity];
                Count = 0;
            }

            /// <summary>
            /// The size of the HashTable in bytes. 
            /// </summary>
            public Int32 Size {
                get {
                    return HashEntry.Size * Capacity;
                }
            }
        }
    }
}
