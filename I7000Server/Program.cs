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
            ThreadPool.SetMaxThreads(5, 5);
            ThreadPool.SetMinThreads(3, 3);

            Module.CreateModul();

            ///
            /// 
           // Modul.GetModul.BuildMeandre("5", "100", "1000");
            ///

            Server srv = new Server(8080);
            srv.Start();
        }

    }
}
