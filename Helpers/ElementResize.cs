using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CokeeDP
{
    class Resize
    {
        public static ControlTemplate GetResizeTemplate(DependencyObject obj)
        {
            return (ControlTemplate)obj.GetValue(ResizeTemplateProperty);
        }

        public static void SetResizeTemplate(DependencyObject obj,ControlTemplate value)
        {
            obj.SetValue(ResizeTemplateProperty,value);
        }

        // Using a DependencyProperty as the backing store for ResizeTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ResizeTemplateProperty =
            DependencyProperty.RegisterAttached("ResizeTemplate",typeof(ControlTemplate),typeof(Resize),new PropertyMetadata(null,(d,e) =>
            {
                var c = d as FrameworkElement;
                if(c == null)
                    return;
                var r = GetResizeable(c);
                if(r == null)
                    return;
                r.Detach();
                r.Attach();

            }));
        public static bool GetIsResizeable(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsResizeableProperty);
        }
        public static void SetIsResizeable(DependencyObject obj,bool value)
        {
            obj.SetValue(IsResizeableProperty,value);
        }

        // Using a DependencyProperty as the backing store for IsResizeable.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsResizeableProperty =
            DependencyProperty.RegisterAttached("IsResizeable",typeof(bool),typeof(Resize),new PropertyMetadata(false,(d,e) =>
            {

                var c = d as FrameworkElement;
                if(c == null)
                    return;
                var r = GetResizeable(c);
                if(r != null)
                    r.Detach();
                if((bool)e.NewValue)
                {
                    if(!c.IsLoaded)
                    {
                        RoutedEventHandler l = null;
                        l = (s,e) =>
                        {
                            var r = new Resizeable(c);
                            r.Attach();
                            SetResizeable(c,r);
                            c.Loaded -= l;
                        };
                        c.Loaded += l;
                    }
                    else
                    {
                        r = new Resizeable(c);
                        r.Attach();
                        SetResizeable(c,r);
                    }
                }
                else
                {
                    SetResizeable(c,null);
                }
            }));
        static Resizeable GetResizeable(DependencyObject obj)
        {
            return (Resizeable)obj.GetValue(ResizeableProperty);
        }

        static void SetResizeable(DependencyObject obj,Resizeable value)
        {
            obj.SetValue(ResizeableProperty,value);
        }

        // Using a DependencyProperty as the backing store for Resizeable.  This enables animation, styling, binding, etc...
        static readonly DependencyProperty ResizeableProperty =
           DependencyProperty.RegisterAttached("Resizeable",typeof(Resizeable),typeof(Resize),new PropertyMetadata(null));

        public class Resizeable
        {
            ResizeType _resizeType;
            enum ResizeType
            {
                None,
                Grid,
                Canvas,
                Window,
                Others,
            }
            FrameworkElement _resizeTarget;

            FrameworkElement _thumbsParent = null;
            public Resizeable(FrameworkElement resizeTarget)
            {
                _resizeTarget = resizeTarget;
            }

            public void Attach()
            {
                FrameworkElement thumbsRoot = null;
                var t = GetResizeTemplate(_resizeTarget);
                if(t != null)
                {
                    var c = new Control();
                    c.Template = t;
                    c.ApplyTemplate();
                    thumbsRoot = c;
                    _thumbsParent = VisualTreeHelper.GetChild(thumbsRoot,0) as FrameworkElement;
                }


                if(thumbsRoot != null)
                {
                    UIElement c;
                    if(_resizeTarget is Window)
                    {
                        c = ((Window)(_resizeTarget)).Content as UIElement;
                    }
                    else
                    {
                        c = _resizeTarget as UIElement;
                    }
                    if(c != null)
                    {
                        var layer = AdornerLayer.GetAdornerLayer(c);
                        if(layer == null)
                            throw new Exception("获取控件装饰层失败，控件可能没有装饰层！");
                        layer.Add(new ResizeAdorner(c,thumbsRoot));
                    }
                }

                //使用缺省样式
                if(thumbsRoot == null)
                {
                    Thumb leftThumb = null, topThumb = null, rightThumb = null, bottomThumb = null, lefTopThumb = null, rightTopThumb = null, rightBottomThumb = null, leftbottomThumb = null;
                    int thumnSize = 10;
                    //初始化thumb
                    leftThumb = new Thumb();
                    leftThumb.Template = new ControlTemplate(typeof(Thumb))
                    {
                        VisualTree = GetFactory(new SolidColorBrush(Colors.Transparent))
                    };
                    leftThumb.HorizontalAlignment = HorizontalAlignment.Left;
                    leftThumb.VerticalAlignment = VerticalAlignment.Stretch;
                    leftThumb.Cursor = Cursors.SizeWE;
                    leftThumb.Width = thumnSize;
                    leftThumb.Height = double.NaN;
                    leftThumb.Margin = new Thickness(0,thumnSize,0,thumnSize);

                    topThumb = new Thumb();
                    topThumb.Template = new ControlTemplate(typeof(Thumb))
                    {
                        VisualTree = GetFactory(new SolidColorBrush(Colors.Transparent))
                    };
                    topThumb.HorizontalAlignment = HorizontalAlignment.Stretch;
                    topThumb.VerticalAlignment = VerticalAlignment.Top;
                    topThumb.Cursor = Cursors.SizeNS;
                    topThumb.Width = double.NaN;
                    topThumb.Height = thumnSize;
                    topThumb.Margin = new Thickness(thumnSize,0,thumnSize,0);

                    rightThumb = new Thumb();
                    rightThumb.Template = new ControlTemplate(typeof(Thumb))
                    {
                        VisualTree = GetFactory(new SolidColorBrush(Colors.Transparent))
                    };
                    rightThumb.HorizontalAlignment = HorizontalAlignment.Right;
                    rightThumb.VerticalAlignment = VerticalAlignment.Stretch;
                    rightThumb.Cursor = Cursors.SizeWE;
                    rightThumb.Width = thumnSize;
                    rightThumb.Height = double.NaN;
                    rightThumb.Margin = new Thickness(0,thumnSize,0,thumnSize);

                    bottomThumb = new Thumb();
                    bottomThumb.Template = new ControlTemplate(typeof(Thumb))
                    {
                        VisualTree = GetFactory(new SolidColorBrush(Colors.Transparent))
                    };
                    bottomThumb.HorizontalAlignment = HorizontalAlignment.Stretch;
                    bottomThumb.VerticalAlignment = VerticalAlignment.Bottom;
                    bottomThumb.Cursor = Cursors.SizeNS;
                    bottomThumb.Width = double.NaN;
                    bottomThumb.Height = thumnSize;
                    bottomThumb.Margin = new Thickness(thumnSize,0,thumnSize,0);

                    lefTopThumb = new Thumb();
                    lefTopThumb.Template = new ControlTemplate(typeof(Thumb))
                    {
                        VisualTree = GetFactory(new SolidColorBrush(Colors.Transparent))
                    };
                    lefTopThumb.HorizontalAlignment = HorizontalAlignment.Left;
                    lefTopThumb.VerticalAlignment = VerticalAlignment.Top;
                    lefTopThumb.Cursor = Cursors.SizeNWSE;
                    lefTopThumb.Width = thumnSize;
                    lefTopThumb.Height = thumnSize;

                    rightTopThumb = new Thumb();
                    rightTopThumb.Template = new ControlTemplate(typeof(Thumb))
                    {
                        VisualTree = GetFactory(new SolidColorBrush(Colors.Transparent))
                    };
                    rightTopThumb.HorizontalAlignment = HorizontalAlignment.Right;
                    rightTopThumb.VerticalAlignment = VerticalAlignment.Top;
                    rightTopThumb.Cursor = Cursors.SizeNESW;
                    rightTopThumb.Width = thumnSize;
                    rightTopThumb.Height = thumnSize;

                    rightBottomThumb = new Thumb();
                    rightBottomThumb.Template = new ControlTemplate(typeof(Thumb))
                    {
                        VisualTree = GetFactory(new SolidColorBrush(Colors.Transparent))
                    };
                    rightBottomThumb.HorizontalAlignment = HorizontalAlignment.Right;
                    rightBottomThumb.VerticalAlignment = VerticalAlignment.Bottom;
                    rightBottomThumb.Cursor = Cursors.SizeNWSE;
                    rightBottomThumb.Width = thumnSize;
                    rightBottomThumb.Height = thumnSize;

                    leftbottomThumb = new Thumb();
                    leftbottomThumb.Template = new ControlTemplate(typeof(Thumb))
                    {
                        VisualTree = GetFactory(new SolidColorBrush(Colors.Transparent))
                    };
                    leftbottomThumb.HorizontalAlignment = HorizontalAlignment.Left;
                    leftbottomThumb.VerticalAlignment = VerticalAlignment.Bottom;
                    leftbottomThumb.Cursor = Cursors.SizeNESW;
                    leftbottomThumb.Width = thumnSize;
                    leftbottomThumb.Height = thumnSize;
                    var grid = new Grid();
                    grid.Children.Add(leftThumb);
                    grid.Children.Add(topThumb);
                    grid.Children.Add(rightThumb);
                    grid.Children.Add(bottomThumb);
                    grid.Children.Add(lefTopThumb);
                    grid.Children.Add(rightTopThumb);
                    grid.Children.Add(rightBottomThumb);
                    grid.Children.Add(leftbottomThumb);
                    UIElement c;
                    if(_resizeTarget is Window)
                    {
                        c = ((Window)(_resizeTarget)).Content as UIElement;
                    }
                    else
                    {
                        c = _resizeTarget as UIElement;
                    }
                    if(c != null)
                    {
                        var layer = AdornerLayer.GetAdornerLayer(c);
                        if(layer == null)
                            throw new Exception("获取控件装饰层失败，控件可能没有装饰层！");
                        layer.Add(new ResizeAdorner(c,grid));
                    }
                    thumbsRoot = grid;
                    _thumbsParent = thumbsRoot;
                }

                //注册事件
                for(int i = 0; i < VisualTreeHelper.GetChildrenCount(_thumbsParent); i++)
                {
                    var thumb = VisualTreeHelper.GetChild(_thumbsParent,i) as Thumb;
                    if(thumb != null)
                    {
                        thumb.DragStarted += Thumb_DragStarted;
                        thumb.DragDelta += Thumb_DragDelta;
                    }
                }
            }
            public void Detach()
            {
                for(int i = 0; i < VisualTreeHelper.GetChildrenCount(_thumbsParent); i++)
                {
                    var thumb = VisualTreeHelper.GetChild(_thumbsParent,i) as Thumb;
                    if(thumb != null)
                    {
                        thumb.DragStarted -= Thumb_DragStarted;
                        thumb.DragDelta -= Thumb_DragDelta;
                    }
                }
                _thumbsParent = null;
            }

            private void Thumb_DragStarted(object sender,DragStartedEventArgs e)
            {
                _resizeType = GetResizeType();
            }
            private void Thumb_DragDelta(object sender,DragDeltaEventArgs e)
            {
                switch(_resizeType)
                {
                    case ResizeType.None:
                        break;
                    case ResizeType.Grid:
                        {
                            var c = _resizeTarget as FrameworkElement;
                            var thumb = sender as FrameworkElement;
                            double left, top, right, bottom, width, height;
                            if(thumb.HorizontalAlignment == HorizontalAlignment.Left)
                            {
                                right = c.Margin.Right;
                                left = c.Margin.Left + e.HorizontalChange;
                                width = (double.IsNaN(c.Width) ? c.ActualWidth : c.Width) - e.HorizontalChange;
                            }
                            else
                            {
                                left = c.Margin.Left;
                                right = c.Margin.Right - e.HorizontalChange;
                                width = (double.IsNaN(c.Width) ? c.ActualWidth : c.Width) + e.HorizontalChange;
                            }

                            if(thumb.VerticalAlignment == VerticalAlignment.Top)
                            {
                                bottom = c.Margin.Bottom;
                                top = c.Margin.Top + e.VerticalChange;
                                height = (double.IsNaN(c.Height) ? c.ActualHeight : c.Height) - e.VerticalChange;
                            }
                            else
                            {
                                top = c.Margin.Top;
                                bottom = c.Margin.Bottom - e.VerticalChange;
                                height = (double.IsNaN(c.Height) ? c.ActualHeight : c.Height) + e.VerticalChange;
                            }
                            if(thumb.HorizontalAlignment != HorizontalAlignment.Center && thumb.HorizontalAlignment != HorizontalAlignment.Stretch)
                            {
                                if(width >= 0)
                                {
                                    c.Margin = new Thickness(left,c.Margin.Top,right,c.Margin.Bottom);
                                    c.Width = width;
                                }
                            }
                            if(thumb.VerticalAlignment != VerticalAlignment.Center && thumb.VerticalAlignment != VerticalAlignment.Stretch)
                            {
                                if(height >= 0)
                                {
                                    c.Margin = new Thickness(c.Margin.Left,top,c.Margin.Right,bottom);
                                    c.Height = height;
                                }
                            }
                        }
                        break;
                    case ResizeType.Canvas:
                        {
                            var c = _resizeTarget as FrameworkElement;
                            var thumb = sender as FrameworkElement;
                            double left, top, width, height;
                            if(thumb.HorizontalAlignment == HorizontalAlignment.Left)
                            {

                                left = double.IsNaN(Canvas.GetLeft(c)) ? 0 : Canvas.GetLeft(c) + e.HorizontalChange;
                                width = c.Width - e.HorizontalChange;
                            }
                            else
                            {
                                left = Canvas.GetLeft(c);
                                width = c.Width + e.HorizontalChange;
                            }
                            if(thumb.VerticalAlignment == VerticalAlignment.Top)
                            {
                                top = double.IsNaN(Canvas.GetTop(c)) ? 0 : Canvas.GetTop(c) + e.VerticalChange;
                                height = c.Height - e.VerticalChange;
                            }
                            else
                            {
                                top = Canvas.GetTop(c);
                                height = c.Height + e.VerticalChange;
                            }
                            if(thumb.HorizontalAlignment != HorizontalAlignment.Center && thumb.HorizontalAlignment != HorizontalAlignment.Stretch)
                            {
                                if(width >= 0)
                                {

                                    Canvas.SetLeft(c,left);
                                    c.Width = width;
                                }
                            }
                            if(thumb.VerticalAlignment != VerticalAlignment.Center && thumb.VerticalAlignment != VerticalAlignment.Stretch)
                            {
                                if(height >= 0)
                                {
                                    Canvas.SetTop(c,top);
                                    c.Height = height;
                                }
                            }
                        }
                        break;
                    case ResizeType.Window:
                        {
                            var c = _resizeTarget as Window;
                            var thumb = sender as FrameworkElement;
                            double left, top, width, height;
                            if(thumb.HorizontalAlignment == HorizontalAlignment.Left)
                            {
                                left = c.Left + e.HorizontalChange;
                                width = c.Width - e.HorizontalChange;

                            }
                            else
                            {
                                left = c.Left;
                                width = c.Width + e.HorizontalChange;
                            }

                            if(thumb.VerticalAlignment == VerticalAlignment.Top)
                            {

                                top = c.Top + e.VerticalChange;
                                height = c.Height - e.VerticalChange;
                            }
                            else
                            {
                                top = c.Top;
                                height = c.Height + e.VerticalChange;
                            }

                            if(thumb.HorizontalAlignment != HorizontalAlignment.Center && thumb.HorizontalAlignment != HorizontalAlignment.Stretch)
                            {
                                if(width > 63)
                                {
                                    c.Width = width;
                                    c.Left = left;
                                }
                            }
                            if(thumb.VerticalAlignment != VerticalAlignment.Center && thumb.VerticalAlignment != VerticalAlignment.Stretch)
                            {
                                if(height > 63)
                                {
                                    c.Top = top;
                                    c.Height = height;
                                }
                            }
                        }
                        break;
                    case ResizeType.Others:

                        break;
                }
            }
            ResizeType GetResizeType()
            {
                ResizeType rt;
                var p = VisualTreeHelper.GetParent(_resizeTarget);
                if(p is Grid)
                {
                    rt = ResizeType.Grid;
                }
                else if(p is Canvas)
                {
                    rt = ResizeType.Canvas;

                }
                else if(_resizeTarget is Window)
                {
                    rt = ResizeType.Window;
                }
                else
                {
                    rt = ResizeType.Grid;
                }
                return rt;
            }

            //拖动逻辑        
            FrameworkElementFactory GetFactory(Brush back)
            {
                var fef = new FrameworkElementFactory(typeof(Rectangle));
                fef.SetValue(Rectangle.FillProperty,back);
                return fef;
            }
            class ResizeAdorner : Adorner
            {
                //布局容器，如果不使用布局容器，则需要给上述8个控件布局，实现和Grid布局定位是一样的，会比较繁琐且意义不大。
                UIElement _child;
                public ResizeAdorner(UIElement adornedElement,UIElement child) : base(adornedElement)
                {
                    _child = child;
                    AddVisualChild(child);
                }

                protected override Visual GetVisualChild(int index)
                {
                    return _child;
                }
                protected override int VisualChildrenCount
                {
                    get
                    {
                        return 1;
                    }
                }
                protected override Size ArrangeOverride(Size finalSize)
                {
                    //直接给grid布局，grid内部的thumb会自动布局。
                    _child.Arrange(new Rect(new Point(0,0),finalSize));
                    return finalSize;
                }
            }

        }
    }
}
