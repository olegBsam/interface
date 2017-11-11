using System;
using System.IO.Ports;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Threading;

namespace I7000Server
{
    class Module
    {
        private SerialPort comPort = null;

        public static Module GetModul = null;


        private double amplitude;
        private double ampl;
        private volatile object setLvlSynch;
        private List<double> time;
        private volatile List<string> times;
        private volatile List<double> value;
        private volatile List<string> valueStr;

        private System.Timers.Timer freqTimer;
        private System.Timers.Timer samplingTimer;
        private System.Timers.Timer maintimer;


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

       
        public void addHistoryMessage(string msg)
        {
            History.WriteHistory(msg + "<br>");
        }

        private long toMillisecond(string str)
        {
            string[] strs = str.Split(new char[] { ':' });
            long ms = long.Parse(strs[1]) * 60 * 1000 + long.Parse(strs[2]) * 1000 + long.Parse(strs[3]);
            return ms;
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

        public string BuildMeandre(string amp, string frequency, string freqDigit, string adcAdr, string dacAdr)
        {
            long starTime = toMillisecond(DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss:fff"));

            initialize();

            amplitude = 0;

            setLvlSynch = new object();

            times.Add(DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss:fff"));
            value.Add(5);

            double.TryParse(amp, out ampl);
            amplitude = ampl;

            double sampleFreq;
            double.TryParse(freqDigit, out sampleFreq);

            double freq;
            double.TryParse(frequency, out freq);

  

            double samplingT = 1 / sampleFreq * 1000;
            double freqT = 1 / freq * 1000;
            double allTime = 10 * freqT;          ///10 периодов

            //Таймер переполняющийся по периоду дискретизации
            samplingTimer = new System.Timers.Timer();
            samplingTimer.AutoReset = true;

            samplingTimer.Interval = samplingT;
            samplingTimer.Elapsed += SendLvl;

            //Таймер переполняющийся по половине периода меандра
            freqTimer = new System.Timers.Timer();
            freqTimer.AutoReset = true;

            freqTimer.Interval = freqT / 2;
            freqTimer.Elapsed += FreqTimer_Elapsed;

            //Таймер, который переполняется по истечению времени, заданного пользователем (количество периодов)
            maintimer = new System.Timers.Timer();
            maintimer.AutoReset = false;
            maintimer.Elapsed += Maintimer_Elapsed;

            maintimer.Interval = allTime;

            freqTimer.Start();
            samplingTimer.Start();
            maintimer.Start();

            while (maintimer.Enabled) ;

            foreach(var o in times)
                time.Add(toMillisecond(o) - starTime);

            return FormatedOutputMeandr(time.ToArray(), value.ToArray());
        }

        private void FreqTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            lock (setLvlSynch)
            {
                amplitude = amplitude == 0 ? ampl : 0;
            }
        }

        private void Maintimer_Elapsed(object sender, ElapsedEventArgs e)
        {
           // lock (setLvlSynch)
            {
                samplingTimer.Stop();
                freqTimer.Stop();
                samplingTimer.Dispose();
                freqTimer.Dispose();
            }
        }

        private void SendLvl(object sender, ElapsedEventArgs e)
        {
            lock (setLvlSynch)
            {
                //WriteToPort("Установить уровень = amplitude");
                //ReadPort("Считать значение с ацп")
                times.Add(DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss:fff"));
                value.Add(amplitude);
            }
            return;
        }
        private void ReadLvl(object sender, ElapsedEventArgs e)
        {
            lock (setLvlSynch)
            {

                //WriteToPort("Установить уровень = amplitude");
                //ReadPort("Считать значение с ацп")
                times.Add(DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss:fff"));
                ThreadPool.QueueUserWorkItem((o) => valueStr.Add(AddRecieve()) /*"считать значение с порта"*/);//Должно работать :)
            }
            return;
        }

        private void initialize()
        {
            if (freqTimer != null)
                freqTimer.Dispose();
            if (samplingTimer != null)
                samplingTimer.Dispose();
            if (maintimer != null)
                maintimer.Dispose();

            time = new List<double>();
            value = new List<double>();
            times = new List<string>();
        }

        public string ReadMeandre(string freqDigit, string adcAdr, string dacAdr)
        {

            long starTime = toMillisecond(DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss:fff"));

            initialize();

            amplitude = 0;

            setLvlSynch = new object();

            double sampleFreq;
            double.TryParse(freqDigit, out sampleFreq);

            double samplingT = 1 / sampleFreq * 1000;
            double allTime = 10_000;

            //Таймер переполняющийся по периоду дискретизации
            samplingTimer = new System.Timers.Timer();
            samplingTimer.AutoReset = true;

            samplingTimer.Interval = samplingT;
            samplingTimer.Elapsed += ReadLvl;

            //Таймер, который переполняется по истечению времени, заданного пользователем (количество периодов)
            maintimer = new System.Timers.Timer();
            maintimer.AutoReset = false;
            maintimer.Elapsed += Maintimer_Elapsed;

            maintimer.Interval = allTime;

            samplingTimer.Start();
            maintimer.Start();

            while (maintimer.Enabled) ;

            foreach (var o in times)
                time.Add(toMillisecond(o) - starTime);

            return FormatedOutputMeandr(time.ToArray(), value.ToArray());
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

        public string AddRecieve()
        {
            StringBuilder sb = new StringBuilder();

            // задержка
            System.Threading.Thread.Sleep(100);
            // буфер для чтения данных из COM-порта
            byte[] dataR = new byte[comPort.BytesToRead];
            // прочитать данные
            comPort.Read(dataR, 0, dataR.Length);
            // добавить ответ в историю команд
            addHistoryMessage("Получен ответ:");

            for (int i = 0; i < dataR.Length; i += 1)
                sb.Append(((char)dataR[i]).ToString());
            //addHistoryMessage(((char)dataR[i]).ToString());
            addHistoryMessage(sb.ToString());
            addHistoryMessage("\n");
            comPort.DiscardInBuffer();

            return sb.ToString();
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
