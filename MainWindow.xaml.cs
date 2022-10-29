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
using System.Windows.Media.Imaging;

namespace CokeeDP
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
        private static Timer hkc;
        private static Timer sec;
        private static Timer wet;
        private FileInfo[] afi;
        private FileInfo[] audio;
        private int AudioNum = 0;
        private String AudioPath, AudioFolder, MediaDuring;
        private int bgn = 0, bing = 0;
        BitmapImage bitmapImage = null;
        //U盘插入后，OS的底层会自动检测，然后向应用程序发送“硬件设备状态改变“的消息
        private string disk, weaWr, hkUrl, nowDowning = "";

        private bool IsPlaying = false, AudioLoaded = false, IsReplay = true;
        private MediaPlayer mediaplayer = new MediaPlayer();
        private DateTime tod;
        private bool usingBing = true;
        private double ver = 2.2;

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
                    zk.Visibility = Visibility.Visible;
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
                DirectoryInfo dir = new DirectoryInfo(AudioFolder);
                if(dir.Exists)audio = dir.GetFiles("*.mp3");
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
                    bing++;
                    if(bing >= 8 || bing <= -2) bing = 0;
                    _ = GetBingWapp();
                    return;
                }
                #region non-bing
                else
                {
                    Uri bgp;
                    if(direction)
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
                if(Convert.ToInt32(Properties.Settings.Default.hkc) <= 100) Properties.Settings.Default.hkc = "100";
                if(Convert.ToInt32(Properties.Settings.Default.wea) <= 9800) Properties.Settings.Default.wea = "9800";
                if(Properties.Settings.Default.timeTo.Length <= 1) Properties.Settings.Default.timeTo = "高考";
                if(Properties.Settings.Default.isDebug) log.Visibility = Visibility.Visible;
                usingBing = Properties.Settings.Default.usingBing;
                AudioFolder = Properties.Settings.Default.AudioFolder;
                Properties.Settings.Default.Save();
                SetTimer(sec,1,hkc,Convert.ToInt32(Properties.Settings.Default.hkc),wet,Convert.ToInt32(Properties.Settings.Default.wea));
               
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
                ChangeWapp(true);
                _ = Hitoko();
            }
       ));
        }

        public void OnNewDay()
        {

        }
        public static ImageSource GetExeIcon(string fileName)
        {
            System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(fileName);
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                        icon.Handle,
                        new Int32Rect(0,0,icon.Width,icon.Height),
                        BitmapSizeOptions.FromEmptyOptions());
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
                    slider.Value = mediaplayer.Position.TotalSeconds;
                    //slider.Maximum = mediaplayer.NaturalDuration.TimeSpan.TotalSeconds;
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
            if(e.StackTrace.Contains("Socket")) NoticeBox.Show(e.ToString(),"Error/网络错误v",MessageBoxIcon.Error,true,1000);
            else NoticeBox.Show(e.ToString(),"Error",MessageBoxIcon.Error,true,5000);
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

        private static string GetWeatherIcon(int code)
        {
            string icon = "";
            if(code < 0 || code == 99) return "\ue94f";//no network
            else if(code <= 3 && DateTime.Now.Hour <= 18 || code == 38) icon = "\ue978";//sunny
            else if(code <= 3 && DateTime.Now.Hour >= 18) icon = "\ue97a";//sunny_night
            else if(code >= 4 && code <= 9) icon = "\ue97c";//cloudy
            else if(code >= 10 && code <= 19) icon = "\ue97e";//rainy
            else if(code >= 20 && code <= 25) icon = "\ue982";//snowy
            else if(code >= 26 && code <= 29) icon = "\ue9c6";//dust
            else if(code >= 30 && code <= 36) icon = "\ue9a0";//windy
            return icon;
        }

        private async Task DownloadResPack()
        {
            var client = new HttpClient(); var a = new WebClient();
            var u2 = await client.GetStringAsync("https://gitee.com/cokee/CokeeDisplayProtect/raw/main/resurl");
            a.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressCallback);
            a.DownloadFileCompleted += new AsyncCompletedEventHandler(ResDwCb);
            a.DownloadFileAsync(new Uri(u2),Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CokeeWapp\\res.zip");
            nowDowning = "资源包";
        }

        private async Task GetBingWapp()
        {


            var client = new HttpClient();var a= new WebClient();
            var u2 = await client.GetStringAsync("https://cn.bing.com/HPImageArchive.aspx?format=js&idx=" + bing + "&n=1");
            JObject dt = JsonConvert.DeserializeObject<JObject>(u2);
            if (File.Exists(Environment.CurrentDirectory + "\\blkimg.txt") && File.ReadAllText(Environment.CurrentDirectory + "\\blkimg.txt").Contains(dt["images"][0]["enddate"].ToString())) { ChangeWapp(false);return; }
            BingImageInfo.Content = dt["images"][0]["copyright"] + " | " + dt["images"][0]["enddate"].ToString();
            var urlstr = "https://www.bing.com/" + dt["images"][0]["url"];
            if(Properties.Settings.Default.IsUHDWapp) urlstr = urlstr.Replace("_1920x1080","_UHD");
            Uri uri = new Uri(urlstr);
            log.Text = bing + "/LoadBingImage:" + uri;
           bitmapImage= new BitmapImage(uri);
            bitmapImage.DownloadProgress += ImageDownloadProgress;
           bitmapImage.DownloadCompleted += DownloadImageCompleted;
            //     DoubleAnimation doubleAnimation;
            br1.Tag = dt["images"][0]["enddate"].ToString();
            br1_blur.Radius = 10;
            bitmapImage.BeginInit();
          /* 
           a.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressCallback);
            a.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCallback);
            a.DownloadFileAsync(uri, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CokeeWapp\\bing.jpg");      */

        }

        private void ImageDownloadProgress(object sender,DownloadProgressEventArgs e)
        {
            if(pro.Visibility != Visibility.Visible) pro.Visibility = Visibility.Visible;
            pro.Value = e.Progress; 
            log.Text = "正在加载壁纸" + e.Progress+"%"; 
        }

        private void DownloadImageCompleted(object sender,EventArgs e)
        {
            if(pro.Visibility != Visibility.Collapsed) pro.Visibility = Visibility.Collapsed;
            log.Text = "Done.";
           br1_blur.Radius = 0;
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
            var a = (Button)sender;
            if (a.Name == "left") ChangeWapp(false);
            else if (a.Name == "right") ChangeWapp(true);
            if(bing >= 8 || bing <= -1) bing = 0;
        }

        private async Task Wea()
        {
            try
            {
                var client = new HttpClient();
                var u2 = await client.GetStringAsync("http://api.seniverse.com/v3/weather/daily.json?key=SISi82MwzaMbmQqSh&location=" + Properties.Settings.Default.city + "&language=zh-Hans&unit=c&start=0&days=3");
                JObject dt = JsonConvert.DeserializeObject<JObject>(u2);
                wea1.Text = "今天 " + dt["results"][0]["daily"][0]["text_day"].ToString() + "," + dt["results"][0]["daily"][0]["high"].ToString() + "°C~" + dt["results"][0]["daily"][0]["low"].ToString() + "°C 湿度:" + dt["results"][0]["daily"][0]["humidity"].ToString();
                wea2.Text = "明天 " + dt["results"][0]["daily"][1]["text_day"].ToString() + "," + dt["results"][0]["daily"][1]["high"].ToString() + "°C~" + dt["results"][0]["daily"][1]["low"].ToString() + "°C 湿度:" + dt["results"][0]["daily"][1]["humidity"].ToString();
                wea3.Text = "后天 " + dt["results"][0]["daily"][2]["text_day"].ToString() + "," + dt["results"][0]["daily"][2]["high"].ToString() + "°C~" + dt["results"][0]["daily"][2]["low"].ToString() + "°C 湿度:" + dt["results"][0]["daily"][2]["humidity"].ToString();
                w1.Text = GetWeatherIcon((int)dt["results"][0]["daily"][0]["code_day"]);
                w2.Text = GetWeatherIcon((int)dt["results"][0]["daily"][1]["code_day"]);
                w3.Text = GetWeatherIcon((int)dt["results"][0]["daily"][2]["code_day"]);
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
                }
                else
                {
                    var t = dt["warning"][0]["title"].ToString();
                    SpecialWeatherBtn.Visibility = Visibility.Visible;

                    SpecialWeatherBtn.Content = t.Substring(t.IndexOf("布") + 1);
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
                var win1 = new Settings();
                win1.Show();
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
                ChangeWapp(true);
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
        ///元素highlight处理
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
                else if(music.Visibility == Visibility.Visible) { music.Visibility = Visibility.Collapsed; IsPlaying = false; mediaplayer.Pause(); }
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
                playIcon.Text = "";
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
            mediaplayer.Position = TimeSpan.FromSeconds(slider.Value);
            audioTime.Content = mediaplayer.Position.ToString(@"mm\:ss") + " / " + mediaplayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
        }

        private void ChangeVolume(object sender,RoutedEventArgs e)
        {
            try
            {
                var tmp = (Button)sender;
                if(tmp.Content.ToString() == "音量")//only 2 modes
                {
                    CancelTheMute();
                    slider_Copy.Visibility = Visibility.Visible;
                    slider_Copy.Value = GetCurrentSpeakerVolume();
                    tmp.Content = "音量 ";
                }
                else if(tmp.Content.ToString() == "音量 ")
                {
                    slider_Copy.Visibility = Visibility.Collapsed;
                    tmp.Content = "音量";
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
                slider.Value = 0;
                slider.Maximum = mediaplayer.NaturalDuration.TimeSpan.TotalSeconds;
                MediaDuring = mediaplayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
                audioTime.Content = "0:00/" + mediaplayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
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
                    playIcon.Text = "";
                    IntlPlayer();
                    slider.Value = 0;
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
                if(tmp.Content.ToString() == "循环开")
                {
                    IsReplay = false;
                    tmp.Content = "循环关";
                }
                else if(tmp.Content.ToString() == "循环关")
                {
                    IsReplay = true;
                    tmp.Content = "循环开";
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
                    playIcon.Text = "";
                }
                else if(tmp.Tag.ToString() == "next")
                {
                    if(AudioNum >= audio.Length) AudioNum = 0;
                    else AudioNum++;
                    playIcon.Text = "";
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
                SetCurrentSpeakerVolume(Convert.ToInt32(slider_Copy.Value));
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
                    playIcon.Text = "";
                    audioTime.Content = "[Paused]" + audioTime.Content;
                    //var tmp = (ButtonHelper)playbtn;
                    IsPlaying = false;
                    mediaplayer.Pause();
                }
                else
                {
                    playIcon.Text = "";
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
            var a = (TextBox)sender;
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
            NoticeBox.Show("https://hitokoto.cn/?uuid=" + hkUrl,"info");
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
    }
}
