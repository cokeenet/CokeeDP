using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Panuon.WPF.UI;
using Serilog;
using System;
using System.Windows;
using System.Windows.Media.Animation;
using Wpf.Ui.Appearance;

namespace CokeeDP
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void AppStartup(object sender, StartupEventArgs e)
        {
            Log.Logger = new LoggerConfiguration().WriteTo.File("log.txt",
               outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}").CreateLogger();
            if (Environment.OSVersion.Version.Major >= 10.0) AppCenter.Start("52a9c4e0-ad42-455b-b1cf-515d8a39f245", typeof(Analytics), typeof(Crashes));
            // Theme.Apply(ThemeType.Light,BackgroundType.Mica);
            Timeline.DesiredFrameRateProperty.OverrideMetadata(
                typeof(Timeline),
                new FrameworkPropertyMetadata { DefaultValue = 120 }
                );
            Log.Debug(e.Args.ToString());
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            
        }
        private static void CurrentDomain_UnhandledException(object sender,UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            Crashes.TrackError(ex);
            Log.Error(string.Format("捕获到未处理异常：{0}\r\n异常信息：{1}\r\n异常堆栈：{2}",ex.GetType(),ex.Message,ex.StackTrace));
        }
        static void Application_ThreadException(object sender,System.Threading.ThreadExceptionEventArgs e)
        {
            Exception ex = e.Exception;
            Crashes.TrackError(ex);
            Log.Error(string.Format("捕获到未处理异常：{0}\r\n异常信息：{1}\r\n异常堆栈：{2}",ex.GetType(),ex.Message,ex.StackTrace));
        }
    }
}