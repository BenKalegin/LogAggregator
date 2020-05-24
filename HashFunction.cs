using System;

namespace LogEntryClustering
{
    /// <summary>
    /// Hash function generator.
    /// Given a set of hash function parameters (a, b, c) and a bound on possible hash value,
    /// generates a hash function that given an element x returns its hashed value.
    /// </summary>
    internal class HashFunction
    {
        private readonly int a;
        private readonly int b;
        private readonly int c;
        private readonly int universeSize;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="a">Hash function parameter</param>
        /// <param name="b">Hash function parameter</param>
        /// <param name="c">Hash function parameter</param>
        /// <param name="universeSize">Upper bound on hash values - should be a Mersenne prime (e.g. 131071 = 2^17 - 1)</param>
        public HashFunction(int a, int b, int c, int universeSize)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.universeSize = universeSize;
        }

        /// <summary>
        /// Hash function calculator
        /// </summary>
        /// <param name="x">Hash function parameter</param>
        /// <returns>Hashed value</returns>
        public int CalculateHash(int x)
        {
            // Modify the hash family as per the size of possible elements in a Set
            x = x & universeSize;
            int hashValue = (int)((a * (x >> 4) + b * x + c) & universeSize);
            return Math.Abs(hashValue);
        }
    }
}