using System.Collections.Generic;
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
}
