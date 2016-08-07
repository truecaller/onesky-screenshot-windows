using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace OneSkyApp.Screenshot
{
    static class ControlExtensions
    {
        public static IEnumerable<T> FindChildren<T>(DependencyObject dependencyObject, Func<T, bool> condition = null)
            where T : DependencyObject
        {
            var count = VisualTreeHelper.GetChildrenCount(dependencyObject);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(dependencyObject, i);
                if (child == null)
                    continue;

                if (child is T && (condition == null || condition((T) child)))
                    yield return child as T;

                foreach (var c in FindChildren(child, condition))
                {
                    yield return c;
                }
            }
        }

        public static bool IsInViewPort(this FrameworkElement element, FrameworkElement rootElement)
        {
            var position = element.GetRelativePosition(rootElement);

            return element.Visibility == Visibility.Visible &&
                           //-1 because sometimes it happens for visible controls :)
                           position.X > -1 && position.Y > -1 &&
                           element.ActualWidth > 0 && element.ActualHeight > 0 &&
                           position.X < rootElement.ActualWidth && position.Y < rootElement.ActualHeight &&
                           //This is important to handle invisible popups
                           !element.IsAnyParentCollapsed();
        }

        static bool IsAnyParentCollapsed(this UIElement element)
        {
            if (element == null)
                return false;

            return element.Visibility == Visibility.Collapsed || IsAnyParentCollapsed(VisualTreeHelper.GetParent(element) as UIElement);
        }

        static readonly Type[] PivotTypes = new[] { typeof(Pivot), typeof(PivotItem), typeof(PivotHeaderItem) };
        public static bool IsPivotRelated(this UIElement element)
        {
            if (element == null || element is TextBlock)
                return false;

            return PivotTypes.Contains(element.GetType()) || IsPivotRelated(VisualTreeHelper.GetParent(element) as UIElement);
        }

        public static Point GetRelativePosition(this UIElement element, UIElement rootElement)
        {
            return element.TransformToVisual(rootElement).TransformPoint(new Point(0, 0));
        }

        static readonly IDictionary<Type, IList<PropertyInfo>> DependencyPropertyCache = new Dictionary<Type, IList<PropertyInfo>>();
        public static IEnumerable<DependencyProperty> GetAllNonEmptyStringProperties(this DependencyObject obj)
        {
            IList<PropertyInfo> propertyInfos;

            var type = obj.GetType();
            if (DependencyPropertyCache.ContainsKey(type))
                propertyInfos = DependencyPropertyCache[type];
            else
            {
                propertyInfos = type.GetRuntimeProperties().Where(p => p.PropertyType == typeof(DependencyProperty)).ToList();
                DependencyPropertyCache.Add(type, propertyInfos);
            }

            return propertyInfos.Where(obj.IsNonEmptyString).Select(p => (DependencyProperty) p.GetValue(obj));
        }

        static bool IsNonEmptyString(this DependencyObject obj, PropertyInfo propertyInfo)
        {
            var dependencyProperty = (DependencyProperty)propertyInfo.GetValue(obj);
            var value = obj.GetValue(dependencyProperty);
            return value is string && !string.IsNullOrWhiteSpace(value.ToString());
        }

        public static async Task<string> CaptureBase64PngAsync(this UIElement uiElement)
        {
            var renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(uiElement);
            var pixels = await renderTargetBitmap.GetPixelsAsync();
            var logicalDpi = DisplayInformation.GetForCurrentView().LogicalDpi;

            using (var stream = new InMemoryRandomAccessStream())
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
                encoder.SetPixelData(
                            BitmapPixelFormat.Bgra8,
                            BitmapAlphaMode.Ignore,
                            (uint)renderTargetBitmap.PixelWidth,
                            (uint)renderTargetBitmap.PixelHeight,
                            logicalDpi,
                            logicalDpi,
                            pixels.ToArray());

                await encoder.FlushAsync();

                var bytes = new byte[stream.Size];
                await stream.AsStream().ReadAsync(bytes, 0, bytes.Length);

                return Convert.ToBase64String(bytes);
            }   
        }
    }
}
