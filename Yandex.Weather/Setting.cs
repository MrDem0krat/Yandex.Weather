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


namespace Yandex.Forecast
{
    public static class Settings
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static uint[] _RefreshPeriod = { 1, 2, 4, 8 };
        private static byte[] additionalEntropy = { 4, 9, 5, 6, 3 };
        private static string _CityName;
        private static WindowState _CurrentWindowState;

        // Период обновления приложения в мс
        public static uint RefreshPeriod
        {
            get { return _RefreshPeriod[Properties.Settings.Default.RefreshPeriodID]*1800000; }
        }
        public static WindowState CurrentWindowState
        {
            get { return _CurrentWindowState; }
            set { _CurrentWindowState = value; }
        }
        public static string CityName
        {
            get { return _CityName; }
            set { _CityName = value; }
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
        // Проверяем, загружен ли файл со списком городов и их ID
        public static bool IsCityListLoaded()
        {
            string webPath = "https://pogoda.yandex.ru/static/cities.xml";
            if(!File.Exists(Properties.Settings.Default.CityListPath))
            {
                XmlDocument result = new XmlDocument();
                if(CheckInternet())
                {
                    result.Load(webPath);
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
        //Проверка подключения к интернету
        public static bool CheckInternet()
        {
            WebClient client = new WebClient();
            try
            {
                client.DownloadString("http://www.ya.ru");
                logger.Debug("Проверка доступа в интернет прошла успешно");
                return true;
            }
            catch (WebException ex)
            {
                logger.Info(String.Format("Не удалось подключиться к интернету. {0}", ex.Message));
                return false;
            }
        }
        public static byte[] ProtectPassword(string src)
        {
            try
            {
                return ProtectedData.Protect(Encoding.UTF8.GetBytes(src),additionalEntropy,DataProtectionScope.CurrentUser);
            }
            catch(CryptographicException e)
            {
                logger.Debug(String.Format("Не удалось зашифровать данные: {0}",e.ToString()));
                return null;
            }
        }
        public static string UnprotectPassword(byte[] src)
        {
            try
            {
                return Encoding.UTF8.GetString(ProtectedData.Unprotect(src,additionalEntropy,DataProtectionScope.CurrentUser));
            }
            catch(CryptographicException e)
            {
                logger.Debug(String.Format("Не удалось расшифровать данные: {0}", e.ToString()));
                return null;
            }
        }
    }
}
