using Microsoft.AppCenter.Crashes;
using Serilog;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Wpf.Ui.Appearance;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;
using NavigationService = Wpf.Ui.Mvvm.Services.NavigationService;

namespace CokeeDP.Views.Windows
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : INavigationWindow
    {
        public SnackbarService snackbarService;
        public NavigationService navigationService;

        public SettingsWindow()
        {
            InitializeComponent();
        }

        public void ProcessErr(Exception e,string msg="")
        {
            if (this.IsLoaded)
            {
                snackbarService.SetSnackbarControl(snackbar);
                snackbarService.ShowAsync($"{msg}发生错误", e.Message + e.StackTrace, SymbolRegular.ErrorCircle24, ControlAppearance.Danger);
            }
            Log.Error(e,"Err");
            if (Environment.OSVersion.Version.Major >= 10.0) Crashes.TrackError(e);
        }

        public Frame GetFrame()
       => RootFrame;

        public INavigation GetNavigation()
            => RootNavigation;

        public bool Navigate(Type pageType)
            => RootNavigation.Navigate(pageType);

        public void SetPageService(IPageService pageService)
            => RootNavigation.PageService = pageService;

        public void ShowWindow()
            => Show();

        public void CloseWindow()
            => Close();

        private void load(object sender, RoutedEventArgs e)
        {
            snackbarService = new SnackbarService();
            snackbarService.SetSnackbarControl(snackbar);
        }

        private void DarkMode(object sender, RoutedEventArgs e)
        {
            snackbarService.Show("实验性功能");
            if (Theme.GetAppTheme() == ThemeType.Light)Theme.Apply(ThemeType.Dark);
            else Theme.Apply(ThemeType.Light);
        }
    }
}