using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

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
            while (locker != null)
            { }

            locker = new Object();
            new Client((TcpClient)stateInfo);
            locker = null;
        }

        public void GetRequest()
        {
            request = string.Empty;

            try
            {
                CurrentClient.ReceiveTimeout = 5;
                while ((count = CurrentClient.GetStream().Read(buffer, 0, buffer.Length)) > 0)
                {
                    request += Encoding.UTF8.GetString(buffer, 0, count);
                    if (request.IndexOf("\r\n\r\n") >= 0 || request.Length > 4096) //\r\n\r\n - конец запроса, иначе принимаем не более 4Кб
                    {
                        break;
                    }
                }
            }
            catch (Exception)
            {
                //Console.WriteLine("Ошибка приема сообщения от клиента: {0}", e.Message);
                return;
            }

            Match reqMatch = Regex.Match(request.ToString(), @"^\w+\s+([^\s\?]+)[^\s]*\s+HTTP/.*|");

            if (reqMatch == Match.Empty)
            {
                SendError(404);
                return;
            }

            //Обработчики кнопок
            if (ButtonsHandlers(reqMatch))
            {
                return;
            }

            //Запросы страниц
            string reqUri = reqMatch.Groups[1].Value;
            reqUri = Uri.UnescapeDataString(reqUri);
            RequestOnPage(reqUri);
        }

        //Запросы на страницы
        private void RequestOnPage(string reqUri)
        {
            string path = Directory.GetCurrentDirectory();
            if (reqUri.IndexOf("..") >= 0)
            {
                SendError(400);
                return;
            }
            if (reqUri.EndsWith("/"))
            {
                SendFile(path.Remove(path.IndexOf("\\bin")) + "\\client\\i7000Control.html");
                return;
            }
            if (reqUri.EndsWith("/favicon.ico"))
            {
                SendFile(path.Remove(path.IndexOf("\\bin")) + "\\client\\favicon.ico");
                return;
            }
            if (reqUri.EndsWith("/client/history.html"))
            {
                lock (fileHistryRead)
                {
                    SendFile(path.Remove(path.IndexOf("\\bin")) + "\\client\\history.html");
                }
                return;
            }
            if (reqUri.EndsWith("/client/validator.js"))
            {
                SendFile(path.Remove(path.IndexOf("\\bin")) + "\\client\\validator.js");
                return;
            }
            if (reqUri.EndsWith("/client/css/styles.css"))
            {
                SendFile(path.Remove(path.IndexOf("\\bin")) + "\\client\\css\\styles.css");
                return;
            }
            if (reqUri.EndsWith("/client/scripts.js"))
            {
                SendFile(path.Remove(path.IndexOf("\\bin")) + "\\client\\scripts.js");
                return;
            }
            if (reqUri.EndsWith("/client/meandr.js"))
            {
                SendFile(path.Remove(path.IndexOf("\\bin")) + "\\client\\meandr.js");
                return;
            }
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
                    contentType = (extension.Length > 1) ? "application/" + extension.Substring(1) : "application/unknown";
                    break;
            }
        }

        //Отправка файла с заголовков
        private void HeaderSending(string contentType, FileStream fs)
        {
            SendMessage(len: fs.Length.ToString(), contentType: contentType);

            try
            {
                while (fs.Position < fs.Length)
                {
                    count = fs.Read(buffer, 0, buffer.Length);
                    CurrentClient.GetStream().Write(buffer, 0, count);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка отправки ответа клиенту: {0}", e.Message);
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
            GetExtension(path.Substring(path.LastIndexOf('.')), out string contentType);

            //Открываем запрошенный файл
            FileStream fileStream;
            try
            {
                fileStream = new FileStream(path, FileMode.Open, FileAccess.Read,
                    FileShare.Read);
            }
            catch (Exception e)
            {
                SendError(500);
                Console.WriteLine("Ошибка чтения файла на сервере: {0}", e.Message);
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

            try
            {
                CurrentClient.GetStream().Write(buffer, 0, buffer.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка отправки ответа клиенту: {0}", e.Message);
            }
        }

        //Отправка кода ошибки
        private void SendError(int code = 0, string codeString = null)
        {
            if (codeString == null)
            {
                codeString = code.ToString() + " " + ((HttpStatusCode)code).ToString();
            }
            //Страница с ошибкой
            string html = "<html><body><h1>" + codeString + "</h1></body></html>";
            try
            {
                //необходимые заголовки
                SendMessage(codeString, html.Length.ToString(), html: html);
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка отправки сообщения об ошибке: {0}", e.Message);
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
                    Module.GetModul.OpenPort(masStr[1], masStr[3]);
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
                    Module.GetModul.WriteToPort(masStr[1]);
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
                    string[] masStr = reqMatch.Groups[0].Value.Split(new string[] { "GET /?", "=", "&", " " }, StringSplitOptions.RemoveEmptyEntries);

                    string result = Module.GetModul.BuildMeandre(masStr[1], masStr[3], masStr[5], masStr[7], masStr[9]);

                    SendMessage(len: result.Length.ToString(), html: result);
                }
                catch (Exception)
                {
                    SendMessage(425.ToString() + " Bad request ", 0.ToString());
                }
                return true;
            }
            else if (reqMatch.Groups[0].Value.Contains("adressADC"))
            {
                string[] masStr = reqMatch.Groups[0].Value.Split(new string[] { "GET /?", "=", "&", " " }, StringSplitOptions.RemoveEmptyEntries);

                string result = masStr[1];

                string answer = Module.GetModul.ReadLvl(result);



                answer = answer.Replace("+", "");

                answer = answer.Replace(",", ".");

                SendMessage(len: answer.Length.ToString(), html: answer);
            }

            return false;
        }
    }
}
