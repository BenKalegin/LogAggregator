using System;
using System.Collections.Generic;
using System.Linq;

namespace LogEntryClustering
{
    public class MinHash1
    {
        // Constructor passed universe size and number of hash functions
        public MinHash1(int universeSize, int numHashFunctions)
        {
            this.numHashFunctions = numHashFunctions;
            // number of bits to store the universe
            int u = BitsForUniverse(universeSize);
            GenerateHashFunctions(u);
        }

        private int numHashFunctions;

        // Returns number of hash functions defined for this instance
        public int NumHashFunctions
        {
            get { return numHashFunctions; }
        }

        public delegate uint Hash(int toHash);
        private Hash[] hashFunctions;

        // Public access to hash functions
        public Hash[] HashFunctions
        {
            get { return hashFunctions; }
        }

        // Generates the Universal Random Hash functions
        // http://en.wikipedia.org/wiki/Universal_hashing
        private void GenerateHashFunctions(int u)
        {
            hashFunctions = new Hash[numHashFunctions];

            // will get the same hash functions each time since the same random number seed is used
            Random r = new Random(10);
            for (int i = 0; i < numHashFunctions; i++)
            {
                uint a = 0;
                // parameter a is an odd positive
                while (a % 1 == 1 || a <= 0)
                    a = (uint)r.Next();
                uint b = 0;
                int maxb = 1 << u;
                // parameter b must be greater than zero and less than universe size
                while (b <= 0) b = (uint)r.Next(maxb); hashFunctions[i] = x => QHash(x, a, b, u);
            }
        }

        // Returns the number of bits needed to store the universe
        public int BitsForUniverse(int universeSize)
        {
            return (int)Math.Truncate(Math.Log((double)universeSize, 2.0)) + 1;
        }

        // Universal hash function with two parameters a and b, and universe size in bits
        private static uint QHash(int x, uint a, uint b, int u)
        {
            return (a * (uint)x + b) >> (32 - u);
        }

        // Returns the list of min hashes for the given set of word Ids
        public List<uint> GetMinHash(List<int> wordIds)
        {
            uint[] minHashes = new uint[numHashFunctions];
            for (int h = 0; h < numHashFunctions; h++)
            {
                minHashes[h] = int.MaxValue;
            }
            foreach (int id in wordIds)
            {
                for (int h = 0; h < numHashFunctions; h++)
                {
                    uint hash = hashFunctions[h](id);
                    minHashes[h] = Math.Min(minHashes[h], hash);
                }
            }
            return minHashes.ToList();
        }

        // Calculates the similarity of two lists of min hash values. Approximately Numerically equivilant to Jaccard Similarity
        public double Similarity(List<uint> l1, List<uint> l2)
        {
            // Jaccard jac = new Jaccard();
            // return Jaccard.Calc(l1, l2);
            return 0;
        }
    }
    public class LSH
    {
        private readonly int[,] minHashes;
        private readonly int numBands;
        private readonly int rowsPerBand;
        private readonly int numSets;
        private readonly List<SortedList<int, List<int>>> lshBuckets = new List<SortedList<int, List<int>>>();

        public LSH(int[,] minHashes, int rowsPerBand)
        {
            this.minHashes = minHashes;
            var numHashFunctions = minHashes.GetUpperBound(1) + 1;
            numSets = minHashes.GetUpperBound(0) + 1;
            this.rowsPerBand = rowsPerBand;
            numBands = numHashFunctions / rowsPerBand;
        }

        public void Calc()
        {
            int thisHash = 0;

            for (int band = 0; band < numBands; band++)
            {
                var thisSL = new SortedList<int, List<int>>();
                for (int s = 0; s < numSets; s++)
                {
                    int hashValue = 0;
                    for (int th = thisHash; th < thisHash + rowsPerBand; th++)
                    {
                        hashValue = unchecked(hashValue * 1174247 + minHashes[s, th]);
                    }
                    if (!thisSL.ContainsKey(hashValue))
                    {
                        thisSL.Add(hashValue, new List<int>());
                    }
                    thisSL[hashValue].Add(s);
                }
                thisHash += rowsPerBand;
                var copy = new SortedList<int, List<int>>();
                foreach (int ic in thisSL.Keys)
                {
                    if (thisSL[ic].Count() > 1)
                    {
                        copy.Add(ic, thisSL[ic]);
                    }
                }
                lshBuckets.Add(copy);
            }
        }

        public List<int> GetNearest(int n)
        {
            List<int> nearest = new List<int>();
            foreach (SortedList<int, List<int>> b in lshBuckets)
            {
                foreach (List<int> li in b.Values)
                {
                    if (li.Contains(n))
                    {
                        nearest.AddRange(li);
                        break;
                    }
                }
            }
            nearest = nearest.Distinct().ToList();
            nearest.Remove(n);  // remove the document itself
            return nearest;
        }
    }
}
