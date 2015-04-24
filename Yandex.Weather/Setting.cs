using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Xml;
using System.Security.Cryptography;
using System.Collections.Generic;
using WeatherLib;

namespace Yandex.Forecast
{
    public static class Settings
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        // Период обновления приложения в мс
        public struct RefreshPeriod
        {
            public static RefreshPeriodItem HalfHour = new RefreshPeriodItem("HalfHour", 1800000, 0);
            public static RefreshPeriodItem Hour = new RefreshPeriodItem("Hour", 3600000, 1);
            public static RefreshPeriodItem TwoHour = new RefreshPeriodItem("TwoHour", 7200000, 2);
            public static RefreshPeriodItem FourHour = new RefreshPeriodItem("FourHour", 14400000, 3);

            public static uint FindValue(string _name)
            {
                switch(_name)
                {
                    default:
                        return 0;
                    case "HalfHour":
                        return HalfHour.Value;
                    case "Hour":
                        return Hour.Value;
                    case "TwoHour":
                        return TwoHour.Value;
                    case "FourHour":
                        return FourHour.Value;
                }
            }
            public static uint FindValue(int _id)
            {
                switch(_id)
                {
                    default:
                        return 0;
                    case 0:
                        return HalfHour.Value;
                    case 1:
                        return Hour.Value;
                    case 2:
                        return TwoHour.Value;
                    case 3:
                        return FourHour.Value;
                }
            }

            public static int FindID(string _name)
            {
                switch (_name)
                {
                    default:
                        return -1;
                    case "HalfHour":
                        return HalfHour.ID;
                    case "Hour":
                        return Hour.ID;
                    case "TwoHour":
                        return TwoHour.ID;
                    case "FourHour":
                        return FourHour.ID;
                }
            }
            public static int FindID(uint _value)
            {
                switch (_value)
                {
                    default:
                        return -1;
                    case 1800000:
                        return HalfHour.ID;
                    case 3600000:
                        return Hour.ID;
                    case 7200000:
                        return TwoHour.ID;
                    case 14400000:
                        return FourHour.ID;
                }
            }

            public static string FindName(int _id)
            {
                switch (_id)
                {
                    default:
                        return null;
                    case 0:
                        return HalfHour.Name;
                    case 1:
                        return Hour.Name;
                    case 2:
                        return TwoHour.Name;
                    case 3:
                        return FourHour.Name;
                }
            }
            public static string FindName(uint _value)
            {
                switch (_value)
                {
                    default:
                        return null;
                    case 1800000:
                        return HalfHour.Name;
                    case 3600000:
                        return Hour.Name;
                    case 7200000:
                        return TwoHour.Name;
                    case 14400000:
                        return FourHour.Name;
                }
            }
        }

        public static WindowState CurrentWindowState
        {
            get;
            set;
        }

        // Настройка логгера
        public static void LoggerConfig()
        {
            LoggingConfiguration config = new LoggingConfiguration();

            FileTarget fileTarget = new FileTarget();
            config.AddTarget("file", fileTarget);

            fileTarget.DeleteOldFileOnStartup = true; // Удалатья страый файл при запуске
            fileTarget.KeepFileOpen = false; //Держать файл открытым
            fileTarget.ConcurrentWrites = true; //
            fileTarget.Encoding = Encoding.Unicode; // Кодировка файла логгера
            fileTarget.ArchiveEvery = FileArchivePeriod.Day; // Период архивирования старых логов
            fileTarget.Layout = NLog.Layouts.Layout.FromString("${longdate} | ${uppercase:${level}} | ${message}"); // Структура сообщения
            fileTarget.FileName = NLog.Layouts.Layout.FromString("${basedir}/logs/${shortdate}.log"); //Структура названия файлов
            fileTarget.ArchiveFileName = NLog.Layouts.Layout.FromString("${basedir}/logs/archives/{shortdate}.rar"); // Структура названия архивов

            LoggingRule ruleFile = new LoggingRule("*", LogLevel.Trace, fileTarget); // Минимальный уровень логгирования - Trace
            config.LoggingRules.Add(ruleFile);

            LogManager.Configuration = config;
        }

        //Настройка иконки в трее
        public static System.Windows.Forms.NotifyIcon TrayIconConfig()
        {
            System.Windows.Forms.NotifyIcon TrayIcon = new System.Windows.Forms.NotifyIcon();
            TrayIcon.Icon = Yandex.Forecast.Properties.Resources.icon;
            TrayIcon.Text = "Яндекс.Погода";
            TrayIcon.Click += delegate(object sender, EventArgs e)
            {
                if ((e as System.Windows.Forms.MouseEventArgs).Button == System.Windows.Forms.MouseButtons.Left)
                {
                    MainWindow.ShowHideMainWindow();
                }
                else
                {
                    MainWindow.TrayMenu.IsOpen = true;
                }
            };
            TrayIcon.Visible = true;
            return TrayIcon;
        }
        
        // Проверяем, загружен ли файл со списком городов
        public static bool IsCityListLoaded()
        {
            if(!File.Exists(Properties.Settings.Default.CityListPath))
            {
                XmlDocument result = new XmlDocument();
                if(Weather.CheckInternet())
                {
                    result.Load(Properties.Settings.Default.WebCityListPath);
                    result.Save(Properties.Settings.Default.CityListPath);
                    logger.Trace("Список городов успешно загружен.");
                    return true;
                }
                else
                {
                    logger.Trace("При загрузке списка городов произошла ошибка. Список не загружен.");
                    return false;
                }
            }
            return true;
        }
    }
    public class RefreshPeriodItem
    {
        public string Name
        {
            get;
            set;
        }
        public uint Value 
        { 
            get; 
            set; 
        }
        public int ID
        {
            get;
            set;
        }
        public RefreshPeriodItem(string _Name, uint _Value, int _ID)
        {
            Name = _Name;
            Value = _Value;
            ID = _ID;
        }
    }
}
