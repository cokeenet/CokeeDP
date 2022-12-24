using CokeeDP.Views.Windows;
using System;
using System.Collections.Generic;
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
using Wpf.Ui.Controls;
using Wpf.Ui.Mvvm.Services;

namespace CokeeDP.Views.Pages
{
    /// <summary>
    /// GeoSearch.xaml 的交互逻辑
    /// </summary>
    public partial class GeoSearch : UiPage
    {
        private SettingsWindow _Window = Application.Current.Windows
             .Cast<Window>()
             .FirstOrDefault(window => window is SettingsWindow) as SettingsWindow;

        public GeoSearch()
        {
            InitializeComponent();
        }
    }
}