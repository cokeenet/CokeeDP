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
        }
    }
}