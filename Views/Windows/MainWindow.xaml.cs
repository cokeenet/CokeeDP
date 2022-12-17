using CokeeDP.Class;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

using NAudio.CoreAudioApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Panuon.WPF.UI;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;

using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Wpf.Ui.Mvvm.Services;
using Button = Wpf.Ui.Controls.Button;
using File = System.IO.File;

namespace CokeeDP.Views.WIndows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const int DBT_DEVICEARRIVAL = 0x8000;  //U盘可用
        public const int DBT_DEVICEREMOVECOMPLETE = 0x8004; //一个设备或媒体已被删除。
        public const int FILE_SHARE_READ = 0x1;
        public const int FILE_SHARE_WRITE = 0x2;
        public const uint GENERIC_READ = 0x80000000;
        public const int GENERIC_WRITE = 0x40000000;
        public const int IOCTL_STORAGE_EJECT_MEDIA = 0x2d4808;
        public const int WM_DEVICECHANGE = 0x219;
        private static Timer OneWordsTimeInterval;
        private static Timer sec;
        private static Timer wet;
        private FileInfo[] afi,Weaicon;
        private FileInfo[] audio;
        private int AudioNum = 0;
        private String AudioPath, AudioFolder, MediaDuring;
        private int bgn = 0, bing = 0;
        BitmapImage bitmapImage = null;
        //U盘插入后，OS的底层会自动检测，然后向应用程序发送“硬件设备状态改变“的消息
        private string disk, weaWr, hkUrl, nowDowning = "";
        SnackbarService snackbarService = new SnackbarService();
        private bool IsPlaying = false, AudioLoaded = false, IsReplay = true;
        private MediaPlayer mediaplayer = new MediaPlayer();
        private DateTime tod;
        private bool usingBing = true;
        private double ver = 3.0;

        public MainWindow()
        {
            InitializeComponent();
            try
            {
                /* if(Environment.GetCommandLineArgs().Length > 0)
                  {
                      string[] pargs = Environment.GetCommandLineArgs();
                      if(pargs[1] == "/p") Close();//this.Owner = pargs[2];
                      if(pargs[1] == "/c") { this.WindowState = WindowState.Minimized; var win1 = new Settings(); win1.Show(); }
                 }            
                */

               FillConfig();
                if(Properties.Settings.Default.enableBigTimeTo)
                {
                    BigCountdown.Visibility = Visibility.Visible;
                    timeTod.Visibility = Visibility.Collapsed;
                }
                if(usingBing)
                {
                    _ = GetBingWapp();
                }
                else
                {
                    if(!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CokeeWapp")) Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CokeeWapp");
                    DirectoryInfo di = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CokeeWapp");
                    afi = di.GetFiles("*.png");ChangeWapp(false);
                }
                time.Content = DateTime.Now.ToString("hh:mm:ss");
                if(Directory.Exists(AudioFolder))
                {
                    DirectoryInfo dir = new DirectoryInfo(AudioFolder);
                    if(dir.Exists) audio = dir.GetFiles("*.mp3");
                }
                //MessageBoxX.Show(audio.Length.ToString());
                //MessageBoxX.Show(Environment.OSVersion.Version.Major.ToString());
                //hitokoto.Content = "hello world!";
            }
            catch(Exception e)
            {
                ProcessErr(e);
            }
        }

        public void ChangeWapp(bool direction)
        {
            try
            {
                if(usingBing)
                {
                    if(bing >= 8 || bing <= -2) bing = 0;
                    if (!direction) bing++;
                    else bing--;
                    if (bing >= 8 || bing <= -1) bing = 0;
                    _ = GetBingWapp();
                    return;
                }
                #region non-bing
                else
                {
                    Uri bgp;
                    if(!direction)
                    {
                        if(bgn == 0)
                        {
                            bgp = new Uri(afi[1].FullName);
                            bgn = afi.Length;
                        }
                        else if(bgn <= afi.Length)
                        {
                            bgp = new Uri(afi[1].FullName);
                            bgn = afi.Length - 1;
                        }
                        else
                        {
                            bgp = new Uri(afi[bgn].FullName);
                            bgn--;
                        }
                    }
                    else
                    {
                        if(bgn == 0)
                        {
                            bgp = new Uri(afi[1].FullName);
                            bgn = 1;
                        }
                        else if(bgn >= afi.Length)
                        {
                            bgp = new Uri(afi[1].FullName);
                            bgn = 1;
                        }
                        else
                        {
                            bgp = new Uri(afi[bgn].FullName);
                            bgn++;
                        }
                    }

                    br1.BeginInit();
                    br1.Source = new BitmapImage(bgp);
                    br1.EndInit();
                    log.Text = bgn + "/LoadLocalPic:" + bgp.ToString();
                    #endregion
                }
            }
            catch(Exception ex)
            {
                ProcessErr(ex);
                log.Text = ex.ToString();
            }
        }

        public void CheckTasks()
        {
        }

        public void FillConfig()
        {
            try
            {
                if(Properties.Settings.Default.timeTod.Length == 0) { tod = new(2025,6,5,00,00,00); Properties.Settings.Default.timeTod = tod.ToString(); }
                else tod = DateTime.Parse(Properties.Settings.Default.timeTod);
                if(Properties.Settings.Default.hk_api.Length == 0) Properties.Settings.Default.hk_api = "https://v1.hitokoto.cn/?c=k";
                if(Convert.ToInt32(Properties.Settings.Default.hkc) <= 300) Properties.Settings.Default.hkc = "300";
                if(Convert.ToInt32(Properties.Settings.Default.wea) <= 9800) Properties.Settings.Default.wea = "9800";
                if(Properties.Settings.Default.timeTo.Length <= 1) Properties.Settings.Default.timeTo = "高考";
                if(Properties.Settings.Default.isDebug) log.Visibility = Visibility.Visible;
                usingBing = Properties.Settings.Default.usingBing;
                AudioFolder = Properties.Settings.Default.AudioFolder;
                Properties.Settings.Default.Save();
                SetTimer(sec,1,hkc,Convert.ToInt32(Properties.Settings.Default.OneWordsTimeInterval),wet,Convert.ToInt32(Properties.Settings.Default.wea));
               
            }
            catch(Exception ex)
            {
                ProcessErr(ex);
                log.Text = ex.ToString();
                Properties.Settings.Default.hkc = "300";
                Properties.Settings.Default.wea = "9800";
            }
        }

        public void OnHitokoUpd(object source,ElapsedEventArgs d)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                ChangeWapp(false);
                pager.PageDown();
                _ = Hitoko();
            }
       ));
        }

        public void OnNewDay()
        {

        }
        public void OnOneSec(object source,ElapsedEventArgs e)
        {
            try
            {
                Dispatcher.Invoke(new Action(delegate
            {
                time.Content = DateTime.Now.ToString("hh:mm:ss");
                timeTo.Content = DateTime.Now.ToString("ddd,M月dd日");
                if(Properties.Settings.Default.enableBigTimeTo)
                {
                    tod_info.Content = "还有" + tod.Subtract(DateTime.Now).TotalDays + "天";
                    big_tod.Content = ((int)tod.Subtract(DateTime.Now).TotalDays);
                }
                else
                    timeTod.Content = "距离[" + Properties.Settings.Default.timeTo + "]还有" + tod.Subtract(DateTime.Now).TotalDays + "天";
                if(DateTime.Now.Hour == 0 && DateTime.Now.Minute == 0 && DateTime.Now.Second == 0)
                {
                    OnNewDay();
                }
                if(IsPlaying)
                {
                    //  audioTime.Content = mediaplayer.Position.ToString(@"mm\:ss") + "/" + MediaDuring;
                    PlaySlider.Value = mediaplayer.Position.TotalSeconds;
                    //PlaySlider.Maximum = mediaplayer.NaturalDuration.TimeSpan.TotalSeconds;
                }
                // new Thread(CheckTasks).Start();          // 创建线程
            }));
            }
            catch(Exception ex)
            {
                ProcessErr(ex);
            }
        }

        public void ProcessErr(Exception e)
        {
            snackbarService.Show("发生错误",e.Message+"  "+e.StackTrace,SymbolRegular.ErrorCircle24);
            Log.Error(e,"Error");
            if(Environment.OSVersion.Version.Major >= 10.0) Crashes.TrackError(e);
        }

        public void SetTimer(System.Timers.Timer a,int ms,System.Timers.Timer b,int ms1,System.Timers.Timer c,int ms2)
        {
            // Create timers with a interval.
            a = new System.Timers.Timer(ms * 1000); a.Elapsed += new ElapsedEventHandler(OnOneSec); a.AutoReset = true; a.Enabled = true;
            b = new System.Timers.Timer(ms1 * 1000); b.Elapsed += new ElapsedEventHandler(OnHitokoUpd); b.AutoReset = true; b.Enabled = true;
            c = new System.Timers.Timer(ms2 * 1000); c.Elapsed += new ElapsedEventHandler(OnWea); c.AutoReset = true; c.Enabled = true;
        }
        public Uri GetWeatherIcon(int code)
        {
            try
            {
                return new Uri("pack://application:,,,/Icons/" + code.ToString() + "-fill.svg");
            }
            catch (Exception ex)
            {
                ProcessErr(ex);
                return null;
            }
        }


        private async Task GetBingWapp()
        {
            try
            {
                var client = new HttpClient(); var a = new WebClient();
                var u2 = await client.GetStringAsync("https://cn.bing.com/hp/api/v1/imagegallery?format=json&ensearch=0");//await client.GetStringAsync("https://cn.bing.com/HPImageArchive.aspx?format=js&idx=" + bing + "&n=1");
                JObject dt = JsonConvert.DeserializeObject<JObject>(u2);
                if(File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CokeeWapp\\blkimg.txt"))
                    if(File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CokeeWapp\\blkimg.txt").Contains(dt["data"]["images"][bing]["isoDate"].ToString()))
                    {
                        ChangeWapp(false);
                        return;
                    }
                BingImageInfo.Content = dt["data"]["images"][bing]["title"].ToString() + " (" + dt["data"]["images"][bing]["copyright"] + ")  | " + dt["data"]["images"][bing]["isoDate"].ToString();
                var urlstr = "https://www.bing.com/" + dt["data"]["images"][bing]["imageUrls"]["landscape"]["highDef"];
                CardInfo.Content = dt["data"]["images"][bing]["caption"].ToString();
                DescPara1.Text = dt["data"]["images"][bing]["description"] + Environment.NewLine + dt["data"]["images"][bing]["descriptionPara2"] + Environment.NewLine + dt["data"]["images"][bing]["descriptionPara3"];
                DescPara1.Text = DescPara1.Text.Replace("性感","__"); //Temp use
                if(Properties.Settings.Default.IsUHDWapp) urlstr = urlstr.Replace("_1920x1080","_UHD");
                Uri uri = new Uri(urlstr);
                log.Text = bing + "/LoadBingImage:" + uri;
                bitmapImage = new BitmapImage(uri);
                bitmapImage.DownloadProgress += ImageDownloadProgress;
                bitmapImage.DownloadCompleted += DownloadImageCompleted;
                br1.Tag = dt["data"]["images"][bing]["isoDate"].ToString();
                DoubleAnimation animation = new DoubleAnimation(0,20,new Duration(TimeSpan.FromSeconds(5)));
                animation.EasingFunction = new CircleEase();
                //animation.AutoReverse = true;
                br1_blur.BeginAnimation(BlurEffect.RadiusProperty,animation);
                //pro_Copy.Value = bing + 1;
                bitmapImage.BeginInit();
            }
            catch(Exception ex)
            {
                ProcessErr(ex);
            }
        }

        private void ImageDownloadProgress(object sender,DownloadProgressEventArgs e)
        {
            if(pro.Visibility != Visibility.Visible) pro.Visibility = Visibility.Visible;
            pro.Value = e.Progress; 
            log.Text = "LoadBingImage (" + e.Progress+"% )"; 
        }

        private void DownloadImageCompleted(object sender,EventArgs e)
        {
           // Disappear(pro,1,20,0.5);
           if(pro.Visibility != Visibility.Collapsed) pro.Visibility = Visibility.Collapsed;
            log.Text = "Image Loaded. Num:"+bing;
            DoubleAnimation animation = new DoubleAnimation(20,0,new Duration(TimeSpan.FromSeconds(5)));
           animation.EasingFunction = new CircleEase();
            //animation.AutoReverse = true;
            br1_blur.BeginAnimation(BlurEffect.RadiusProperty,animation);
            br1.Source = bitmapImage;
        }

        private void DownloadFileCallback(object sender,AsyncCompletedEventArgs e)
        {
            if(pro.Visibility != Visibility.Visible) pro.Visibility = Visibility.Collapsed;
            log.Text = "Done.";
            if(e.Cancelled)
            {
                log.Text = "File download cancelled.";
            }
            if(e.Error != null)
            {
                log.Text = e.Error.ToString();
            }
            if(e.Error == null&&!e.Cancelled)
            {
                br1.BeginInit();
                br1_blur.Radius = 0;
                br1.Source = new BitmapImage(new Uri(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CokeeWapp\\bing.jpg"));
                br1.EndInit();
            }
        }
            private async Task Hitoko()
        {
            try
            {
                /* if (CokeeDP.Properties.Settings.Default.isDebug)
                 {
                     return;
                 }*/
                var client = new HttpClient();
                JObject dt = JsonConvert.DeserializeObject<JObject>(await client.GetStringAsync(Properties.Settings.Default.hk_api));
                string who = dt["from_who"].ToString();
                hkUrl = dt["uuid"].ToString();
                hitokoto.Content = who == "null"
                    ? dt["hitokoto"].ToString() + "--《" + dt["from"].ToString() + "》"
                    : dt["hitokoto"].ToString() + "--《" + dt["from"].ToString() + "》" + dt["from_who"].ToString();
            }
            catch(Exception ex)
            {
                ProcessErr(ex);
                log.Text = ex.ToString();
            }
        }

        private void Load(object sender,RoutedEventArgs e)
        {
            try
            {
                this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
                this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
                br1.Width= System.Windows.SystemParameters.PrimaryScreenWidth;
                br1.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
                if(Directory.Exists(Environment.CurrentDirectory))
                {
                    var a=new DirectoryInfo(Environment.CurrentDirectory);
                    Weaicon = a.GetFiles();
                }
                if(Environment.OSVersion.Version.Major >= 10.0)
                    AppCenter.Start("75515c2c-52fd-4db8-a6c1-84682e1860de",typeof(Analytics),typeof(Crashes));
                HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;
                DriveInfo[] s = DriveInfo.GetDrives();
                s.Any(t =>
                {
                    if(t.DriveType == DriveType.Removable)
                    {
                        usb.Visibility = Visibility.Visible;
                        disk = t.Name;
                        //MessageBox.Show("U盘插入,盘符为：" + t.Name);
                        usb.Visibility = Visibility.Visible;
                        diskName.Content = t.VolumeLabel + "(" + t.Name + ")";
                        diskInfo.Content = (t.TotalFreeSpace / 1073741824).ToString() + "GB/" + (t.TotalSize / 1073741824).ToString() + "GB";
                        return true;
                    }
                    return false;
                });
                _ = Hitoko();
                _ = Wea();
                _ = weaWrn();
                hwndSource.AddHook(new HwndSourceHook(WndProc));//挂钩
            }
            catch(Exception ex)
            {
                ProcessErr(ex);
                log.Text = ex.ToString();
            }
        }

        private void OnWea(object sender,ElapsedEventArgs e)
        {
            //async get http wea info
            Dispatcher.Invoke(new Action(delegate
            {
                _ = Wea();
                _ = weaWrn();
            }
            ));
        }

        private void WappChangeBtnHandler(object sender,RoutedEventArgs e)
        {
            var a = (Button)sender;if(bing >= 8 || bing <= -1) bing = 0;
            if (a.Name == "left") ChangeWapp(false);
            else if (a.Name == "right") ChangeWapp(true);
            
        }

        private async Task Wea()
        {
            try
            {
                var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };
                var client = new HttpClient(handler);
                string u2 = await client.GetStringAsync("https://devapi.qweather.com/v7/weather/7d?location="+Properties.Settings.Default.CityId+"&key=6572127bcec647faba394b17fbd9614f");
                //MessageBox.Show(u2);
                JObject dt = JsonConvert.DeserializeObject<JObject>(u2);
                wea1.Text = "今天 " + dt["daily"][0]["textDay"].ToString() + "," + dt["daily"][0]["tempMax"].ToString() + "°C~" + dt["daily"][0]["tempMin"].ToString() + "°C 湿度:" + dt["daily"][0]["humidity"].ToString();
                wea2.Text = "明天 " + dt["daily"][1]["textDay"].ToString() + "," + dt["daily"][1]["tempMax"].ToString() + "°C~" + dt["daily"][1]["tempMin"].ToString() + "°C 湿度:" + dt["daily"][1]["humidity"].ToString();
                wea3.Text = "后天 " + dt["daily"][2]["textDay"].ToString() + "," + dt["daily"][2]["tempMax"].ToString() + "°C~" + dt["daily"][2]["tempMin"].ToString() + "°C 湿度:" + dt["daily"][2]["humidity"].ToString();
                wea4.Text = dt["daily"][3]["fxDate"].ToString().Substring(5) +" "+ dt["daily"][3]["textDay"].ToString() + "," + dt["daily"][3]["tempMax"].ToString() + "°C~" + dt["daily"][3]["tempMin"].ToString() + "°C 湿度:" + dt["daily"][3]["humidity"].ToString();
                wea5.Text = dt["daily"][4]["fxDate"].ToString().Substring(5) + " " + dt["daily"][4]["textDay"].ToString() + "," + dt["daily"][4]["tempMax"].ToString() + "°C~" + dt["daily"][4]["tempMin"].ToString() + "°C 湿度:" + dt["daily"][4]["humidity"].ToString();
                wea6.Text = dt["daily"][5]["fxDate"].ToString().Substring(5) + " " + dt["daily"][5]["textDay"].ToString() + "," + dt["daily"][5]["tempMax"].ToString() + "°C~" + dt["daily"][5]["tempMin"].ToString() + "°C 湿度:" + dt["daily"][5]["humidity"].ToString();


                w1.Source = GetWeatherIcon((int)dt["daily"][0]["iconDay"]);
                w2.Source = GetWeatherIcon((int)dt["daily"][1]["iconDay"]);
                w3.Source = GetWeatherIcon((int)dt["daily"][2]["iconDay"]);
                w4.Source = GetWeatherIcon((int)dt["daily"][3]["iconDay"]);
                w5.Source = GetWeatherIcon((int)dt["daily"][4]["iconDay"]);
                w6.Source = GetWeatherIcon((int)dt["daily"][5]["iconDay"]);
            }
            catch(Exception ex)
            {
                ProcessErr(ex);
                log.Text = ex.ToString();
            }
        }


        private async Task weaWrn()
        {
            try
            {
                var handler = new HttpClientHandler();
                var client = new HttpClient(handler);
                handler.ClientCertificateOptions = ClientCertificateOption.Automatic;
                handler.AllowAutoRedirect = true;
                var u2 = await client.GetStringAsync("https://devapi.qweather.com/v7/warning/now?location=101220801&key=6572127bcec647faba394b17fbd9614f&gzip=n");
                //MessageBox.Show(u2);
                var dt = JsonConvert.DeserializeObject<JObject>(u2);
                if(!dt["warning"].HasValues)
                {
                    SpecialWeatherBtn.Visibility = Visibility.Collapsed;
                    SpecialWeatherBtn1.Visibility = Visibility.Collapsed;
                }
                else
                {
                    var t = dt["warning"][0]["title"].ToString();
                    SpecialWeatherBtn.Visibility = Visibility.Visible;
                    SpecialWeatherBtn1.Visibility = Visibility.Visible;
                    string TextShort;
                    if(t.Contains("发布"))TextShort= t.Substring(t.IndexOf("布") + 1);
                    else TextShort= t.Substring(t.IndexOf("新") + 1);
                    SpecialWeatherBtn.Content = TextShort;
                    weaWr = dt["warning"][0]["text"].ToString();
                }
            }
            catch(Exception ex)
            {
                ProcessErr(ex);
                log.Text = ex.ToString();
            }
        }

        //http://api.seniverse.com/v3/weather/daily.json?key=SISi82MwzaMbmQqSh&location=fuyang&language=zh-Hans&unit=c&start=0&days=3
        private void hitokoto_MouseDown(object sender,MouseButtonEventArgs e) => _ = Hitoko();

        private void OnCloseWindow(object sender,MouseButtonEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                if(IsPlaying)
                {
                   if(MessageBoxX.Show(this,"有媒体正在播放。确认退出吗?","警告",MessageBoxButton.OKCancel,MessageBoxIcon.Warning)==MessageBoxResult.OK)Close();
                }
                else Close();
            }));
        }

        private IntPtr WndProc(IntPtr hwnd,int msg,IntPtr wParam,IntPtr lParam,ref bool handled)
        {
            try
            {
                if(msg == WM_DEVICECHANGE)
                {
                    switch(wParam.ToInt32())
                    {
                        case DBT_DEVICEARRIVAL:
                            DriveInfo[] s = DriveInfo.GetDrives();
                            s.Any(t =>
                            {
                                if(t.DriveType == DriveType.Removable)
                                {
                                    usb.Visibility = Visibility.Visible;
                                    disk = t.Name;
                                    log.Text = DateTime.Now.ToString("R") + "_Usbdrive.Install(" + t.Name + "," + t.DriveType + ");";
                                    diskName.Content = t.VolumeLabel + "(" + t.Name + ":)";
                                    diskInfo.Content = (t.TotalFreeSpace / 1073741824).ToString() + "GB/" + (t.TotalSize / 1073741824).ToString() + "GB";
                                    return true;
                                }
                                return false;
                            });
                            break;

                        case DBT_DEVICEREMOVECOMPLETE:
                            //MessageBox.Show("U盘卸载");
                            log.Text = DateTime.Now.ToString("R") + "_Usbdrive.Uninstall();";
                            usb.Visibility = Visibility.Collapsed;
                            break;

                        default:
                            break;
                    }
                }
            }
            catch(Exception ex)
            {
                ProcessErr(ex);
                log.Text = ex.ToString();
            }
            return IntPtr.Zero;
        }

        [DllImport("kernel32.dll",SetLastError = true,CharSet = CharSet.Auto)]
        private static extern IntPtr CreateFile(
         string lpFileName,
         uint dwDesireAccess,
         uint dwShareMode,
         IntPtr SecurityAttributes,
         uint dwCreationDisposition,
         uint dwFlagsAndAttributes,
         IntPtr hTemplateFile);

        [DllImport("kernel32.dll",ExactSpelling = true,SetLastError = true,CharSet = CharSet.Auto)]
        private static extern bool DeviceIoControl(
            IntPtr hDevice,
            uint dwIoControlCode,
            IntPtr lpInBuffer,
            uint nInBufferSize,
            IntPtr lpOutBuffer,
            uint nOutBufferSize,
            out uint lpBytesReturned,
            IntPtr lpOverlapped
        );

        private void ExitUsbDrive(object sender,RoutedEventArgs e)
        {
            try
            {
                string filename = @"\\.\" + disk.Remove(2);
                //打开设备，得到设备的句柄handle.
                IntPtr handle = CreateFile(filename,GENERIC_READ | GENERIC_WRITE,FILE_SHARE_READ | FILE_SHARE_WRITE,IntPtr.Zero,0x3,0,IntPtr.Zero);
                // 向目标设备发送设备控制码。IOCTL_STORAGE_EJECT_MEDIA-弹出U盘
                uint byteReturned;
                bool result = DeviceIoControl(handle,IOCTL_STORAGE_EJECT_MEDIA,IntPtr.Zero,0,IntPtr.Zero,0,out byteReturned,IntPtr.Zero);
                if(!result) NoticeBox.Show("U盘退出失败","Error",MessageBoxIcon.Warning,true,3000);
                else usb.Visibility = Visibility.Collapsed;
            }
            catch(Exception ex)
            {
                ProcessErr(ex);
                log.Text = ex.ToString();
            }
        }

        private void ShowSetting(object sender,RoutedEventArgs e)
        {
            try
            {
                var settingsWindow = new SettingsWindow();
                settingsWindow.Show();
            }
            catch(Exception ex)
            {
                ProcessErr(ex);
                log.Text = ex.ToString();
            }
        }
        /// <summary>
        ///星期标签点击处理
        /// </summary>
        private void OnwklabC(object sender,MouseButtonEventArgs e)
        {
            try
            {
                
                ChangeWapp(false);
                pager.PageDown();
                _ = Hitoko();
            }
            catch(Exception ex)
            {
                ProcessErr(ex);
                log.Text = ex.ToString();
            }
        }
        /// <summary>
        ///一言-右键菜单
        /// </summary>
        /// <param name="sender">(Label)</param>
        private void Like_menu(object sender,MouseButtonEventArgs e)
        {
            lik.PlacementTarget = hitokoto;
            lik.IsOpen = true;
        }

        /// <summary>
        ///元素拖动处理
        /// </summary>
        /// <param name="sender">(Label)被拖的标签</param>
        private void Drag(object sender,DragEventArgs e)
        {
            // return DragEventHandler.;
        }
        /// <summary>
        ///元素tempMaxlight处理
        /// </summary>
        /// <param name="sender">(Label)被拖的</param>
        private void Light(object sender,DragEventArgs e)
        {
            /* for (int i = 0; i < MainGrid.Children.Count; i++)
             {
                 MainGrid.Children[i].DragEnter+=Drag();
             }      */
        }

        private void ShowPlayer(object sender,MouseButtonEventArgs e)
        {
            try
            {
                if(music.Visibility == Visibility.Collapsed) music.Visibility = Visibility.Visible;
                else if(music.Visibility == Visibility.Visible) { music.Visibility = Visibility.Collapsed; IsPlaying = false; playbtn.Icon=SymbolRegular.Play48; mediaplayer.Pause(); }
                IntlPlayer();
            }
            catch(Exception ex)
            {
                ProcessErr(ex);
            }
        }
        public void IntlPlayer()
        {
            try
            {
                if(AudioNum >= audio.Length || AudioNum < 0) AudioNum = 0;
                AudioPath = audio[AudioNum].FullName;
                mediaplayer.Open(new Uri(audio[AudioNum].FullName));
                mediaplayer.Volume = 1;
                mediaplayer.MediaOpened += MediaLoaded;
                mediaplayer.MediaEnded += MediaEnded;
                audioName.Content = audio[AudioNum].Name;
                
                //audioTime.Content = "[Paused]" + audioTime.Content;
                //var tmp = (ButtonHelper)playbtn;
                // IsPlaying = false;
            }
            catch(Exception ex)
            {
                ProcessErr(ex);
            }
        }

        private void SilderChanged(object sender,RoutedPropertyChangedEventArgs<double> e)
        {
            mediaplayer.Position = TimeSpan.FromSeconds(PlaySlider.Value);
            audioTime.Content = mediaplayer.Position.ToString(@"mm\:ss") + " / " + mediaplayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
        }

        private void ChangeVolume(object sender,RoutedEventArgs e)
        {
            try
            {
                var tmp = (Button)sender;
                if(VolumeSlider.Visibility==Visibility.Collapsed)
                {
                    CancelTheMute();
                    VolumeText.Visibility = Visibility.Visible;
                    VolumeSlider.Visibility = Visibility.Visible;
                    VolumeText.Content = "音量:"+ GetCurrentSpeakerVolume()+"%";
                    VolumeSlider.Value = GetCurrentSpeakerVolume();
                }
                else if(VolumeSlider.Visibility == Visibility.Visible)
                {
                    VolumeSlider.Visibility = Visibility.Collapsed;
                    VolumeText.Visibility=Visibility.Collapsed;
                }
            }
            catch(Exception ex)
            {
                ProcessErr(ex);
            }
        }

        private void MediaLoaded(object sender,EventArgs e)
        {
            try
            {
                audioName.Content = audio[AudioNum].Name;
                PlaySlider.Value = 0;
                PlaySlider.Maximum = mediaplayer.NaturalDuration.TimeSpan.TotalSeconds;
                MediaDuring = mediaplayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
                audioTime.Content = "00:00/" + mediaplayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
               /* playIcon.Text = "";
                IsPlaying = true;
                mediaplayer.Play();*/
            }
            catch(Exception ex)
            {
                ProcessErr(ex);
            }
        }
        private void MediaEnded(object sender,EventArgs e)
        {
            try
            {
                if(IsReplay)
                {
                    //playIcon.Text = "";
                    IntlPlayer();
                    PlaySlider.Value = 0;
                    return;
                }
                else
                {
                    if(AudioNum >= audio.Length) AudioNum = 0;
                    else AudioNum++;
                    IntlPlayer();
                }
            }
            catch(Exception ex)
            {
                ProcessErr(ex);
            }
        }
        private void BtnReplayHandler(object sender,RoutedEventArgs e)
        {
            try
            {
                var tmp = (Button)sender;
                if(tmp.Content.ToString() == "单曲循环")
                {
                    IsReplay = false;
                    tmp.Content = "列表循环";
                }
                else if(tmp.Content.ToString() == "列表循环")
                {
                    IsReplay = true;
                    tmp.Content = "单曲循环";
                }
            }
            catch(Exception ex)
            {
                ProcessErr(ex);
            }
        }
        private void PlayerBtnProc(object sender,RoutedEventArgs e)
        {
            try
            {
                var tmp = (Button)sender;
                if(tmp.Tag.ToString() == "prev")
                {
                    if(AudioNum == 0) AudioNum = audio.Length;
                    else AudioNum--;
                    playbtn.Icon = SymbolRegular.Play48;
                }
                else if(tmp.Tag.ToString() == "next")
                {
                    if(AudioNum >= audio.Length) AudioNum = 0;
                    else AudioNum++;
                    playbtn.Icon = SymbolRegular.Play48;
                }
                IntlPlayer();
            }
            catch(Exception ex)
            {
                ProcessErr(ex);
            }
        }
        private void VolumeChanged(object sender,RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                SetCurrentSpeakerVolume(Convert.ToInt32(VolumeSlider.Value));
            }
            catch(Exception ex)
            {
                ProcessErr(ex);
            }
        }
        private void OnBtnPlay(object sender,RoutedEventArgs e)
        {
            try
            {
                if(IsPlaying)
                {
                    playbtn.Icon = SymbolRegular.Play48;
                    IsPlaying = false;
                    mediaplayer.Pause();
                }
                else
                {
                    playbtn.Icon = SymbolRegular.Pause48;
                    IsPlaying = true;
                    mediaplayer.Play();
                }
            }
            catch(Exception ex)
            {
                ProcessErr(ex);
            }
        }
        private int GetCurrentSpeakerVolume()
        {
            try
            {
                int volume = 0;
                var enumerator = new MMDeviceEnumerator();

                //获取音频输出设备
                IEnumerable<MMDevice> speakDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render,DeviceState.Active).ToArray();
                if(speakDevices.Count() > 0)
                {
                    MMDevice mMDevice = speakDevices.ToList()[0];
                    volume = Convert.ToInt16(mMDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
                }
                return volume;
            }
            catch(Exception ex)
            {
                ProcessErr(ex);
                return 0;
            }
        }
        private void SetCurrentSpeakerVolume(int volume)
        {
            try
            {
                var enumerator = new MMDeviceEnumerator();
                IEnumerable<MMDevice> speakDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render,DeviceState.Active).ToArray();
                if(speakDevices.Count() > 0)
                {
                    MMDevice mMDevice = speakDevices.ToList()[0];
                    mMDevice.AudioEndpointVolume.MasterVolumeLevelScalar = volume / 100.0f;
                }
            }
            catch(Exception ex)
            {
                ProcessErr(ex);
            }
        }

        public void CancelTheMute()
        {
            try
            {
                var enumerator = new MMDeviceEnumerator();
                IEnumerable<MMDevice> speakDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render,DeviceState.Active).ToArray();
                MMDevice mMDevice = speakDevices.ToList()[0];
                mMDevice.AudioEndpointVolume.Mute = false;//系统音量静音
            }
            catch(Exception ex)
            {
                ProcessErr(ex);
            }
        }

        private void Textbox_Chg(object sender,TextChangedEventArgs e)
        {
            var a = (System.Windows.Controls.TextBox)sender;
            hitokoto.Content = a.Text;
        }

      

        private void BtnSaveHandler(object sender,RoutedEventArgs e)
        {
            NoticeBox.Show("已收藏至文件 " + filePath,"info",MessageBoxIcon.Info,true,1000);
            WriteInfo(hitokoto.Content.ToString(), @"D:\cokee_hitokoto.txt");
        }

        private string filePath = @"D:\cokee_hitokoto.txt";

        private void WriteInfo(string info,string filepath)
        {
            using(FileStream stream = new FileStream(filepath,FileMode.Append))
            {
                using(StreamWriter writer = new StreamWriter(stream))
                {
                    writer.WriteLine($"{DateTime.Now},{info};");
                }
            }
        }

        private void FastProgClick(object sender,RoutedEventArgs e)
        {
            try
            {
                if (!File.Exists(@"C:\Program Files(x86)\Seewo\EasiNote5\swenlauncher\swenlauncher.exe")) Process.Start(@"C:\Program Files(x86)\Seewo\EasiNote5\swenlauncher\swenlauncher.exe");
                else Process.Start(@"D:\Program Files(x86)\Seewo\EasiNote5\swenlauncher\swenlauncher.exe");
            }
            catch(Exception ex)
            {
                ProcessErr(ex);
            }
        }

        private void kbshow(object sender, RoutedEventArgs e)
        {
            // Process.Start("explore.exe", @"C:\Program Files\Common Files\microsoft shared\ink\TabTip.exe");
            MainWindow.GetWindow(this).WindowState = WindowState.Normal;
        }

        private void FuncT1(object sender, MouseButtonEventArgs e)
        {
            //br1_blur.BeginAnimation(br1_blur.Radius,)
        }

        private void FuncT2(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Process.Start("C:\\Windows\\explorer.exe");
            }
            catch(Exception ex)
            {
                ProcessErr(ex);
            }
        }

        private void DislikeImage(object sender, RoutedEventArgs e)
        {
            WriteInfo(br1.Tag.ToString()+"\n", Environment.CurrentDirectory + "\\blkimg.txt");
            NoticeBox.Show("屏蔽成功.");
            _=GetBingWapp();
        }

        /// <summary>
        ///一言处理
        /// </summary>
        private void Viewsour(object sender,RoutedEventArgs e)
        {
            
            snackbarService.Show("链接已复制","https://hitokoto.cn/?uuid=" + hkUrl,SymbolRegular.CopyAdd24);
                //("https://hitokoto.cn/?uuid=" + hkUrl,"info");
        }

        private void Likeit(object sender,RoutedEventArgs e)
        {
            if(hkself.Visibility == Visibility.Collapsed) hkself.Visibility = Visibility.Visible;
            else hkself.Visibility = Visibility.Collapsed;
            //Process.Start(@"D:\ink\TabTip.exe");
            //NoticeBox.Show("Done!", "Info", MessageBoxIcon.Info,true,1000);
        }
        /// <summary>
        ///u盘处理-打开
        /// </summary>
        private void OpenUsb(object sender,RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("explorer.exe",disk);
            }
            catch(Exception ex)
            {
                ProcessErr(ex);
                log.Text = ex.ToString();
            }
        }

        /// <summary>
        ///特殊天气按钮-按下处理
        /// </summary>
        /// <param name="sender">(Btn)</param>
        private void WeaWarnBtnHandler(object sender,RoutedEventArgs e)
        {
            try
            {
                MessageBoxX.Show(this,weaWr,(string)SpecialWeatherBtn.Content,MessageBoxIcon.Info,DefaultButton.YesOK);
            }
            catch(Exception ex)
            {
                ProcessErr(ex);
                log.Text = ex.ToString();
                MessageBoxX.Show(this,ex.ToString(),"Error",MessageBoxIcon.Warning,DefaultButton.YesOK);
            }
        }
        private void ResDwCb(object sender,AsyncCompletedEventArgs e)
        {
            pro.Visibility = Visibility.Collapsed;
            ZipArchive archive = ZipFile.Open(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CokeeWapp\\res.zip",ZipArchiveMode.Read);
            if(File.Exists(Environment.SpecialFolder.MyDocuments + "\\CokeeWapp\\ver")) Directory.Delete(Environment.SpecialFolder.MyDocuments + "\\CokeeWapp");
            archive.ExtractToDirectory(Environment.SpecialFolder.MyDocuments + "\\CokeeWapp");
            if(File.Exists(Environment.SpecialFolder.MyDocuments + "\\CokeeWapp\\ver")) log.Text = "资源包下载成功.";
        }
        private void Updatecb(object sender,AsyncCompletedEventArgs e)
        {
            pro.Visibility = Visibility.Collapsed;
            ZipArchive archive = ZipFile.Open(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CokeeWapp\\update\\update.zip",ZipArchiveMode.Read);
            if(Directory.Exists(Environment.SpecialFolder.MyDocuments + "\\CokeeWapp\\update\\unzip")) Directory.Delete(Environment.SpecialFolder.MyDocuments + "\\CokeeWapp\\update\\unzip");
            archive.ExtractToDirectory(Environment.SpecialFolder.MyDocuments + "\\CokeeWapp\\update\\unzip");
        }
        //downing
        private void DownloadProgressCallback(object sender,DownloadProgressChangedEventArgs e)
        {
            if(pro.Visibility != Visibility.Visible) pro.Visibility = Visibility.Visible;
            pro.Value = e.ProgressPercentage;
            log.Text = "正在加载" + nowDowning + "... " + e.ProgressPercentage + "% " + e.BytesReceived / 1048576 + "MB of" + e.TotalBytesToReceive / 1048576;
        }

        private async Task CheckUpdate()
        {
            try
            {
                var client = new HttpClient(); var a = new WebClient(); var uri = "";
                var u2 = await client.GetStringAsync("https://gitee.com/api/v5/repos/cokee/CokeeDisplayProtect/releases?page=1&per_page=1&direction=desc ");
                JObject dt = JsonConvert.DeserializeObject<JObject>(u2);
                if((double)dt[0]["name"] > ver)
                    if(dt[0]["assets"][0]["name"].ToString() != "update.zip" && dt[0]["assets"][1]["name"].ToString() == "update.zip") uri = dt[0]["assets"][1]["browser_download_url"].ToString();
                    else uri = dt[0]["assets"][0]["browser_download_url"].ToString();
                a.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressCallback);
                a.DownloadFileCompleted += new AsyncCompletedEventHandler(Updatecb);
                a.DownloadFileAsync(new Uri(uri),Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CokeeWapp\\update\\update.zip");
            }
            catch(Exception ex)
            {
                ProcessErr(ex);
                log.Text = ex.ToString();
            }
        }
        public void Appear(FrameworkElement element,int direction = 0,int distance = 20,double duration = .3)
        {
            //将所选控件的Visibility属性改为Visible，这里要首先执行否则动画看不到
            ObjectAnimationUsingKeyFrames VisbilityAnimation = new ObjectAnimationUsingKeyFrames();
            DiscreteObjectKeyFrame kf = new DiscreteObjectKeyFrame(Visibility.Visible,new TimeSpan(0,0,0));
            VisbilityAnimation.KeyFrames.Add(kf);
            element.BeginAnimation(Border.VisibilityProperty,VisbilityAnimation);

            //创建新的缩放动画
            TranslateTransform TT = new TranslateTransform();
            element.RenderTransform = TT;
            //创建缩放动画函数，可以自己修改
            EasingFunctionBase easeFunction = new CircleEase() { EasingMode = EasingMode.EaseInOut };

            //判断动画方向
            if(direction == 0)
            {
                DoubleAnimation Animation = new DoubleAnimation(-distance,0,new Duration(TimeSpan.FromSeconds(duration)));
                Animation.EasingFunction = easeFunction;
                element.RenderTransform.BeginAnimation(TranslateTransform.YProperty,Animation);
            }
            else if(direction == 1)
            {
                DoubleAnimation Animation = new DoubleAnimation(distance,0,new Duration(TimeSpan.FromSeconds(duration)));
                Animation.EasingFunction = easeFunction;
                element.RenderTransform.BeginAnimation(TranslateTransform.XProperty,Animation);
            }
            else if(direction == 2)
            {
                DoubleAnimation Animation = new DoubleAnimation(distance,0,new Duration(TimeSpan.FromSeconds(duration)));
                Animation.EasingFunction = easeFunction;
                element.RenderTransform.BeginAnimation(TranslateTransform.YProperty,Animation);
            }
            else if(direction == 3)
            {
                DoubleAnimation Animation = new DoubleAnimation(-distance,0,new Duration(TimeSpan.FromSeconds(duration)));
                Animation.EasingFunction = easeFunction;
                element.RenderTransform.BeginAnimation(TranslateTransform.XProperty,Animation);
            }
            else throw new Exception("无效的方向！");

            //将所选控件的可见度按动画函数方式显现
            DoubleAnimation OpacityAnimation = new DoubleAnimation(0,1,new Duration(TimeSpan.FromSeconds(duration)));
            OpacityAnimation.EasingFunction = easeFunction;
            element.BeginAnimation(Border.OpacityProperty,OpacityAnimation);
        }

        /// <summary>
        /// 淡出动画(控件名, 0：上方；1：右方；2：下方；3：左方, 淡出的距离，持续时间)
        /// </summary>
        /// <param name="element">控件名</param>
        /// <param name="direction">0：上方；1：右方；2：下方；3：左方</param>
        /// <param name="distance">淡出的距离</param>
        /// <param name="duration">持续时间</param>
        public void Disappear(FrameworkElement element,int direction = 0,int distance = 20,double duration = .3)
        {
            //创建新的缩放动画
            TranslateTransform TT = new TranslateTransform();
            element.RenderTransform = TT;
            //创建缩放动画函数，可以自己修改
            EasingFunctionBase easeFunction = new CircleEase() { EasingMode = EasingMode.EaseInOut };

            //判断动画方向
            if(direction == 0)
            {
                DoubleAnimation Animation = new DoubleAnimation(-distance,new Duration(TimeSpan.FromSeconds(duration)));
                Animation.EasingFunction = easeFunction;
                element.RenderTransform.BeginAnimation(TranslateTransform.YProperty,Animation);
            }
            else if(direction == 1)
            {
                DoubleAnimation Animation = new DoubleAnimation(distance,new Duration(TimeSpan.FromSeconds(duration)));
                Animation.EasingFunction = easeFunction;
                element.RenderTransform.BeginAnimation(TranslateTransform.XProperty,Animation);
            }
            else if(direction == 2)
            {
                DoubleAnimation Animation = new DoubleAnimation(distance,new Duration(TimeSpan.FromSeconds(duration)));
                Animation.EasingFunction = easeFunction;
                element.RenderTransform.BeginAnimation(TranslateTransform.YProperty,Animation);
            }
            else if(direction == 3)
            {
                DoubleAnimation Animation = new DoubleAnimation(-distance,new Duration(TimeSpan.FromSeconds(duration)));
                Animation.EasingFunction = easeFunction;
                element.RenderTransform.BeginAnimation(TranslateTransform.XProperty,Animation);
            }
            else
                throw new Exception("无效的方向！");

            //将所选控件的可见度按动画函数方式消失
            DoubleAnimation OpacityAnimation = new DoubleAnimation(1,0,new Duration(TimeSpan.FromSeconds(duration)));
            OpacityAnimation.EasingFunction = easeFunction;
            element.BeginAnimation(Border.OpacityProperty,OpacityAnimation);

            //将所选控件的Visibility属性改为Collapsed，这样不占用空间
            ObjectAnimationUsingKeyFrames VisbilityAnimation = new ObjectAnimationUsingKeyFrames();
            DiscreteObjectKeyFrame kf = new DiscreteObjectKeyFrame(Visibility.Collapsed,new TimeSpan(0,0,1));
            VisbilityAnimation.KeyFrames.Add(kf);
            element.BeginAnimation(Border.VisibilityProperty,VisbilityAnimation);
        }
    }
}
