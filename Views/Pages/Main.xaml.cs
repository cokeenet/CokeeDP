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

        public Main()
        {
            InitializeComponent();
            BingWappSwitch.IsChecked = Properties.Settings.Default.BingWappEnable;
            UHDModeSwitch.IsChecked = Properties.Settings.Default.IsUHDWapp;
            SnowFlakeSwitch.IsChecked = Properties.Settings.Default.SnowEnable;
            folderBox.Text = Properties.Settings.Default.AudioFolder;
            timeBox.Text = (Convert.ToInt32(Properties.Settings.Default.OneWordsTimeInterval) / 60).ToString();
            cityName.Text = "当前选择:" + Properties.Settings.Default.City;
            oneWord.SelectedIndex = Properties.Settings.Default.OneWordsComboBoxIndex;
            countDownBox.Text = Properties.Settings.Default.CountdownName;
            countDownPicker.SelectedDate = Properties.Settings.Default.CountdownTime;
        }

        private void OnSwitchChecked(object sender, RoutedEventArgs e)
        {
        }

        private void TextBoxHandler(object sender, RoutedEventArgs e)
        {
            var textBox = sender as Wpf.Ui.Controls.TextBox;
            switch (textBox.Tag)
            {
                case "audioDir":
                    Properties.Settings.Default.AudioFolder = textBox.Text; break;
                case "time":
                    Properties.Settings.Default.OneWordsTimeInterval = (Convert.ToInt32(textBox.Text) * 60).ToString(); break;
                case "CountDownName":
                    Properties.Settings.Default.CountdownName = textBox.Text; break;
                default:
                    break;
            }
            Properties.Settings.Default.Save();
            _Window.snackbarService.ShowAsync("已保存", "(●'◡'●)", SymbolRegular.Save28);
        }

        private void CardAction_Click(object sender, RoutedEventArgs e)
        {
            _Window.RootFrame.Source = new Uri(@"pack://application:,,,/Views/Pages/GeoSearch.xaml");
        }

        private void SwitchEventsHandler(object sender, RoutedEventArgs e)
        {
            var toggleSwitch = (ToggleSwitch)sender;
            bool enable = (bool)toggleSwitch.IsChecked;
            if (toggleSwitch.IsChecked == null) enable = false;
            switch (toggleSwitch.Tag)
            {
                case "bing":
                    Properties.Settings.Default.BingWappEnable = enable; break;
                case "uhd":
                    Properties.Settings.Default.IsUHDWapp = enable; break;
                case "SnowFlake":
                    Properties.Settings.Default.SnowEnable = enable; break;
                default:
                    break;
            }
            _Window.snackbarService.ShowAsync("已保存", "(●'◡'●)", SymbolRegular.Save28);
            Properties.Settings.Default.Save();
        }

        private void DatePickerHandler(object sender, RoutedEventArgs e)
        {
            DatePicker datePicker = sender as DatePicker;
            if (datePicker == null) return;
            Properties.Settings.Default.CountdownTime = (DateTime)datePicker.SelectedDate;
            _Window.snackbarService.ShowAsync("已保存", "(●'◡'●)", SymbolRegular.Save28);
        }

        private void ComboBoxHandler(object sender, EventArgs e)
        {
            var comboBox = sender as ComboBox;
            var item = comboBox.SelectedItem as ComboBoxItem;
            if (item == null) return;
            if (item.Tag.ToString() == "none") Properties.Settings.Default.OneWordsApi = "https://v1.hitokoto.cn/";
            else Properties.Settings.Default.OneWordsApi = "https://v1.hitokoto.cn/?c=" + item.Tag;
            Properties.Settings.Default.OneWordsComboBoxIndex = comboBox.SelectedIndex;
            _Window.snackbarService.ShowAsync("已保存", "(●'◡'●)", SymbolRegular.Save28);
        }
    }
}