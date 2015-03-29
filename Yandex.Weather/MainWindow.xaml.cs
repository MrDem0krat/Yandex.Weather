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


namespace Yandex.Forecast
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string city_id = "26894";
        private string img_path = "content/graphics/weather_icons/";
        public MainWindow()
        {
            InitializeComponent();
            GridWeatherWeek.Visibility = Visibility.Hidden;
            Weather.LoadWeather(city_id);
            RefreshTodayWeather();
        }

        private void button_load_Click(object sender, RoutedEventArgs e)
        {
            Weather.LoadWeather(city_id);
            RefreshTodayWeather();
        }

        private void button_settings_Click(object sender, RoutedEventArgs e)
        {

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
    }
}
