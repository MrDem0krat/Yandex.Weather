using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WeatherLib;
using MySql.Data.MySqlClient;
using NLog;
using NLog.Config;
using NLog.Targets;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Drawing;


namespace Yandex.Forecast
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string city_id = "26894";
        private string img_path = "content/graphics/weather_icons/";
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        
        public MainWindow()
        {
            InitializeComponent();
            LoggerConfig();
            CreateTrayIcon();
            GridWeatherWeek.Visibility = Visibility.Hidden;
            logger.Info("Приложение запущено");
            try
            {
                Weather.LoadWeather(city_id);
                RefreshTodayWeather();
                logger.Trace("Погода успешно обновлена.");
            }
            catch (Exception e)
            {
                logger.ErrorException("При обновлении прогноза произошла ошибка: ", e); //Исправить
            }
        }

        //Настройка логгера
        private void LoggerConfig()
        {
            LoggingConfiguration config = new LoggingConfiguration();

            FileTarget fileTarget = new FileTarget();
            config.AddTarget("file", fileTarget);

            fileTarget.DeleteOldFileOnStartup = true;
            fileTarget.KeepFileOpen = false;
            fileTarget.ConcurrentWrites = true;
            fileTarget.Encoding = Encoding.Unicode;
            fileTarget.ArchiveEvery = FileArchivePeriod.Day;
            fileTarget.Layout = NLog.Layouts.Layout.FromString("${longdate} | ${uppercase:${level}} | ${message}");
            fileTarget.FileName = NLog.Layouts.Layout.FromString("${basedir}/logs/${shortdate}.log");
            fileTarget.ArchiveFileName = NLog.Layouts.Layout.FromString("${basedir}/logs/archives/{shortdate}.rar");

            LoggingRule ruleFile = new LoggingRule("*", LogLevel.Trace, fileTarget);
            config.LoggingRules.Add(ruleFile);

            LogManager.Configuration = config;
        }
    }
}
