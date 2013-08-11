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
            private HashEntry[] entries;

            /// <summary>
            /// The value used to index into the array storing the HashEntrys.
            /// </summary>
            private UInt64 indexer;

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
                indexer = (UInt64)Capacity;
                entries = new HashEntry[Capacity];
            }

            public HashEntry Probe(UInt64 key) {
                return entries[key % indexer];
            }

            public void Store(HashEntry entry) {
                UInt64 index = entry.Key % indexer;
                if (entries[index].Misc == 0)
                    Count++;
                entries[index] = entry;
            }

            public void Clear() {
                entries = new HashEntry[Capacity];
                Count = 0;
            }

            public Int32 Size {
                get {
                    return HashEntry.Size * Capacity;
                }
            }
        }
    }
}
