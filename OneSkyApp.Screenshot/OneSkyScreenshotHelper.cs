using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources.Core;
using Windows.Graphics.Display;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Newtonsoft.Json;
using System.Diagnostics;

namespace OneSkyApp.Screenshot
{
    public static class OneSkyScreenshotHelper
    {
        static string _apiKey, _apiSecret, _projectId, _oneSkyResourceFileName;

        static readonly Frame RootFrame = (Frame)Window.Current.Content;

        static readonly IDictionary<string, string> ResourceDictionary = new Dictionary<string, string>();
        static IEnumerable<KeyValuePair<string, string>> _formattedResources;

        static Image _cameraImage;
        static ProgressRing _progressRing;

        /// <summary>
        /// Captures png screenshot, tags resource keys and uploads them to OneSky for specified project
        /// </summary>
        /// <param name="apiKey">In OneSkyApp, Go to "Site Settings" -> Go to the tab "API Keys & Usage"</param>
        /// <param name="apiSecret">In OneSkyApp, Go to "Site Settings" -> Go to the tab "API Keys & Usage"</param>
        /// <param name="projectId">In OneSkyApp, Go to "Projects" and you will see there is a number next to the name of each of your project.</param>
        /// <param name="oneSkyResourceFileName">Name of the string file in OneSkyApp</param>
        /// <param name="mainResourceName">Name of the main resource in your solution (without .resw) e.g Resources</param>
        /// <returns></returns>
        public static async Task StartCapturingAsync(string apiKey, string apiSecret, string projectId, string oneSkyResourceFileName = "",
            string mainResourceName = "Resources")
        {
            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(apiSecret) || string.IsNullOrWhiteSpace(projectId))
                throw new ArgumentException($"{nameof(apiKey)}, {nameof(apiSecret)} and {nameof(projectId)} are required to use OneSkyScreenshotHelper");

            var task = InjectControlsAsync();

            _apiKey = apiKey;
            _apiSecret = apiSecret;
            _projectId = projectId;
            _oneSkyResourceFileName = oneSkyResourceFileName;
            
            FillResourceDictionary(mainResourceName);
            await task;
        }

        static async Task InjectControlsAsync()
        {
            //Allow existing controls to render first
            await Task.Delay(600);

            var panel = ControlExtensions.FindChildren<Panel>(RootFrame).First();
            _cameraImage = new Image
            {
                Source = new BitmapImage(new Uri("ms-appx:///OneSkyAssets/camera-icon.png", UriKind.Absolute)),
                Width = 60,
                Height = 60,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(20)
            };

            _cameraImage.Tapped += OnCameraImageTapped;
            panel.Children.Add(_cameraImage);

            _progressRing = new ProgressRing();
            panel.Children.Add(_progressRing);
        }

        static void FillResourceDictionary(string mainResourceName)
        {
            mainResourceName += "/";
            var mainResourceMap = ResourceManager.Current.MainResourceMap;
            var defaultContext = ResourceContext.GetForCurrentView();

            foreach (var resource in mainResourceMap.Where(x => x.Key.Contains(mainResourceName)))
            {
                var resourceValue = mainResourceMap.GetValue(resource.Key, defaultContext)?.ValueAsString;
                if (!string.IsNullOrWhiteSpace(resourceValue))
                    ResourceDictionary.Add(resource.Key.Replace(mainResourceName, string.Empty), resourceValue);
            }

            _formattedResources = ResourceDictionary.Where(x => x.Value.Contains("{") && x.Value.Contains("}")).ToList();
        }

        static async void OnCameraImageTapped(object sender, TappedRoutedEventArgs tappedRoutedEventArgs)
        {
            _cameraImage.Visibility = Visibility.Collapsed;

            try
            {
                var screenshots = await CreateScreenshotsAsync();

                //Show progress after screenshot is captured
                _progressRing.IsActive = true;

                var screenshotData = JsonConvert.SerializeObject(screenshots);
                Debug.WriteLine($"OneSky.ScreenshotData: {screenshotData}");
                await Request.UploadAsync(_apiKey, _apiSecret, _projectId, screenshotData);

                await new MessageDialog("Screenshot has been sent to OneSky.").ShowAsync();
            }
            catch(Exception e)
            {
                await new MessageDialog($"Failed to send screenshot to OneSky. {e.Message}").ShowAsync();
                Debug.WriteLine($"OneSky.Exception: {e}");
            }
            finally
            {
                _cameraImage.Visibility = Visibility.Visible;
                _progressRing.IsActive = false;
            }
        }

        static async Task<Screenshots> CreateScreenshotsAsync()
        {
            var screenshot = await CreateScreenshotAsync();
            return new Screenshots {screenshots = new[] {screenshot}};
        }

        static async Task<Screenshot> CreateScreenshotAsync()
        {
            var base64ImageTask = RootFrame.CaptureBase64PngAsync();
            var tags = CreateTags();

            var pageName = ControlExtensions.FindChildren<Page>(RootFrame).First().GetType().Name;

            return new Screenshot
            {
                name = $"{pageName}_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.png",
                image = $"data:image/png;base64,{await base64ImageTask}",
                tags = tags
            };
        }

        static IEnumerable<Tag> CreateTags()
        {
            //Special handling for pivots to avoid duplication
            var controls = ControlExtensions.FindChildren<FrameworkElement>(RootFrame, x => x.IsInViewPort(RootFrame) && !x.IsPivotRelated());

            return from control in controls
                   let properties = control.GetAllNonEmptyStringProperties()
                   from property in properties
                   select TryCreateTag(control, property) into tag
                   where tag != null
                   select tag;
        }

        static readonly double RawPixelsPerViewPixel = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
        static Tag TryCreateTag(FrameworkElement element, DependencyProperty property)
        {
            var propertyValue = element.GetValue(property).ToString();

            var resourceKey = FindResourceByBinding(element, property, propertyValue);

            if (string.IsNullOrEmpty(resourceKey))
                resourceKey = FindResourceByValue(propertyValue);

            if (string.IsNullOrEmpty(resourceKey))
                return null;

            var position = element.GetRelativePosition(RootFrame);

            var tag = new Tag
            {
                //Replace / with . to handle Uid cases
                key = resourceKey.Replace("/", "."),
                x = (int)Math.Floor(position.X * RawPixelsPerViewPixel),
                y = (int)Math.Floor(position.Y * RawPixelsPerViewPixel),
                width = (int)Math.Ceiling(element.ActualWidth * RawPixelsPerViewPixel),
                height = (int)Math.Ceiling(element.ActualHeight * RawPixelsPerViewPixel)
            };

            if (!string.IsNullOrWhiteSpace(_oneSkyResourceFileName))
                tag.file = _oneSkyResourceFileName;

            return tag;
        }

        static string FindResourceByBinding(FrameworkElement element, DependencyProperty property, string propertyValue)
        {
            var bindingExpression = element.GetBindingExpression(property);
            var bindingPath = bindingExpression?.ParentBinding.Path?.Path;

            if (string.IsNullOrEmpty(bindingPath))
                return null;

            var splittedPaths = bindingPath.Split(new[] {"."}, StringSplitOptions.RemoveEmptyEntries);
            var expectedKey = splittedPaths.Last();

            return ResourceDictionary.ContainsKey(expectedKey) && ResourceDictionary[expectedKey] == propertyValue ? expectedKey : null;
        }

        static string FindResourceByValue(string propertyValue)
        {
            var matchedValues = ResourceDictionary.Where(x => x.Value == propertyValue).ToList();

            if(!matchedValues.Any() && _formattedResources != null)
                return TryFindResourceInFormattedString(propertyValue);
            
            return matchedValues.Count == 1 ? matchedValues.First().Key : null;
        }

        static string TryFindResourceInFormattedString(string propertyValue)
        {
            var words = propertyValue.Split(' ');
            return (from resource in _formattedResources
                    let resourceWords = resource.Value.Split(' ')
                    where words.Length == resourceWords.Length
                    let isMatch = !words.Where((t, i) => t != resourceWords[i] && (!resourceWords[i].Contains("{") || !resourceWords[i].Contains("}"))).Any()
                    where isMatch select resource.Key).FirstOrDefault();
        }
    }
}
