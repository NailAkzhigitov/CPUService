using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Data.SqlServerCe;
using System.IO;
namespace CPULoadService
{

    class ProcessInfo
    {
        List<ProcessDescription> procdesc = new List<ProcessDescription>();

        SqlCeConnection sqlConn = new SqlCeConnection("Data Source=" + Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location),"CPUDatabase.sdf"));

       

        public static void UpdateProcessList()
        {
            // получим список процессов
            Process[] ProcessList = Process.GetProcesses();
            //UpdateCpuUsagePercent(NewProcessList);
           // UpdateExistingProcesses(NewProcessList);
            //AddNewProcesses(NewProcessList);
            //PerformanceCounter cpuCounter;
            //cpuCounter = new PerformanceCounter();
            //cpuCounter.CategoryName = "Processor";
            ////cpuCounter.CounterName = "% Processor Time";
            //cpuCounter.InstanceName = "_Total";
            foreach (Process proc in ProcessList)
            {
                SaveData(proc);
            }
        
        }

        public static void SaveData(Process proc)
        {
            PerformanceCounter process_cpu = new PerformanceCounter("Process", "% Processor Time", proc.ProcessName);
            float procVal = process_cpu.NextValue();
            //proc.ProcessName;
            //proc.Id;
            //proc.p
        }
    }

    class ProcessDescription
    {
        /// <summary>
        /// Имя процесса
        /// </summary>
        string Name { set; get; }

        /// <summary>
        /// Путь к файлу процесса
        /// </summary>
        string Path { set; get; }


    }
}
