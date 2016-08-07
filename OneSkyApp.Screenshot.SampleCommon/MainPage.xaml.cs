using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace OneSkyApp.Screenshot.SampleCommon
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            Loaded += MainPage_Loaded;
        }

        void MainPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var text = StringResources.AllKeys.FormattedText;
            textBlock.Text = string.Format(text, text.Split(' ').Length);
        }
    }
}
