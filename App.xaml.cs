using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

using CokeeDP.Views.Windows;

using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

using Serilog;
using Serilog.Sink.AppCenter;

using Timeline = System.Windows.Media.Animation.Timeline;

namespace CokeeDP
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void AppStartup(object sender, StartupEventArgs e)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("log.txt",
               outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.AppCenterSink(null, Serilog.Events.LogEventLevel.Information, AppCenterTarget.ExceptionsAsCrashes, "52a9c4e0-ad42-455b-b1cf-515d8a39f245")
                .CreateLogger();

            if (Environment.OSVersion.Version.Major >= 10.0) AppCenter.Start("52a9c4e0-ad42-455b-b1cf-515d8a39f245", typeof(Analytics), typeof(Crashes));

            // Theme.Apply(ThemeType.Light,BackgroundType.Mica);

            Timeline.DesiredFrameRateProperty.OverrideMetadata(
                typeof(Timeline),
                new FrameworkPropertyMetadata { DefaultValue = 120 }
                );
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            //Double process not allow
            if (Process.GetProcessesByName("CokeeDP.exe").Length >= 2 || Process.GetProcessesByName("CokeeDP.scr").Length >= 2)
            {
                //MessageBox.Show("1");
                Environment.Exit(0);
            }
        }

        private void Current_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            Exception ex = e.Exception;
            Crashes.TrackError(ex);
            Log.Error($"捕获到未处理异常：{ex.GetType()}\r\n异常信息：{ex.Message}\r\n异常堆栈：{ex.StackTrace}", e);
            var a = Application.Current.MainWindow as MainWindow;
            if (a != null)
            {
                a.ProcessErr(ex);
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            Crashes.TrackError(ex);
            Log.Error($"捕获到未处理异常：{ex.GetType()}\r\n异常信息：{ex.Message}\r\n异常堆栈：{ex.StackTrace}", e);
        }
    }
}