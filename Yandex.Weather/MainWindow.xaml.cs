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
            Settings.LoggerConfig();
            Settings.TrayIconConfig();
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

    }
}
