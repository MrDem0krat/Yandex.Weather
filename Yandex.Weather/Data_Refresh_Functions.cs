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
        // Обновление погоды на текущий день
        public void RefreshTodayWeather()
        {
            List<string> day_parts = new List<string>();
            day_parts.AddRange(new string[] {"morning", "day", "evening", "night" });
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
        public void RefreshWeekWeather(int _start_day)
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
                        RefreshDaypart(item, weather[(_start_day + day) * 4 + (part * 2 + 1)]);
                    }
                }
            }
        }

        // Обновление погоды на часть дня
        public void RefreshDaypart(object _item, Weather _weather)
        {
            Label label;
            Image image;
            List<string> dayparts_rus = new List<string>();
            dayparts_rus.AddRange(new string[] {"Утро", "День", "Вечер", "Ночь"});
            List<string> dayparts = new List<string>();
            dayparts.AddRange(new string[] { "morning", "day", "evening", "night" });

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
                    label.Content = String.Format("{0}, {1:dd}.{1:MM}", dayparts_rus[dayparts.IndexOf(_weather.PartOfDay)], _weather.Date);
                if (label.Name.StartsWith("last_refresh"))
                    label.Content = String.Format("Последнее обновление {0:dd}.{0:MM}.{0:yyyy} в {0:HH}:{0:mm}", DateTime.Now);
            }
            if (_item is Image)
            {
                image = _item as Image;
                image.Source = new BitmapImage(new Uri(String.Format("{0}{1}.png", img_path, _weather.Condition), UriKind.Relative));
                image.ToolTip = _weather.Condition;
            }
        }

        // Локализация вывода направления ветра
        string wind_direction_rus(string _src)
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