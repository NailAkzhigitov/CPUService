//
// Служба получения списка запущенных процессов и записи данные в БД
//
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.IO;
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
        private bool IsExit;            // флаг выхода 
        string connString;              // строка подключения к БД
        ProccessList procList;          // 
        /// <summary>
        /// Конструктор
        /// </summary>
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
                // читаем номер порта для запуска сокета  
                try
                {
                    port = Convert.ToInt32(ConfigurationManager.AppSettings["CPULoadServicePort"]);
                }
                catch { port = 3125; }
                // поток для приема сообщений от пользователей
                listenerThread = new Thread(ListenerThread);
                listenerThread.IsBackground = true;
                listenerThread.Name = "Listener";
                listenerThread.Start();
                // поток проверки списка процессов
                workThread = new Thread(WorkThread);
                workThread.IsBackground = true;
                workThread.Name = "WorkThread";
                // флаг завершения работы
                IsExit = false;
                procList = new ProccessList();
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
            string request = String.Empty;
            int command = 0;
            byte[] dataWrite;
            try
            {
                // получаем сообщение
                request = strR.ReadLine();
                WriteDataToLog("Пришла команда " + request);
            }
            catch { }
            // если размер > 3
            if (request.Length == 3)
            {
                try
                {
                    command = Convert.ToInt32(request);
                }
                catch { }
                if (command == 200)
                {
                    string commText = strR.ReadLine();
                    if (commText.Length > 0)
                    {
                        WriteDataToLog(commText);
                        string[] paramsList = commText.Split('#');
                        if (paramsList.Length > 0)
                        {
                            try
                            {
                                DateTime dt1, dt2;
                                dt1 = DateTime.Parse(paramsList[0]);
                                dt2 = DateTime.Parse(paramsList[1]);
                                DataTable dt = new DataTable();
                                using (SqlCeConnection sqlConn = new SqlCeConnection(connString))
                                {
                                    SqlCeCommand sqlComm = new SqlCeCommand();
                                    SqlCeDataAdapter da = new SqlCeDataAdapter();

                                    sqlComm.CommandText = "select ProccessId, ProccessName, Round(AVG(ProccessPercent),2) AS Expr1 ";
                                    sqlComm.CommandText += "FROM CPULoadData where RecordDate between @dt1 and @dt2 ";
                                    sqlComm.CommandText += "GROUP BY ProccessId, ProccessName ORDER BY Expr1 DESC ";
                                    sqlComm.Parameters.AddWithValue("@dt1", dt1);
                                    sqlComm.Parameters.AddWithValue("@dt2", dt2);
                                    sqlComm.Connection = sqlConn;
                                    sqlComm.CommandType = CommandType.Text;
                                    if (sqlConn.State == ConnectionState.Closed)
                                        sqlConn.Open();
                                    da.SelectCommand = sqlComm;
                                    da.Fill(dt);
                                    sqlConn.Close();
                                }
                                int i = 0;
                                // формируем ответ
                                string answer = "ans#";
                                foreach (DataRow dr in dt.Rows)
                                {
                                    answer += dr["ProccessId"].ToString() + ";" + dr["ProccessName"].ToString() + ";" + dr["Expr1"].ToString() + "#";
                                    i++;
                                    if (i == 10)
                                        break;
                                }
                                // отправка ответа
                                dataWrite = Encoding.UTF8.GetBytes(answer + "\r\n");
                                strW.Write(dataWrite, 0, dataWrite.Length);
                                // запись в лог
                                WriteDataToLog("Клиенту отправлен ответ " + answer);
                            }
                            catch (Exception ex) { WriteDataToLog("Ошибка отправки данных " + ex.Message); }
                        }
                    }
                }
            }
            strR.Close();
            strW.Close();

        }
        /// <summary>
        /// Поток для проверки процессов
        /// </summary>
        protected void WorkThread()
        {
            WriteDataToLog("Запуск потока проверки процессов");
       
            while (!IsExit)
            {
                // получение данных и запись в базу данных
                UpdateData();
                // прервем поток на 1 сек
                Thread.Sleep(1000);
            }
            WriteDataToLog("Основной поток завершил работу");
        }

        /// <summary>
        /// Получение данных и запись в БД
        /// </summary>
        private void UpdateData()
        {
            // получим список процессов
            Process[] ProcessList = Process.GetProcesses();
           
            // запись в БД
            using (SqlCeConnection sqlConn = new SqlCeConnection(connString))
            {
                // для каждого процесса в списке
                foreach (Process process in ProcessList)
                {
                    if (process.Id == 0) continue; // игнорируем процесс бездействие системы
                    try
                    {
                        // запишем в БД данные
                        SqlCeCommand sqlComm = new SqlCeCommand();
                        sqlComm.CommandText = "insert into CPULoadData(ProccessId, ProccessName, ProccessPercent) ";
                        sqlComm.CommandText += " values (@pid, @pname, @pperc); ";
                        sqlComm.Parameters.AddWithValue("@pid", process.Id);
                        sqlComm.Parameters.AddWithValue("@pname", process.ProcessName);
                        sqlComm.Parameters.AddWithValue("@pperc", procList.CheckProcess(process));
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
  
    /// <summary>
    /// Список процессов
    /// </summary>
    class ProccessList
    {
        // список процессов
        List<ProccessCounter> perfList = new List<ProccessCounter>();
        /// <summary>
        /// Проверка процесса
        /// </summary>
        /// <param name="process"></param>
        /// <returns></returns>
        public double CheckProcess(Process process)
        {
            bool IsExists = false;
            // проверяем по всему списку
            foreach (ProccessCounter pc in perfList)
            {
                // если нашли процесс, возвращаем счетчик
                if (pc.process.ProcessName == process.ProcessName)
                {
                    IsExists = true;
                    return pc.perfCounter.NextValue() / Environment.ProcessorCount;
                }
            }
            // если не нашли, добавим в список
            if (!IsExists)
            {
                ProccessCounter tmpProc = new ProccessCounter();
                tmpProc.process = process;
                tmpProc.perfCounter = new PerformanceCounter("Process", "% Processor Time", process.ProcessName, true);
                perfList.Add(tmpProc);
            }
            return 0;
        }
    }
    /// <summary>
    /// Данные процесса
    /// </summary>
    class ProccessCounter
    {
        public Process process;
        public PerformanceCounter perfCounter;
    }
}
