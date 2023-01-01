using CokeeDP.Views.Windows;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
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
using Wpf.Ui.Controls;
using Wpf.Ui.Mvvm.Services;
using Button = Wpf.Ui.Controls.Button;
using System.Threading;
using System.Timers;
using System.Windows.Interop;
using Wpf.Ui.Common;
using Timer = System.Timers.Timer;

namespace CokeeDP.Views.Pages
{
    /// <summary>
    /// GeoSearch.xaml 的交互逻辑
    /// </summary>
    public class City
    {
        public string CityName
        {
            get; set;
        }

        public string CityCode
        {
            get; set;
        }

        public string Desc
        {
            get; set;
        }
    }

    public partial class GeoSearch : UiPage
    {
        private SettingsWindow _Window = Application.Current.Windows
             .Cast<Window>()
             .FirstOrDefault(window => window is SettingsWindow) as SettingsWindow;

        protected Timer timer = new Timer(30000);
        protected bool CanSearch = true;

        public GeoSearch()
        {
            InitializeComponent();
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = false;
        }

        private void Timer_Elapsed(object sender,ElapsedEventArgs e) => timer.Enabled = false;

        private void Button_Click(object sender,RoutedEventArgs e)
        {
            _Window.RootFrame.Source = new Uri(@"pack://application:,,,/Views/Pages/Main.xaml");
        }

        private async Task GetGeoInfo()
        {
            try
            {
                string u2;
                var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };
                var client = new HttpClient(handler);
                u2 = await client.GetStringAsync("https://geoapi.qweather.com/v2/city/lookup?location=" + searchBox.Text + "&key=6572127bcec647faba394b17fbd9614f");
                JObject dt = JsonConvert.DeserializeObject<JObject>(u2);
                if(dt["code"].ToString() != "200") throw new HttpRequestException("获取信息失败，网络异常。");
                foreach(var item in dt["location"])
                {
                    CityList.Items.Add(new City()
                    {
                        CityName = item["name"].ToString(),
                        CityCode = item["id"].ToString() + "|" + item["country"].ToString() + " " + item["adm1"].ToString() + " " + item["adm2"].ToString(),
                        Desc = item["country"].ToString() + " " + item["adm1"].ToString() + item["adm2"].ToString() + "  时区:" + item["tz"].ToString()
                    });
                }
            }
            catch(Exception ex)
            {
                _Window.ProcessErr(ex);
            }
        }

        private void SetCurrentGeoId(object sender,RoutedEventArgs e)
        {
            try
            {
                Button button = sender as Button;
                Properties.Settings.Default.CityId = button.Tag.ToString().Split("|")[0];
                Properties.Settings.Default.City = button.Tag.ToString().Split("|")[1];
                Properties.Settings.Default.CachedWeatherData = "";
                Properties.Settings.Default.CachedWeatherTime = DateTime.Parse("2000/01/01");
                _Window.snackbarService.ShowAsync("设置成功","已更换城市为" + Properties.Settings.Default.City,SymbolRegular.Checkmark32);
            }
            catch(Exception ex)
            {
                _Window.ProcessErr(ex);
            }
        }

        private void SearchBtnHandle(object sender,RoutedEventArgs e)
        {
            // c = new System.Timers.Timer(ms2 * 1000); c.Elapsed += new ElapsedEventHandler(OnWea);
            // c.AutoReset = true; c.Enabled = true;
            try
            {
                if(!timer.Enabled)
                {
                    _ = GetGeoInfo();
                    timer.Enabled = true;
                }
                else
                {
                    _Window.snackbarService.ShowAsync("搜索频率太快了!","由于我没有钱钱，30秒内只能搜索一次。多多谅解喵",SymbolRegular.ClockAlarm24);
                }
            }
            catch(Exception ex)
            {
                _Window.ProcessErr(ex);
            }
        }
    }
}