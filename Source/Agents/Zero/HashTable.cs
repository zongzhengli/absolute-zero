using System;
using System.Collections.Generic;

namespace AbsoluteZero {
    partial class Zero {
        private class HashTable {
            private HashEntry[] entries;
            private UInt64 indexer;

            public Int32 Count = 0;
            public Int32 Capacity;

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
