using Microsoft.AppCenter.Crashes;

using Panuon.WPF.UI.Configurations;

using Serilog;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace CokeeDP.Views.Windows
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SnackbarService snackbarService;
        public Wpf.Ui.NavigationService navigationService;

        public SettingsWindow()
        {
            InitializeComponent();
        }

        public void ProcessErr(Exception e, string msg = "")
        {
            if (this.IsLoaded)
            {
                snackbarService.SetSnackbarPresenter(snackbar);
                snackbarService.Show($"{msg}发生错误", e.Message + e.StackTrace, ControlAppearance.Danger, new SymbolIcon(SymbolRegular.ErrorCircle24), TimeSpan.FromSeconds(1));
            }
            Log.Error(e, "Err");
            if (Environment.OSVersion.Version.Major >= 10.0) Crashes.TrackError(e);
        }

        public Frame GetFrame()
       => RootFrame;

        public INavigationView GetNavigation()
            => RootNavigation;

        public bool Navigate(Type pageType)
            => RootNavigation.Navigate(pageType);

        public void SetPageService(IPageService pageService)
            => RootNavigation.SetPageService(pageService);

        public void ShowWindow()
            => Show();

        public void CloseWindow()
            => Close();

        private void load(object sender, RoutedEventArgs e)
        {
            snackbarService = new SnackbarService();
            snackbarService.SetSnackbarPresenter(snackbar);
        }

        private void DarkMode(object sender, RoutedEventArgs e)
        {
            snackbarService.Show("实验性功能", "", ControlAppearance.Light, null, TimeSpan.FromSeconds(1));
            if (ApplicationThemeManager.GetAppTheme() == ApplicationTheme.Light) ApplicationThemeManager.Apply(ApplicationTheme.Dark);
            else ApplicationThemeManager.Apply(ApplicationTheme.Light);
        }


    }
}