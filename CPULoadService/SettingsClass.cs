using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace CPULoadService
{
    /// <summary>
    /// класс настроек
    /// </summary>
    public class SettingsClass
    {
        /// <summary>
        /// Порт сервиса 
        /// </summary>
        public int ServicePort { get; set; }

        /// <summary>
        /// Уровень минимальной загрузки процессора 
        /// </summary>
        public double LoadLevel { get; set; }

        /// <summary>
        /// Интервал наблюдения 
        /// </summary>
        public int WatchInterval { get; set; }

        /// <summary>
        /// Время работы приложения
        /// </summary>
        public int ApplicationWorkTime { get; set; }

        /// <summary>
        /// Файл для запуска 
        /// </summary>
        public string ApplicationPath { get; set; }

        /// <summary>
        /// Флаг активности
        /// </summary>
        public string IsActive { get; set; }

        /// <summary>
        /// Время запуска приложения
        /// </summary>
        public DateTime AppStartTime { get; set; }

        /// <summary>
        /// Статус приложения
        /// </summary>
        public string AppStatus { get; set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        public SettingsClass()
        {
            // установки по умолчанию
            ServicePort = 1310;
            LoadLevel = 5;
            WatchInterval = 600;
            ApplicationWorkTime = 600;
            ApplicationPath = "";
            IsActive = "1";

            // Чтение параметров из файла
            List<string> lst = new List<string>();
            lst.AddRange(ConfigurationManager.AppSettings.AllKeys);
            for (int i = 0; i < lst.Count; i++)
            {
                try
                {
                    switch (lst[i])
                    {
                        // порт сервиса
                        case "ServicePort":
                            if (Convert.ToInt32(ConfigurationManager.AppSettings["ServicePort"]) > 0)
                            {
                                ServicePort = Convert.ToInt32(ConfigurationManager.AppSettings["ServicePort"]);
                            }
                            break;

                        // уровень загрузки процессора
                        case "LoadLevel":
                            if (Convert.ToDouble(ConfigurationManager.AppSettings["LoadLevel"]) > 0)
                            {
                                LoadLevel = Convert.ToDouble(ConfigurationManager.AppSettings["LoadLevel"]);
                            }
                            break;
                        // интервал времени
                        case "WatchInterval":
                            if (Convert.ToInt32(ConfigurationManager.AppSettings["WatchInterval"]) > 0)
                            {
                                WatchInterval = Convert.ToInt32(ConfigurationManager.AppSettings["WatchInterval"]);
                            }
                            break;
                        // время работы приложения
                        case "ApplicationWorkTime":
                            if (Convert.ToInt32(ConfigurationManager.AppSettings["ApplicationWorkTime"]) > 0)
                            {
                                ApplicationWorkTime = Convert.ToInt32(ConfigurationManager.AppSettings["ApplicationWorkTime"]);
                            }
                            break;
                        // приложение
                        case "ApplicationPath":
                            ApplicationPath = ConfigurationManager.AppSettings["ApplicationPath"];
                            break;
                        // вкл/откл приложение
                        case "IsActive":
                            IsActive = ConfigurationManager.AppSettings["IsActive"];
                            break;

                        default:
                            break;
                    }
                }
                catch { }
            }
        }

        /// <summary>
        /// Сохранение настроек в файл конфигурации
        /// </summary>
        public void SaveSettings()
        {
            // создаем объект
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            // вносим изменения
            config.AppSettings.Settings["ServicePort"].Value = ServicePort.ToString();
            config.AppSettings.Settings["LoadLevel"].Value = LoadLevel.ToString();
            config.AppSettings.Settings["WatchInterval"].Value = WatchInterval.ToString();
            config.AppSettings.Settings["ApplicationWorkTime"].Value = ApplicationWorkTime.ToString();
            config.AppSettings.Settings["ApplicationPath"].Value = ApplicationPath;
            config.AppSettings.Settings["IsActive"].Value = IsActive;
            // сохраняем
            config.Save(ConfigurationSaveMode.Modified);
            // обновялем
            ConfigurationManager.RefreshSection("appSettings");

        }

        /// <summary>
        /// Запуск процесса
        /// </summary>
  /*      public void StartOutProcess()
        {
            // проверка на существование
            if (File.Exists(ApplicationPath))
            {
                AppStatus = "Приложение запущено";
                // вернет информацию о процессе для отслеживания
                ApplicationLoader.PROCESS_INFORMATION procInfo;
                // запуск приложения
                ApplicationLoader.StartProcessAndBypassUAC(ApplicationPath, out procInfo);

                try
                {
                    // получим процесс по ИД
                    Process process = Process.GetProcessById((int)procInfo.dwProcessId);
                    // если запущен
                    if (!process.HasExited)
                    {
                        process.WaitForExit(ApplicationWorkTime * 1000); // задержка
                        // если запущен
                        if (!process.HasExited)
                        {
                            process.Kill(); // Посылаем команду на закрытие
                        }
                    }
                }
                catch { }
                AppStatus = "Приложение не активно";
            }
        }*/
    }
}
