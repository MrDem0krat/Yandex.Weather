using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WeatherLib;

namespace Yandex.Forecast
{
    public partial class MainWindow : Window
    {
        public void RefreshTodayWeather()// Обновление погоды на текущий день
        {
            string[] _day_parts = { "morning", "day", "evening", "night" };
            string[] _day_parts_rus = { "Утро", "День", "Вечер", "Ночь" };
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
                RefreshDaypart(item, 0, Weather.Now());
            }

            //_________Обновление прогноза на ближайшие сутки_________
            int day_time_now = 0;
            if (DateTime.Now.Hour >= 6)
                if (DateTime.Now.Hour >= 12)
                    if (DateTime.Now.Hour >= 18)
                        day_time_now = 3;
                    else
                        day_time_now = 2;
                else
                    day_time_now = 1;
            else
                day_time_now = 4;

            for (int i = 0; i < 4; i++)
            {
                items = new List<object>();
                items.Add(GridWeatherDay.FindName(String.Format("temperature_info_today_{0}", _day_parts[i])));
                items.Add(GridWeatherDay.FindName(String.Format("wind_info_today_{0}", _day_parts[i])));
                items.Add(GridWeatherDay.FindName(String.Format("pressure_info_today_{0}", _day_parts[i])));
                items.Add(GridWeatherDay.FindName(String.Format("humidity_info_today_{0}", _day_parts[i])));
                items.Add(GridWeatherDay.FindName(String.Format("weather_type_info_today_{0}", _day_parts[i])));
                items.Add(GridWeatherDay.FindName(String.Format("daypart_info_today_{0}", _day_parts[i])));
                items.Add(GridWeatherDay.FindName(String.Format("img_info_today_{0}", _day_parts[i])));

                foreach (object item in items)
                {
                    RefreshDaypart(item, (i + day_time_now) % 4, weatherList[i + day_time_now]); // Добавить в класс метод возвращающий погоду в заданный день в заданную часть дня
                }
            }
        }

        public void RefreshWeekWeather(int _start_day)// Обновление погоды на 4 дня
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
                        RefreshDaypart(item, part * 2 + 1, weather[(_start_day + day) * 4 + (part * 2 + 1)]);
                    }

                }
            }
        }

        public void RefreshDaypart(object _item, int _day_part_num, Weather _weather)// Обновление погоды на часть дня
        {
            Label label;
            Image image;
            string[] dayparts_rus = { "Утро", "День", "Вечер", "Ночь" };

            if (_item is Label)
            {
                label = _item as Label;
                if (label.Name.StartsWith("temperature"))
                {
                    if (_weather.Temperature > 0)
                        label.Content = String.Format("+{0}°C", _weather.Temperature);
                    else
                        label.Content = String.Format("{0}°C", _weather.Temperature);
                }
                if (label.Name.StartsWith("wind"))
                    label.Content = String.Format("Ветер: {0}, {1} м/с", wind_direction_rus(_weather.WindDirection), _weather.WindSpeed);
                if (label.Name.StartsWith("pressure"))
                    label.Content = String.Format("Давление: {0} мм.рт.ст.", _weather.Pressure);
                if (label.Name.StartsWith("humidity"))
                    label.Content = String.Format("Влажность: {0}%", _weather.Humidity);
                if (label.Name.StartsWith("weather_type"))
                {
                    label.Content = _weather.TypeShort;
                    label.ToolTip = _weather.Type;
                }
                if (label.Name.StartsWith("daypart") || label.Name.StartsWith("date"))
                    label.Content = String.Format("{0}, {1:dd}.{1:MM}", dayparts_rus[_day_part_num % 4], _weather.Date);
                if (label.Name.StartsWith("last_refresh"))
                    label.Content = String.Format("Последнее обновление {0:dd}.{0:MM}.{0:yyyy} в {0:HH}:{0:mm}", _weather.Date);
            }

            if (_item is Image)
            {
                image = _item as Image;
                image.Source = new BitmapImage(new Uri(String.Format("{0}{1}.png", img_path, _weather.Condition), UriKind.Relative));
                image.ToolTip = _weather.Condition;
            }
        }

        string wind_direction_rus(string _src)// Локализация вывода направления ветра
        {
            switch (_src)
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
    }
}