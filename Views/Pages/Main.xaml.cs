using System;
using System.Collections.Generic;
using System.Configuration;
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
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Wpf.Ui.Mvvm.Services;
using CokeeDP.Views.Windows;
using Wpf.Ui.Common;
using CokeeDP.Properties;

namespace CokeeDP.Views.Pages
{
    /// <summary>
    /// Main.xaml 的交互逻辑
    /// </summary>
    public partial class Main : UiPage
    {
        private SettingsWindow _Window = Application.Current.Windows
            .Cast<Window>()
            .FirstOrDefault(window => window is SettingsWindow) as SettingsWindow;
        public AppSettings settings = AppSettingsExtensions.LoadSettings();
        public Main()
        {
            InitializeComponent();
            BingWappSwitch.IsChecked = settings.BingWappEnable;
            BingVideoSwitch.IsChecked = settings.BingVideoEnable;
            UHDModeSwitch.IsChecked = settings.UHDEnable;
            SnowFlakeSwitch.IsChecked = settings.SnowEnable;
            folderBox.Text = settings.AudioFolder;
            timeBox.Text = (Convert.ToInt32(settings.OneWordsTimeInterval) / 60).ToString();
            cityName.Text = "当前选择:" + settings.City;
            oneWord.SelectedIndex = settings.OneWordsComboBoxIndex;
            countDownBox.Text = settings.CountdownName;
            countDownPicker.SelectedDate = settings.CountdownTime;

        }

        private void OnSwitchChecked(object sender, RoutedEventArgs e)
        {
        }

        private void TextBoxHandler(object sender, RoutedEventArgs e)
        {
            var textBox = sender as Wpf.Ui.Controls.TextBox;
            try
            {
                
                if (textBox.Text.Length < 1 || textBox.Text == null) return;
                switch (textBox.Tag)
                {
                    case "audioDir":
                        settings.AudioFolder = textBox.Text; break;
                    case "time":
                        settings.OneWordsTimeInterval = (Convert.ToInt32(textBox.Text) * 60).ToString(); break;
                    case "CountDownName":
                        settings.CountdownName = textBox.Text; break;
                    case "audioTime":
                        settings.AudioAutoPlayTime = TimeOnly.Parse(textBox.Text); break;
                    default:
                        break;
                }
                AppSettingsExtensions.SaveSettings(settings);
                _Window.snackbarService.ShowAsync("已保存", "(●'◡'●)", SymbolRegular.Save28);
            }
            catch (Exception ex)
            {
                _Window.snackbarService.ShowAsync("Error:", ex.ToString(), SymbolRegular.Save28);
                textBox.Text = null;
            }

        }

        private void CardAction_Click(object sender, RoutedEventArgs e)
        {
            AppSettingsExtensions.SaveSettings(settings);
            this.NavigationService.Navigate(new Uri(@"pack://application:,,,/Views/Pages/GeoSearch.xaml"));
            //_Window.RootFrame.Source = new Uri(@"pack://application:,,,/Views/Pages/GeoSearch.xaml");
        }

        private void SwitchEventsHandler(object sender, RoutedEventArgs e)
        {
            var toggleSwitch = (ToggleSwitch)sender;
            bool enable = (bool)toggleSwitch.IsChecked;
            if (toggleSwitch.IsChecked == null) enable = false;
            switch (toggleSwitch.Tag)
            {
                case "bing":
                    settings.BingWappEnable = enable; break;
                case "bingVideo":
                    settings.BingVideoEnable = enable; break;
                case "uhd":
                    settings.UHDEnable = enable; break;
                case "SnowFlake":
                    settings.SnowEnable = enable; break;
                default:
                    break;
            }
            _Window.snackbarService.ShowAsync("已保存", "(●'◡'●)", SymbolRegular.Save28);
            AppSettingsExtensions.SaveSettings(settings);
        }

        private void DatePickerHandler(object sender, RoutedEventArgs e)
        {
            DatePicker datePicker = sender as DatePicker;
            if (datePicker == null) return;
            settings.CountdownTime = (DateTime)datePicker.SelectedDate;
            AppSettingsExtensions.SaveSettings(settings);
            _Window.snackbarService.ShowAsync("已保存", "(●'◡'●)", SymbolRegular.Save28);
        }

        private void ComboBoxHandler(object sender, EventArgs e)
        {
            var comboBox = sender as ComboBox;
            var item = comboBox.SelectedItem as ComboBoxItem;
            if (item == null) return;
            if (item.Tag.ToString() == "none") settings.OneWordsApi = "https://v1.hitokoto.cn/";
            else settings.OneWordsApi = "https://v1.hitokoto.cn/?c=" + item.Tag;
            settings.OneWordsComboBoxIndex = comboBox.SelectedIndex;
            AppSettingsExtensions.SaveSettings(settings);
            
            _Window.snackbarService.ShowAsync("已保存", "(●'◡'●)", SymbolRegular.Save28);
        }
    }
}