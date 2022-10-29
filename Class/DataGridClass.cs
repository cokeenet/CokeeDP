using Panuon.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CokeeDP.Class
{
    public class ImageItem : NotifyPropertyChangedBase
    {
        public string Name { get => _displayName; set => Set(ref _displayName,value); }
        public string Path { get => p; set => Set(ref p,value); }
        public ImageSource ImageSr { get => ee; set => Set(ref ee,value); }
        private ImageSource ee;
        private string _displayName, p;
    }
    public class FastProgram
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public BitmapImage image { get; set; }
    }
    [ValueConversion(typeof(byte[]),typeof(BitmapImage))]
        public class ImageConverter : IValueConverter
        {
            public object Convert(object value,Type targetType,object parameter,System.Globalization.CultureInfo culture)
        {
            var ExePath = "";
            if(!string.IsNullOrEmpty(value.ToString()))
                {
                    ExePath = (string)value;
                }
                if(ExePath == null)
                {
                    return "";
                }
                return GetExeIcon(ExePath);                //以流的方式显示图片的方法
            }
        //转换器中二进制转化为BitmapImage  datagrid绑定仙石的
        public static ImageSource GetExeIcon(string fileName)
        {
            System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(fileName);
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                        icon.Handle,
                        new Int32Rect(0,0,icon.Width,icon.Height),
                        BitmapSizeOptions.FromEmptyOptions());
        }
        public object ConvertBack(object value,Type targetType,object parameter,System.Globalization.CultureInfo culture)
            {
                return null;
            }
        }
}
