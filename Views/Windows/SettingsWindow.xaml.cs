using System;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Controls;
using Wpf.Ui.Controls.Interfaces;
using Wpf.Ui.Mvvm.Contracts;
using Wpf.Ui.Mvvm.Services;

namespace CokeeDP.Views.Windows
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsWindow : UiWindow
    {
        public SnackbarService snackbarService
        {
            get; set;
        }

        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void load(object sender,RoutedEventArgs e)
        {
            snackbarService = new SnackbarService();
            snackbarService.SetSnackbarControl(snackbar);
        }
    }
}