using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LogEntryClustering
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("LSH Test.");

            await new LshTest().Run();
        }
    }

    internal class LshTest
    {
        public async Task Run()
        {
            var lsh = new MinHashSimilarity(0.7, new TokenMaskingNgramGeneratorAndHasher(3, 1), 400, 20, 20);
            
            ILogReader logReader = new LogReader();
            var hit = 0;
            var miss = 0;

            await foreach (string line in logReader.ReadNextLine())
            {
                var similarExists = lsh.LookForSimilarDocument(line);
                if (similarExists)
                    hit++; 
                else 
                    miss++;
                Console.ForegroundColor = similarExists ? ConsoleColor.DarkGray : ConsoleColor.White;
                Console.WriteLine(line);
            }
            Console.ResetColor();
            Console.WriteLine($"hit/miss:{hit}/{miss}");
        }
    }
}
