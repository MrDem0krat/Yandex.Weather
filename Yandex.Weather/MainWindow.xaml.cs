using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NLog;

namespace Yandex.Forecast
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        public static System.Windows.Forms.NotifyIcon TrayIcon = new System.Windows.Forms.NotifyIcon();
        public static ContextMenu TrayMenu = new ContextMenu();
        public static SplashScreen splash = new SplashScreen("content/graphics/Screen.png");
        
        public MainWindow()
        {
            Application.Current.MainWindow.Hide();
            InitializeComponent();
            Yandex.Forecast.MainWindow.TrayIcon = Settings.TrayIconConfig();
            Yandex.Forecast.MainWindow.TrayMenu = Resources["TrayMenu"] as ContextMenu;
            GridWeatherWeek.Visibility = Visibility.Hidden;
            Yandex.Forecast.MainWindow.logger.Debug("Настройки успешно загружены");
            Task.Factory.StartNew(async () =>
                { 
                    await RefreshAsync();
                    splash.Close(TimeSpan.FromSeconds(0.5));
                    Dispatcher.Invoke(() => Application.Current.MainWindow.Show());
                });
        }
    }
}
