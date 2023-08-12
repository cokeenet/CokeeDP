using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
        [JsonIgnore]
        public bool IsSelected
        {
            get; set;
        }
    }
    public partial class Birth : UiPage
    {
        public List<Person> peopleList;
        private SettingsWindow _Window = Application.Current.Windows
             .Cast<Window>()
             .FirstOrDefault(window => window is SettingsWindow) as SettingsWindow;
        public Birth()
        {
            InitializeComponent();
        }

        private async void ImportBirthData(object sender, RoutedEventArgs e)
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
                            await _Window.snackbarService.ShowAsync($"无效的日期格式: {cells[1]}", "错误", SymbolRegular.Warning28, ControlAppearance.Danger);
                        }
                    }
                }
                dataGrid.ItemsSource = people;
                await _Window.snackbarService.ShowAsync($"成功导入 {people.Count} 条数据", $"共 {lines.Length} 条数据。", SymbolRegular.Info24, ControlAppearance.Success);
            }
            catch (Exception ex)
            {
                _Window.ProcessErr(ex);
            }
        }
        private async void Load(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }
        public async Task WriteToJsonAsync()
        {
            try
            {
                peopleList = (List<Person>)dataGrid.Items.Cast<Person>();
                string json = JsonConvert.SerializeObject(peopleList, Formatting.Indented);
                await File.WriteAllTextAsync("D:\\Program Files (x86)\\CokeeTech\\CokeeDP\\birth.json", json);
                await _Window.snackbarService.ShowAsync("数据已保存", json);
            }
            catch (Exception ex)
            {
                _Window.ProcessErr(ex);
            }
        }
        public async Task LoadDataAsync()
        {
            try
            {
                string json = await File.ReadAllTextAsync("D:\\Program Files (x86)\\CokeeTech\\CokeeDP\\birth.json");
                peopleList = JsonConvert.DeserializeObject<List<Person>>(json);
                dataGrid.ItemsSource = peopleList;
                await _Window.snackbarService.ShowAsync($"已加载 {peopleList.Count} 条数据");
            }
            catch (FileNotFoundException)
            {
                await _Window.snackbarService.ShowAsync("找不到生日数据文件。");
            }
            catch (Exception ex)
            {
                _Window.ProcessErr(ex, "加载数据时出现错误");
            }
        }

        private async void SaveData(object sender, RoutedEventArgs e)
        {
            await WriteToJsonAsync();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Name == "selectAllCheckBox")
            {
                bool isChecked = checkBox.IsChecked ?? false;

                if (dataGrid.ItemsSource is ObservableCollection<Person> people)
                {
                    foreach (var person in people)
                    {
                        person.IsSelected = isChecked;
                    }
                }
            }
        }

        private async void Reload(object sender, RoutedEventArgs e)
        {
            peopleList = null;
            dataGrid.ItemsSource = null;
            await LoadDataAsync();
        }
        private int clickCount = 0;
        private async void DeleteItem(object sender, RoutedEventArgs e)
        {
            try
            {

            clickCount++;

            if (clickCount == 1)
            {
                await _Window.snackbarService.ShowAsync("再次点击按钮以确认删除操作");
            }
            else if (clickCount == 2)
            {
                foreach (Person item in dataGrid.Items)
                {
                    if (item.IsSelected)
                    {
                        peopleList.Remove(item);
                    }
                }
                // 重置计数器和确认状态
                await WriteToJsonAsync();
                clickCount = 0;
            }

            }
            catch (Exception ex)
            {
                _Window.ProcessErr(ex);
            }
        }
    }
}