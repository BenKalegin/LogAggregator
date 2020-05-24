using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Text;

namespace LogEntryClustering
{
    /// <summary>
    /// Given list of token generates string compositions (ngrams) then hashes and return ngrams.
    /// Different implementation can return different number of ngram hashes
    /// </summary>
    public interface INgramGeneratorAndHasher
    {
        ImmutableArray<int> Generate(in ReadOnlySpan<string> tokens);
    }

    class TokenShinglesNgramGeneratorAndHasher : INgramGeneratorAndHasher
    {
        private readonly int tokensInShingle;

        public TokenShinglesNgramGeneratorAndHasher(int tokensInShingle)
        {
            this.tokensInShingle = tokensInShingle;
        }

        ImmutableArray<int> INgramGeneratorAndHasher.Generate(in ReadOnlySpan<string> tokens)
        {
            if (tokens.Length < tokensInShingle)
                // don't take into account incomplete tails 
                return ImmutableArray<int>.Empty;

            var s = new StringBuilder();

            for (var i = 0; i < tokensInShingle; i++)
            {
                var token = tokens[i];
                s.Append(token);
            }

            return new ImmutableArray<int> {s.ToString().GetHashCode()};
        }
    }
    class TokenMaskingNgramGeneratorAndHasher : INgramGeneratorAndHasher
    {
        private readonly int ngramLength;
        private readonly int numberOfMaskedOut;

        public TokenMaskingNgramGeneratorAndHasher(int ngramLength, int numberOfMaskedOut)
        {
            this.ngramLength = ngramLength;
            this.numberOfMaskedOut = numberOfMaskedOut;
        }

        ImmutableArray<int> INgramGeneratorAndHasher.Generate(in ReadOnlySpan<string> tokens)
        {
            if (tokens.Length < ngramLength)
                // don't take into account incomplete tails 
                return ImmutableArray<int>.Empty;

            var result = new List<int>();
            for (int maskPos = 0; maskPos < ngramLength; maskPos++)
            {
                var s = new StringBuilder();
                for (var i = 0; i < ngramLength; i++)
                {
                    if (i - maskPos == 0 || i - maskPos == numberOfMaskedOut-1)
                    {
                        var token = tokens[i];
                        s.Append(token);
                    }
                }
                result.Add(s.ToString().GetHashCode());
            }

            return result.ToImmutableArray();
        }
    }
}