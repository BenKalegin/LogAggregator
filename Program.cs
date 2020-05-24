using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LogEntryClustering
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new LshTest().Run();
        }
    }

    internal class LshTest
    {
        public async Task Run()
        {
            Console.WriteLine("LSH Test. Program reads the log file and aggregate log by similarity.");

            var lsh = new MinHashSimilarity(0.7, new TokenMaskingNgramGeneratorAndHasher(3, 1), 400, 20, 20);
            
            ILogReader logReader = new LogReader();
            int lineNum = -1;
            var histogramByOriginalId = new ConcurrentDictionary<int, int>();
            var lineById = new ConcurrentDictionary<int, string>();

            await foreach (string line in logReader.ReadNextLine())
            {
                var currentDocId = Interlocked.Increment(ref lineNum);
                {
                    var similarId = lsh.LookForSimilarDocument(line, currentDocId);
                    if (similarId >= 0)
                    {
                        histogramByOriginalId.AddOrUpdate(similarId, 1,  (key, value) => value + 1);
                    }
                    else
                    {
                        if (!histogramByOriginalId.TryAdd(currentDocId, 1))
                            throw new Exception("Should be first histogram entry");
                        lineById[currentDocId] = line;
                    }
                    Console.Write(similarId < 0 ? "O" : "o");
                }
            }

            var hits = histogramByOriginalId.Where(pair => pair.Value > 1).Sum(pair => pair.Value);
            var miss = histogramByOriginalId.Count(pair => pair.Value == 1);

            Console.WriteLine("");
            Console.WriteLine($"Duplicates found (hit count {hits}, total lines {lineNum}, hit ratio: {1.0 * hits/(hits+miss):F}):");
            Console.WriteLine("");

            foreach (var count in histogramByOriginalId.OrderByDescending(pair => pair.Value).Where(pair => pair.Value > 1)) 
                Console.WriteLine($"{count.Value:D3} of | {lineById[count.Key]}");


            Console.WriteLine("");
            Console.WriteLine($"Unique records ({miss}):");
            Console.WriteLine("");
            foreach (var count in histogramByOriginalId.Where(pair => pair.Value == 1))
                Console.WriteLine($"1 of | {lineById[count.Key]}");
        }
    }
}
