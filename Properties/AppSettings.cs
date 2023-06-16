using Microsoft.AppCenter.Ingestion.Models;
using System;
using System.IO;
using System.Text.Json;
using Serilog;
using Log = Serilog.Log;
using System.Linq;

namespace CokeeDP.Properties
{

    public class AppSettings
    {
        public string OneWordsApi { get; set; } = "https://v1.hitokoto.cn/?c=k";
        public bool OneWordsEnable { get; set; } = false;
        public bool disable_clock1 { get; set; } = false;
        public string OneWordsTimeInterval { get; set; } = "300";
        public string WeatherTimeInterval { get; set; } = "9800";
        public string City { get; set; } = "中国 北京";
        public string CountdownName { get; set; } = "高考";
        public DateTime CountdownTime { get; set; } = new DateTime(2025, 6, 5);
        public bool BingWappEnable { get; set; } = true;
        public bool EnableBigTimeTo { get; set; } = false;
        public string Font { get; set; } = "Segoe UI, 8.25pt";
        public bool isChildLocked { get; set; } = false;
        public bool isDebug { get; set; } = false;
        public string AudioFolder { get; set; } = null;
        public bool UHDEnable { get; set; } = true;
        public string CityId { get; set; } = "101010100";
        public string BlockedImageIds { get; set; } = null;
        public string CachedWeatherData { get; set; } = null;
        public int OneWordsComboBoxIndex { get; set; } = 0;
        public DateTime CachedWeatherTime { get; set; } = new DateTime(2000, 1, 1);
        public bool SnowEnable { get; set; } = false;
        public bool BingVideoEnable { get; set; } = false;

    }
    public static class AppSettingsExtensions
    {
        private const string SETTINGS_FILE_NAME = "D:\\Program Files (x86)\\CokeeTech\\CokeeDP\\config.json";

        public static AppSettings LoadSettings()
        {
            try
            {
                var dir = SETTINGS_FILE_NAME.Split("config.json")[0];
                if(!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                //if(!File.Exists(SETTINGS_FILE_NAME)) File.Create(SETTINGS_FILE_NAME);
                var content = File.ReadAllText(SETTINGS_FILE_NAME);
                return JsonSerializer.Deserialize<AppSettings>(content);
            }
            catch (Exception e)
            {
                Log.Error($"Error while loading settings:"+e.ToString());
                return new AppSettings();
            }
        }

        public static void SaveSettings(this AppSettings settings)
        {
            var content = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });

            try
            {
                var dir = SETTINGS_FILE_NAME.Split("config.json")[0];
                if(!Directory.Exists(dir)) Directory.CreateDirectory(dir);
               // if(!File.Exists(SETTINGS_FILE_NAME)) File.Create(SETTINGS_FILE_NAME);
                File.WriteAllText(SETTINGS_FILE_NAME, content);
            }
            catch (Exception e)
            {
                Log.Error($"Error while saving settings: "+e.ToString());
            }
        }

    }
}