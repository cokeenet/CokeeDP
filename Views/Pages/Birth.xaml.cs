using System;
using System.Globalization;
using System.Linq;
using System.Windows;

using CokeeDP.Views.Windows;

using Wpf.Ui.Common;
using Wpf.Ui.Controls;

namespace CokeeDP.Views.Pages
{
    /// <summary>
    /// Main.xaml 的交互逻辑
    /// </summary>
    public class Person
    {
        public string Name
        {
            get; set;
        }
        public DateTime BirthDate
        {
            get; set;
        }
    }
    public partial class Birth : UiPage
    {
        private SettingsWindow _Window = Application.Current.Windows
             .Cast<Window>()
             .FirstOrDefault(window => window is SettingsWindow) as SettingsWindow;
        public Birth()
        {
            InitializeComponent();
        }

        private void ImportBirthData(object sender, RoutedEventArgs e)
        {
            try
            {
                int succ = 0;
                string[] lines = DataBox.Text.Split('\n');
                foreach (string line in lines)
                {
                    string[] cells = line.Trim().Split('\t'); // 去除首尾空格并以制表符分割
                    if (cells.Length >= 2)
                    {
                        if (DateTime.TryParseExact(cells[1], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime birthDate))
                        {
                            dataGrid.Items.Add(new Person { Name = cells[0], BirthDate = birthDate });
                            succ++;
                        }
                        else
                        {
                            // 日期格式无效
                            _Window.snackbarService.ShowAsync($"无效的日期格式: {cells[1]}", "错误", SymbolRegular.Warning28, ControlAppearance.Danger);
                        }
                    }
                    else
                    {
                        // 数据列数不足
                        _Window.snackbarService.ShowAsync("数据格式不正确，请确保每行包含姓名和日期信息。", "错误", SymbolRegular.Warning28,ControlAppearance.Danger);
                    }
                }
                _Window.snackbarService.ShowAsync($"成功导入 {succ} 条数据",$"共 {lines.Length} 条数据。",SymbolRegular.Info24);
                if (succ != lines.Length)
                {

                }
            }
            catch (Exception ex)
            {
                _Window.ProcessErr(ex);
                throw;
            }
        }
    }
}