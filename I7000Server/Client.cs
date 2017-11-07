using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace I7000Server
{
    public class Client
    {

        string request = "";
        byte[] buffer = new byte[1024];
        int count;

        public Client(TcpClient newClient)
        {
           // SendFile(newClient, "i7000Control.html");
            //string html = readFile("i7000Control.html");

            //string str = "HTTP/1.1 200 OK\nContent-type: text/html\nContent-Length:"
            //    + html.Length.ToString() + "\n\n" + html;

            //byte[] buf = Encoding.UTF8.GetBytes(str);

            //newClient.GetStream().Write(buf, 0, buf.Length);

            GetRequest(newClient);

            newClient.Close();
        }

        public void GetRequest(TcpClient client)
        {
            while ((count = client.GetStream().Read(buffer, 0, buffer.Length)) > 0)
            {
                request += Encoding.UTF8.GetString(buffer, 0, count);
                if (request.IndexOf("\r\n\r\n") >= 0 || request.Length > 4096) //\r\n\r\n - конец запроса, иначе принимаем не более 4Кб
                    break;
            }

            string req = request.ToString();

            string[] mas = req.Split(new string[] { "GET /?", "=", "&" }, StringSplitOptions.None);

            if (req.Contains("favicon"))
                return;

            if (mas[1] != null && mas[1] == "portNumber")
            {
                mas = req.Split(new string[] { "GET /?portNumber=", "&speed=", "&" },
                    StringSplitOptions.None);
                int portNumber = 0;
                int.TryParse(mas[1], out portNumber);
                int speed = 0;
                int.TryParse(mas[2], out speed);
                OpenPort(portNumber, speed);
                return;
            }
            if (mas[1] != null && mas[1] == "command")
            {
                mas = req.Split(new string[] { "GET /?command=", "&" },
                   StringSplitOptions.None);
                string command = mas[0];
                Command(command);
                return;
            }         

            Match ReqMatch = Regex.Match(request.ToString(), @"^\w+\s+([^\s\?]+)[^\s]*\s+HTTP/.*|");

            if (ReqMatch == Match.Empty)
            {
                SendError(client, 404);
                return;
            }
           
            string reqUri = ReqMatch.Groups[1].Value;
            reqUri = Uri.UnescapeDataString(reqUri);
            if (reqUri.IndexOf("..") >= 0)
            {
                SendError(client, 400);
                return;
            }
            if (reqUri.EndsWith("/"))
            {
                SendFile(client, "i7000Control.html");
            }
        }

        public void Command(string command)
        {

        }
        public void OpenPort(int portNumber, int speed)
        {
            if (speed == 0)
                speed = 9600;

        }


        public bool SendFile(TcpClient client, string path)
        {
            if (!File.Exists(path))
            {
                SendError(client, 400);
                return false;
            }
            string contentType = "";
            GetExtension(path.Substring(path.LastIndexOf('.')), out contentType);

            ///Ответ на запрос
            //Открываем запрошенный файл
            FileStream fileStream;
            try
            {
                fileStream = new FileStream(path, FileMode.Open, FileAccess.Read,
                    FileShare.Read);
            }
            catch (Exception)
            {
                SendError(client, 500);
                return false;
            }

            HeaderSending(client, contentType, fileStream);

            fileStream.Close();
            return true;
        }

      
        private void GetExtension(string extension, out string contentType)
        {
            switch (extension)
            {
                case ".htm":
                case ".html":
                    contentType = "text/html";
                    break;
                case ".css":
                    contentType = "text/stylesheet";
                    break;
                case ".js":
                    contentType = "text/javascript";
                    break;
                case ".jpg":
                    contentType = "image/jpeg";
                    break;
                case ".jpeg":
                case ".png":
                case ".gif":
                    contentType = "image/" + extension.Substring(1);
                    break;
                default:
                    if (extension.Length > 1)
                        contentType = "application/" + extension.Substring(1);
                    else
                        contentType = "application/unknown";
                    break;
            }
        }

        //Создание потока клиента
        private static Object locker = null;
        public static void ClientThread(Object stateInfo)
        {
            while (locker != null);
            locker = new Object();
            new Client((TcpClient)stateInfo);
            locker = null;
        }

        private void HeaderSending(TcpClient client, string contentType, FileStream fs)
        {
            string headers = "HTTP/1.1 200 OK\nContent-Type: " +
                contentType + "\nContent-Length: " + fs.Length + "\n\n";

            byte[] headerBuf = Encoding.UTF8.GetBytes(headers);
            client.GetStream().Write(headerBuf, 0, headerBuf.Length);
                        
            while(fs.Position < fs.Length)
            {
                count = fs.Read(buffer, 0, buffer.Length);
                client.GetStream().Write(buffer, 0, count);
            }
        }

        private void SendError(TcpClient client, int code)
        {
            try
            {
                string codeString = code.ToString() + " " + ((HttpStatusCode)code).ToString();
                //Страница с ошибкой
                string html = "<html><body><h1>" + codeString + "</h1></body></html>";
                //необходимые заголовки
                //ответ сервера, тип и длина содержимого.
                string pageStr = "HTTP/1.1 " + codeString +
                    "\nContent-type: text/html\nContent-Length:" + html.Length.ToString() + "\n\n" + html;

                buffer = Encoding.UTF8.GetBytes(pageStr);
                client.GetStream().Write(buffer, 0, buffer.Length);
            }
            catch (Exception)
            {
                Console.WriteLine("Error!");
            }
            finally
            {
                client.Close();
            }
        }
    }
}
