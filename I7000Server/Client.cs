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

        public volatile static object fileHistryRead = new object();
        private volatile static object fileIco = new object();
        private volatile static object fileMainPage = new object();

        string request = "";
        byte[] buffer = new byte[1024];
        int count;

        public Client(TcpClient newClient)
        {
            GetRequest(newClient);
            newClient.Close();
        }

        //Создание потока клиента
        private static Object locker = null;
        public static void ClientThread(Object stateInfo)
        {
            while (locker != null) ;
            locker = new Object();
            new Client((TcpClient)stateInfo);
            locker = null;
        }

        public void GetRequest(TcpClient client)
        {
            request = string.Empty;
            while ((count = client.GetStream().Read(buffer, 0, buffer.Length)) > 0)
            {
                request += Encoding.UTF8.GetString(buffer, 0, count);
                if (request.IndexOf("\r\n\r\n") >= 0 || request.Length > 4096) //\r\n\r\n - конец запроса, иначе принимаем не более 4Кб
                    break;
            }


            Match ReqMatch = Regex.Match(request.ToString(), @"^\w+\s+([^\s\?]+)[^\s]*\s+HTTP/.*|");

            if (ReqMatch == Match.Empty)
            {
                SendError(client, 404);
                return;
            }

            //Кнопки
            if (ReqMatch.Groups[0].Value.Contains("portNumber"))
            {
                try
                {
                    string reqStr = ReqMatch.Groups[0].Value;
                    string[] masStr = reqStr.Split(new string[] { "GET /?", "=", "&", " " }, StringSplitOptions.RemoveEmptyEntries);
                    Modul.GetModul.openPort(masStr[1], masStr[3]);
                    sendOK(client);
                }
                catch (Exception e)
                {
                    sendNotOk(client, 425.ToString());
                }
                return;
            }
            else if (ReqMatch.Groups[0].Value.Contains("command"))
            {
                string reqStr = ReqMatch.Groups[0].Value;
                string[] masStr = reqStr.Split(new string[] { "GET /?", "=", "&", " " }, StringSplitOptions.RemoveEmptyEntries);

                try
                {
                    Modul.GetModul.WriteToPort(masStr[1]);
                    sendOK(client);
                }
                catch (Exception)
                {
                    sendNotOk(client, 427.ToString());
                }
                return;
            }
            else if (ReqMatch.Groups[0].Value.Contains("frequency"))
            {
                try
                {
                    string reqStr = ReqMatch.Groups[0].Value;
                    string[] masStr = reqStr.Split(new string[] { "GET /?", "=", "&", " " }, StringSplitOptions.RemoveEmptyEntries);

                    string result = Modul.GetModul.BuildMeandre(masStr[1], masStr[3], masStr[5]);

                    sendOK(client, result, result.Length);
                }
                catch (Exception)
                {
                    sendNotOk(client, 425.ToString());
                }
                return;
            }
            else if (ReqMatch.Groups[0].Value.Contains("auto"))
            {
                try
                {
                    string reqStr = ReqMatch.Groups[0].Value;
                    string[] masStr = reqStr.Split(new string[] { "GET /?", "=", "&", " " }, StringSplitOptions.RemoveEmptyEntries);

                    string result = Modul.GetModul.ReadMeandre(masStr[1]);

                    sendOK(client, result, result.Length);
                }
                catch (Exception e)
                {
                    sendNotOk(client, 425.ToString());
                }
                return;
            }


            //Запросы на страницы
            string reqUri = ReqMatch.Groups[1].Value;
            reqUri = Uri.UnescapeDataString(reqUri);

            #region SendPage();
            if (reqUri.IndexOf("..") >= 0)
            {
                SendError(client, 400);
                return;
            }
            if (reqUri.EndsWith("/"))
            {
                string path = Directory.GetCurrentDirectory();
                path = path.Remove(path.IndexOf("\\bin")) + "\\i7000Control.html";
                SendFile(client, path);
                return;
            }
            if (reqUri.EndsWith("/favicon.ico"))
            {
                string path = Directory.GetCurrentDirectory();
                path = path.Remove(path.IndexOf("\\bin")) + "\\favicon.ico";
                SendFile(client, path);
                return;
            }
            if (reqUri.EndsWith("/history.html"))
            {
                lock (fileHistryRead)
                {
                    string path = Directory.GetCurrentDirectory();
                    path = path.Remove(path.IndexOf("\\bin")) + "\\history.html";
                    SendFile(client, path);
                }
                return;
            }
            #endregion
        }



        //Получение типов содержимого
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

        //Отправка заголовка
        private void HeaderSending(TcpClient client, string contentType, FileStream fs)
        {
            string headers = "HTTP/1.1 200 OK\nContent-Type: " +
                contentType + "\nContent-Length: " + fs.Length + "\n\n";

            byte[] headerBuf = Encoding.UTF8.GetBytes(headers);
            client.GetStream().Write(headerBuf, 0, headerBuf.Length);

            while (fs.Position < fs.Length)
            {
                count = fs.Read(buffer, 0, buffer.Length);
                client.GetStream().Write(buffer, 0, count);
            }
        }

        //Ответы сервера
        public bool SendFile(TcpClient client, string path)
        {
            if (!File.Exists(path))
            {
                SendError(client, 400);
                return false;
            }
            string contentType = "";
            GetExtension(path.Substring(path.LastIndexOf('.')), out contentType);

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

        private void sendOK(TcpClient client,string mes = "200 OK", int len = 0)
        {
            string headers = "HTTP/1.1 "+ mes +"\nContent-Type: text/html\nContent-Length: 0\n\n";
            buffer = Encoding.UTF8.GetBytes(headers);
            client.GetStream().Write(buffer, 0, buffer.Length);
        }
        private void sendNotOk(TcpClient client, string codeString)
        {
            try
            {
                string headers = "HTTP/1.1 " + codeString + " Bad request \nContent-Type: text/html\nContent-Length: 0\n\n";
                buffer = Encoding.UTF8.GetBytes(headers);
                client.GetStream().Write(buffer, 0, buffer.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error!");
            }
            finally
            {
                client.Close();
            }
        }
        //Отправка кода ошибки
        private void SendError(TcpClient client, int code = 0, string codeString = null)
        {
            try
            {
                if (codeString == null)
                    codeString = code.ToString() + " " + ((HttpStatusCode)code).ToString();
                //Страница с ошибкой
                string html = "<html><body><h1>" + codeString + "</h1></body></html>";
                //необходимые заголовки
                //ответ сервера, тип и длина содержимого.
                string pageStr = "HTTP/1.1 " + codeString +
                    " \nContent-type: text/html\nContent-Length:" + html.Length.ToString() + "\n\n" + html;

                buffer = Encoding.UTF8.GetBytes(pageStr);
                client.GetStream().Write(buffer, 0, buffer.Length);
            }
            catch (Exception e)
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
