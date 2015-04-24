using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Text.RegularExpressions;
using System.IO;

namespace Yandex.Forecast
{
    /// <summary>
    /// Логика взаимодействия для Settings.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        struct City
        {
            public string Name { get; set; }
            public string ID { get; set; }
            public override string ToString()
            {
                return Name;
            }
        }

        public SettingsWindow()
        {
            InitializeComponent();
        }
        
        //Обработчики нажатий на кнопки окна настроек 
        private void Ok_Click(object sender, RoutedEventArgs e) // Проверка ввода данных
        {
            DialogResult = true;
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ShowCityList();
            cityNameBox.Text = WeatherLib.Weather.CityName;
            RefreshPeriodBox.SelectedIndex = Settings.RefreshPeriod.FindID(Properties.Settings.Default.RefreshPeriod);
            MinimazeTrayBox.IsChecked = !Properties.Settings.Default.CanClose;
            LoginBox.Text = WeatherLib.WeatherDatabase.User;
            PasswordBox.Password = "    ";
            ServerBox.Text = WeatherLib.WeatherDatabase.Server;
            PortBox.Text = WeatherLib.WeatherDatabase.Port.ToString();
        }
        
        // Функция поиска города
        private List<City> FindCityList()
        {
            StringBuilder text = new StringBuilder();
            List<City> result = new List<City>();
            if(Settings.IsCityListLoaded())
            {
                Regex findString = new Regex(@"(?=\s*)<.*(?=\<)", RegexOptions.IgnoreCase); // Выборка каждой строки, описывающей город
                Regex findName = new Regex(@"(?<=\>).*", RegexOptions.IgnoreCase); // Поиск названия города в строке
                Regex findID = new Regex("(?<=id=\").*?(?=\")", RegexOptions.IgnoreCase); //поиск id города в строке

                using(StreamReader reader = File.OpenText(Properties.Settings.Default.CityListPath))
                {
                    string line;
                    do
                    {
                        line = reader.ReadLine();
                        text.AppendLine(line);
                    } while (line != null);
                    reader.Close();
                }

                MatchCollection matches = findString.Matches(text.ToString());
                City city;
                foreach(Match match in matches)
                {
                    city = new City();
                    city.Name = findName.Match(match.Value).Value;
                    city.ID = findID.Match(match.Value).Value;
                    result.Add(city);
                }
            }
            return result;
        }

        // Загрузка в CityNameBox списка городов
        private void ShowCityList()
        {
            List<City> list = FindCityList();
            cityNameBox.Items.Clear();
            cityNameBox.SelectedValuePath = "ID";
            cityNameBox.ItemsSource = list;
        }

    }
}
