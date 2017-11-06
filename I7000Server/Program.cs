using System;
using System.Threading;

namespace I7000Server
{
    class Program
    {
        static void Main(string[] args)
        {
            int thCount = Environment.ProcessorCount;
            if (thCount < 2)
                thCount = 2;
            ThreadPool.SetMaxThreads(thCount, thCount);
            ThreadPool.SetMinThreads(2, 2);

            Server srv = new Server(8080);
            srv.Start();
        }

    }
}
