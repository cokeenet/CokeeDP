using Panuon.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace CokeeDP.Class
{
    public class UIAnimation
    {
        /// <summary>
        /// 淡入动画 (控件名, 0：上方；1：右方；2：下方；3：左方, 淡入的距离，持续时间)
        /// </summary>
        /// <param name="element">控件名</param>
        /// <param name="direction">0：上方；1：右方；2：下方；3：左方</param>
        /// <param name="distance">淡入的距离</param>
        /// <param name="duration">持续时间</param>
        public void Appear(FrameworkElement element, int direction = 0, int distance = 20, double duration = .3)
        {
            //将所选控件的Visibility属性改为Visible，这里要首先执行否则动画看不到
            ObjectAnimationUsingKeyFrames VisbilityAnimation = new ObjectAnimationUsingKeyFrames();
            DiscreteObjectKeyFrame kf = new DiscreteObjectKeyFrame(Visibility.Visible, new TimeSpan(0, 0, 0));
            VisbilityAnimation.KeyFrames.Add(kf);
            element.BeginAnimation(Border.VisibilityProperty, VisbilityAnimation);

            //创建新的缩放动画
            TranslateTransform TT = new TranslateTransform();
            element.RenderTransform = TT;
            //创建缩放动画函数，可以自己修改
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
            //创建缩放动画函数，可以自己修改
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

            //将所选控件的Visibility属性改为Collapsed，这样不占用空间
            ObjectAnimationUsingKeyFrames VisbilityAnimation = new ObjectAnimationUsingKeyFrames();
            DiscreteObjectKeyFrame kf = new DiscreteObjectKeyFrame(Visibility.Collapsed, new TimeSpan(0, 0, 1));
            VisbilityAnimation.KeyFrames.Add(kf);
            element.BeginAnimation(Border.VisibilityProperty, VisbilityAnimation);
        }
    }
    public class ImageItem : NotifyPropertyChangedBase
    {
        public string Name
        {
            get => _displayName; set => Set(ref _displayName, value);
        }
        public string Path
        {
            get => p; set => Set(ref p, value);
        }
        public ImageSource ImageSr
        {
            get => ee; set => Set(ref ee, value);
        }
        private ImageSource ee;
        private string _displayName, p;
    }
    public class FastProgram
    {
        public int Id
        {
            get; set;
        }
        public string Name
        {
            get; set;
        }
        public BitmapImage image
        {
            get; set;
        }
    }

    [ValueConversion(typeof(byte[]), typeof(BitmapImage))]
    public class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var ExePath = "C:\\Program Files (x86)\\Seewo\\EasiNote5\\swenlauncher\\swenlauncher.exe";
            if (!string.IsNullOrEmpty(value.ToString()))
            {
                ExePath = (string)value;
            }
            if (ExePath == null)
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
                        new Int32Rect(0, 0, icon.Width, icon.Height),
                        BitmapSizeOptions.FromEmptyOptions());
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
