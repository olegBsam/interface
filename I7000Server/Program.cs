using System;
using System.Threading;

namespace I7000Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Module.CreateModul();


            //  Module.GetModul.BuildMeandre("5", "2", "10", "4", "3");
            int thCount = Environment.ProcessorCount;
            if (thCount < 2)
            {
                thCount = 2;
            }

            ThreadPool.SetMaxThreads(10, 10);
            ThreadPool.SetMinThreads(3, 3);


            ///
            /// 
           // Modul.GetModul.BuildMeandre("5", "100", "1000");
            ///

            Server srv = new Server(8080);
            srv.Start();
        }

    }
}
