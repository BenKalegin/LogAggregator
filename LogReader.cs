using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace LogEntryClustering
{
    internal interface ILogReader
    {
        IAsyncEnumerable<string> ReadNextLine(string fileName);
    }

    class LogReader : ILogReader
    {
        async IAsyncEnumerable<string> ILogReader.ReadNextLine(string fileName)
        {
            using var reader = new StreamReader(fileName);
            string line;
            while ((line = await reader.ReadLineAsync()) != null)
                yield return line;
        }
    }
}
