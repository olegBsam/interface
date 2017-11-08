using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace I7000Server
{
    static class History
    {
        public static void WriteHistory(string path, string message)
        {
            lock (Client.fileHistryRead)
            {
                File.AppendAllText(path, message + Environment.NewLine, Encoding.UTF8);
            }
        }
    }
}
