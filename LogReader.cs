using System.Collections.Generic;
using System.IO;

namespace LogEntryClustering
{
    internal interface ILogReader
    {
        IAsyncEnumerable<string> ReadNextLine();
    }

    class LogReader : ILogReader
    {
        // todo download and cache
        const string FileName = "C:\\Users\\Feudo\\Downloads\\logs\\Windows_2k.log.txt";

        async IAsyncEnumerable<string> ILogReader.ReadNextLine()
        {
            using var reader = new StreamReader(FileName);
            string line;
            while ((line = await reader.ReadLineAsync()) != null)
                yield return line;
        }
    }
}
