using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace CPULoadService
{
    public partial class CPULoadService : ServiceBase
    {

        // Путь к папке с программой
        private string ServicePath;
        private Thread listenerThread;  // поток для работы с клиентами
        private Thread workThread;      // поток для работы и запуска файлов
        private TcpListener listener;   // слушатель порта
        private int port;               // порт для прослушки
        TcpClient client;
        private bool IsExit;
        double CpuUsagePercent;
        // класс для настроек 
        //public WatchSattings WS;
        //private DateTime lastExceeds; // время последнего превышения порога

        string connString;
        
        public CPULoadService()
        {
            InitializeComponent();
            try
            {
                // путь к службе
                ServicePath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                connString = "Data Source=" + Path.Combine(ServicePath, "CPUDatabase.sdf");
                // журнал событий - системный - приложение
                this.ServiceName = "CPULoadService";
                this.eventLog1 = new System.Diagnostics.EventLog();
                this.eventLog1.Source = this.ServiceName;
                this.eventLog1.Log = "Application";
                // иниц класс настроек
                //WS = new WatchSattings();
                // порт
                //port = WS.ServicePort;
                // поток для приема сообщений от пользователей
                listenerThread = new Thread(ListenerThread);
                listenerThread.IsBackground = true;
                listenerThread.Name = "Listener";
                listenerThread.Start();
                // поток проверки списка процессов
                workThread = new Thread(WorkThread);
                workThread.IsBackground = true;
                workThread.Name = "WorkThread";
                // время последнего превышения порога - время запуска службы
                //lastExceeds = DateTime.Now;

                IsExit = false;
            }
            catch (Exception ex)
            {
                WriteDataToLog("Ошибка инициализации службы:" + ex.Message);
            }
        }
        /// <summary>
        /// Запуск сулжбы
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            workThread.Start();
        }
        /// <summary>
        /// Останов службы
        /// </summary>
        protected override void OnStop()
        {
            IsExit = true;
            workThread.Abort();
        }

        /// <summary>
        /// Запись данных лога 
        /// </summary>
        /// <param name="logmessage">Текст сообщения</param>
        public void WriteDataToLog(string logmessage)
        {
            eventLog1.WriteEntry(logmessage, EventLogEntryType.Information);
        }

        /// <summary>
        /// Поток для обработки команд пользователя
        /// </summary>
        protected void ListenerThread()
        {
            try
            {
                IPAddress ipAddress = IPAddress.Any;
                listener = new TcpListener(ipAddress, port);
                // запуск потока для прослушивания подключений клиентов
                listener.Start();
                while (true)
                {
                    client = listener.AcceptTcpClient();
                    // Здесь создается поток для работы с клиентом
                    Thread newclient = new Thread(new ThreadStart(WorkWithClient));
                    newclient.Start();
                }
            }
            catch (SocketException ex)
            {
                WriteDataToLog(ex.Message);
            }
        }
        /// <summary>
        /// Работа с клиентами, пока не отключится, соединение сохраняется
        /// </summary>
        protected void WorkWithClient()
        {
            WriteDataToLog("Подключился клиент!");
            StreamReader strR = new StreamReader(client.GetStream());
            NetworkStream strW = client.GetStream();
            string prihod = "prihod";
            int command = 0;
            byte[] dataWrite;
            try
            {
                // получаем сообщение
                prihod = strR.ReadLine();
                WriteDataToLog("Пришла команда " + prihod);
            }
            catch { }
            // если размер > 3
            if (prihod.Length == 3)
            {
                try
                {
                    command = Convert.ToInt32(prihod);
                }
                catch { }
                // разбираем команду
                switch (command)
                {
                    // получить настройки
                    case 100:
                        //WriteDataToLog("Пришла команда 100");
                        // посылаем ответ
                        /*string sendSettings = "100#";
                        sendSettings += WS.LoadLevel.ToString() + "#";
                        sendSettings += WS.WatchInterval.ToString() + "#";
                        sendSettings += WS.ApplicationWorkTime.ToString() + "#";
                        sendSettings += WS.ApplicationPath.ToString() + "#";
                        sendSettings += WS.IsActive.ToString() + "#";*/
                        //dataWrite = Encoding.UTF8.GetBytes(sendSettings + "\r\n");
                        //strW.Write(dataWrite, 0, dataWrite.Length);
                        //WriteDataToLog("Клиенту отправлен ответ " + sendSettings);
                        break;
                    // установить настройки
                    case 200:
                        //WriteDataToLog("Пришла команда 200");
                        //string commText = strR.ReadLine();
                        //if (commText.Length > 0)
                        //{
                        //    WriteDataToLog(commText);
                        //    string[] paramsList = commText.Split('#');
                        //    if (paramsList.Length > 0)
                        //    {
                        //        // Проверяем, существует ли файл
                        //        if (File.Exists(paramsList[3]))
                        //        {
                        //            WS.LoadLevel = Convert.ToInt32(paramsList[0]);
                        //            WS.WatchInterval = Convert.ToInt32(paramsList[1]);
                        //            WS.ApplicationWorkTime = Convert.ToInt32(paramsList[2]);
                        //            WS.ApplicationPath = paramsList[3];
                        //            WS.IsActive = paramsList[4];
                        //            WS.SaveSettings();
                        //            // отправим ответ, что настройки сохранены
                        //            dataWrite = Encoding.UTF8.GetBytes("200" + "\r\n");
                        //            strW.Write(dataWrite, 0, dataWrite.Length);
                        //        }
                        //        else
                        //        {
                        //            // отправим ответ, что файл не найден
                        //            dataWrite = Encoding.UTF8.GetBytes("404" + "\r\n");
                        //            strW.Write(dataWrite, 0, dataWrite.Length);
                        //        }
                        //    }
                        //}
                        break;
                    // получить состояние сервера
                    case 300:
                        //WriteDataToLog("Пришла команда 300");
                        //// посылаем ответ
                        //string sendSettings300 = "";
                        //sendSettings300 += WS.AppStatus + "; Время запуска";
                        //sendSettings300 += WS.AppStartTime.ToString("dd.MM.yyyy HH:mm:ss") + ";";
                        //dataWrite = Encoding.UTF8.GetBytes(sendSettings300 + "\r\n");
                        //strW.Write(dataWrite, 0, dataWrite.Length);
                        //WriteDataToLog("Клиенту отправлен ответ " + sendSettings300);
                        break;
                }
            }
            strR.Close();
            strW.Close();

        }
        /// <summary>
        /// Поток для индексации файлов
        /// </summary>
        protected void WorkThread()
        {
            WriteDataToLog("Старт проверки процессов");
       
            while (!IsExit)
            {
                UpdateData();
                // прервем поток на 1 сек
                Thread.Sleep(1000);
            }
            WriteDataToLog("Основной поток завершил работу");
        }

        private void UpdateData()
        {
            // сохраним данные снимка
           /* try
            {
                SqlCeCommand sqlComm = new SqlCeCommand();
                sqlComm.CommandText = "insert into Files(Path, FileName, CheckSum, UserId) ";
                sqlComm.CommandText += " values (@fpath, @fname, @fchecksum, @fuser); ";
                sqlComm.Parameters.AddWithValue("@fpath", mainWin.selectedImage.Path);
                sqlComm.Parameters.AddWithValue("@fname", mainWin.selectedImage.FileName);
                sqlComm.Parameters.AddWithValue("@fchecksum", mainWin.GetChecksum(mainWin.selectedImage.Path));
                sqlComm.Parameters.AddWithValue("@fuser", mainWin.ID_User);
                sqlComm.Connection = mainWin.dbConn;
                sqlComm.CommandType = CommandType.Text;
                if (mainWin.dbConn.State == ConnectionState.Closed)
                    mainWin.dbConn.Open();
                sqlComm.ExecuteNonQuery();
                mainWin.selectedImage.FileID = GetLastInsertedID(mainWin.dbConn);
                mainWin.dbConn.Close();
            }
            catch (Exception ex) { MessageBox.Show("Ошибка вставки записи о файле: " + ex.Message); return; }*/

            Process[] ProcessList = Process.GetProcesses();
            ////ProccessInfo[] procList = new ProccessInfo();
            List<ProccessInfo> procList = new List<ProccessInfo>();
            PerformanceCounter TotalCpuUsage = new PerformanceCounter("Process", "% Processor Time", "Idle");
            double TotalCpuUsageValue =  TotalCpuUsage.NextValue();
            //PerformanceCounter perf = new PerformanceCounter();
            var counters = new List<PerformanceCounter>();
            double TotalTime = 0;
            ////var counters = new List<double>();
            foreach (Process process in ProcessList)
            {
                //var counter = new PerformanceCounter("Process", "% Processor Time", process.ProcessName, true);
                //counter.NextValue();
                //counters.Add(counter);
            //    //process.
                if (process.Id == 0) continue;
                try
                {
                    ProccessInfo pi = new ProccessInfo();
                    pi.ProccessId = process.Id;
                    pi.ProccessName = process.ProcessName;
                    pi.proccess = process;
                   // counter.NextValue(); Thread.Sleep(100);
                   // pi.ProccessPerc = counter.NextValue(); //process.TotalProcessorTime.TotalMilliseconds;
                    procList.Add(pi);
                    //        //counters.Add(process.TotalProcessorTime.TotalMilliseconds);
                    TotalTime += process.TotalProcessorTime.TotalMilliseconds;
                }
                catch { }
                CpuUsagePercent = TotalTime / (100 - TotalCpuUsageValue);
            //    //
            }
            ////WriteDataToLog("Scan" +);
            //int i = 0;
            ////Thread.Sleep(500);
            using (SqlCeConnection sqlConn = new SqlCeConnection(connString))
            {
                foreach (var proc in procList)
                {
                    try
                    {
                        SqlCeCommand sqlComm = new SqlCeCommand();
                        sqlComm.CommandText = "insert into CPULoadData(ProccessId, ProccessName, ProccessPercent) ";
                        sqlComm.CommandText += " values (@pid, @pname, @pperc); ";
                        sqlComm.Parameters.AddWithValue("@pid", proc.ProccessId);
                        sqlComm.Parameters.AddWithValue("@pname", proc.ProccessName);
                        //PerformanceCounter process_cpu = new PerformanceCounter("Process", "% Processor Time", proc.ProcessName);
                        long NewCpuUsage = (long)proc.proccess.TotalProcessorTime.TotalMilliseconds;

                        sqlComm.Parameters.AddWithValue("@pperc", NewCpuUsage / CpuUsagePercent);//proc.ProccessPerc * TotalTime / (100 - TotalCpuUsageValue));//process_cpu.NextValue());
                        sqlComm.Connection = sqlConn;
                        sqlComm.CommandType = CommandType.Text;
                        if (sqlConn.State == ConnectionState.Closed)
                            sqlConn.Open();
                        sqlComm.ExecuteNonQuery();
                        sqlConn.Close();
                    }
                    catch (Exception ex) { WriteDataToLog("Ошибка записи данных в БД: " + ex.Message); IsExit = true; }
                }
            }

        }

    }

    class ProccessInfo
    {
        public int ProccessId { set; get; }
        public string ProccessName { set; get; }
        public double ProccessPerc { set; get; }
        public Process proccess;
        //PerformanceCounter perf { set; get; }
    }
}
