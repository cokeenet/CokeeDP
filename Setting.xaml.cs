using CokeeDP.Class;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Win32;
using Panuon.WPF;
using Panuon.WPF.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
namespace CokeeDP
{
    

    public partial class Settings : WindowX
    {
        private string version = "V2.2";

        public Settings()
        {
            InitializeComponent();

            hk.Text = CokeeDP.Properties.Settings.Default.hk_api;
            weac.Text = CokeeDP.Properties.Settings.Default.wea;
            hkc.Text = CokeeDP.Properties.Settings.Default.hkc;
            city.Text = CokeeDP.Properties.Settings.Default.city;
            hk.Text = CokeeDP.Properties.Settings.Default.hk_api;
            timeto.Text = CokeeDP.Properties.Settings.Default.timeTo;
            timetoT.Text = CokeeDP.Properties.Settings.Default.timeTod;
            checkBox.IsChecked = CokeeDP.Properties.Settings.Default.usingBing;
            checkBox_Copy.IsChecked = CokeeDP.Properties.Settings.Default.enableBigTimeTo;
            checkBox_Copy1.IsChecked = CokeeDP.Properties.Settings.Default.isDebug; 
            checkBox_Copy2.IsChecked = CokeeDP.Properties.Settings.Default.IsUHDWapp;
            audioFd.Text=Properties.Settings.Default.AudioFolder;
            if(DateTime.Now.CompareTo(new(2025,6,5,00,00,00)) >= 0) 
            {
                checkBox_Copy.Visibility= Visibility.Visible;
                checkBox_Copy.IsEnabled = true;
                over_flag.Visibility = Visibility.Visible;
            }
                

                Dispatcher.Invoke(new Action(delegate
            {
                if(Properties.Settings.Default.isChildLocked)childBorder.Visibility= Visibility.Visible;
                //Thread thread =new Thread()
                loadPic();
                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CokeeWapp\\ver")) vers.Text = "软件版本:" + version + " 资源包版本:" + System.IO.File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CokeeWapp\\ver");
                else vers.Text = "软件版本:" + version + " [无资源包]";
               // if (Environment.OSVersion.Version.Major >= 10.0 && AppCenter.IsEnabledAsync().Result) t.Text = "AppCenter Running.V" + AppCenter.SdkVersion;
            }
       ));

            _ = GetUpdateLog();
        }

        private async Task GetUpdateLog()
        {
            try
            {
                var client = new HttpClient();
                update1.Text = await client.GetStringAsync("http://gitee.com/cokee/CokeeDisplayProtect/raw/main/update");
            }
            catch (Exception e)
            {
                ProcessErr(e);
                update1.Text = e.ToString();
            }
        }

        public void ProcessErr(Exception e)
        {
            NoticeBox.Show(e.ToString(), "Error", MessageBoxIcon.Error,true,5000);
             if (Environment.OSVersion.Version.Major >= 10.0) Crashes.TrackError(e);
        }

        public void load()
        {
        }

        private void TextBoxHandler(object sender, TextChangedEventArgs e)
        {
            try
            {
                TextBox n = (TextBox)sender;
                if (n.Tag.ToString() == "hkapi") CokeeDP.Properties.Settings.Default.hk_api = n.Text;
                if (n.Tag.ToString() == "hk") CokeeDP.Properties.Settings.Default.hkc = n.Text;
                if (n.Tag.ToString() == "wea") CokeeDP.Properties.Settings.Default.wea = n.Text;
                if (n.Tag.ToString() == "city") CokeeDP.Properties.Settings.Default.city = n.Text;
                if (n.Tag.ToString() == "ttN") CokeeDP.Properties.Settings.Default.timeTo = n.Text;
                if (n.Tag.ToString() == "ttT") CokeeDP.Properties.Settings.Default.timeTod = n.Text;
                if(n.Tag.ToString()== "audiofolder") CokeeDP.Properties.Settings.Default.AudioFolder = n.Text;
            }
            catch (Exception ex)
            {
                ProcessErr(ex);
            }
        }

        private void Button_Click(object sender,RoutedEventArgs e)
        {
            #region checkFolderExist
            if(!Directory.Exists(Properties.Settings.Default.AudioFolder))NoticeBox.Show("警告:媒体文件夹不存在,请重新输入.","Error",MessageBoxIcon.Error,true,3000);
            #endregion
            NoticeBox.Show("Saved!ヾ(≧▽≦*)o","info",MessageBoxIcon.Success,true,3000);
            CokeeDP.Properties.Settings.Default.Save();
        }

        private void loadPic()
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CokeeWapp");
               if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CokeeWapp\\ver")) LsbExamples.Items.Add(new ImageItem()
                {
                    Name = "[资源包]Version:" + File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CokeeWapp\\ver"),
                    Path = "respack",
                    ImageSr = new BitmapImage(new Uri(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CokeeWapp\\res.jpg"))
                });            
                FileInfo[] afi = di.GetFiles("*.png");
                Uri bgp; BitmapImage a = new BitmapImage();
                foreach (FileInfo i in afi)
                {
                    bgp = new Uri(i.FullName);
                    //MakeThumbnail(i.FullName, 192, 168, "HW").Save(BitmapImage());
                    //a.StreamSource = new FileStream(di.FullName + "\\tmp000", FileMode.OpenOrCreate);
                    a = new BitmapImage(bgp);
                    a.DecodePixelHeight=90;
                    LsbExamples.Items.Add(new ImageItem()
                    {
                        Name = i.Name + "(" + a.PixelWidth + "x" + a.PixelHeight + ")",
                        Path = i.FullName,
                        ImageSr = a
                    });        

                }             
            }
            catch (Exception e)
            {
                NoticeBox.Show(e.ToString(),"Error",MessageBoxIcon.Error,true,5000);
                ProcessErr(e);
            }
        }


        private void HitokoComboBoxHandler(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ComboBox a = (ComboBox)e.Source;
                ComboBoxItem b = (ComboBoxItem)a.SelectedItem;
                if (b.Tag.ToString() != "edit"&&b.Tag.ToString()!=null) { hk.Text = "https://v1.hitokoto.cn/?c=" + b.Tag.ToString(); CokeeDP.Properties.Settings.Default.hk_api = hk.Text; }
                //NoticeBox.Show(sender.ToString()+"##"+e.ToString(), true, MessageBoxIcon.Info);
            }
            catch (Exception ex)
            {
                ProcessErr(ex);
            }
        }

        private void Cilck(object sender, RoutedEventArgs e)
        {
            NoticeBox.Show(sender.ToString(),"tips",MessageBoxIcon.Info,true,5000);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button button = (Button)ContextMenuService.GetPlacementTarget(LogicalTreeHelper.GetParent(sender as MenuItem));
                if (button.Tag.ToString() == "respack") MessageBoxX.Show("Not Support", "Info");
                else if (MessageBoxX.Show("Are you sure?", "Q:", MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes) { File.Delete(button.Tag.ToString()); /*LsbExamples.Items.Clear();*/ loadPic(); }
            }
            catch (Exception ex)
            {
                ProcessErr(ex);
            }
        }

        private string GenNPicName()
        {
            try
            {
                Random r;
            re: r = new Random();
                int a = r.Next(99);
                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CokeeWapp\\bg" + a + ".png")) goto re;
                else return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CokeeWapp\\bg" + a + ".png";
            }
            catch (Exception e)
            {
                ProcessErr(e);
                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CokeeWapp\\bg999.png";
            }
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "选择图片";
                openFileDialog.FileName = string.Empty;
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Filter = "图片文件(.jpg;.bmp;.png)|*.jpg;*.bmp;*.png|所有文件|*.*";//another method to set filter
                if (openFileDialog.ShowDialog() == true)
                {
                    File.Copy(openFileDialog.FileName, GenNPicName());
                    //LsbExamples.Items.Clear();
                    loadPic();
                    NoticeBox.Show("Done", "info", MessageBoxIcon.Success, true,3000);
                }
            }
            catch (Exception ex)
            {
                ProcessErr(ex);
            }
        }

        private void check(object sender, RoutedEventArgs e)
        {
            var a = (CheckBox)sender;
            // if (a.IsChecked == null) { NoticeBox.Show("Null>_<", "info", MessageBoxIcon.Success, 3000); CokeeDP.Properties.Settings.Default.usingBing = false;return; }
            CokeeDP.Properties.Settings.Default.usingBing = (bool)a.IsChecked;
            NoticeBox.Show(CokeeDP.Properties.Settings.Default.usingBing.ToString(), "info", MessageBoxIcon.Success, true,3000);
            CokeeDP.Properties.Settings.Default.Save();
           /* if (Environment.OSVersion.Version.Major >= 10.0) Analytics.TrackEvent("Test", new Dictionary<string, string>
            {
                {"usingBing",CokeeDP.Properties.Settings.Default.usingBing.ToString()}
            });                  */
        }

        private void child(object sender,RoutedEventArgs e)
        {
            var a = (CheckBox)sender;
            // if (a.IsChecked == null) { NoticeBox.Show("Null>_<", "info", MessageBoxIcon.Success, 3000); CokeeDP.Properties.Settings.Default.usingBing = false;return; }
            CokeeDP.Properties.Settings.Default.isChildLocked = (bool)a.IsChecked;
            NoticeBox.Show(CokeeDP.Properties.Settings.Default.isChildLocked.ToString(),"info",MessageBoxIcon.Success,true,3000);
            
        }

        private void birth_impory(object sender,RoutedEventArgs e)
        {
            NoticeBox.Show("咕咕咕","鸽了🕊",MessageBoxIcon.Info,true,3000);
        }

        private void DaoJiShiCheck(object sender,RoutedEventArgs e)
        {
            var a = (CheckBox)sender;
            // if (a.IsChecked == null) { NoticeBox.Show("Null>_<", "info", MessageBoxIcon.Success, 3000); CokeeDP.Properties.Settings.Default.usingBing = false;return; }
            CokeeDP.Properties.Settings.Default.enableBigTimeTo = (bool)a.IsChecked;
            NoticeBox.Show(CokeeDP.Properties.Settings.Default.enableBigTimeTo.ToString(),"info",MessageBoxIcon.Info,true,3000);
            CokeeDP.Properties.Settings.Default.Save();
        }

        private void debug(object sender,RoutedEventArgs e)
        {
            var a = (CheckBox)sender;
            // if (a.IsChecked == null) { NoticeBox.Show("Null>_<", "info", MessageBoxIcon.Success, 3000); CokeeDP.Properties.Settings.Default.usingBing = false;return; }
            CokeeDP.Properties.Settings.Default.isDebug = (bool)a.IsChecked;
            NoticeBox.Show(CokeeDP.Properties.Settings.Default.isDebug.ToString(),"info",MessageBoxIcon.Info,true,3000);
            CokeeDP.Properties.Settings.Default.Save();
            /*if(Environment.OSVersion.Version.Major >= 10.0) Analytics.TrackEvent("Test",new Dictionary<string,string>
            {
                {"Debug",CokeeDP.Properties.Settings.Default.isDebug.ToString()}
            });           */
        }

        private void CheckChildPwBtnHandler(object sender,RoutedEventArgs e)
        {
            if(timeto_Copy.Text.Contains("cokee*"))
            {
                NoticeBox.Show("Saved!ヾ(≧▽≦*)o","info",MessageBoxIcon.Success,true,3000);
                CokeeDP.Properties.Settings.Default.Save();
            }
            else
            {
                NoticeBox.Show("笨蛋(╯‵□′)╯︵┻━┻\nE/SecurityHelper:Password Error.","info",MessageBoxIcon.Error,true,3000);
            }
        }

        private void UHDCkbox(object sender,RoutedEventArgs e)
        {
            var a = (CheckBox)sender;
            // if (a.IsChecked == null) { NoticeBox.Show("Null>_<", "info", MessageBoxIcon.Success, 3000); CokeeDP.Properties.Settings.Default.usingBing = false;return; }
            CokeeDP.Properties.Settings.Default.IsUHDWapp = (bool)a.IsChecked;
            NoticeBox.Show(CokeeDP.Properties.Settings.Default.IsUHDWapp.ToString(),"info",MessageBoxIcon.Info,true,3000);
            CokeeDP.Properties.Settings.Default.Save();
        }
    }
}