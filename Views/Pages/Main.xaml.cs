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
        public SnackbarService snackbarService { get; set; }

        public Main()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender,RoutedEventArgs e)
        {
            snackbarService = new SnackbarService();
            snackbarService.SetSnackbarControl(snackbar);
            BingWappSwitch.IsChecked = Properties.Settings.Default.BingWappEnable;
            UHDModeSwitch.IsChecked = Properties.Settings.Default.IsUHDWapp;
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
                case "dark":
                    if((bool)toggleSwitch.IsChecked)
                    {
                        Theme.Apply(ThemeType.Dark,BackgroundType.Mica);
                        break;
                    }
                    else
                    {
                        Theme.Apply(ThemeType.Light,BackgroundType.Mica);
                        break;
                    }
                default:
                    break;
            }
            Properties.Settings.Default.Save();
        }

        private void TextBodHandler(object sender,TextChangedEventArgs e)
        {
            var textBox = sender as Wpf.Ui.Controls.TextBox;
            switch(textBox.Tag)
            {
                case "audioDir":
                    Properties.Settings.Default.AudioFolder = textBox.Text; break;
                //case
                default:
                    break;
            }
            Properties.Settings.Default.Save();
            //snackbarService.Show("Saved",Properties.Settings.Default.AudioFolder);
        }
    }
}