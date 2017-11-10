using System;
using System.IO.Ports;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace I7000Server
{
    class Module
    {
        private SerialPort comPort = null;

        public static Module GetModul = null;

        public static void CreateModul()
        {
            if (GetModul == null)
            {
                GetModul = new Module();
            }
        }

        private Module()
        {
            while (comPort != null) ;
            comPort = new SerialPort();
            comPort.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
        }

        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            AddRecieve();
        }

        public void WriteToPort(string command)
        {
            // записать команду в COM-порт (символ окончания команды – 0x0D)
            comPort.WriteLine(command + (char)0x0D);
            // выдать сообщение в историю
            addHistoryMessage("Записана команда:" + command + "\n");
        }

        public void AddRecieve()
        {
            // задержка
            System.Threading.Thread.Sleep(100);
            // буфер для чтения данных из COM-порта
            byte[] dataR = new byte[comPort.BytesToRead];
            // прочитать данные
            comPort.Read(dataR, 0, dataR.Length);
            // добавить ответ в историю команд
            addHistoryMessage("Получен ответ:");
            for (int i = 0; i < dataR.Length; i += 1)
                addHistoryMessage(((char)dataR[i]).ToString());
            addHistoryMessage("\n");
            comPort.DiscardInBuffer();
        }


        public void ClosePort()
        {
            // освободить выходной буфер
            comPort.DiscardOutBuffer();
            // освободить входной буфер
            comPort.DiscardInBuffer();
            // закрыть порт
            if (comPort.IsOpen)
                comPort.Close();
            addHistoryMessage("Порт закрыт. \n");
        }

        public void addHistoryMessage(string msg)
        {
            History.WriteHistory(msg + "<br>");
        }

        private string FormatedOutputMeandr(double[] time, double[] value)
        {
            StringBuilder sb = new StringBuilder();

            if((time.Length == value.Length) && time.Length > 0)
            {
                sb.Append(time[0]);
                sb.Append(" " + value[0]);

                for(int i = 1; i < time.Length; i++)
                {
                    sb.Append(" " + time[i]);
                    sb.Append(" " + value[i]);
                }
            }
            return sb.ToString();
        }

        public async Task<int> SendLvl(object amp)
        {
            string s = DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss:fff");
            Console.WriteLine(c++ + '.' + s);

            //double A = 0;
            //double.TryParse(amp.ToString() , out A);
            return 1;
        }

        volatile int c;

        public string BuildMeandre(string amp, string freq, string freqDigit)
        {
            ////кол-во периодов
            //int count = 10;
            ////Амплитуда колебания сигнала
            //double A = 0;
            ////Частота меандра
            //int  F = 0,
            ////Частота дискретизации
            // Fd = 0;

            //double.TryParse(amp, out A);
            //int.TryParse(freq, out F);
            //int.TryParse(freqDigit, out Fd);

            ////Время периода
            //double T = 1 / (double)F;
            //double AllTime = T * (double)count;
            //int ScanCount = (int)(AllTime * Fd);

            //Timer timer = new Timer(200);


            //timer.Elapsed += async (o, e) => await SendLvl(amp);
            //timer.Start();

            //while (c < ScanCount) ;
            //timer.Stop();


            ////c = 0;
            ////Timer timer = new Timer(new TimerCallback(SendLvl), A, 0, 5);
            ////while (c < ScanCount) ;
            ////timer.Dispose();

            //for (int i = 0; i < count; i++)
            //{

            //}

            int ScanCount = 10;



            double[] time = new double[]  { 1, 2,  3,  3, 4, 5, 6,  6, 7 };
            double[] value = new double[] { 5, 5, -5, -5, 5, 5, 5, -5, 6 };

            ///Построение меандра

            return FormatedOutputMeandr(time, value);
        }

        public string ReadMeandre(string freqDigid)
        {
            StringBuilder sb = new StringBuilder();




            return sb.ToString();
        }

        public void openPort(string portName, string speed)
        {
            portName = "COM" + portName;
            if (comPort.IsOpen)
                comPort.Close();
            else
            {// порт ранее открыт не был
             // название COM-порта
                comPort.PortName = portName;
                // скорость работы COM-порта
                int baudRate = 9600;
                int.TryParse(speed, out baudRate);
                comPort.BaudRate = baudRate;
                // число бит данных
                comPort.DataBits = 8;
                // число стоповых бит - один
                comPort.StopBits = StopBits.One;
                // бит паритета - нет
                comPort.Parity = Parity.None;
                // квитировать установление связи - нет
                comPort.Handshake = Handshake.None;
                // число принимаемых бит
                comPort.ReceivedBytesThreshold = 8;
                // размер буфера для записи
                comPort.WriteBufferSize = 20;
                // размер буфера для чтения
                comPort.ReadBufferSize = 20;
                // время таймаута чтения - по умолчанию
                comPort.ReadTimeout = -1;
                // время таймаута записи - по умолчанию
                comPort.WriteTimeout = -1;
                // сигнал готовности терминала к передаче данных - не установлен
                comPort.DtrEnable = false;
                // открыть порт
                comPort.Open();
                // запрос передатчика - установлен
                comPort.RtsEnable = true;
                // задержка
                System.Threading.Thread.Sleep(100);
                addHistoryMessage("Порт " + portName + " открыт \n");
            }
        }
    }
}
