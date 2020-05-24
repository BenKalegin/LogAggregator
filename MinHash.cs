using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace LogEntryClustering
{
    public class MinHash
	{
        private readonly INgramGeneratorAndHasher ngramGeneratorAndHasher;
		private readonly ImmutableArray<HashFunction> hashFunctions;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ngramGeneratorAndHasher"></param>
        /// <param name="numHashFunctions">number of hash functions to compute min hash. expected error is O(1/√k), so for 400 hashes estimation of J(A,B) will have error not more than 0.05 </param>
        public MinHash(INgramGeneratorAndHasher ngramGeneratorAndHasher, int numHashFunctions)
        {
            this.ngramGeneratorAndHasher = ngramGeneratorAndHasher;
            hashFunctions = GenerateHashFunctions(numHashFunctions);
        }

        private ImmutableArray<HashFunction> GenerateHashFunctions(int numHashFunctions)
        {
			// use universal hashing https://en.wikipedia.org/wiki/Universal_hashing

            const int universeSize = int.MaxValue; // Max Integer (2^31-1) is a Mersenne prime
            var r = new Random();

			return Enumerable.Range(0, numHashFunctions).Select(_ =>
                new HashFunction(r.Next(universeSize), r.Next(universeSize), r.Next(universeSize), universeSize)).ToImmutableArray();
        }

        /// <summary>
		/// Compute the MinHash Sketch from an array of tokens.
		/// Update the hash tables according to the min values of the sketch.
		/// </summary>
		/// <param name="tokens">A list of tokens</param>
		/// <returns>An array of minimum hash values</returns>
		public int[] ComputeSketch(string[] tokens)
		{
			// Maintain an array of minimum hash values
			var hashMinimumValues = new int[hashFunctions.Length];

            // Since we're looking for minimum values, it's important to initialize the array to max int
			Array.Fill(hashMinimumValues, int.MaxValue);

			if (tokens == null || tokens.Length == 0)
			{
				return hashMinimumValues;
			}

            var tokensSpan = tokens.AsSpan();

			for (int tokenIndex = 0; tokenIndex < tokens.Length; tokenIndex++)
            {
                var slice = tokensSpan.Slice(tokenIndex);
                var hashCodes = ngramGeneratorAndHasher.Generate(slice);

                foreach (var hashCode in hashCodes)
                {
                    // Go over all hash functions			
                    for (int hashIndex = 0; hashIndex < hashFunctions.Length; hashIndex++)
                    {
                        // compute hash value of token with current hash function
                        int hashValue = hashFunctions[hashIndex].CalculateHash(hashCode);

                        // Update minimum value at index hashIndex
                        hashMinimumValues[hashIndex] = Math.Min(hashMinimumValues[hashIndex], hashValue);
                    }
                }
            }

			// Return the MinHash Sketch
			return hashMinimumValues;
		}

		/// <summary>
		/// Compares two MinHash sketches
		/// </summary>
		/// <param name="firstMinHashSketch">The first MinHash sketch to compare</param>
		/// <param name="secondMinHashSketch">The second MinHash sketch to compare</param>
		/// <returns>Similarity result (between 0 and 1)</returns>
		public double CompareSketches(int[] firstMinHashSketch, int[] secondMinHashSketch)
		{
			// count equal hashes
			int equalHashes = 0;
			for (int i = 0; i < hashFunctions.Length; i++)
			{
				if (firstMinHashSketch[i] == secondMinHashSketch[i])
				{
					equalHashes++;
				}
			}

			return (1.0 * equalHashes) / hashFunctions.Length; // similarity index
		}
	}
}
