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

namespace Yandex.Forecast
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public static System.Windows.Forms.NotifyIcon TrayIcon = new System.Windows.Forms.NotifyIcon();
        public static ContextMenu TrayMenu = new ContextMenu();
        
        public MainWindow()
        {
            InitializeComponent();
            logger.Info("Приложение запущено");
            Settings.LoggerConfig();
            TrayIcon = Settings.TrayIconConfig();
            TrayMenu = Resources["TrayMenu"] as ContextMenu;
            GridWeatherWeek.Visibility = Visibility.Hidden;
            logger.Debug("Настройки успешно загружены");
            RefreshWeather();
        }
    }
}
