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
                logger.ErrorException("При обновлении прогноза произошла ошибка: ", e);
            }
        }

        private void button_load_Click(object sender, RoutedEventArgs e)
        {
            Weather.LoadWeather(city_id);
            RefreshTodayWeather();
        }

        private void button_settings_Click(object sender, RoutedEventArgs e)
        {
            logger.Trace("Нажата тестовая кнопка настроек");
            Weather weather = new Weather();
            try
            {
                if (Weather.BaseCheck())
                {
                    weather = Weather.Now();
                    weather.WriteToBase();
                }

            }
            catch (Exception ex)
            {
                logger.Error(String.Format("В методе '{0}' произошла ошибка: {1}, источник: {2}", ex.TargetSite, ex.Message, ex.Source));
            }
        }

        private void button_forecast_type_Click(object sender, RoutedEventArgs e)
        {
            if (GridWeatherDay.Visibility != Visibility.Hidden)
            {
                GridWeatherDay.Visibility = Visibility.Hidden;
                GridWeatherWeek.Visibility = Visibility.Visible;
                button_forecast_type.Content = "Прогноз на сегодня";
                RefreshWeekWeather(0);
                button_move_left.IsEnabled = false;
                button_move_right.IsEnabled = true;
            }
            else
            {
                GridWeatherWeek.Visibility = Visibility.Hidden;
                GridWeatherDay.Visibility = Visibility.Visible;
                button_forecast_type.Content = "Прогноз на 8 дней";
            }
        }

        private void button_last_days_Click(object sender, RoutedEventArgs e)
        {
            RefreshWeekWeather(4);
            button_move_right.IsEnabled = false;
            button_move_left.IsEnabled = true;
        }

        private void button_first_days_Click(object sender, RoutedEventArgs e)
        {
            RefreshWeekWeather(0);
            button_move_left.IsEnabled = false;
            button_move_right.IsEnabled = true;
        }

        private void ShowHideTray_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void MenuExitTray_Click(object sender, RoutedEventArgs e)
        {

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
