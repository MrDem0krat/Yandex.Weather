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
using System;
using System.Collections.Generic;

namespace Yandex.Forecast
{
    // Выпилить файл
    public partial class MainWindow : Window
    {
        
        public bool funWithEx3()
        {
            Weather weather = new Weather();
            try 
            {
                if(Weather.BaseCheck())
                {
                    weather = Weather.Now();
                    weather.WriteToBase();
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("Метод: {0}, Ошибка: {1}, Источник: {2}",ex.TargetSite, ex.Message, ex.Source));
            }
            return true;
        }
    }
}
