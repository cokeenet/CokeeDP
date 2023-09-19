using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
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

using Cokee.ClassService.Views.Pages;

using CokeeDP.Properties;
using CokeeDP.Views.Pages;

using Microsoft.AppCenter.Crashes;

using NAudio.CoreAudioApi;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using OpenCvSharp;
using OpenCvSharp.Extensions;
//using Quartz.Impl;
//using Quartz;
using Serilog;
using Serilog.Sink.AppCenter;

using Windows.Data.Text;
using Windows.Media.Playback;

using Wpf.Ui.Appearance;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Wpf.Ui.Mvvm.Services;

using Button = Wpf.Ui.Controls.Button;
using Clipboard = Wpf.Ui.Common.Clipboard;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using Directory = System.IO.Directory;
using File = System.IO.File;
using MediaPlayer = System.Windows.Media.MediaPlayer;
using Point = System.Windows.Point;
using Timer = System.Timers.Timer;
using Window = System.Windows.Window;

namespace CokeeDP.Views.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///  Made with Heart and Love
    ///  By Cokee. Last Edit: 20230722
    public partial class MainWindow : Window
    {
        public const string CACHE_DIR = "D:\\Program Files (x86)\\CokeeTech\\CokeeDP\\Cache";
        private static Timer OneWordsTimer;
        private static Timer SecondTimer;
        private static Timer WeatherTimer;
        private static Timer CapTimer = new Timer(10 * 60 * 1000);
        private List<FileInfo> ImageArray = new List<FileInfo>();
        private FileInfo[] AudioArray;
        private int AudioNum = 0;
        private string AudioFolder;
        private int bgn = -1, bing = 0, videoCount = 0;
        private string disk, weaWr, hkUrl, nowDowning = "";
        private SnackbarService snackbarService;
        private bool IsPlaying = false, AudioLoaded = false, IsWaitingTask = false;
        private int PlayingRule = 3, TaskCd;
        private MediaPlayer mediaplayer = new MediaPlayer();
        private DateTime CountDownTime, TaskedTime;
        public Version version;
        public double AudioScroll = 0;
        public TimeTasks[] timeTasks;
        public UIElement debugCard;
        public bool IsUsbOpened = false;
        public AppSettings settings = AppSettingsExtensions.LoadSettings();
        public MainWindow()
        {
            InitializeComponent();
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("log.txt",
               outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.AppCenterSink(null, Serilog.Events.LogEventLevel.Information, AppCenterTarget.ExceptionsAsCrashes, "52a9c4e0-ad42-455b-b1cf-515d8a39f245")
                .WriteTo.RichTextBox(log)
                .CreateLogger();
            try
            {
                /*if (Environment.GetCommandLineArgs().Length > 0)
                {
                    string[] pargs = Environment.GetCommandLineArgs();
                    if (pargs.Length >= 1)
                    {
                        if (pargs[1] == "/p" && pargs.Length >= 2)
                        {
                            IntPtr parentWindowHandle = new IntPtr(Convert.ToInt32(pargs[2], 16));
                            IntPtr childWindowHandle = new WindowInteropHelper(this).Handle;
                            // 将窗口设置为Monitor窗口的子窗口
                            SetParent(childWindowHandle, parentWindowHandle);
                        }
                        if (pargs[1] == "/c")
                        {
                            WindowState = WindowState.Minimized;
                            var win1 = new SettingsWindow();
                            win1.Show();
                        }
                    }

                }*/
                FillConfig();
                if (settings.EnableBigTimeTo) BigCountdown.Visibility = Visibility.Visible;
                if (settings.BingVideoEnable) _ = GetBingVideo();
                else if (settings.BingWappEnable) _ = GetBingWapp();
                else
                {
                    //Using Local Picture
                    var path = "D:\\Program Files (x86)\\CokeeTech\\CokeeDP\\Picture";
                    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                    DirectoryInfo[] ImageDir = new DirectoryInfo(path).GetDirectories();
                    foreach (var item in ImageDir)
                    {
                        foreach (var pic in item.GetFiles("*.jpg"))
                        {
                            //ImageArray
                            ImageArray.Add(pic);
                        }
                    }
                    ChangeWapp(false);
                }
                verString.Content = $"循星🌟 Ver {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(2)}";
                TimeLabel.Content = DateTime.Now.ToString("HH:mm:ss");
                //Get AudioFiles
                if (Directory.Exists(AudioFolder))
                {
                    DirectoryInfo dir = new DirectoryInfo(AudioFolder);
                    if (dir.Exists)
                    {
                        AudioArray = dir.GetFiles("*.mp3");
                    }
                }
                if (Directory.Exists(CACHE_DIR)) 
                {
                    DirectoryInfo dir=new DirectoryInfo(CACHE_DIR);
                    foreach (var item in dir.GetFiles())
                    {
                        DateTime a;
                        if (DateTime.TryParse(item.Name, out a))
                        {
                            if (DateTime.Now.Subtract(a).Days >= 7) item.Delete();
                        }
                        else if (dir.GetFiles().Length >= 15)
                        {
                            foreach (var i in dir.GetFiles())
                            {
                                i.Delete();
                            }
                        }
                    }
                }
                else Directory.CreateDirectory(CACHE_DIR);
                CheckBirthDay();
                debugCard = debugPage;
                //pager.Items.Remove(debugCard);
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
                if (settings.BingVideoEnable) _ = GetBingVideo();
                else if (settings.BingWappEnable && !settings.BingVideoEnable)
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
                    if (bgn == -1) bgn = new Random().Next(0, ImageArray.Count);
                    Uri bgp;
                    if (direction)
                    {
                        //snackbarService.ShowAsync(bgn.ToString(),ImageArray.Count().ToString());
                        bgn--;
                        if (bgn < ImageArray.Count)
                        {
                            bgn = 0;
                            bgp = new Uri(ImageArray[0].FullName);
                        }
                        else bgp = new Uri(ImageArray[bgn].FullName);
                        SetImageText(ImageArray[bgn].DirectoryName);
                    }
                    else
                    {
                        bgn++;
                        if (bgn >= ImageArray.Count)
                        {
                            bgn = 0;
                            bgp = new Uri(ImageArray[bgn].FullName);

                        }
                        else
                        {
                            bgp = new Uri(ImageArray[bgn].FullName);
                        }
                        SetImageText(ImageArray[bgn].DirectoryName);
                    }
                    BitmapImage image = new BitmapImage(bgp);
                    // image.Rotation = Rotation.Rotate270;

                    br1.BeginInit();
                    br1.Source = image;

                    //  br1.StretchDirection = StretchDirection.UpOnly;
                    br1.EndInit();
                    Log.Information($"{bgn} / LoadLocalPic:{bgp}");

                    #endregion non-bing
                }
            }
            catch (Exception ex)
            {
                ProcessErr(ex);
            }
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        public void SetImageText(string desc, string info, string title)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                DescPara1.Text = desc;
                BingImageInfo.Content = info;
                CardInfo.Content = title;
            }
       ));

        }
        public void SetImageText(string folderPath)
        {
            Dispatcher.Invoke(new Action(delegate
            {
                DescPara1.Text = File.ReadAllText(folderPath + "\\desc.txt");
                BingImageInfo.Content = File.ReadAllText(folderPath + "\\info.txt");
                CardInfo.Content = File.ReadAllText(folderPath + "\\title.txt");
            }
       ));

        }
        public void FillConfig()
        {
            try
            {
                //CountDownLabel.IsResizeable = true;
                //如配置文件损坏或不正确，用默认配置覆盖
                if (settings.CountdownTime.Year <= 2000)
                {
                    CountDownTime = DateTime.Parse("2025/06/05");
                    settings.CountdownTime = CountDownTime;
                }
                else CountDownTime = settings.CountdownTime;
                if (settings.OneWordsApi.Length == 0) { settings.OneWordsApi = "https://v1.hitokoto.cn/?c=k"; }
                if (Convert.ToInt32(settings.OneWordsTimeInterval) <= 10) settings.OneWordsTimeInterval = 100;
                if (Convert.ToInt32(settings.WeatherTimeInterval) <= 9800) settings.WeatherTimeInterval = 9800;
                if (settings.CountdownName.Length <= 1) settings.CountdownName = "高考";
                AudioFolder = settings.AudioFolder;
                AppSettingsExtensions.SaveSettings(settings);
                SetTimer(SecondTimer, 1, OneWordsTimer, Convert.ToInt32(settings.OneWordsTimeInterval), WeatherTimer, Convert.ToInt32(settings.WeatherTimeInterval));
            }
            catch (Exception ex)
            {
                ProcessErr(ex);
                settings.OneWordsTimeInterval = 100;
                settings.WeatherTimeInterval = 9800;
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
            CheckBirthDay();
        }
        public void CheckBirthDay()
        {
            string DATA_FILE = "D:\\Program Files (x86)\\CokeeTech\\CokeeClass\\students.json";
            if (File.Exists(DATA_FILE))
            {
                List<Student> students = new List<Student>();
                students = JsonConvert.DeserializeObject<List<Student>>(File.ReadAllText(DATA_FILE));
                Student nearest = null; int type = 0;
                foreach (var person in students)
                {
                    string shortBirthStr=null;
                    try
                    {
                         shortBirthStr= person.BirthDay.ToString("MM-dd");
                    }
                    catch(Exception ex)
                    {
                        ProcessErr(ex);
                    }
                    
                    if (DateTime.Now.ToString("MM-dd") == shortBirthStr)
                    {
                        nearest = person; type = 1; break;
                    }
                    else if (DateTime.Now.AddDays(1).ToString("MM-dd") == shortBirthStr)
                    {
                        nearest = person; type = 2; continue;
                    }
                }
                if (type == 1)
                {
                    birthBar.IsOpen = true;
                    birthBar.Message = $"🎉 今天是 {nearest.Name} 的生日！";
                }
                else if (type == 2)
                {
                    birthBar.IsOpen = true;
                    birthBar.Message = $"🎉 明天是 {nearest.Name} 的生日！";
                }
                else birthBar.IsOpen = false;
            }
        }
        public void OnOneSecondTimer(object source, ElapsedEventArgs e)
        {
            try
            {
                Dispatcher.Invoke(new Action(delegate
            {
                TimeLabel.Content = DateTime.Now.ToString("HH:mm:ss");
                timeTo.Content = DateTime.Now.ToString("ddd,M月dd日");
                if (settings.EnableBigTimeTo)
                {
                    tod_info.Content = "还有" + CountDownTime.Subtract(DateTime.Now).TotalDays + "天";
                    big_tod.Content = ((int)CountDownTime.Subtract(DateTime.Now).TotalDays);
                }
                else
                    CountDownLabel.Content = $"距离[{settings.CountdownName}]还有 {CountDownTime.Subtract(DateTime.Now).TotalDays} 天";
                if (DateTime.Now.Hour == 0 && DateTime.Now.Minute == 0 && DateTime.Now.Second == 0)
                {
                    OnNewDay();
                }
                if (IsPlaying)
                {
                    if (AudioScroll > MusicScroll.ScrollableWidth) AudioScroll = 0;
                    else AudioScroll = AudioScroll + 5;
                    MusicScroll.ScrollToHorizontalOffset(AudioScroll);
                    // audioTime.Content = mediaplayer.Position.ToString(@"mm\:ss") + "/" + MediaDuring;
                    PlaySlider.Value = mediaplayer.Position.TotalSeconds;
                    //PlaySlider.Maximum = mediaplayer.NaturalDuration.TimeSpan.TotalSecondTimeronds;
                }
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
                if ((e.Message + e.StackTrace).Contains("Http")) { NetIcon.Symbol = SymbolRegular.CellularOff24; netBar.IsOpen = true; }
                snackbarService.SetSnackbarControl(snackbar);
                snackbarService.ShowAsync("发生错误", e.ToString().Substring(0, 50), SymbolRegular.ErrorCircle24, ControlAppearance.Danger);
                Clipboard.SetText(e.Message + e.StackTrace);
            }
            Log.Error(e, "Error");
            if (Environment.OSVersion.Version.Major >= 10.0) Crashes.TrackError(e);
        }

        public void SetTimer(Timer a, int ms, Timer b, int ms1, Timer c, int ms2)
        {
            // Create timers with a interval.
            a = new Timer(ms * 1000); a.Elapsed += new ElapsedEventHandler(OnOneSecondTimer); a.AutoReset = true; a.Enabled = true;
            b = new Timer(ms1 * 1000); b.Elapsed += new ElapsedEventHandler(OnHitokoUpd); b.AutoReset = true; b.Enabled = true;
            c = new Timer(ms2 * 1000); c.Elapsed += new ElapsedEventHandler(OnWeatherTimer); c.AutoReset = true; c.Enabled = true;

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
                BitmapImage bitmapImage;
                var client = new HttpClient();
                // 从Bing获取图片Json，存在一天的时差
                var u2 = await client.GetStringAsync("https://cn.bing.com/hp/api/v1/imagegallery?format=json&ensearch=0");
                //旧API await client.GetStringAsync("https://cn.bing.com/HPImageArchive.aspx?format=js&idx=" + bing + "&n=1");
                JObject dt = JsonConvert.DeserializeObject<JObject>(u2);
                if (settings.BlockedImageIds != null)
                {
                    if (settings.BlockedImageIds.Contains(dt["data"]["images"][bing]["isoDate"].ToString()))
                    {
                        ChangeWapp(false);
                        return;
                    }
                }
                BingImageInfo.Content = $"{dt["data"]["images"][bing]["title"]}  ({dt["data"]["images"][bing]["copyright"]}) | {dt["data"]["images"][bing]["isoDate"]} ";
                var urlstr = "https://www.bing.com/" + dt["data"]["images"][bing]["imageUrls"]["landscape"]["highDef"];
                CardInfo.Content = dt["data"]["images"][bing]["caption"].ToString();
                DescPara1.Text = $" {dt["data"]["images"][bing]["description"]} {Environment.NewLine} {dt["data"]["images"][bing]["descriptionPara2"]} {Environment.NewLine} {dt["data"]["images"][bing]["descriptionPara3"]}";
                if (settings.UHDEnable) urlstr = urlstr.Replace("_1920x1080", "_UHD");
                Uri uri = new Uri(urlstr);
                Log.Information($"{bing} /LoadBingImage:{uri}");
                if (File.Exists($"{CACHE_DIR}\\{dt["data"]["images"][bing]["isoDate"]}.png")) 
                {
                    bitmapImage = new BitmapImage(new Uri($"{CACHE_DIR}\\{dt["data"]["images"][bing]["isoDate"]}.png"));
                    if (pro.Visibility != Visibility.Collapsed) pro.Visibility = Visibility.Collapsed;
                    Log.Information($"Image Loaded.(CACHED)😺 Day: {bing}");
                    DoubleAnimation animation1 = new DoubleAnimation(20, 0, new Duration(TimeSpan.FromSeconds(5)));
                    animation1.EasingFunction = new CircleEase();
                    //animation.AutoReverse = true;
                    br1_blur.BeginAnimation(BlurEffect.RadiusProperty, animation1);
                    br1.Source = bitmapImage;
                    return;
                }
                else 
                {
                    bitmapImage = new BitmapImage(uri);
                    bitmapImage.DownloadProgress += ImageDownloadProgress;
                }
                bitmapImage.DownloadCompleted += (a, b) => DownloadImageCompleted(a, b, bitmapImage, dt["data"]["images"][bing]["isoDate"].ToString());
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
        private async Task GetBingVideo()
        {
            try
            {
                var client = new HttpClient();
                // 从MSN获取配置Json
                var u2 = await client.GetStringAsync("https://ntp.msn.cn/resolver/api/resolve/v3/config/?expType=AppConfig&expInstance=default&apptype=edgeChromium&v=20230501.202&targetScope={\"audienceMode\":\"adult\",\"browser\":{\"browserType\":\"edgeChromium\",\"version\":\"112\",\"ismobile\":\"false\"},\"deviceFormFactor\":\"desktop\",\"domain\":\"ntp.msn.cn\",\"locale\":{\"content\":{\"language\":\"zh\",\"market\":\"cn\"},\"display\":{\"language\":\"zh\",\"market\":\"cn\"}},\"os\":\"windows\",\"platform\":\"web\",\"pageType\":\"ntp\"}");
                JObject dt = JsonConvert.DeserializeObject<JObject>(u2);
                videoCount = dt["configs"]["BackgroundImageWC/default"]["properties"]["video"]["data"].Count();
                bing = new Random().Next(videoCount);
                if (dt["configs"]["BackgroundImageWC/default"]["properties"]["localizedStrings"]["video_titles"]["video" + bing].ToString().Contains("水母") || dt["configs"]["BackgroundImageWC/default"]["properties"]["localizedStrings"]["video_titles"]["video" + bing].ToString().Contains("蜂")) return;
                Uri videoUri;
                if (settings.UHDEnable) videoUri = new Uri("https://prod-streaming-video-msn-com.akamaized.net/" + dt["configs"]["BackgroundImageWC/default"]["properties"]["video"]["data"][bing]["video"]["v2160"].ToString() + ".mp4");
                else videoUri = new Uri("https://prod-streaming-video-msn-com.akamaized.net/" + dt["configs"]["BackgroundImageWC/default"]["properties"]["video"]["data"][bing]["video"]["v1080"].ToString() + ".mp4");
                Log.Information($"{bing} /LoadBingImage:{videoUri}");
                br1_blur.Radius = 10;
                br2.Loaded += (sender, e) => br2.Play();
                br2.MediaEnded += (sender, e) =>
                {
                    br2.Position = TimeSpan.Zero;
                    br2.Play();
                };
                br2.Unloaded += (sender, e) => br2.Stop();
                br2.BufferingStarted += Br2_BufferingStarted;
                br2.BufferingEnded += Br2_BufferingEnded;
                br2.Source = videoUri;
                CardInfo.Content = BingImageInfo.Content = dt["configs"]["BackgroundImageWC/default"]["properties"]["localizedStrings"]["video_titles"]["video" + bing].ToString();//.Split("\"video" + bing + "\"")[0];
                DescPara1.Text = dt["configs"]["BackgroundImageWC/default"]["properties"]["localizedStrings"]["video_titles"]["video" + bing].ToString() + Environment.NewLine + Environment.NewLine + "版权:" + dt["configs"]["BackgroundImageWC/default"]["properties"]["video"]["data"][bing]["attribution"].ToString();
                DescPara1.Text = DescPara1.Text + Environment.NewLine + Environment.NewLine + "提示:上课期间不要打开视频！！";
            }
            catch (Exception e)
            {
                ProcessErr(e);
            }
        }

        private void Br2_BufferingEnded(object sender, RoutedEventArgs e)
        {
            if (pro.Visibility != Visibility.Collapsed) pro.Visibility = Visibility.Collapsed;
            Log.Information($"DynVideo Loaded.😺 Day: {bing}");
            DoubleAnimation animation = new DoubleAnimation(20, 0, new Duration(TimeSpan.FromSeconds(5)));
            animation.EasingFunction = new CircleEase();
            //animation.AutoReverse = true;
            br2_blur.BeginAnimation(BlurEffect.RadiusProperty, animation);
            br2.Play();
        }
        private void Br2_BufferingStarted(object sender, RoutedEventArgs e)
        {
            if (pro.Visibility != Visibility.Visible) pro.Visibility = Visibility.Visible;
            DoubleAnimation animation = new DoubleAnimation(0, 20, new Duration(TimeSpan.FromSeconds(5)));
            animation.EasingFunction = new CircleEase();
            //animation.AutoReverse = true;
            br2_blur.BeginAnimation(BlurEffect.RadiusProperty, animation);
            //Log.Information($"LoadBingDynVideo {br2.BufferingProgress * 100}%");
        }

        private void ImageDownloadProgress(object sender, DownloadProgressEventArgs e)
        {
            if (pro.Visibility != Visibility.Visible) pro.Visibility = Visibility.Visible;
            pro.Value = e.Progress;
            //Log.Information($"LoadBingImage {e.Progress}");
        }

        private void DownloadImageCompleted(object sender, EventArgs e, BitmapImage bitmap,string isoDate)
        {
            try
            {
                if (pro.Visibility != Visibility.Collapsed) pro.Visibility = Visibility.Collapsed;
                Log.Information($"Image Loaded.😺 Day: {bing}");
                DoubleAnimation animation = new DoubleAnimation(20, 0, new Duration(TimeSpan.FromSeconds(5)));
                animation.EasingFunction = new CircleEase();
                //animation.AutoReverse = true;
                br1_blur.BeginAnimation(BlurEffect.RadiusProperty, animation);
                br1.Source = bitmap;
                if (!File.Exists($"{CACHE_DIR}\\{isoDate}.png"))
                {
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));
                    using (var fileStream = new System.IO.FileStream($"{CACHE_DIR}\\{isoDate}.png", FileMode.Create))
                    {
                        encoder.Save(fileStream);
                    }
                }
            }
            catch (Exception ex)
            {
                ProcessErr(ex);
            }
        }

        private async Task Hitoko()
        {
            try
            {
                /* if (CokeeDP.settings.isDebug)
                  {
                      return;
                  }*/
                var client = new HttpClient();
                JObject dt = JsonConvert.DeserializeObject<JObject>(await client.GetStringAsync(settings.OneWordsApi));
                var BlackWordList = "5LmzfOWls3zoibJ86ISxfOWVqnzlroV86KOk5a2QfOiQneiOiXzluop85aW5fOaBi+eIsXx+";
                foreach (var word in Encoding.UTF8.GetString(Convert.FromBase64String(BlackWordList)).Split("|"))
                {
                    // Log.Information(word.ToString());
                    if (dt.ToString().Contains(word.ToString())) { hitokoto.Content = "*一言已被屏蔽。"; _ = Hitoko(); return; }
                }
                string who = dt["from_who"].ToString();
                hkUrl = dt["uuid"].ToString();
                if (dt["hitokoto"] != null) { NetIcon.Symbol = SymbolRegular.CellularData120; netBar.IsOpen = false; }
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
                // themeService.SetTheme(ThemeType.);//TODO
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
                _ = GetWeatherInfo();
                //new Thread(VideoCap).Start();

                CapTimer.Elapsed += CapTimer_Elapsed;
                CapTimer.AutoReset = true;
                CapTimer.Enabled = true;

                hwndSource.AddHook(new HwndSourceHook(WndProc));//挂钩
                if (settings.SnowEnable) { StartSnowing(MainCanvas); } //雪花效果，不成熟

                //isloaded = true;
            }
            catch (Exception ex)
            {
                ProcessErr(ex);
            }
        }
        private void CapTimer_Elapsed(object sender, ElapsedEventArgs e) => VideoCap();

        private void VideoCap()
        {
            try
            {
                // 设置参数
                const int cameraIndex = 1;
                const int frameWidth = 3264;
                const int frameHeight = 2448;
                #region 
                var outputPath = $@"D:\CokeeDP\Cache\{DateTime.Now:MM-dd}";
                #endregion
                // 检查目录是否存在
                /*   if (!Directory.Exists(outputPath))
                   {
                       serviceController.Start();
                       serviceController.WaitForStatus(ServiceControllerStatus.Running);
                   }           */

                using (var video = new VideoCapture(cameraIndex, VideoCaptureAPIs.ANY))
                {
                    video.Set(VideoCaptureProperties.FrameWidth, frameWidth);
                    video.Set(VideoCaptureProperties.FrameHeight, frameHeight);


                    using (var mat = new Mat())
                    {
                        video.Read(mat);
                        var fileName = $"{DateTime.Now:HH-mm-ss}-dp.png";
                        var filePath = System.IO.Path.Combine(outputPath, fileName);

                        using (var bitmap = BitmapConverter.ToBitmap(mat))
                        {
                            bitmap.Save(filePath, ImageFormat.Png);
                        }

                        // 显示消息
                        //await Dispatcher.InvokeAsync(() => log.Text = $"Caped! {DateTime.Now:HH-mm}");
                        //snackbarService.ShowAsync($"Captured! {DateTime.Now:HH-mm}");
                    }
                }
            }
            catch (Exception ex)
            {
                ProcessErr(ex);
            }
        }

        private void OnWeatherTimer(object sender, ElapsedEventArgs e)
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
            var a = (Button)sender; if ((bing >= 8 || bing <= -1) && settings.BingWappEnable) bing = 0;
            if (a.Name == "left") ChangeWapp(true);
            else if (a.Name == "right") ChangeWapp(false);
        }

        private async Task GetWeatherInfo()
        {
            try
            {
                string u2, u3;
                if (DateTime.Now.Subtract(settings.CachedWeatherTime).Hours > 6 || !settings.CachedWeatherData.Contains("|"))
                {
                    var handler = new HttpClientHandler() { AutomaticDecompression = DecompressionMethods.GZip };
                    var client = new HttpClient(handler);
                    u2 = await client.GetStringAsync("https://devapi.qweather.com/v7/weather/7d?location=" + settings.CityId + "&key=6572127bcec647faba394b17fbd9614f");
                    u3 = await client.GetStringAsync("https://devapi.qweather.com/v7/warning/now?location=" + settings.CityId + "&key=6572127bcec647faba394b17fbd9614f");
                    //MessageBoxX.Show("https://devapi.qweather.com/v7/warning/now?location=" + settings.CityId + " &key=6572127bcec647faba394b17fbd9614f");
                    settings.CachedWeatherData = u2 + "|" + u3;
                    settings.CachedWeatherTime = DateTime.Now;
                    AppSettingsExtensions.SaveSettings(settings);
                }
                else
                {
                    u2 = settings.CachedWeatherData.Split("|")[0];
                    u3 = settings.CachedWeatherData.Split("|")[1];
                }
                weaupd1.Text = settings.City + " 最近更新: " + settings.CachedWeatherTime.ToString("yyyy/MM/dd HH:mm:ss");
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

                if (!dt1.ContainsKey("warning") || dt1["code"].ToString() != "200" || dt["code"].ToString() != "200")
                {
                    SpecialWeatherBtn.Visibility = Visibility.Collapsed;
                    SpecialWeatherBtn1.Visibility = Visibility.Collapsed;
                    throw new HttpRequestException($"天气数据加载失败。网络异常。CODE1:{dt["code"]} CODE2:{dt1["code"]}");
                }
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
                settings.CachedWeatherData = "";
                ProcessErr(ex);
            }
        }

        private void hitokoto_MouseDown(object sender, MouseButtonEventArgs e) => _ = Hitoko();

        private Object locker1 = new Object();

        private void OnCloseWindow(object sender, MouseButtonEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                if (IsUsbOpened)
                {
                    snackbarService.ShowAsync("请确认已关闭U盘内课件", "再次点击以退出", SymbolRegular.Info28);
                    IsUsbOpened = false;
                }
                if (IsPlaying)
                {
                    Wpf.Ui.Controls.MessageBox messageBox = new Wpf.Ui.Controls.MessageBox();
                    messageBox.Title = "嘿！!";
                    messageBox.Content = "有媒体正在播放。请先暂停媒体后重试。(生气)";
                    messageBox.ButtonLeftName = "取消";
                    messageBox.ButtonRightName = "取消";
                    messageBox.MicaEnabled = true;
                    messageBox.Show();
                    // messageBox.ButtonLeftClick += MessageBox_ButtonLeftClick;
                    // if (messageBox.ShowDialog() == true) Close();
                }
                else
                {
                    Point position = e.GetPosition(this);
                    double pX = position.X;
                    double pY = position.Y;

                    // Sets the Height/Width of the circle to the mouse coordinates.
                    Canvas.SetLeft(CloseOpin, pX - CloseOpin.Width);
                    Canvas.SetTop(CloseOpin, pY - CloseOpin.Height);
                    Random random = new Random();

                    // 获取 Colors 类中定义的颜色数量
                    int colorCount = typeof(Colors).GetProperties().Length;

                    // 生成随机索引
                    int randomIndex = random.Next(colorCount);

                    // 获取随机颜色
                    Color randomColor = ((Color)ColorConverter.ConvertFromString(typeof(Colors).GetProperties()[randomIndex].Name));
                    randomColor.A = (byte)(randomColor.A - 100);
                    var colorAnim = new ColorAnimation
                    {
                        From = closeSCB.Color,
                        //By = Colors.Pink,
                        //Duration = new Duration(new TimeSpan(0, 0, 3)),
                        SpeedRatio = 1.3,
                        To = randomColor,
                    };
                    var scaleAnim = new DoubleAnimation
                    {
                        To = 100,
                        SpeedRatio = 0.1,
                        EasingFunction = new CircleEase()
                    };
                    colorAnim.Completed += (a, b) => { Close(); Environment.Exit(0); };
                    scaleAnim.Completed += (a, b) => { scaleTran.ScaleX = 1; scaleTran.ScaleY = 1; };
                    closeSCB.BeginAnimation(SolidColorBrush.ColorProperty, colorAnim);
                    scaleTran.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
                    scaleTran.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);
                    //Close();
                }
            }));
        }

        private void MessageBox_ButtonLeftClick(object sender, RoutedEventArgs e) => Close();

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
                    diskInfo.Content = (t.TotalFreeSpace / 1024 / 1024/1000) + "GB/" + (t.TotalSize / 1024 / 1024/1000) + "GB";//TODO:改进算法，这个结果是错的
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

        public const int DBT_DEVICEARRIVAL = 0x8000;  //设备可用
        public const int DBT_DEVICEREMOVECOMPLETE = 0x8004; //设备被删除
        public const int FILE_SHARE_READ = 0x1;
        public const int FILE_SHARE_WRITE = 0x2;
        public const uint GENERIC_READ = 0x80000000;
        public const int GENERIC_WRITE = 0x40000000;
        public const int IOCTL_STORAGE_EJECT_MEDIA = 0x2d4808;
        public const int WM_DEVICECHANGE = 0x219;

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
                if (!result) snackbarService.ShowAsync("U盘退出失败", "请检查程序占用，关闭已打开的文件夹，PPT，WORD等。", SymbolRegular.Warning24, ControlAppearance.Danger);
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
                OnHitokoUpd(null, null);
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


        private Object locker = new Object();
        private Object locker2 = new Object();

        private void ShowPlayer(object sender, MouseButtonEventArgs e)
        {
            try
            {
                lock (locker)
                {
                    if (File.Exists(AudioFolder + "\\Last.DAT")) AudioNum = Convert.ToInt32(File.ReadAllText(AudioFolder + "\\Last.DAT"));
                    if (IsWaitingTask) AudioNum++;
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
                //AudioPath = AudioArray[AudioNum].FullName;
                mediaplayer.MediaFailed += (sender, e) => { throw e.ErrorException; };
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
        private void RepeatPointSet(object sender, MouseButtonEventArgs e)
        {
            Log.Information($"已设置Repeat Point:{mediaplayer.Position.ToString(@"mm\:ss")}");
            File.WriteAllText(AudioFolder + "\\Repeat.DAT", mediaplayer.Position.ToString());
        }

        private void SliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
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
                PlaySlider.Maximum = mediaplayer.NaturalDuration.TimeSpan.TotalSeconds;
                audioTime.Content = "00:00/" + mediaplayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
                File.WriteAllText(AudioFolder + "\\Last.DAT", AudioNum.ToString());// if (File.Exists(AudioFolder + "\\Last.DAT")) AudioNum = Convert.ToInt32(File.ReadAllText(AudioFolder + "\\Last.DAT"));
                AudioLoaded = true;
                PlaySlider.Value = mediaplayer.Position.Seconds;
                if (PlayingRule == 3)
                {
                    if (File.Exists(AudioFolder + "\\Repeat.DAT"))
                    {
                        TimeSpan tp;
                        var res = TimeSpan.TryParse(File.ReadAllText(AudioFolder + "\\Repeat.DAT"), out tp);
                        if (res)
                        {
                            mediaplayer.Position = tp;
                            snackbarService.ShowAsync("成功跳转至指定时间戳。", "咕咕咕", SymbolRegular.Info12, ControlAppearance.Success);
                            Log.Information("成功跳转至指定时间戳。");
                        }
                        else snackbarService.ShowAsync("未检测到时间戳文件", "咕咕咕", SymbolRegular.Info12);


                    }
                }
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
                    IntlPlayer();
                    return;
                }
                else if (PlayingRule == 2)
                {
                    if (AudioNum >= AudioArray.Length) AudioNum = 0;
                    else AudioNum++;
                    IntlPlayer();
                }
                else if (PlayingRule == 3)
                {
                    IntlPlayer();
                    return;
                }
                else
                {
                    IsPlaying = false;
                    playbtn.Icon = SymbolRegular.Play48;
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
                        tmp.Content = "听两遍";
                        tmp.Tag = "3";
                        tmp.Icon = SymbolRegular.AnimalCat16;
                        PlayingRule = 3;
                        break;
                    case "3":
                        tmp.Content = "播完停止";
                        tmp.Tag = "0";
                        tmp.Icon = SymbolRegular.ArrowRepeatAllOff24;
                        PlayingRule = 0;
                        break;
                    default:
                        break;
                }
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
                    AudioScroll = 0;
                    MusicScroll.ScrollToLeftEnd();
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
            snackbarService.ShowAsync("一言已收藏", "已收藏至文件 " + @"D:\cokee_hitokoto.txt", SymbolRegular.Heart48);
            //NoticeBox.Show("已收藏至文件 " + filePath, "info", MessageBoxIcon.Info, true, 1000);
            WriteInfo(hitokoto.Content.ToString(), @"D:\cokee_hitokoto.txt");
        }

        private void WriteInfo(string info, string filepath)
        {
            using (FileStream stream = new FileStream(filepath, FileMode.Append))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.WriteLine($"{DateTime.Now} | {info};");
                }
            }
        }

        private void DebugPage(object sender, RoutedEventArgs e)
        {
            pager.Items.Insert(0, debugCard);
        }
        //测试函数 防止屏保无法退出
        private void FuncT1(object sender, MouseButtonEventArgs e)
        {
            Environment.Exit(0);
            //br1_blur.BeginAnimation(br1_blur.Radius,)
            //MainWindow.GetWindow(this).WindowState = WindowState.Normal;
            //StartSnowing(MainCanvas);
        }

        private void BtnHandler(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            switch (btn.Tag.ToString())
            {
                case "exit":
                    Environment.Exit(0);
                    break;
                case "explore":
                    Process.Start("C:\\Windows\\explorer.exe");
                    break;
                case "cap":
                    VideoCap();
                    break;
            }
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
            settings.BlockedImageIds += br1.Tag.ToString() + "|";
            settings.SaveSettings();
            snackbarService.ShowAsync("屏蔽成功", "已屏蔽日期为 " + br1.Tag.ToString() + " 的图片。", SymbolRegular.CheckmarkCircle24);
            _ = GetBingWapp();
        }

        /// <summary>
        ///一言处理
        /// </summary>
        private void ViewHitokoSource(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText("https://hitokoto.cn/?uuid=" + hkUrl);
            snackbarService.ShowAsync("链接已复制", "https://hitokoto.cn/?uuid=" + hkUrl, SymbolRegular.CopyAdd24);
        }

        private void EditText(object sender, RoutedEventArgs e)
        {
            if (hkself.Visibility == Visibility.Collapsed) hkself.Visibility = Visibility.Visible;
            else hkself.Visibility = Visibility.Collapsed;
            //Process.Start(@"D:\ink\TabTip.exe");
            //NoticeBox.Show("Done!", "Info", MessageBoxIcon.Info,true,1000);
        }

        /// <summary>
        ///u盘处理-打开
        /// </summary>
        private void OpenUsb(object sender, RoutedEventArgs e)
        {
            try
            {
                IsUsbOpened = true;
                Process.Start("explorer.exe", disk);
            }
            catch (Exception ex)
            {
                ProcessErr(ex);
            }
        }

        private async void LoadPage(object sender, RoutedEventArgs e)
        {
            frame.Source = new Uri(textBox.Text);
            await snackbarService.ShowAsync("Loaded Page:");
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

        private void Dialog_ButtonLeftClick(object sender, RoutedEventArgs e) => dialog.Hide();

        //---目前没什么用的函数
        /*    private async Task CheckUpdate()
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
            }*/


    }
}