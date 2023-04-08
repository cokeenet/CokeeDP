using AForge.Video.DirectShow;
using Microsoft.AppCenter.Crashes;
using NAudio.CoreAudioApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
using System.Threading;
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
using Wpf.Ui.Appearance;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Wpf.Ui.Mvvm.Services;
using Button = Wpf.Ui.Controls.Button;
using Clipboard = Wpf.Ui.Common.Clipboard;
using Directory = System.IO.Directory;
using File = System.IO.File;
//using MessageBox = Wpf.Ui.Controls.MessageBox;
using Point = System.Windows.Point;
using Timer = System.Timers.Timer;

namespace CokeeDP.Views.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public class HwItem
    {
        public string Name
        {
            get; set;
        }

        public DateTime DateTime
        {
            get; set;
        }
        public int Id
        {
            get; set;
        }
        public string Desc
        {
            get; set;
        }
    }

    public partial class MainWindow : Window
    {
        Point _startPosition;
        //滚动条当前位置
        double _startVerticalOffset;
        double _startHorizontalOffset;
        public const int DBT_DEVICEARRIVAL = 0x8000;  //设备可用
        public const int DBT_DEVICEREMOVECOMPLETE = 0x8004; //设备或媒体被删除
        public const int FILE_SHARE_READ = 0x1;
        public const int FILE_SHARE_WRITE = 0x2;
        public const uint GENERIC_READ = 0x80000000;
        public const int GENERIC_WRITE = 0x40000000;
        public const int IOCTL_STORAGE_EJECT_MEDIA = 0x2d4808;
        public const int WM_DEVICECHANGE = 0x219;
        private static Timer OneWordsTimer;
        private static Timer SecondTimer;
        private static Timer WeatherTimer;
        private FileInfo[] ImageArray;
        private FileInfo[] AudioArray;
        private int AudioNum = 0;
        private String AudioPath, AudioFolder, MediaDuring;
        private int bgn = 0, bing = 0;
        private BitmapImage bitmapImage = null;
        private string disk, weaWr, hkUrl, nowDowning = "";

        private SnackbarService snackbarService;
        private bool IsPlaying = false, AudioLoaded = false;
        private int PlayingRule = 0;
        private MediaPlayer mediaplayer = new MediaPlayer();
        private DateTime CountDownTime;
        private bool UsingBing = true;
        public string Version = "Ver 3.1";
        public double ver = 3.1;

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
                if (Properties.Settings.Default.EnableBigTimeTo)
                {
                    BigCountdown.Visibility = Visibility.Visible;
                }
                if (UsingBing)
                {
                    _ = GetBingWapp();
                }
                else
                {
                    if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CokeeWapp")) Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CokeeWapp");
                    DirectoryInfo AudioDir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CokeeWapp");
                    ImageArray = AudioDir.GetFiles("*.png"); ChangeWapp(false);
                }
                TimeLabel.Content = DateTime.Now.ToString("HH:mm:ss");
                if (Directory.Exists(AudioFolder))
                {
                    DirectoryInfo dir = new DirectoryInfo(AudioFolder);
                    if (dir.Exists)
                    {
                        AudioArray = dir.GetFiles("*.mp3");
                    }
                }
                //MessageBoxX.Show(AudioArray.Length.ToString());
                //MessageBoxX.Show(Environment.OSVersion.Version.Major.ToString());
                //hitokoto.Content = "hello world!";
            }
            catch (Exception e)
            {
                ProcessErr(e);
            }
        }

        public void ChangeWapp(bool direction)
        {
            try
            {
                if (UsingBing)
                {
                    if (bing >= 6 || bing <= 0) bing = 0;
                    if (!direction) bing++;
                    else bing--;
                    if (bing >= 6 || bing <= 0) bing = 0;
                    _ = GetBingWapp();
                    return;
                }

                #region non-bing

                else
                {
                    Uri bgp;
                    if (!direction)
                    {
                        if (bgn == 0)
                        {
                            bgp = new Uri(ImageArray[1].FullName);
                            bgn = ImageArray.Length;
                        }
                        else if (bgn <= ImageArray.Length)
                        {
                            bgp = new Uri(ImageArray[1].FullName);
                            bgn = ImageArray.Length - 1;
                        }
                        else
                        {
                            bgp = new Uri(ImageArray[bgn].FullName);
                            bgn--;
                        }
                    }
                    else
                    {
                        if (bgn == 0)
                        {
                            bgp = new Uri(ImageArray[1].FullName);
                            bgn = 1;
                        }
                        else if (bgn >= ImageArray.Length)
                        {
                            bgp = new Uri(ImageArray[1].FullName);
                            bgn = 1;
                        }
                        else
                        {
                            bgp = new Uri(ImageArray[bgn].FullName);
                            bgn++;
                        }
                    }

                    br1.BeginInit();
                    br1.Source = new BitmapImage(bgp);
                    br1.EndInit();
                    log.Text = bgn + "/LoadLocalPic:" + bgp.ToString();

                    #endregion non-bing
                }
            }
            catch (Exception ex)
            {
                ProcessErr(ex);
            }
        }

        public void CheckTasks()
        {
        }

        public void FillConfig()
        {
            try
            {
                if (Properties.Settings.Default.CountdownTime.Year <= 2000)
                {
                    CountDownTime = DateTime.Parse("2025/06/05");
                    Properties.Settings.Default.CountdownTime = CountDownTime;
                }
                else CountDownTime = Properties.Settings.Default.CountdownTime;
                if (Properties.Settings.Default.OneWordsApi.Length == 0) Properties.Settings.Default.OneWordsApi = "https://v1.hitokoto.cn/?c=k";
                if (Convert.ToInt32(Properties.Settings.Default.OneWordsTimeInterval) <= 300) Properties.Settings.Default.OneWordsTimeInterval = "300";
                if (Convert.ToInt32(Properties.Settings.Default.WeatherTimeInterval) <= 9800) Properties.Settings.Default.WeatherTimeInterval = "9800";
                if (Properties.Settings.Default.CountdownName.Length <= 1) Properties.Settings.Default.CountdownName = "高考";
                if (Properties.Settings.Default.isDebug) log.Visibility = Visibility.Visible;
                UsingBing = Properties.Settings.Default.BingWappEnable;
                AudioFolder = Properties.Settings.Default.AudioFolder;
                Properties.Settings.Default.Save();
                SetTimer(SecondTimer, 1, OneWordsTimer, Convert.ToInt32(Properties.Settings.Default.OneWordsTimeInterval), WeatherTimer, Convert.ToInt32(Properties.Settings.Default.WeatherTimeInterval));
            }
            catch (Exception ex)
            {
                ProcessErr(ex);
                Properties.Settings.Default.OneWordsTimeInterval = "300";
                Properties.Settings.Default.WeatherTimeInterval = "9800";
            }
        }

        public void OnHitokoUpd(object source, ElapsedEventArgs d)
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
            snackbarService.ShowAsync("是新的一天!", "哇你还没睡觉啊>_<", SymbolRegular.WeatherMoon16);
        }

        public void OnOneSecondTimer(object source, ElapsedEventArgs e)
        {
            try
            {
                Dispatcher.Invoke(new Action(delegate
            {
                TimeLabel.Content = DateTime.Now.ToString("HH:mm:ss");
                timeTo.Content = DateTime.Now.ToString("ddd,M月dd日")
                if (Properties.Settings.Default.EnableBigTimeTo)
                {
                    tod_info.Content = "还有" + CountDownTime.Subtract(DateTime.Now).TotalDays + "天";
                    big_tod.Content = ((int)CountDownTime.Subtract(DateTime.Now).TotalDays);
                }
                else
                    CountDownLabel.Content = "距离[" + Properties.Settings.Default.CountdownName + "]还有" + CountDownTime.Subtract(DateTime.Now).TotalDays + "天";
                if (DateTime.Now.Hour == 0 && DateTime.Now.Minute == 0 && DateTime.Now.Second == 0)
                {
                    OnNewDay();
                }
                if (IsPlaying)
                {
                    // audioTime.Content = mediaplayer.Position.ToString(@"mm\:ss") + "/" + MediaDuring;
                    PlaySlider.Value = mediaplayer.Position.TotalSeconds;
                    //PlaySlider.Maximum = mediaplayer.NaturalDuration.TimeSpan.TotalSecondTimeronds;
                }
                // new Thread(CheckTasks).Start(); // 创建线程
            }));
            }
            catch (Exception ex)
            {
                ProcessErr(ex);
            }
        }

        public void ProcessErr(Exception e)
        {
            if (this.IsLoaded)
            {
                snackbarService.SetSnackbarControl(snackbar);
                snackbarService.ShowAsync("发生错误", e.Message + e.StackTrace, SymbolRegular.ErrorCircle24);
            }
            Log.Error(e, "Error");
            log.Text = e.ToString();
            if (Environment.OSVersion.Version.Major >= 10.0) Crashes.TrackError(e);
        }

        public void SetTimer(Timer a, int ms, Timer b, int ms1, Timer c, int ms2)
        {
            // Create timers with a interval.
            a = new Timer(ms * 1000); a.Elapsed += new ElapsedEventHandler(OnOneSecondTimer); a.AutoReset = true; a.Enabled = true;
            b = new Timer(ms1 * 1000); b.Elapsed += new ElapsedEventHandler(OnHitokoUpd); b.AutoReset = true; b.Enabled = true;
            c = new Timer(ms2 * 1000); c.Elapsed += new ElapsedEventHandler(OnWea); c.AutoReset = true; c.Enabled = true;
        }

        public Stream GetWeatherIcon(int code)
        {
            try
            {
                System.Reflection.Assembly assembly = GetType().Assembly;
                Stream streamSmall = assembly.GetManifestResourceStream("CokeeDP.Icons." + code.ToString() + "-fill.svg");
                if (streamSmall == null) streamSmall = assembly.GetManifestResourceStream("CokeeDP.Icons." + code.ToString() + ".svg");
                return streamSmall;
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
                if (Properties.Settings.Default.BlockedImageIds.Contains(dt["data"]["images"][bing]["isoDate"].ToString()))
                {
                    ChangeWapp(false);
                    return;
                }
                BingImageInfo.Content = dt["data"]["images"][bing]["title"].ToString() + " (" + dt["data"]["images"][bing]["copyright"] + ")  | " + dt["data"]["images"][bing]["isoDate"].ToString();
                var urlstr = "https://www.bing.com/" + dt["data"]["images"][bing]["imageUrls"]["landscape"]["highDef"];
                CardInfo.Content = dt["data"]["images"][bing]["caption"].ToString();
                DescPara1.Text = dt["data"]["images"][bing]["description"] + Environment.NewLine + dt["data"]["images"][bing]["descriptionPara2"] + Environment.NewLine + dt["data"]["images"][bing]["descriptionPara3"];
                if (Properties.Settings.Default.IsUHDWapp) urlstr = urlstr.Replace("_1920x1080", "_UHD");
                Uri uri = new Uri(urlstr);
                log.Text = bing + "/LoadBingImage:" + uri;
                bitmapImage = new BitmapImage(uri);
                bitmapImage.DownloadProgress += ImageDownloadProgress;
                bitmapImage.DownloadCompleted += DownloadImageCompleted;
                br1.Tag = dt["data"]["images"][bing]["isoDate"].ToString();
                DoubleAnimation animation = new DoubleAnimation(0, 20, new Duration(TimeSpan.FromSeconds(5)));
                animation.EasingFunction = new CircleEase();
                //animation.AutoReverse = true;
                br1_blur.BeginAnimation(BlurEffect.RadiusProperty, animation);
                //pro_Copy.Value = bing + 1;
                //bitmapImage.BeginInit();
            }
            catch (Exception ex)
            {
                ProcessErr(ex);
            }
        }

        private void ImageDownloadProgress(object sender, DownloadProgressEventArgs e)
        {
            if (pro.Visibility != Visibility.Visible) pro.Visibility = Visibility.Visible;
            pro.Value = e.Progress;
            log.Text = "LoadBingImage (" + e.Progress + "% )";
        }

        private void DownloadImageCompleted(object sender, EventArgs e)
        {
            try
            {
                if (pro.Visibility != Visibility.Collapsed) pro.Visibility = Visibility.Collapsed;
                log.Text = "Image Loaded. Num:" + bing;
                DoubleAnimation animation = new DoubleAnimation(20, 0, new Duration(TimeSpan.FromSeconds(5)));
                animation.EasingFunction = new CircleEase();
                //animation.AutoReverse = true;
                br1_blur.BeginAnimation(BlurEffect.RadiusProperty, animation);
                br1.Source = bitmapImage;
            }
            catch (Exception ex)
            {
                ProcessErr(ex);
            }
            // Disappear(pro,1,20,0.5);
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
                JObject dt = JsonConvert.DeserializeObject<JObject>(await client.GetStringAsync(Properties.Settings.Default.OneWordsApi));
                string who = dt["from_who"].ToString();
                hkUrl = dt["uuid"].ToString();
                hitokoto.Content = who == "null"
                    ? dt["hitokoto"].ToString() + "--《" + dt["from"].ToString() + "》"
                    : dt["hitokoto"].ToString() + "--《" + dt["from"].ToString() + "》" + dt["from_who"].ToString();
            }
            catch (Exception ex)
            {
                ProcessErr(ex);
            }
        }

        private void Load(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
                this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
                snackbarService = new SnackbarService();
                snackbarService.SetSnackbarControl(snackbar);
                //ThemeService themeService = new ThemeService();
                //themeService.SetTheme(ThemeType.Light);//TODO
                Theme.Apply(ThemeType.Light, BackgroundType.Auto);
                HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;
                DriveInfo[] s = DriveInfo.GetDrives();
                s.Any(t =>
                {
                    if (t.DriveType == DriveType.Removable)
                    {
                        ShowUsbCard(false, t);
                        return true;
                    }
                    return false;
                });
                _ = Hitoko();
                _ = GetWeatherInfo(); videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                hwndSource.AddHook(new HwndSourceHook(WndProc));//挂钩

                if (Properties.Settings.Default.SnowEnable) { StartSnowing(MainCanvas); }
            }
            catch (Exception ex)
            {
                ProcessErr(ex);
            }
        }

        private void OnWea(object sender, ElapsedEventArgs e)
        {
            //async get http wea info
            Dispatcher.Invoke(new Action(delegate
            {
                _ = GetWeatherInfo();
            }
            ));
        }

        private void WappChangeBtnHandler(object sender, RoutedEventArgs e)
        {
            var a = (Button)sender; if (bing >= 8 || bing <= -1) bing = 0;
            if (a.Name == "left") ChangeWapp(false);
            else if (a.Name == "right") ChangeWapp(true);
        }

        private async Task GetWeatherInfo()
        {
            try
            {
                string u2, u3;
                if (DateTime.Now.Subtract(Properties.Settings.Default.CachedWeatherTime).Hours > 6 || !Properties.Settings.Default.CachedWeatherData.Contains("|"))
                {
                    var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };
                    var client = new HttpClient(handler);

                    u2 = await client.GetStringAsync("https://devapi.qweather.com/v7/weather/7d?location=" + Properties.Settings.Default.CityId + "&key=6572127bcec647faba394b17fbd9614f");
                    u3 = await client.GetStringAsync("https://devapi.qweather.com/v7/warning/now?location=" + Properties.Settings.Default.CityId + "&key=6572127bcec647faba394b17fbd9614f");
                    //MessageBoxX.Show("https://devapi.qweather.com/v7/warning/now?location=" + Properties.Settings.Default.CityId + " &key=6572127bcec647faba394b17fbd9614f");
                    Properties.Settings.Default.CachedWeatherData = u2 + "|" + u3;
                    Properties.Settings.Default.CachedWeatherTime = DateTime.Now;
                }
                else
                {
                    u2 = Properties.Settings.Default.CachedWeatherData.Split("|")[0];
                    u3 = Properties.Settings.Default.CachedWeatherData.Split("|")[1];
                }
                weaupd1.Text = Properties.Settings.Default.City + " 最近更新: " + Properties.Settings.Default.CachedWeatherTime.ToString("yyyy/MM/dd HH:mm:ss");
                weaupd2.Text = weaupd1.Text;
                JObject dt = JsonConvert.DeserializeObject<JObject>(u2);
                wea1.Text = "今天 " + dt["daily"][0]["textDay"].ToString() + "," + dt["daily"][0]["tempMax"].ToString() + "°C~" + dt["daily"][0]["tempMin"].ToString() + "°C 湿度:" + dt["daily"][0]["humidity"].ToString();
                wea2.Text = "明天 " + dt["daily"][1]["textDay"].ToString() + "," + dt["daily"][1]["tempMax"].ToString() + "°C~" + dt["daily"][1]["tempMin"].ToString() + "°C 湿度:" + dt["daily"][1]["humidity"].ToString();
                wea3.Text = "后天 " + dt["daily"][2]["textDay"].ToString() + "," + dt["daily"][2]["tempMax"].ToString() + "°C~" + dt["daily"][2]["tempMin"].ToString() + "°C 湿度:" + dt["daily"][2]["humidity"].ToString();
                wea4.Text = dt["daily"][3]["fxDate"].ToString().Substring(5) + " " + dt["daily"][3]["textDay"].ToString() + "," + dt["daily"][3]["tempMax"].ToString() + "°C~" + dt["daily"][3]["tempMin"].ToString() + "°C 湿度:" + dt["daily"][3]["humidity"].ToString();
                wea5.Text = dt["daily"][4]["fxDate"].ToString().Substring(5) + " " + dt["daily"][4]["textDay"].ToString() + "," + dt["daily"][4]["tempMax"].ToString() + "°C~" + dt["daily"][4]["tempMin"].ToString() + "°C 湿度:" + dt["daily"][4]["humidity"].ToString();
                wea6.Text = dt["daily"][5]["fxDate"].ToString().Substring(5) + " " + dt["daily"][5]["textDay"].ToString() + "," + dt["daily"][5]["tempMax"].ToString() + "°C~" + dt["daily"][5]["tempMin"].ToString() + "°C 湿度:" + dt["daily"][5]["humidity"].ToString();
                w1.StreamSource = GetWeatherIcon((int)dt["daily"][0]["iconDay"]);
                w2.StreamSource = GetWeatherIcon((int)dt["daily"][1]["iconDay"]);
                w3.StreamSource = GetWeatherIcon((int)dt["daily"][2]["iconDay"]);
                w4.StreamSource = GetWeatherIcon((int)dt["daily"][3]["iconDay"]);
                w5.StreamSource = GetWeatherIcon((int)dt["daily"][4]["iconDay"]);
                w6.StreamSource = GetWeatherIcon((int)dt["daily"][5]["iconDay"]);
                JObject dt1 = JsonConvert.DeserializeObject<JObject>(u3);

                if (!dt1.ContainsKey("warning") || dt1["code"].ToString() != "200")
                    throw new HttpRequestException("天气预警加载失败。网络异常。CODE:" + dt1["code"].ToString());
                if (!dt1["warning"].HasValues)
                {
                    SpecialWeatherBtn.Visibility = Visibility.Collapsed;
                    SpecialWeatherBtn1.Visibility = Visibility.Collapsed;
                }
                else
                {
                    var t = dt1["warning"][0]["title"].ToString();
                    SpecialWeatherBtn.Visibility = Visibility.Visible;
                    SpecialWeatherBtn1.Visibility = Visibility.Visible;
                    string TextShort;
                    if (t.Contains("发布")) TextShort = t.Substring(t.IndexOf("布") + 1);
                    else TextShort = t.Substring(t.IndexOf("新") + 1);
                    SpecialWeatherBtn.Content = TextShort;
                    SpecialWeatherBtn1.Content = TextShort;
                    weaWr = dt1["warning"][0]["text"].ToString();
                }
            }
            catch (Exception ex)
            {
                Properties.Settings.Default.CachedWeatherData = "";
                ProcessErr(ex);
            }
        }

        private void hitokoto_MouseDown(object sender, MouseButtonEventArgs e) => _ = Hitoko();

        private Object locker1 = new Object();

        private void OnCloseWindow(object sender, MouseButtonEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                if (IsPlaying)
                {
                    Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                    messageBox.Content = "有媒体正在播放。确认关闭程序吗？";
                    messageBox.ButtonLeftName = "确认";
                    messageBox.ButtonRightName = "取消";
                    messageBox.MicaEnabled = true;

                    //messageBox.Icon = SymbolRegular.Warning28;
                    if ((bool)messageBox.ShowDialog()) Close();

                }
                else Close();
            }));
        }


        private void ShowUsbCard(bool isUnplug, DriveInfo t = null)
        {
            lock (locker1)
            {
                DoubleAnimation anim1 = new DoubleAnimation(0, TimeSpan.FromSeconds(1));
                DoubleAnimation anim2 = new DoubleAnimation(368, TimeSpan.FromSeconds(1));
                anim1.EasingFunction = new CircleEase();
                anim2.Completed += Anim3_Completed;
                anim2.EasingFunction = new CircleEase();
                if (!isUnplug)
                {
                    usb.Visibility = Visibility.Visible;
                    tranUsb.BeginAnimation(TranslateTransform.XProperty, anim1);
                    disk = t.Name;
                    diskName.Content = t.VolumeLabel + "(" + t.Name + ")";
                    diskInfo.Content = (t.TotalFreeSpace / 1073741824).ToString() + "GB/" + (t.TotalSize / 1073741824).ToString() + "GB";//TODO:改进算法，这个结果是错的
                }
                else if (isUnplug)
                {
                    tranUsb.BeginAnimation(TranslateTransform.XProperty, anim2);
                }
            }
        }

        private void Anim3_Completed(object sender, EventArgs e) => usb.Visibility = Visibility.Collapsed;

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            try
            {
                if (msg == WM_DEVICECHANGE)
                {
                    switch (wParam.ToInt32())
                    {
                        case DBT_DEVICEARRIVAL:
                            DriveInfo[] s = DriveInfo.GetDrives();
                            s.Any(t =>
                            {
                                if (t.DriveType == DriveType.Removable)
                                {
                                    ShowUsbCard(false, t);
                                    return true;
                                }
                                return false;
                            });
                            break;

                        case DBT_DEVICEREMOVECOMPLETE:
                            //MessageBox.Show("U盘卸载");
                            log.Text = "EVENT : Usbdrive.Uninstall";
                            ShowUsbCard(true);
                            break;

                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                ProcessErr(ex);
            }
            return IntPtr.Zero;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr CreateFile(
         string lpFileName,
         uint dwDesireAccess,
         uint dwShareMode,
         IntPtr SecondTimerurityAttributes,
         uint dwCreationDisposition,
         uint dwFlagsAndAttributes,
         IntPtr hTemplateFile);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
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

        private void ExitUsbDrive(object sender, RoutedEventArgs e)
        {
            try
            {
                string filename = @"\\.\" + disk.Remove(2);
                //打开设备，得到设备的句柄handle.
                IntPtr handle = CreateFile(filename, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, 0x3, 0, IntPtr.Zero);
                // 向目标设备发送设备控制码。IOCTL_STORAGE_EJECT_MEDIA-弹出U盘
                uint byteReturned;
                bool result = DeviceIoControl(handle, IOCTL_STORAGE_EJECT_MEDIA, IntPtr.Zero, 0, IntPtr.Zero, 0, out byteReturned, IntPtr.Zero);
                if (!result) snackbarService.ShowAsync("U盘退出失败", "请检查程序占用，关闭已打开的文件夹，PPT，WORD等。", SymbolRegular.Warning24);
                else ShowUsbCard(true);
            }
            catch (Exception ex)
            {
                ProcessErr(ex);
            }
        }

        private void ShowSetting(object sender, RoutedEventArgs e)
        {
            try
            {
                var settingsWindow = new SettingsWindow();
                settingsWindow.Owner = this;
                settingsWindow.Show();
            }
            catch (Exception ex)
            {
                ProcessErr(ex);
            }
        }

        /// <summary>
        ///星期标签点击处理
        /// </summary>
        private void DateLabelClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ChangeWapp(false);
                pager.PageDown();
                _ = Hitoko();
            }
            catch (Exception ex)
            {
                ProcessErr(ex);
            }
        }

        /// <summary>
        ///一言-右键菜单
        /// </summary>
        /// <param name="sender">(Label)</param>
        private void Like_menu(object sender, MouseButtonEventArgs e)
        {
            lik.PlacementTarget = hitokoto;
            lik.IsOpen = true;
        }

        /// <summary>
        ///元素拖动处理
        /// </summary>
        /// <param name="sender">(Label)被拖的标签</param>
        private void Drag(object sender, DragEventArgs e)
        {
            // return DragEventHandler.;
        }

        /// <summary>
        ///元素tempMaxlight处理
        /// </summary>
        /// <param name="sender">(Label)被拖的</param>
        private void Light(object sender, DragEventArgs e)
        {
            /* for (int i = 0; i < MainGrid.Children.Count; i++)
             {
                 MainGrid.Children[i].DragEnter+=Drag();
             }      */
        }

        private Object locker = new Object();
        private Object locker2 = new Object();

        private void ShowPlayer(object sender, MouseButtonEventArgs e)
        {
            try
            {
                lock (locker)
                {
                    DoubleAnimation anim1 = new DoubleAnimation(0, TimeSpan.FromSeconds(1));
                    DoubleAnimation anim2 = new DoubleAnimation(-365, TimeSpan.FromSeconds(1));
                    anim1.EasingFunction = new CircleEase();
                    anim2.Completed += Anim2_Completed;
                    anim2.EasingFunction = new CircleEase();
                    if (music.Visibility == Visibility.Collapsed)
                    {
                        music.Visibility = Visibility.Visible;
                        tranT.BeginAnimation(TranslateTransform.XProperty, anim1);
                    }
                    else if (music.Visibility == Visibility.Visible)
                    {
                        tranT.BeginAnimation(TranslateTransform.XProperty, anim2);
                        IsPlaying = false;
                        playbtn.Icon = SymbolRegular.Play48;
                        mediaplayer.Pause();
                    }
                }

                IntlPlayer();
            }
            catch (Exception ex)
            {
                ProcessErr(ex);
            }
        }

        private void Anim2_Completed(object sender, EventArgs e) => music.Visibility = Visibility.Collapsed;

        public void IntlPlayer()
        {
            try
            {
                if (!Directory.Exists(AudioFolder))
                {
                    throw new DirectoryNotFoundException("听力文件夹未找到 : " + AudioFolder);
                }
                if (Directory.Exists(AudioFolder) && AudioArray.Length == 0)
                {
                    DirectoryInfo dir = new DirectoryInfo(AudioFolder);
                    AudioArray = dir.GetFiles("*.mp3");
                    if (AudioArray.Length == 0) throw new FileNotFoundException("听力文件夹内没有.mp3文件。请转换音频为.mp3格式。");
                }
                if (AudioNum >= AudioArray.Length || AudioNum < 0) AudioNum = 0;
                AudioPath = AudioArray[AudioNum].FullName;
                mediaplayer.Open(new Uri(AudioArray[AudioNum].FullName));
                mediaplayer.Volume = 1;
                mediaplayer.MediaOpened += MediaLoaded;
                mediaplayer.MediaEnded += MediaEnded;
                audioName.Content = AudioArray[AudioNum].Name;

                //audioTime.Content = "[Paused]" + audioTime.Content;
                //var tmp = (ButtonHelper)playbtn;
                // IsPlaying = false;
            }
            catch (Exception ex)
            {
                ProcessErr(ex);
            }
        }

        private void SilderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (AudioLoaded == true)
            {
                mediaplayer.Position = TimeSpan.FromSeconds(PlaySlider.Value);
                audioTime.Content = mediaplayer.Position.ToString(@"mm\:ss") + " / " + mediaplayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
            }
        }

        private void ChangeVolume(object sender, RoutedEventArgs e)
        {
            try
            {
                var tmp = (Button)sender;
                if (VolumeSlider.Visibility == Visibility.Collapsed)
                {
                    CancelTheMute();
                    VolumeText.Visibility = Visibility.Visible;
                    VolumeSlider.Visibility = Visibility.Visible;
                    VolumeText.Content = "音量:" + GetCurrentSpeakerVolume() + "%";
                    VolumeSlider.Value = GetCurrentSpeakerVolume();
                }
                else if (VolumeSlider.Visibility == Visibility.Visible)
                {
                    VolumeSlider.Visibility = Visibility.Collapsed;
                    VolumeText.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                ProcessErr(ex);
            }
        }

        private void MediaLoaded(object sender, EventArgs e)
        {
            try
            {
                audioName.Content = AudioArray[AudioNum].Name;
                PlaySlider.Value = 0;
                PlaySlider.Maximum = mediaplayer.NaturalDuration.TimeSpan.TotalSeconds;
                MediaDuring = mediaplayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
                audioTime.Content = "00:00/" + mediaplayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
                AudioLoaded = true;
                /* playIcon.Text = "";
                 IsPlaying = true;
                 mediaplayer.Play();*/
            }
            catch (Exception ex)
            {
                ProcessErr(ex);
            }
        }

        private void MediaEnded(object sender, EventArgs e)
        {
            try
            {
                if (PlayingRule == 1)
                {
                    //playIcon.Text = "";
                    IntlPlayer();
                    PlaySlider.Value = 0;
                    return;
                }
                else if (PlayingRule == 2)
                {
                    if (AudioNum >= AudioArray.Length) AudioNum = 0;
                    else AudioNum++;
                    IntlPlayer();
                }
                else 
                {
                    IsPlaying = false;
                }
            }
            catch (Exception ex)
            {
                ProcessErr(ex);
            }
        }

        private void BtnReplayHandler(object sender, RoutedEventArgs e)
        {
            try
            {
                var tmp = (Button)sender;
                switch (tmp.Tag.ToString())
                {
                    case "0":
                        tmp.Content = "单曲循环";
                        tmp.Icon = SymbolRegular.ArrowRepeat124;
                        PlayingRule = 1;
                        tmp.Tag = "1";
                        break;
                    case "1":
                        tmp.Content = "列表循环";
                        tmp.Icon = SymbolRegular.ArrowRepeatAll24;
                        tmp.Tag = "2";
                        PlayingRule = 2;
                        break;
                    case "2":
                        tmp.Content = "播完停止";
                        tmp.Tag = "0";
                        tmp.Icon = SymbolRegular.ArrowRepeatAllOff24;
                        PlayingRule = 0;
                        break;
                    default:
                        break;
                }/*
                if (tmp.Content.ToString() == "单曲循环")
                {
                    IsReplay = false;
                    tmp.Content = "列表循环";
                }
                else if (tmp.Content.ToString() == "列表循环")
                {
                    IsReplay = true;
                    tmp.Content = "单曲循环";
                }*/
            }
            catch (Exception ex)
            {
                ProcessErr(ex);
            }
        }

        private void PlayerBtnProc(object sender, RoutedEventArgs e)
        {
            try
            {
                var tmp = (Button)sender;
                if (tmp.Tag.ToString() == "prev")
                {
                    if (AudioNum == 0) AudioNum = AudioArray.Length;
                    else AudioNum--;
                    playbtn.Icon = SymbolRegular.Play48;
                }
                else if (tmp.Tag.ToString() == "next")
                {
                    if (AudioNum >= AudioArray.Length) AudioNum = 0;
                    else AudioNum++;
                    playbtn.Icon = SymbolRegular.Play48;
                }
                IntlPlayer();
            }
            catch (Exception ex)
            {
                ProcessErr(ex);
            }
        }

        private void VolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                VolumeText.Content = "音量:" + Convert.ToInt32(e.NewValue) + "%";
                SetCurrentSpeakerVolume(Convert.ToInt32(e.NewValue));
            }
            catch (Exception ex)
            {
                ProcessErr(ex);
            }
        }

        private void OnBtnPlay(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IsPlaying)
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
            catch (Exception ex)
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
                IEnumerable<MMDevice> speakDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToArray();
                if (speakDevices.Count() > 0)
                {
                    MMDevice mMDevice = speakDevices.ToList()[0];
                    volume = Convert.ToInt16(mMDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
                }
                else throw new EntryPointNotFoundException("未找到音频设备");
                return volume;
            }
            catch (Exception ex)
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
                IEnumerable<MMDevice> speakDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToArray();
                if (speakDevices.Count() > 0)
                {
                    foreach (var mMDevice in speakDevices.ToList())
                    {
                        mMDevice.AudioEndpointVolume.MasterVolumeLevelScalar = volume / 100.0f;
                    };
                }
            }
            catch (Exception ex)
            {
                ProcessErr(ex);
            }
        }

        public void CancelTheMute()
        {
            try
            {
                var enumerator = new MMDeviceEnumerator();
                IEnumerable<MMDevice> speakDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active).ToArray();
                foreach (var mMDevice in speakDevices.ToList())
                {
                    mMDevice.AudioEndpointVolume.Mute = false;//系统音量静音
                };
            }
            catch (Exception ex)
            {
                ProcessErr(ex);
            }
        }

        private void Textbox_Chg(object sender, TextChangedEventArgs e)
        {
            var a = (System.Windows.Controls.TextBox)sender;
            hitokoto.Content = a.Text;
        }

        private void BtnSaveHandler(object sender, RoutedEventArgs e)
        {
            if (snackbarService.GetSnackbarControl() == null) snackbarService.SetSnackbarControl(snackbar);
            snackbarService.ShowAsync("一言已收藏", "已收藏至文件 " + filePath, SymbolRegular.Heart48);
            //NoticeBox.Show("已收藏至文件 " + filePath, "info", MessageBoxIcon.Info, true, 1000);
            WriteInfo(hitokoto.Content.ToString(), @"D:\cokee_hitokoto.txt");
        }

        private string filePath = @"D:\cokee_hitokoto.txt";

        private void WriteInfo(string info, string filepath)
        {
            using (FileStream stream = new FileStream(filepath, FileMode.Append))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.WriteLine($"{DateTime.Now},{info};");
                }
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
            catch (Exception ex)
            {
                ProcessErr(ex);
            }
        }

        private void StartSnowing(Canvas panel)
        {
            Random random = new Random();
            Task.Factory.StartNew(new Action(() =>
            {
                for (int j = 0; j < 25; j++)
                {
                    Thread.Sleep(j * 100);
                    Dispatcher.Invoke(new Action(() =>
                    {
                        int snowCount = random.Next(0, 10);
                        for (int i = 0; i < snowCount; i++)
                        {
                            int width = random.Next(10, 50);
                            SymbolIcon pack = new SymbolIcon();
                            int snowType = random.Next(3);
                            switch (snowType)
                            {
                                case 0: pack.Symbol = SymbolRegular.WeatherSnowflake20; break;
                                case 1: pack.Symbol = SymbolRegular.WeatherSnowflake24; break;
                                case 2: pack.Symbol = SymbolRegular.WeatherSnowflake48; break;
                                default:
                                    break;
                            }
                            pack.Width = width;
                            pack.Height = width;
                            pack.FontSize = random.Next(10, 40); ;
                            pack.Foreground = System.Windows.Media.Brushes.White;
                            pack.BorderThickness = new Thickness(0);
                            pack.RenderTransform = new RotateTransform();

                            int left = random.Next(0, (int)panel.ActualWidth);
                            Canvas.SetLeft(pack, left);
                            panel.Children.Add(pack);
                            int seconds = random.Next(20, 30);
                            DoubleAnimationUsingPath doubleAnimation = new DoubleAnimationUsingPath()        //下降动画
                            {
                                Duration = new Duration(new TimeSpan(0, 0, seconds)),
                                RepeatBehavior = RepeatBehavior.Forever,
                                PathGeometry = new PathGeometry(new List<PathFigure>() { new PathFigure(new Point(left, 0), new List<PathSegment>() { new LineSegment(new Point(left, panel.ActualHeight), false) }, false) }),
                                Source = PathAnimationSource.Y
                            };
                            pack.BeginAnimation(Canvas.TopProperty, doubleAnimation);
                            DoubleAnimation doubleAnimation1 = new DoubleAnimation(360, new Duration(new TimeSpan(0, 0, 10)))              //旋转动画
                            {
                                RepeatBehavior = RepeatBehavior.Forever,
                            };
                            pack.RenderTransform.BeginAnimation(RotateTransform.AngleProperty, doubleAnimation1);
                        }
                    }));
                }
            }));
        }

        private void DislikeImage(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.BlockedImageIds += br1.Tag.ToString() + "|";
            snackbarService.ShowAsync("屏蔽成功", "已屏蔽日期为 " + br1.Tag.ToString() + " 的图片。", SymbolRegular.CheckmarkCircle24);
            _ = GetBingWapp();
        }

        /// <summary>
        ///一言处理
        /// </summary>
        private void Viewsour(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText("https://hitokoto.cn/?uuid=" + hkUrl);
            snackbarService.ShowAsync("链接已复制", "https://hitokoto.cn/?uuid=" + hkUrl, SymbolRegular.CopyAdd24);
        }

        private void Likeit(object sender, RoutedEventArgs e)
        {
            if (hkself.Visibility == Visibility.Collapsed) hkself.Visibility = Visibility.Visible;
            else hkself.Visibility = Visibility.Collapsed;
            //Process.Start(@"D:\ink\TabTip.exe");
            //NoticeBox.Show("Done!", "Info", MessageBoxIcon.Info,true,1000);
        }

        private void TouchDown(object sender, TouchEventArgs e)
        {
        }

        private void MouseDown(object sender, MouseButtonEventArgs e)
        {
            /* MainCanvas.MouseMove -= MouseMove;
             DoubleAnimation doubleAnimation = new DoubleAnimation(100,TimeSpan.FromSeconds(1));
             CloseOpin.BeginAnimation(Ellipse.HeightProperty,doubleAnimation);
             CloseOpin.BeginAnimation(Ellipse.HeightProperty,doubleAnimation);                                 */
            Close();
        }

        private void MouseMove(object sender, MouseEventArgs e)
        {
            /*/System.Windows.Point position = e.GetPosition(this);
            double pX = position.X;
            double pY = position.Y;

            // Sets the Height/Width of the circle to the mouse coordinates.
            Canvas.SetLeft(CloseOpin, pX);
            Canvas.SetTop(CloseOpin, pY);
            // CloseOpin.Width = pX;
            //CloseOpin.Height = pY;   */
        }

        /// <summary>
        ///u盘处理-打开
        /// </summary>
        private void OpenUsb(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start("explorer.exe", disk);
            }
            catch (Exception ex)
            {
                ProcessErr(ex);
            }
        }

        private void BorderLoader(object sender, RoutedEventArgs e)
        {
            PointAnimationUsingKeyFrames keyFrames = new PointAnimationUsingKeyFrames();
            keyFrames.Duration = new Duration(TimeSpan.FromSeconds(4));
            keyFrames.RepeatBehavior = RepeatBehavior.Forever;
            LinearPointKeyFrame lpk0 = new LinearPointKeyFrame(new Point(0, 0), KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0)));
            LinearPointKeyFrame lpk1 = new LinearPointKeyFrame(new Point(1, 0), KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1)));
            LinearPointKeyFrame lpk2 = new LinearPointKeyFrame(new Point(1, 1), KeyTime.FromTimeSpan(TimeSpan.FromSeconds(2)));
            LinearPointKeyFrame lpk3 = new LinearPointKeyFrame(new Point(0, 1), KeyTime.FromTimeSpan(TimeSpan.FromSeconds(3)));
            LinearPointKeyFrame lpk4 = new LinearPointKeyFrame(new Point(0, 0), KeyTime.FromTimeSpan(TimeSpan.FromSeconds(4)));
            keyFrames.KeyFrames.Add(lpk0);
            keyFrames.KeyFrames.Add(lpk1);
            keyFrames.KeyFrames.Add(lpk2);
            keyFrames.KeyFrames.Add(lpk3);
            keyFrames.KeyFrames.Add(lpk4);

            MusicBorder.BeginAnimation(LinearGradientBrush.StartPointProperty, keyFrames);
            ThicknessAnimation thicknessAnimation = new ThicknessAnimation();
            double aa = audioName.ActualWidth - MusicBorder.ActualWidth;
            if (aa > 0)
            {
                thicknessAnimation.From = new Thickness(0, 0, 0, 0);
                thicknessAnimation.By = new Thickness(-aa - 20, 0, 0, 0);
                thicknessAnimation.Duration = new Duration(TimeSpan.FromSeconds(10))
                {
                };
                thicknessAnimation.BeginTime = TimeSpan.FromSeconds(3);
                thicknessAnimation.Completed += (object? sender, EventArgs e) =>
                {
                    audioName.BeginAnimation(Label.MarginProperty, thicknessAnimation);
                };
                audioName.BeginAnimation(Label.MarginProperty, thicknessAnimation);
            }
        }

        /// <summary>
        ///特殊天气按钮-按下处理
        /// </summary>
        /// <param name="sender">(Btn)</param>
        private void ShowWeatherWarns(object sender, RoutedEventArgs e)
        {
            try
            {
                dialog.Show((string)SpecialWeatherBtn.Content, weaWr);
                dialog.ButtonLeftClick += Dialog_ButtonLeftClick;
            }
            catch (Exception ex)
            {
                ProcessErr(ex);
                //MessageBoxX.Show(this, ex.ToString(), "Error", MessageBoxIcon.Warning, DefaultButton.YesOK);
            }
        }


        /*
        /// <summary>
        /// 可触摸滚动的ScrollViewer控件
        /// </summary>
        public class TouchableScrollViewer : ScrollViewer
        {
            //触摸点的坐标
            
            public TouchableScrollViewer()
            {
                TouchDown += TouchableScrollViewer_TouchDown;

                TouchUp += TouchableScrollViewer_TouchUp;
            }
            private void TouchableScrollViewer_TouchDown(object sender, TouchEventArgs e)
            {
                //添加触摸移动监听
                TouchMove -= TouchableScrollViewer_TouchMove;
                TouchMove += TouchableScrollViewer_TouchMove;

                //获取ScrollViewer滚动条当前位置
                _startVerticalOffset = VerticalOffset;
                _startHorizontalOffset = HorizontalOffset;

                //获取相对于ScrollViewer的触摸点位置
                TouchPoint point = e.GetTouchPoint(this);
                _startPosition = point.Position;
            }

            private void TouchableScrollViewer_TouchUp(object sender, TouchEventArgs e)
            {
                //注销触摸移动监听
                TouchMove -= TouchableScrollViewer_TouchMove;
            }


        }*/



private void Dialog_ButtonLeftClick(object sender, RoutedEventArgs e) => dialog.Hide();

//---目前没什么用的函数
private async Task CheckUpdate()
{
    try
    {
        var client = new HttpClient(); var a = new WebClient(); var uri = "";
        var u2 = await client.GetStringAsync("https://gitee.com/api/v5/repos/cokee/CokeeDisplayProtect/releases?page=1&per_page=1&direction=desc ");
        JObject dt = JsonConvert.DeserializeObject<JObject>(u2);
        if ((double)dt[0]["name"] > ver)
            if (dt[0]["assets"][0]["name"].ToString() != "update.zip" && dt[0]["assets"][1]["name"].ToString() == "update.zip") uri = dt[0]["assets"][1]["browser_download_url"].ToString();
            else uri = dt[0]["assets"][0]["browser_download_url"].ToString();
        a.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressCallback);
        a.DownloadFileCompleted += new AsyncCompletedEventHandler(Updatecb);
        a.DownloadFileAsync(new Uri(uri), Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CokeeWapp\\update\\update.zip");
    }
    catch (Exception ex)
    {
        ProcessErr(ex);
    }
}

private void ResDwCb(object sender, AsyncCompletedEventArgs e)
{
    pro.Visibility = Visibility.Collapsed;
    ZipArchive archive = ZipFile.Open(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CokeeWapp\\res.zip", ZipArchiveMode.Read);
    if (File.Exists(Environment.SpecialFolder.MyDocuments + "\\CokeeWapp\\ver")) Directory.Delete(Environment.SpecialFolder.MyDocuments + "\\CokeeWapp");
    archive.ExtractToDirectory(Environment.SpecialFolder.MyDocuments + "\\CokeeWapp");
    if (File.Exists(Environment.SpecialFolder.MyDocuments + "\\CokeeWapp\\ver")) log.Text = "资源包下载成功.";
}

private void Updatecb(object sender, AsyncCompletedEventArgs e)
{
    pro.Visibility = Visibility.Collapsed;
    ZipArchive archive = ZipFile.Open(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CokeeWapp\\update\\update.zip", ZipArchiveMode.Read);
    if (Directory.Exists(Environment.SpecialFolder.MyDocuments + "\\CokeeWapp\\update\\unzip")) Directory.Delete(Environment.SpecialFolder.MyDocuments + "\\CokeeWapp\\update\\unzip");
    archive.ExtractToDirectory(Environment.SpecialFolder.MyDocuments + "\\CokeeWapp\\update\\unzip");
}

//downing
private void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
{
    if (pro.Visibility != Visibility.Visible) pro.Visibility = Visibility.Visible;
    pro.Value = e.ProgressPercentage;
    log.Text = "正在加载" + nowDowning + "... " + e.ProgressPercentage + "% " + e.BytesReceived / 1048576 + "MB of" + e.TotalBytesToReceive / 1048576;
}

private void DownloadFileCallback(object sender, AsyncCompletedEventArgs e)
{
    if (pro.Visibility != Visibility.Visible) pro.Visibility = Visibility.Collapsed;
    log.Text = "Done.";
    if (e.Cancelled)
    {
        log.Text = "File download cancelled.";
    }
    if (e.Error != null)
    {
        log.Text = e.Error.ToString();
    }
    if (e.Error == null && !e.Cancelled)
    {
        br1.BeginInit();
        br1_blur.Radius = 0;
        br1.Source = new BitmapImage(new Uri(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CokeeWapp\\bing.jpg"));
        br1.EndInit();
    }
}

public void Appear(FrameworkElement element, int direction = 0, int distance = 20, double duration = .3)
{
    //将所选控件的Visibility属性改为Visible
    ObjectAnimationUsingKeyFrames VisbilityAnimation = new ObjectAnimationUsingKeyFrames();
    DiscreteObjectKeyFrame kf = new DiscreteObjectKeyFrame(Visibility.Visible, new TimeSpan(0, 0, 0));
    VisbilityAnimation.KeyFrames.Add(kf);
    element.BeginAnimation(Border.VisibilityProperty, VisbilityAnimation);

    //创建新的缩放动画
    TranslateTransform TT = new TranslateTransform();
    element.RenderTransform = TT;
    EasingFunctionBase easeFunction = new CircleEase() { EasingMode = EasingMode.EaseInOut };

    //判断动画方向
    if (direction == 0)
    {
        DoubleAnimation Animation = new DoubleAnimation(-distance, 0, new Duration(TimeSpan.FromSeconds(duration)));
        Animation.EasingFunction = easeFunction;
        element.RenderTransform.BeginAnimation(TranslateTransform.YProperty, Animation);
    }
    else if (direction == 1)
    {
        DoubleAnimation Animation = new DoubleAnimation(distance, 0, new Duration(TimeSpan.FromSeconds(duration)));
        Animation.EasingFunction = easeFunction;
        element.RenderTransform.BeginAnimation(TranslateTransform.XProperty, Animation);
    }
    else if (direction == 2)
    {
        DoubleAnimation Animation = new DoubleAnimation(distance, 0, new Duration(TimeSpan.FromSeconds(duration)));
        Animation.EasingFunction = easeFunction;
        element.RenderTransform.BeginAnimation(TranslateTransform.YProperty, Animation);
    }
    else if (direction == 3)
    {
        DoubleAnimation Animation = new DoubleAnimation(-distance, 0, new Duration(TimeSpan.FromSeconds(duration)));
        Animation.EasingFunction = easeFunction;
        element.RenderTransform.BeginAnimation(TranslateTransform.XProperty, Animation);
    }
    else throw new Exception("无效的方向！");

    //将所选控件的可见度按动画函数方式显现
    DoubleAnimation OpacityAnimation = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(duration)));
    OpacityAnimation.EasingFunction = easeFunction;
    element.BeginAnimation(Border.OpacityProperty, OpacityAnimation);
}

/// <summary>
/// 淡出动画(控件名, 0：上方；1：右方；2：下方；3：左方, 淡出的距离，持续时间)
/// </summary>
/// <param name="element">控件名</param>
/// <param name="direction">0：上方；1：右方；2：下方；3：左方</param>
/// <param name="distance">淡出的距离</param>
/// <param name="duration">持续时间</param>
public void Disappear(FrameworkElement element, int direction = 0, int distance = 20, double duration = .3)
{
    //创建新的缩放动画
    TranslateTransform TT = new TranslateTransform();
    element.RenderTransform = TT;
    //创建缩放动画函数
    EasingFunctionBase easeFunction = new CircleEase() { EasingMode = EasingMode.EaseInOut };

    //判断动画方向
    if (direction == 0)
    {
        DoubleAnimation Animation = new DoubleAnimation(-distance, new Duration(TimeSpan.FromSeconds(duration)));
        Animation.EasingFunction = easeFunction;
        element.RenderTransform.BeginAnimation(TranslateTransform.YProperty, Animation);
    }
    else if (direction == 1)
    {
        DoubleAnimation Animation = new DoubleAnimation(distance, new Duration(TimeSpan.FromSeconds(duration)));
        Animation.EasingFunction = easeFunction;
        element.RenderTransform.BeginAnimation(TranslateTransform.XProperty, Animation);
    }
    else if (direction == 2)
    {
        DoubleAnimation Animation = new DoubleAnimation(distance, new Duration(TimeSpan.FromSeconds(duration)));
        Animation.EasingFunction = easeFunction;
        element.RenderTransform.BeginAnimation(TranslateTransform.YProperty, Animation);
    }
    else if (direction == 3)
    {
        DoubleAnimation Animation = new DoubleAnimation(-distance, new Duration(TimeSpan.FromSeconds(duration)));
        Animation.EasingFunction = easeFunction;
        element.RenderTransform.BeginAnimation(TranslateTransform.XProperty, Animation);
    }
    else
        throw new Exception("无效的方向！");

    //将所选控件的可见度按动画函数方式消失
    DoubleAnimation OpacityAnimation = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(duration)));
    OpacityAnimation.EasingFunction = easeFunction;
    element.BeginAnimation(Border.OpacityProperty, OpacityAnimation);

    //将所选控件的Visibility属性改为Collapsed
    ObjectAnimationUsingKeyFrames VisbilityAnimation = new ObjectAnimationUsingKeyFrames();
    DiscreteObjectKeyFrame kf = new DiscreteObjectKeyFrame(Visibility.Collapsed, new TimeSpan(0, 0, 1));
    VisbilityAnimation.KeyFrames.Add(kf);
    element.BeginAnimation(Border.VisibilityProperty, VisbilityAnimation);
}
    }
}