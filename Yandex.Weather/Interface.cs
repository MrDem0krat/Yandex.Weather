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
//using System.Drawing;
using WeatherLib;

namespace Yandex.Forecast
{
    public partial class MainWindow : Window
    {
        private SettingsWindow settingsWnd;
  
        // Перегрузка обработчиков событий сворачивания и закрытия главного окна
        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            if(this.WindowState == WindowState.Minimized)
            {
                Hide();
                (TrayMenu.Items[0] as MenuItem).Header = "Показать";
            }
            else
            {
                Settings.CurrentWindowState = WindowState;
            }
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            if (!Properties.Settings.Default.CanClose)
            {
                e.Cancel = true;
                Settings.CurrentWindowState = this.WindowState;
                (TrayMenu.Items[0] as MenuItem).Header = "Показать";
                Hide();
            }
            else
            {
                TrayIcon.Visible = false;
            }
        }

        public static void ShowHideMainWindow()
        {
            if (Application.Current.MainWindow.WindowState != WindowState.Minimized)
                Settings.CurrentWindowState = Application.Current.MainWindow.WindowState;
            TrayMenu.IsOpen = false;
            if (Application.Current.MainWindow.IsVisible)
            {
                Application.Current.MainWindow.Hide();
                (TrayMenu.Items[0] as MenuItem).Header = "Показать";
            }
            else
            {
                Application.Current.MainWindow.Show();
                (TrayMenu.Items[0] as MenuItem).Header = "Свернуть";
                Application.Current.MainWindow.WindowState = Settings.CurrentWindowState;
                Application.Current.MainWindow.Activate();
            }
        }
        
        #region Data Refresh functions
        // Обновление погоды на текущий день
        public void RefreshTodayWeather()
        {
            List<string> day_parts = new List<string>();
            day_parts.AddRange(new string[] { "morning", "day", "evening", "night" });
            List<Weather> weatherList;
            List<object> items;

            weatherList = Weather.ReadAll();
            city_name.Content = Weather.CityName();

            //_________Обновление погоды сейчас (fact)________________
            items = new List<object>();
            items.Add(img_info_today_now);
            items.Add(temperature_info_today_now);
            items.Add(wind_info_today_now);
            items.Add(pressure_info_today_now);
            items.Add(humidity_info_today_now);
            items.Add(weather_type_info_today_now);
            items.Add(last_refresh);
            foreach (object item in items)
            {
                RefreshDaypart(item, Weather.Now());
            }

            //_________Обновление прогноза на ближайшие сутки_________
            int day_time_now = day_parts.IndexOf(Weather.Now().PartOfDay) + 1;
            for (int part = 0; part < 4; part++)
            {
                items = new List<object>();
                items.Add(GridWeatherDay.FindName(String.Format("temperature_info_today_{0}", day_parts[part])));
                items.Add(GridWeatherDay.FindName(String.Format("wind_info_today_{0}", day_parts[part])));
                items.Add(GridWeatherDay.FindName(String.Format("pressure_info_today_{0}", day_parts[part])));
                items.Add(GridWeatherDay.FindName(String.Format("humidity_info_today_{0}", day_parts[part])));
                items.Add(GridWeatherDay.FindName(String.Format("weather_type_info_today_{0}", day_parts[part])));
                items.Add(GridWeatherDay.FindName(String.Format("daypart_info_today_{0}", day_parts[part])));
                items.Add(GridWeatherDay.FindName(String.Format("img_info_today_{0}", day_parts[part])));

                foreach (object item in items)
                {
                    RefreshDaypart(item, weatherList[part + day_time_now]);
                }
            }
        }

        // Обновление погоды на 4 дня
        public void RefreshWeekWeather(int start_day)
        {
            List<object> items;
            string[] _day_parts = { "day", "night" };
            List<Weather> weather;
            weather = Weather.ReadAll();

            for (int part = 0; part < 2; part++)
            {
                for (int day = 1; day <= 4; day++)
                {
                    items = new List<object>();
                    items.Add(GridWeatherWeek.FindName(String.Format("date_info_{0}_{1}", _day_parts[part], day)));
                    items.Add(GridWeatherWeek.FindName(String.Format("img_info_{0}_{1}", _day_parts[part], day)));
                    items.Add(GridWeatherWeek.FindName(String.Format("temperature_info_{0}_{1}", _day_parts[part], day)));
                    items.Add(GridWeatherWeek.FindName(String.Format("wind_info_{0}_{1}", _day_parts[part], day)));
                    items.Add(GridWeatherWeek.FindName(String.Format("pressure_info_{0}_{1}", _day_parts[part], day)));
                    items.Add(GridWeatherWeek.FindName(String.Format("humidity_info_{0}_{1}", _day_parts[part], day)));
                    items.Add(GridWeatherWeek.FindName(String.Format("weather_type_info_{0}_{1}", _day_parts[part], day)));

                    foreach (object item in items)
                    {
                        RefreshDaypart(item, weather[(start_day + day) * 4 + (part * 2 + 1)]);
                    }
                }
            }
        }

        // Обновление погоды на часть дня
        public void RefreshDaypart(object item, Weather weather)
        {
            Label label;
            Image image;
            List<string> dayparts_rus = new List<string>();
            dayparts_rus.AddRange(new string[] { "Утро", "День", "Вечер", "Ночь" });
            List<string> dayparts = new List<string>();
            dayparts.AddRange(new string[] { "morning", "day", "evening", "night" });

            if (item is Label)
            {
                label = item as Label;
                if (label.Name.StartsWith("temperature"))
                {
                    if (weather.Temperature > 0)
                        label.Content = String.Format("+{0}°C", weather.Temperature);
                    else
                        label.Content = String.Format("{0}°C", weather.Temperature);
                }
                if (label.Name.StartsWith("wind"))
                    label.Content = String.Format("Ветер: {0}, {1} м/с", wind_direction_rus(weather.WindDirection), weather.WindSpeed);
                if (label.Name.StartsWith("pressure"))
                    label.Content = String.Format("Давление: {0} мм.рт.ст.", weather.Pressure);
                if (label.Name.StartsWith("humidity"))
                    label.Content = String.Format("Влажность: {0}%", weather.Humidity);
                if (label.Name.StartsWith("weather_type"))
                {
                    label.Content = weather.TypeShort;
                    label.ToolTip = weather.Type;
                }
                if (label.Name.StartsWith("daypart") || label.Name.StartsWith("date"))
                    label.Content = String.Format("{0}, {1:dd}.{1:MM}", dayparts_rus[dayparts.IndexOf(weather.PartOfDay)], weather.Date);
                if (label.Name.StartsWith("last_refresh"))
                    label.Content = String.Format("Последнее обновление {0:dd}.{0:MM}.{0:yyyy} в {0:HH}:{0:mm}", DateTime.Now);
            }
            if (item is Image)
            {
                image = item as Image;
                image.Source = new BitmapImage(new Uri(String.Format("{0}{1}.png", Properties.Settings.Default.ImagesPath, weather.Condition), UriKind.Relative));
                image.ToolTip = weather.Condition;
            }
        }

        // Локализация вывода направления ветра
        string wind_direction_rus(string src)
        {
            switch (src)
            {
                case "n":
                    return "↓С";
                case "ne":
                    return "↙СВ";
                case "e":
                    return "←В";
                case "se":
                    return "↖ЮВ";
                case "s":
                    return "↑Ю";
                case "sw":
                    return "↗ЮЗ";
                case "w":
                    return "→З";
                case "nw":
                    return "↘СЗ";
                default:
                    return "";
            }
        }
        #endregion
        #region Buttons
        //Обработчики нажатий кнопок главного окна
        private void button_load_Click(object sender, RoutedEventArgs e)
        {
            Weather.LoadWeather(Properties.Settings.Default.CityID);
            RefreshTodayWeather();
        }
        private void button_settings_Click(object sender, RoutedEventArgs e)
        {
            settingsWnd = new SettingsWindow();
            if(settingsWnd.ShowDialog() == true)
            {
                Properties.Settings.Default.CityName = settingsWnd.cityNameBox.SelectedItem.ToString();
                Properties.Settings.Default.CityID = settingsWnd.cityNameBox.SelectedValue.ToString();
                Properties.Settings.Default.RefreshPeriodID = settingsWnd.RefreshPeriodBox.SelectedIndex;
                Properties.Settings.Default.CanClose = !settingsWnd.MinimazeTrayBox.IsChecked.Value;
                Properties.Settings.Default.DataBaseUser = settingsWnd.LoginBox.Text;
                Properties.Settings.Default.DataBaseServer = settingsWnd.ServerBox.Text;
                Properties.Settings.Default.DataBasePort = settingsWnd.PortBox.Text;
                if (settingsWnd.PasswordBox.Password != "    ")
                    Properties.Settings.Default.DataBasePassword = System.Convert.ToBase64String(Settings.ProtectPassword(settingsWnd.PasswordBox.Password));
                Properties.Settings.Default.Save();
            }
            else
            {
                Settings.CityName = Properties.Settings.Default.CityName;
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
        // Обработчики нажатий пунков контекстного меню иконки в трее
        private void ShowHideTray_Click(object sender, RoutedEventArgs e)
        {
            ShowHideMainWindow();
        }
        private void MenuExitTray_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.CanClose = true;
            Close();
        }
        private void test_button_Click(object sender, RoutedEventArgs e)
        {
            logger.Trace("Нажата тестовая кнопка настроек");
            Weather weather = new Weather();
            Weather.DataBaseUser = Properties.Settings.Default.DataBaseUser;
            Weather.DataBasePassword = Settings.UnprotectPassword(System.Convert.FromBase64String(Properties.Settings.Default.DataBasePassword));
            Weather.DataBaseServer = Properties.Settings.Default.DataBaseServer;
            Weather.DataBasePort = Properties.Settings.Default.DataBasePort;
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
                logger.Error(String.Format("В тестовом методе '{0}' произошла ошибка: {1}, источник: {2}", ex.TargetSite, ex.Message, ex.Source));
            }
        }
        #endregion
    }
}
