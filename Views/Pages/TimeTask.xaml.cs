using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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

namespace CokeeDP.Views.Pages
{
    /// <summary>
    /// Main.xaml 的交互逻辑
    /// </summary>
    public class TimeTasks
    {
        public string TaskName
        {
            get; set;
        }

        public string Id
        {
            get; set;
        }

        public DateTime Time
        {
            get; set;
        }
        public string Action
        {
            get; set;
        }
        public TimeTasks(string name,string id,DateTime time,string action)
        {
             TaskName=name;
            Id=id;
            Time=time;
            Action=action;
        }
    }
    public partial class TimeTask : UiPage
    {
        public TimeTask()
        {
            InitializeComponent();
        }
    }
}