using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogEntryClustering
{
	public class MinHashSimilarity
	{
		/// <summary>
		/// The internal min hash instance
		/// </summary>
		private readonly MinHash minHash;

		/// <summary>
		/// The threshold in which documents are considered similar
		/// </summary>
		private readonly double threshold;

		/// <summary>
		/// Number of bands for LSH comparison
		/// </summary>
		private readonly int bands;

		/// <summary>
		/// number of rows for LSH comparison
		/// </summary>
		private readonly int rows;

		/// <summary>
		/// Buckets for LSH comparison
		/// </summary>
		private readonly Dictionary<string, List<int[]>> buckets;

		/// <summary>
		/// Default Constructor
		/// Works best if threshold is ~90%
		/// </summary>
		/// <param name="threshold">The threshold in which documents are considered similar</param>
		public MinHashSimilarity(double threshold) : this(threshold, new TokenShinglesNgramGeneratorAndHasher(5), 400, 20, 20)
		{
			// Likelihood of an LSH match between two documents (1-(1-J(A,B)^rows)^bands) | J(A,B) = Jaccard index, rows = 20, bands = 20
			// J(A,B)   Probability of getting compared
			// .7       .016
			// .8       .206
			// .85      .546
			// .861     .642 // sCurve ((1/b)^(1/r))
			// .87      .720
			// .9       .925
			// .95      .999
		}

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="threshold">The threshold in which documents are considered similar</param>
        /// <param name="ngramGenAndHash"></param>
        /// <param name="numHashFunctions">number of min hash functions to compute</param>
        /// <param name="bands">number of bands for LSH comparison</param>
        /// <param name="rows">number of rows for LSH comparison</param>
        public MinHashSimilarity(double threshold, INgramGeneratorAndHasher ngramGenAndHash, int numHashFunctions, int bands, int rows)
		{
			if (threshold < 0 || threshold > 1)
			{
				throw new Exception($"MinHashSimilarity - Illegal threshold: {threshold}");
			}
			if (bands * rows != numHashFunctions)
			{
				throw new Exception("MinHashSimilarity - bands * rows != numHashFunctions");
			}
            minHash = new MinHash(ngramGenAndHash, numHashFunctions);

			this.threshold = threshold;
			this.bands = bands;
			this.rows = rows;
			buckets = new Dictionary<string, List<int[]>>();
		}

		/// <summary>
		/// Clears all history of documents
		/// </summary>
		public void ClearDocuments()
		{
			buckets.Clear();
		}

		/// <summary>
		/// Given a string document, looks whether a similar document was already seen
		/// </summary>
		/// <param name="doc">The new document to compare to</param>
		/// <returns>true if a similar document was already seen</returns>
		public bool LookForSimilarDocument(string doc)
		{
            var tokens = doc.Split(' ', '\t', '\r', '\n').Where(t => !string.IsNullOrWhiteSpace(t)).ToArray();
            int[] minHashes = minHash.ComputeSketch(tokens);
			var bandHashes = new string[bands];
			HashSet<int[]> comparedSketches = new HashSet<int[]>();

			for (int i = 0; i < bands; i++)
			{
				bandHashes[i] = ComputeBandHash(minHashes, i);

				if (buckets.ContainsKey(bandHashes[i]))
				{
					foreach (int[] sketchToCompare in buckets[bandHashes[i]])
					{
						if (!comparedSketches.Contains(sketchToCompare))
						{
                            var similarity = minHash.CompareSketches(minHashes, sketchToCompare);
                            if (similarity >= threshold)
							{
								// Found a similar document
								return true;
							}

							// Avoid comparing two documents twice
							comparedSketches.Add(sketchToCompare);
						}
					}
				}
			}

			// No match found, add document to buckets
			for (int i = 0; i < bands; i++)
			{
				if (!buckets.ContainsKey(bandHashes[i]))
				{
					buckets.Add(bandHashes[i], new List<int[]>());
				}
				buckets[bandHashes[i]].Add(minHashes);
			}

			return false;
		}

		/// <summary>
		/// Computes a hash for quick bucket match search
		/// </summary>
		/// <param name="minHashes">The MinHashes for row values</param>
		/// <param name="i">The ith band</param>
		/// <returns>The computed hash for the ith band</returns>
		private string ComputeBandHash(int[] minHashes, int i)
		{
			StringBuilder bandHashSb = new StringBuilder((rows + 1) * 10);
			for (int j = 0; j < rows; j++)
			{
				// adding the rows corresponding to ith band
				bandHashSb.Append(minHashes[i * rows + j].ToString().PadLeft(10, '0'));
			}
			// adding the number i to distinguish between bands
			bandHashSb.Append(i.ToString().PadLeft(10, '0'));
			return bandHashSb.ToString();
		}
	}
}
