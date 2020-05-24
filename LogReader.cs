using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace LogEntryClustering
{
    internal interface ILogReader
    {
        IAsyncEnumerable<string> ReadNextLine();
    }

    class LogReader : ILogReader
    {
        // Courtesy of https://github.com/logpai/loghub
        string CurrentDirectory => Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
        string DataFolder => Path.GetFullPath(Path.Combine(CurrentDirectory, "..\\..\\..\\data"));

        string WindowsFileName => Path.Combine(DataFolder,  "Windows_2k.log.txt");
        string ZookeeperFileName => Path.Combine(DataFolder, "Zookeeper_2k.log.txt");

        async IAsyncEnumerable<string> ILogReader.ReadNextLine()
        {
            using var reader = new StreamReader(WindowsFileName);
            string line;
            while ((line = await reader.ReadLineAsync()) != null)
                yield return line;
        }
    }
}
