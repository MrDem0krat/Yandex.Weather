using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Yandex.Forecast
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Yandex.Forecast.MainWindow.splash.Show(false, true);
            Settings.LoggerConfig();
            Yandex.Forecast.MainWindow.logger.Info("Приложение запущено");
        }
    }
}
