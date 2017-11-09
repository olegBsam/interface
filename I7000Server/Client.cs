using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

namespace I7000Server
{
    public class Client
    {

        public volatile static object fileHistryRead = new object();

        private string request = "";
        private byte[] buffer = new byte[1024];
        private int count;

        private TcpClient CurrentClient { get; set; }

        public Client(TcpClient newClient)
        {
            CurrentClient = newClient;
            GetRequest();
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

        public void GetRequest()
        {
            request = string.Empty;
            while ((count = CurrentClient.GetStream().Read(buffer, 0, buffer.Length)) > 0)
            {
                request += Encoding.UTF8.GetString(buffer, 0, count);
                if (request.IndexOf("\r\n\r\n") >= 0 || request.Length > 4096) //\r\n\r\n - конец запроса, иначе принимаем не более 4Кб
                    break;
            }


            Match reqMatch = Regex.Match(request.ToString(), @"^\w+\s+([^\s\?]+)[^\s]*\s+HTTP/.*|");

            if (reqMatch == Match.Empty)
            {
                SendError(404);
                return;
            }

            //Обработчики кнопок
            if (ButtonsHandlers(reqMatch))
                return;

            //Запросы страниц
            string reqUri = reqMatch.Groups[1].Value;
            reqUri = Uri.UnescapeDataString(reqUri);

            #region SendPage();
            if (reqUri.IndexOf("..") >= 0)
            {
                SendError(400);
                return;
            }
            if (reqUri.EndsWith("/"))
            {
                string path = Directory.GetCurrentDirectory();
                SendFile(path.Remove(path.IndexOf("\\bin")) + "\\i7000Control.html");
                return;
            }
            if (reqUri.EndsWith("/favicon.ico"))
            {
                string path = Directory.GetCurrentDirectory();
                SendFile(path.Remove(path.IndexOf("\\bin")) + "\\favicon.ico");
                return;
            }
            if (reqUri.EndsWith("/history.html"))
            {
                lock (fileHistryRead)
                {
                    string path = Directory.GetCurrentDirectory();
                    SendFile(path.Remove(path.IndexOf("\\bin")) + "\\history.html");
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

        //Отправка файла с заголовков
        private void HeaderSending(string contentType, FileStream fs)
        {
            SendMessage(len: fs.Length.ToString(), contentType: contentType);

            while (fs.Position < fs.Length)
            {
                count = fs.Read(buffer, 0, buffer.Length);
                CurrentClient.GetStream().Write(buffer, 0, count);
            }
        }


        //Отправка файла с заголовком (страниц)
        public bool SendFile(string path)
        {
            if (!File.Exists(path))
            {
                SendError(400);
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
                SendError(500);
                return false;
            }

            HeaderSending(contentType, fileStream);
            fileStream.Close();
            return true;
        }

        private void SendMessage(string code = "200 OK", string len = "0", string html = "", string contentType = "text/html")
        {
            string pageStr = "HTTP/1.1 " + code +
                      " \nContent-type: " + contentType + "\nContent-Length:" + len + "\n\n" + html;
            buffer = Encoding.UTF8.GetBytes(pageStr);
            CurrentClient.GetStream().Write(buffer, 0, buffer.Length);
        }

        //Отправка кода ошибки
        private void SendError(int code = 0, string codeString = null)
        {
            try
            {
                if (codeString == null)
                    codeString = code.ToString() + " " + ((HttpStatusCode)code).ToString();
                //Страница с ошибкой
                string html = "<html><body><h1>" + codeString + "</h1></body></html>";
                //необходимые заголовки
                SendMessage(codeString, html.Length.ToString(), html: html);
            }
            catch (Exception)
            {
                Console.WriteLine("Error!");
            }
            finally
            {
                CurrentClient.Close();
            }
        }


        //Обработка кнопок
        public bool ButtonsHandlers(Match reqMatch)
        {
            if (reqMatch.Groups[0].Value.Contains("portNumber"))
            {
                try
                {
                    string reqStr = reqMatch.Groups[0].Value;
                    string[] masStr = reqStr.Split(new string[] { "GET /?", "=", "&", " " }, StringSplitOptions.RemoveEmptyEntries);
                    Modul.GetModul.openPort(masStr[1], masStr[3]);
                    SendMessage();
                }
                catch (Exception)
                {
                    SendMessage(425.ToString() + " Bad request ", 0.ToString());
                }
                return true;
            }
            else if (reqMatch.Groups[0].Value.Contains("command"))
            {
                string reqStr = reqMatch.Groups[0].Value;
                string[] masStr = reqStr.Split(new string[] { "GET /?", "=", "&", " " }, StringSplitOptions.RemoveEmptyEntries);

                try
                {
                    Modul.GetModul.WriteToPort(masStr[1]);
                    SendMessage();
                }
                catch (Exception)
                {
                    SendMessage(427.ToString() + " Bad request ", 0.ToString());
                }
                return true;
            }
            else if (reqMatch.Groups[0].Value.Contains("frequency"))
            {
                try
                {
                    string reqStr = reqMatch.Groups[0].Value;
                    string[] masStr = reqStr.Split(new string[] { "GET /?", "=", "&", " " }, StringSplitOptions.RemoveEmptyEntries);

                    string result = Modul.GetModul.BuildMeandre(masStr[1], masStr[3], masStr[5]);

                    SendMessage(len: result.Length.ToString(), html: result);
                }
                catch (Exception)
                {
                    SendMessage(425.ToString() + " Bad request ", 0.ToString());
                }
                return true;
            }
            else if (reqMatch.Groups[0].Value.Contains("auto"))
            {
                try
                {
                    string reqStr = reqMatch.Groups[0].Value;
                    string[] masStr = reqStr.Split(new string[] { "GET /?", "=", "&", " " }, StringSplitOptions.RemoveEmptyEntries);

                    string result = Modul.GetModul.ReadMeandre(masStr[1]);

                    SendMessage(result, result.Length.ToString());
                }
                catch (Exception)
                {
                    SendMessage(425.ToString() + " Bad request ", 0.ToString());
                }
                return true;
            }
            return false;
        }
    }
}
