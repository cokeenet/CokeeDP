﻿using Cokee.ClassService.Views.Controls;
using Cokee.ClassService.Views.Pages;

using Newtonsoft.Json;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;

using Wpf.Ui.Common;
using Wpf.Ui.Controls;

using static System.Windows.Forms.LinkLabel;

using Button = Wpf.Ui.Controls.Button;
using MessageBox = System.Windows.MessageBox;

namespace CokeeDP.Views.Controls
{
    /// <summary>
    /// PostNote.xaml 的交互逻辑
    /// </summary>
    public class StickyItem
    {
        public string Name
        {
            get; set;
        }
        public StickyItem(string _name)
        {
            Name = _name;
        }
    }
    public partial class StickyNote : UserControl
    {

        public StickyNote()
        {
            InitializeComponent();
            RandomNext();
            name.MouseDown += Upd;
        }

        private void Upd(object sender, MouseButtonEventArgs e) => RandomNext();

        public void RandomNext()
        {
            string INK_DIR = @$"D:\Program Files (x86)\CokeeTech\CokeeClass\ink";
            DirectoryInfo directoryInfo = new DirectoryInfo(INK_DIR);
            var files = directoryInfo.GetFiles("*.ink");
            string stu = files[new Random().Next(files.Length)].Name.Replace(".ink","");
            name.Content = stu;
            string INK_FILE = @$"D:\Program Files (x86)\CokeeTech\CokeeClass\ink\{stu}.ink";
            if (File.Exists(INK_FILE))
            {
                FileStream fs = new FileStream(INK_FILE, FileMode.Open);
                ink.Strokes = new StrokeCollection(fs);
                fs.Close();
            }
        }
    }
}
