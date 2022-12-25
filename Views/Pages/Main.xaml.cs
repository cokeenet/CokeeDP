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
            folderBox.Text = Properties.Settings.Default.AudioFolder;
            timeBox.Text = (Convert.ToInt32(Properties.Settings.Default.OneWordsTimeInterval) / 60).ToString();
            cityName.Text = "当前选择:" + Properties.Settings.Default.City;
        }

        private void OnSwitchChecked(object sender,RoutedEventArgs e)
        {
            var toggleSwitch = (ToggleSwitch)sender;
            switch(toggleSwitch.Tag)
            {
                case "bing":
                    Properties.Settings.Default.BingWappEnable = (bool)toggleSwitch.IsChecked; break;
                case "uhd":
                    Properties.Settings.Default.IsUHDWapp = (bool)toggleSwitch.IsChecked; break;
                default:
                    break;
            }
            _Window.snackbarService.ShowAsync("Saved");
            Properties.Settings.Default.Save();
        }

        private void TextBodHandler(object sender,TextChangedEventArgs e)
        {
            var textBox = sender as Wpf.Ui.Controls.TextBox;
            switch(textBox.Tag)
            {
                case "audioDir":
                    Properties.Settings.Default.AudioFolder = textBox.Text; break;
                case "time":
                    Properties.Settings.Default.OneWordsTimeInterval = (Convert.ToInt32(textBox.Text) * 60).ToString(); break;
                default:
                    break;
            }
            Properties.Settings.Default.Save();
            //snackbarService.Show("Saved",Properties.Settings.Default.AudioFolder);
        }

        private void ComboBoxHandler(object sender,RoutedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            var item = comboBox.SelectedItem as ComboBoxItem;
            Properties.Settings.Default.OneWordsApi = "https://v1.hitokoto.cn/?c=" + item.Tag;
        }

        private void CardAction_Click(object sender,RoutedEventArgs e)
        {
            _Window.RootFrame.Source = new Uri(@"pack://application:,,,/Views/Pages/GeoSearch.xaml");
        }

        private void CardAction_Click_1(object sender,RoutedEventArgs e)
        {
        }
    }
}