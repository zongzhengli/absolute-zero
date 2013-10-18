using System;
using System.Collections.Generic;

namespace AbsoluteZero {

    /// <summary>
    /// Encapsulates the transposition table component of the Absolute Zero chess engine. 
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
            /// The value used to index into the array storing the hash entries.
            /// </summary>
            private UInt64 _indexer;

            /// <summary>
            /// The number of entries stored in the hash table. 
            /// </summary>
            public Int32 Count = 0;

            /// <summary>
            /// The number of entries that can be stored in the hash table. 
            /// </summary>
            public Int32 Capacity;

            /// <summary>
            /// Constructs a hash table of the given size in bytes. 
            /// </summary>
            /// <param name="megabytes">The size of the new hash table in bytes.</param>
            public HashTable(Int32 bytes) {
                Capacity = bytes / HashEntry.Size;
                _indexer = (UInt64)Capacity;
                _entries = new HashEntry[Capacity];
            }

            /// <summary>
            /// Finds the entry in the hash table for the given key. The return value 
            /// indicates whether the entry was found. 
            /// </summary>
            /// <param name="key">The key to find the entry for.</param>
            /// <param name="entry">Contains the entry found when the method returns.</param>
            /// <returns>Whether the entry was found in the hash table.</returns>
            public bool TryProbe(UInt64 key, out HashEntry entry) {
                return (entry = _entries[key % _indexer]).Key == key;
            }
            
            /// <summary>
            /// Stores the given entry in the hash table. 
            /// </summary>
            /// <param name="entry">The entry to store.</param>
            public void Store(HashEntry entry) {
                UInt64 index = entry.Key % _indexer;
                if (_entries[index].Misc == 0)
                    Count++;
                _entries[index] = entry;
            }

            /// <summary>
            /// Clears the hash table of all entries. 
            /// </summary>
            public void Clear() {
                _entries = new HashEntry[Capacity];
                Count = 0;
            }

            /// <summary>
            /// The size of the hash table in bytes. 
            /// </summary>
            public Int32 Size {
                get {
                    return HashEntry.Size * Capacity;
                }
            }
        }
    }
}
