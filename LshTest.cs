using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace LogEntryClustering
{
    internal class LshTest
    {
        // Courtesy of https://github.com/logpai/loghub
        string CurrentDirectory => Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
        string DataFolder => Path.GetFullPath(Path.Combine(CurrentDirectory, "..\\..\\..\\data"));

        const string WindowsFileName = "Windows_2k.log.txt";
        const string ZookeeperFileName = "Zookeeper_2k.log.txt";
        const string HadoopFileName = "Hadoop_2k.log.txt";
        const string SparkFileName = "Spark_2k.log.txt";
        const string SshFileName = "SSH_2k.log.txt";

        public async Task Run()
        {
            Console.WriteLine("LSH Test. Program reads the log file and aggregates log entries by similarity.");

            while (true)
            {
                Console.WriteLine(
                    "Please select log file to aggregate: (w)indows (z)ookeeper (h)adoop (s)sh s(p)ark or (q)uit: ");

                var choice = char.ToLower(Console.ReadKey().KeyChar);

                string fileName = choice switch
                {
                    'w' => WindowsFileName,
                    'z' => ZookeeperFileName,
                    'h' => HadoopFileName,
                    's' => SparkFileName,
                    _ => null
                };

                if (fileName == null)
                {
                    if (choice == 'q')
                        break;
                    Console.WriteLine("Invalid choice, please retry.");
                    continue;
                }

                await RunLshForFile(fileName);
            }
        }

        private async Task RunLshForFile(string fileName)
        {
            var lsh = new MinHashSimilarity(0.86, new TokenMaskingNgramGeneratorAndHasher(3, 1), 400, 20, 20);

            ILogReader logReader = new LogReader();
            int lineNum = -1;
            var histogramByOriginalId = new ConcurrentDictionary<int, int>();
            var lineById = new ConcurrentDictionary<int, string>();

            Console.WriteLine($"---- reading file {fileName} ----------");
            await foreach (string line in logReader.ReadNextLine(Path.Combine(DataFolder, fileName)))
            {
                var currentDocId = Interlocked.Increment(ref lineNum);
                {
                    var similarId = lsh.LookForSimilarDocument(line, currentDocId);
                    if (similarId >= 0)
                    {
                        histogramByOriginalId.AddOrUpdate(similarId, 1, (key, value) => value + 1);
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

            Console.WriteLine("---------------");
            Console.WriteLine("");
            Console.WriteLine(
                $"Duplicates found (hit count {hits}, total lines {lineNum}, hit ratio: {1.0 * hits / (hits + miss):F}):");
            Console.WriteLine("");

            foreach (var count in histogramByOriginalId.OrderByDescending(pair => pair.Value).Where(pair => pair.Value > 1))
                Console.WriteLine($"{count.Value:D3} similar to: {lineById[count.Key]}");


            Console.WriteLine("");
            Console.WriteLine($"Unique records ({miss}):");
            Console.WriteLine("");
            foreach (var count in histogramByOriginalId.Where(pair => pair.Value == 1))
                Console.WriteLine($"no similarities to: {lineById[count.Key]}");
        }
    }
}