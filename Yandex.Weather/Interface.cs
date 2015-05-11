using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
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
                Weather.SaveAuthData();
                Properties.Settings.Default.Save();
                TrayIcon.Visible = false;
                logger.Info("Приложение закрыто");
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
        #region DataRefreshAsync
        // Обновление отображаемого прогноза погоды
        private async Task RefreshAsync()
        {
            Dispatcher.Invoke(() => RotateStatusImg(true));
            await Weather.LoadAsync();
            await RefreshTodayAsync();
            if (Dispatcher.Invoke(() => { return btnMoveRight.IsEnabled; }))
                await RefreshWeekAsync(0);
            else
                await RefreshWeekAsync(4);
            Dispatcher.Invoke(() => RotateStatusImg(false));
        } 
        // Обновление погоды на ближайшие сутки
        private async Task RefreshTodayAsync()
        {
            List<Weather> weather; //Weather forecast
            List<object> IItems; //Interface Items
            Weather now = await Weather.NowAsync();
            int DayPartNow = Weather.DayPart.IndexOf(now.PartOfDay) + 1;
            weather = await Weather.ReadAllAsync();
            Dispatcher.Invoke(() => { lblCityName.Content = Weather.CityName; });

            // Обновление прогноза на текущий момент времени 
            IItems = new List<object>();
            IItems.Add(imgWeatherNow);
            IItems.Add(lblTemperatureNow);
            IItems.Add(lblWindNow);
            IItems.Add(lblPressureNow);
            IItems.Add(lblHumidityNow);
            IItems.Add(lblWeatherTypeNow);
            IItems.Add(lblLastRefresh);
            Dispatcher.Invoke(() =>
                {
                    foreach (object item in IItems)
                        RefreshDaypart(item, now);

                    //Обновление прогноза на ближайшие сутки
                    for (int part = 0; part < 4; part++)
                    {
                        IItems = new List<object>();
                        
                        IItems.Add(GridWeatherDay.FindName(String.Format("lblTemperatureToday_{0}", part)));
                        IItems.Add(GridWeatherDay.FindName(String.Format("lblWindToday_{0}", part)));
                        IItems.Add(GridWeatherDay.FindName(String.Format("lblPressureToday_{0}", part)));
                        IItems.Add(GridWeatherDay.FindName(String.Format("lblHumidityToday_{0}", part)));
                        IItems.Add(GridWeatherDay.FindName(String.Format("lblWeatherTypeToday_{0}", part)));
                        IItems.Add(GridWeatherDay.FindName(String.Format("lblDaypartToday_{0}", part)));
                        IItems.Add(GridWeatherDay.FindName(String.Format("imgWeatherToday_{0}", part)));
                        foreach (object item in IItems)
                        {
                            RefreshDaypart(item, weather[part + DayPartNow]);
                        }
                    }
                });
            logger.Trace("Погода успешно обновлена");
        } 
        // Обновление погоды на неделю
        private async Task RefreshWeekAsync(int StartFromDay)
        {
            List<object> IItems;
            List<Weather> weather;
            weather = await Weather.ReadAllAsync();
            Dispatcher.Invoke(() =>
                {
                    for (int part = 0; part < 2; part++)
                    {
                        for (int day = 0; day < 4; day++)
                        {
                            IItems = new List<object>();
                            IItems.Add(GridWeatherWeek.FindName(String.Format("lblDate{0}_{1}", Weather.DayPart.ValueOf(part * 2 + 1), day)));
                            IItems.Add(GridWeatherWeek.FindName(String.Format("imgWeather{0}_{1}", Weather.DayPart.ValueOf(part * 2 + 1), day)));
                            IItems.Add(GridWeatherWeek.FindName(String.Format("lblTemperature{0}_{1}", Weather.DayPart.ValueOf(part * 2 + 1), day)));
                            IItems.Add(GridWeatherWeek.FindName(String.Format("lblWind{0}_{1}", Weather.DayPart.ValueOf(part * 2 + 1), day)));
                            IItems.Add(GridWeatherWeek.FindName(String.Format("lblPressure{0}_{1}", Weather.DayPart.ValueOf(part * 2 + 1), day)));
                            IItems.Add(GridWeatherWeek.FindName(String.Format("lblHumidity{0}_{1}", Weather.DayPart.ValueOf(part * 2 + 1), day)));
                            IItems.Add(GridWeatherWeek.FindName(String.Format("lblWeatherType{0}_{1}", Weather.DayPart.ValueOf(part * 2 + 1), day)));
                            foreach (object item in IItems)
                            {
                                RefreshDaypart(item, weather[(StartFromDay + day) * 4 + (part * 2 + 1)]);
                            }
                        }
                    }
                });
            logger.Debug("Погода на неделю успешно обновлена");
        }
        // Обновление погоды на часть дня
        private void RefreshDaypart(object item, Weather weather)
        {
            Label label;
            Image image;
            
            if (item is Label)
            {
                label = item as Label;
                if (label.Name.StartsWith("lblTemperature"))
                {
                    if (weather.Temperature > 0)
                        label.Content = String.Format("+{0}°C", weather.Temperature);
                    else
                        label.Content = String.Format("{0}°C", weather.Temperature);
                }
                if (label.Name.StartsWith("lblWind"))
                    label.Content = String.Format("Ветер: {0}, {1} м/с", Weather.WindDirectionRus(weather.WindDirection), weather.WindSpeed);
                if (label.Name.StartsWith("lblPressure"))
                    label.Content = String.Format("Давление: {0} мм.рт.ст.", weather.Pressure);
                if (label.Name.StartsWith("lblHumidity"))
                    label.Content = String.Format("Влажность: {0}%", weather.Humidity);
                if (label.Name.StartsWith("lblWeatherType"))
                {
                    label.Content = weather.TypeShort;
                    label.ToolTip = weather.Type;
                }
                if (label.Name.StartsWith("lblDaypart") || label.Name.StartsWith("lblDate"))
                    label.Content = String.Format("{0}, {2:dd}.{2:MM} ({1})", Weather.GetRusDayOfWeek(weather.Date.DayOfWeek), Weather.DayPartRus.Convert(weather.PartOfDay), weather.Date);
                if (label.Name.StartsWith("lblLastRefresh"))
                    label.Content = String.Format("Последнее обновление {0:dd}.{0:MM}.{0:yyyy} в {0:HH}:{0:mm}", DateTime.Now);
            }
            if (item is Image)
            {
                image = item as Image;
                image.Source = new BitmapImage(new Uri(String.Format("{0}{1}.png", Properties.Settings.Default.ImagesPath, weather.Condition), UriKind.Relative));
                image.ToolTip = weather.Condition;
            }
        }
        //Вращение изображения отображающего статус обновления
        private void RotateStatusImg(bool Rotate, bool isError = false)
        {
            if (Rotate)
            {
                imgStatusRefresh.Source = new BitmapImage(new Uri("pack://siteoforigin:,,,/Resources/refresh.png", UriKind.RelativeOrAbsolute));
                DoubleAnimation dAnimation = new DoubleAnimation(0, 360, new Duration(TimeSpan.FromSeconds(3)));
                RotateTransform RotateTransform = new RotateTransform();
                imgStatusRefresh.RenderTransform = RotateTransform;
                imgStatusRefresh.RenderTransformOrigin = new Point(0.5, 0.5);
                dAnimation.RepeatBehavior = RepeatBehavior.Forever;
                RotateTransform.BeginAnimation(RotateTransform.AngleProperty, dAnimation);
            }
            else
            {
                imgStatusRefresh.Source = new BitmapImage(new Uri("pack://siteoforigin:,,,/Resources/check.png", UriKind.RelativeOrAbsolute));
                imgStatusRefresh.RenderTransform = new RotateTransform();
            }
        } 
        #endregion
        #region Buttons
        //Обработчики нажатий кнопок главного окна
        private void button_load_Click(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() => RefreshAsync());
        }
        private void button_settings_Click(object sender, RoutedEventArgs e)
        {
            settingsWnd = new SettingsWindow();
            if(settingsWnd.ShowDialog() == true) // При нажатии ОК сохранаяем все настройки
            {
                Weather.CityName = settingsWnd.CityName.SelectedItem.ToString();
                Weather.CityID = settingsWnd.CityName.SelectedValue.ToString();
                Weather.CityNameEng = Weather.ReadCityNameEng();
                Properties.Settings.Default.RefreshPeriod = Settings.RefreshPeriod.FindName(settingsWnd.RefreshPeriod.SelectedIndex);
                Properties.Settings.Default.CanClose = !settingsWnd.isMinimazeToTray.IsChecked.Value;
                WeatherDatabase.User = settingsWnd.Login.Text;
                WeatherDatabase.Server = settingsWnd.Server.Text;
                WeatherDatabase.Port = uint.Parse(settingsWnd.Port.Text);
                if (settingsWnd.Password.Password != "    ")
                    WeatherDatabase.Password = settingsWnd.Password.Password;
                Properties.Settings.Default.Save();
            }
        }
        private void button_forecast_type_Click(object sender, RoutedEventArgs e)
        {
            if (GridWeatherDay.Visibility != Visibility.Hidden)
            {
                GridWeatherDay.Visibility = Visibility.Hidden;
                GridWeatherWeek.Visibility = Visibility.Visible;
                btnForecastType.Content = "Прогноз на сегодня";
                btnMoveLeft.IsEnabled = false;
                btnMoveRight.IsEnabled = true;
            }
            else
            {
                GridWeatherWeek.Visibility = Visibility.Hidden;
                GridWeatherDay.Visibility = Visibility.Visible;
                btnForecastType.Content = "Прогноз на 8 дней";
            }
        }
        private void button_last_days_Click(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() => RefreshWeekAsync(4));
            btnMoveRight.IsEnabled = false;
            btnMoveLeft.IsEnabled = true;
        }
        private void button_first_days_Click(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() => RefreshWeekAsync(0));
            btnMoveLeft.IsEnabled = false;
            btnMoveRight.IsEnabled = true;
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
            RotateStatusImg(true);
            Task.Factory.StartNew(async () =>
                {
                    Weather weather = new Weather();
                    if (await WeatherDatabase.CheckAsync())
                    {
                        weather = await Weather.NowAsync();
                        await WeatherDatabase.WriteAsync(weather);
                    }
                    Dispatcher.Invoke(() => RotateStatusImg(false));
                });
        }
        #endregion
    }
}
