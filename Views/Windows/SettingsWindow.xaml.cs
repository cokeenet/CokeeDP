using Microsoft.AppCenter.Crashes;
using Serilog;
using System;
using System.Windows;
using Wpf.Ui;
using Wpf.Ui.Controls;
using NavigationService = Wpf.Ui.NavigationService;

namespace CokeeDP.Views.Windows
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : FluentWindow
    {
        public SnackbarService snackbarService;
        public NavigationService navigationService;

        public SettingsWindow()
        {
            InitializeComponent();
        }

        public void ProcessErr(Exception e, string msg = "")
        {
            if (this.IsLoaded)
            {
                snackbarService.SetSnackbarPresenter(snackbar);
                snackbarService.Show($"{msg}发生错误", e.Message + e.StackTrace, ControlAppearance.Danger, new SymbolIcon(SymbolRegular.ErrorCircle24));
            }
            Log.Error(e, "Err");
            if (Environment.OSVersion.Version.Major >= 10.0) Crashes.TrackError(e);
        }
        private void load(object sender, RoutedEventArgs e)
        {
            snackbarService = new SnackbarService();
            snackbarService.SetSnackbarPresenter(snackbar);
        }

        private void DarkMode(object sender, RoutedEventArgs e)
        {
            snackbarService.Show("实验性功能");
        }


    }
}