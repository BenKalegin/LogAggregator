﻿using System.Collections.Generic;
using System.Linq;

namespace LogEntryClustering
{
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
