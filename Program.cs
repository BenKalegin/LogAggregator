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
            int lineNum = 0;
            var docsForDemo = new List<string>();


            await foreach (string line in logReader.ReadNextLine())
            {
                // lock to prevent console color race condition
                lock (this)
                {
                    var similarId = lsh.LookForSimilarDocument(line, lineNum);
                    docsForDemo.Add(line);
                    if (similarId >= 0)
                        hit++;
                    else
                        miss++;
                    Console.ForegroundColor = similarId >= 0 ? ConsoleColor.DarkGray : ConsoleColor.White;
                    Console.WriteLine(line);
                    if (similarId >= 0)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine(docsForDemo[similarId]);
                    }

                    lineNum++;
                }
            }
            Console.ResetColor();
            Console.WriteLine($"hit/miss:{hit}/{miss}");
        }
    }
}
