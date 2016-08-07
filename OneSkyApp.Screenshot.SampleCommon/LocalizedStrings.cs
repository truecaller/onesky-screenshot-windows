









 
//------------------------------------------------------------------------------
//     Add a global shortcut in MSVS for beeing able to generate this file after 
//     you modify localization strings from resource files use:
//     MENU / TOOLS / OPTIONS / Keyboard
//     Add Command 'Build.TransformAllT4Templates' with shortcut Ctrl+Shift+Alt+T
//------------------------------------------------------------------------------

using Windows.ApplicationModel.Resources; 

namespace OneSkyApp.Screenshot.SampleCommon
{
    public class StringResources
	{
	  static StringResources()
		{
			AllKeys = new LocalizedStrings();
		}

		public static LocalizedStrings AllKeys { get; }

		public LocalizedStrings Keys => AllKeys;
	}

    public class LocalizedStrings
	{
		static readonly ResourceLoader ResourceLoader = ResourceLoader.GetForViewIndependentUse(); 	
  
		public string FormattedText => ResourceLoader.GetString("FormattedText");
  
		public string PivotItem1 => ResourceLoader.GetString("PivotItem1");
  
		public string PivotItem2 => ResourceLoader.GetString("PivotItem2");
 
	}
}

