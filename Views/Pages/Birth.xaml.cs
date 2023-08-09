using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using CokeeDP.Views.Windows;

using Newtonsoft.Json;

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
        public string BirthDateStr
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
                List<Person> people = new List<Person>();
                string[] lines = DataBox.Text.Split('\n');
                foreach (string line in lines)
                {
                    string[] cells = line.Split(' '); // 去除首尾空格并以制表符分割
                    if (cells.Length >= 2)
                    {
                        cells[1] = cells[1].Insert(4, "-");
                        cells[1] = cells[1].Insert(7, "-");
                        if (DateTime.TryParse(cells[1], out DateTime birthDate))
                        {
                            people.Add(new Person { Name = cells[0], BirthDateStr = cells[1] });
                        }
                        else
                        {
                            // 日期格式无效
                            _Window.snackbarService.ShowAsync($"无效的日期格式: {cells[1]}", "错误", SymbolRegular.Warning28, ControlAppearance.Danger);
                        }
                    }
                }
                dataGrid.ItemsSource = people;
                _Window.snackbarService.ShowAsync($"成功导入 {people.Count} 条数据", $"共 {lines.Length} 条数据。", SymbolRegular.Info24, ControlAppearance.Success);
            }
            catch (Exception ex)
            {
                _Window.ProcessErr(ex);
            }
        }

        private void DataGridEditHandler(object sender, DataGridCellEditEndingEventArgs e)
        {
            WriteToConfig();
        }
        private void Load(object sender, RoutedEventArgs e)
        {
            new Thread(LoadData).Start();
        }
        public void WriteToConfig()
        {
            try
            {
                List<Person> people = new List<Person>();
                foreach (var item in dataGrid.ItemsSource)
                {
                    if (item is Person person)
                    {
                        people.Add(person);
                    }
                }
                string json = JsonConvert.SerializeObject(people, Formatting.Indented);
                File.WriteAllText("D:\\Program Files (x86)\\CokeeTech\\CokeeDP\\birth.json", json);
                _Window.snackbarService.ShowAsync($"数据已保存");
            }
            catch (Exception ex)
            {
                _Window.ProcessErr(ex);
            }

        }
        public void LoadData()
        {
            try
            {
                string json = File.ReadAllText("D:\\Program Files (x86)\\CokeeTech\\CokeeDP\\birth.json");
                List<Person> people = JsonConvert.DeserializeObject<List<Person>>(json);
                dataGrid.ItemsSource = people;
                _Window.snackbarService.ShowAsync($"已加载 {people.Count} 条数据");
            }
            catch (FileNotFoundException)
            {
                _Window.snackbarService.ShowAsync("找不到生日数据文件。");
            }
            catch (Exception ex)
            {
                _Window.snackbarService.ShowAsync("加载数据时出现错误：" + ex.Message);
            }
        }
    }
}