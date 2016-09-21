OneSky Auto Screenshot Library for Windows
======================================

This library helps you easily upload screenshots of your app that contains localizable strings to your OneSky project.

* Supported for **WP8.1 and W10 UWP** apps.
* Strings assigned through **Bindings, Uid or code-behind** are detected.
* Formatted strings `{0}` are also supported, however the logic can be improved.
* Viewport visibility is handled, (i.e If you take screenshot while on first tab, strings in other tabs are ignored).
* Strings in `ApplicationBar` are currently **NOT** handled.

**NOTE:** This library is **not designed for distribution** on `Windows Store` so remember to **remove** before submission.
To include the library only for `Debug` builds, modify your App's `csproj` file like this
```xml
	<Import Project="..\OneSkyApp.Screenshot\OneSkyApp.Screenshot.projitems" Label="Shared" Condition="'$(Configuration)' == 'Debug'" />
```

Installation
------------
1. Download Zip from this GitHub repository.
2. Extract archive and add `OneSkyApp.Screenshot` project into your solution. 
3. Add reference to `OneSkyApp.Screenshot shared project` in your root project.
4. Add `NuGet` package `Newtonsoft.Json` to your root project

Integration
-----------

In your ```App.xaml.cs``` file, import the library:

```c#
using OneSkyApp.Screenshot;
```

Add following code in ```OnLaunched()```:

```c#
await OneSkyScreenshotHelper.StartCapturingAsync(ONESKY_API_KEY, ONESKY_API_SECRET, ONESKY_PROJECT_ID, "Resources.resw");
```

```ONESKY_API_KEY```, ```ONESKY_API_SECRET``` can be found in **Site Settings** under **API Keys & Usage** on **OneSky Web Admin**.
```ONESKY_PROJECT_ID``` can be found under **All Projects** page.

UWP App Screenshots
-------------------

![Sample UWP App](https://github.com/truecaller/onesky-screenshot-windows/raw/master/Screenshots/resources.png)

![Sample UWP App](https://github.com/truecaller/onesky-screenshot-windows/raw/master/Screenshots/sampleapp.png)

OneSky Screenshots
-------------------

![Sample UWP App](https://github.com/truecaller/onesky-screenshot-windows/raw/master/Screenshots/phraseview.png)

![Sample UWP App](https://github.com/truecaller/onesky-screenshot-windows/raw/master/Screenshots/screenshotview.png)
