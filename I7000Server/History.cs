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
        private static string path;
        static History()
        {
            string str = Directory.GetCurrentDirectory();
            path = str.Remove(str.IndexOf("\\bin")) + "\\client\\history.html";
        }
        
        public static void WriteHistory(string message)
        {
            lock (Client.fileHistryRead)
            {
                File.AppendAllText(path, message + Environment.NewLine, Encoding.UTF8);
            }
        }
    }
}
