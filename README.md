


<br />
<p align="center">
  <h1 align="center">Mopups</h3>
  <p align="center">
    Customisable Popups for MAUI
    <br />
  </p>
</p>



<!-- TABLE OF CONTENTS -->

## Table of Contents

[![NuGet Downloads](https://img.shields.io/nuget/dt/Mopups.svg)](https://www.nuget.org/packages/Mopups/)
[![nuget](https://img.shields.io/nuget/v/Mopups.svg)](https://www.nuget.org/packages/Mopups/)
* [About the Project](#about-the-project)
* [Getting Started](#getting-started)
  * [Installation](#installation)
* [Usage](#usage)
* [License](#license)
* [Contact](#contact)



<!-- ABOUT THE PROJECT -->
## About The Project

Mopups is a replacement for the "Rg.Plugins.Popups" plugin for Xamarin. Mopups intends to provide a similar experience to this plugin, however also clean up the code base and provide forward looking enhancements. Developers familar with the original plugin should find it a smooth transition, but we do recommend reading the wiki and reaching out with any issues.

The "PreBaked" is a neat blend of Mopups and AsyncAwaitBestPractices plugins to bring you a quick way to add popups into your MAUIs App using familiar concepts 

Platforms Supported (Current)
- Android 
- iOS
- Windows
- MacOS (Mac Catalyst) (This is a bit iffy..)


Below is a video by @jfversluis introducing Mopups
[![Watch the video](https://img.youtube.com/vi/OGWhgASmSto/hqdefault.jpg)](https://youtu.be/OGWhgASmSto)


<!-- GETTING STARTED -->
## Getting Started

First, you must follow the [initialisation](https://github.com/LuckyDucko/Mopups/wiki/Setup)

### Installation

You can install the nuget by looking up 'Mopups' in your nuget package manager, or by getting it [here](https://www.nuget.org/packages/Mopups/)



<!-- USAGE EXAMPLES -->
## Usage
here is an example of what this plugin makes easy (Looks slow due to giphy)

![Gif Example](https://j.gifs.com/xn4mw9.gif)


### New Example
To Use the plugin for its inbuilt popup pages in a basic setting (Dual/Single Response, Login, TextInput EntryInput,and loader.) All you need are these one liners

`SingleResponse Popup Page`
```csharp
return await SingleResponseViewModel.AutoGenerateBasicPopup(Color.HotPink, Color.Black, "I Accept", Color.Gray, "Good Job, enjoy this single response example", "thumbsup.png");
```

`DualResponse Popup Page`
```csharp
return await DualResponseViewModel.AutoGenerateBasicPopup(Color.WhiteSmoke, Color.Red, "Okay", Color.WhiteSmoke, Color.Green, "Looks Good!", Color.DimGray, "This is an example of a dual response popup page", "thumbsup.png");
```

`Loader Popup Page`
```csharp
  await PreBakedMopupService.GetInstance().WrapTaskInLoader(Task.Delay(10000), Color.Blue, Color.White, LoadingReasons(), Color.Black);
```

`Text Input PopupPage`
```csharp
await TextInputViewModel.AutoGenerateBasicPopup(Color.WhiteSmoke, Color.Red, "Cancel", Color.WhiteSmoke, Color.Green, "Submit", Color.DimGray, "Text input Example", string.Empty);
```
`Entry Input PopupPage`
```csharp
await EntryInputViewModel.AutoGenerateBasicPopup(Color.WhiteSmoke, Color.Red, "Cancel", Color.WhiteSmoke, Color.Green, "Submit", Color.DimGray, "Text input Example", string.Empty);
```

`LoginPage PopupPage`
```csharp
var (username, password) = await LoginViewModel.AutoGenerateBasicPopup(Color.WhiteSmoke, Color.Red, "Cancel", Color.WhiteSmoke, Color.Green, "Submit", Color.DimGray, string.Empty, "Username Here", string.Empty, "Password here", "thumbsup.png", 0, 0);
```

or, to return from the loader a value
```csharp
await PreBakedMopupService.GetInstance().WrapReturnableTaskInLoader<bool, LoaderPopupPage>(IndepthCheckAgainstDatabase(), Color.Blue, Color.White, LoadingReasons(), Color.Black);
```

you can also add in synchronous functions, however they are wrapped in a task
```csharp

private bool LongRunningFunction(int millisecondDelay)
{
    Thread.Sleep(millisecondDelay);
    return true;
}
await PreBakedMopupService.GetInstance().WrapReturnableFuncInLoader(LongRunningFunction, 6000, Color.Blue, Color.White, LoadingReasons(), Color.Black);

```

## That's it! for advanced usage read on

In Version 1.2.0, Mopups has added some pre created pages that can provide users the ability to return data from popups. I have also added the ability to overload the look of these pages and create your own. 

I do wish it were simpler, however, with the limited time i have to work on this, it'll have to do. 

This set of API's will be used for when the basic API wont cut it, without relying on me making another overload for every situation under the sun.

This API introduces

 `GeneratePopup<TPopupPage>`
Which allows you to supply your own popuppage xaml which will then be attached to whatever VM you called it from. 

`GeneratePopup(Dictionary<string, object> propertyDictionary)`
Which allows you have a dictionary of values that a popup uses, pass and automatically attach to the appropriate properties on the VM side

These are both non-static. and require you to have an instance of the ViewModel to work with. Hence

`<ViewModelClassNameHere>.GenerateVM()`
Which provides you with a new instance of that VM

`<ViewModelClassNameHere>.PullViewModelProperties()`
Which collects all the properties of a VM, and provides them to you in a dictionary, so you can reuse and also while debugging, check what exists/whats been changed 
Returns this `Dictionary<string, (object property, Type propertyType)> `

However, for initialisation, i internally (and you can use) the following function
`<ViewModelClassNameHere>.InitialiseOptionalProperties(Dictionary<string, object> optionalProperties)`
Which will attempt to set each of the viewmodel properties with the corrosponding value in the dictionary

So, to fix that, i provide
`<ViewModelClassNameHere>.FinalisePreparedProperties(Dictionary<string, (object property, Type propertyType)> viewModelProperties)`
Which takes in the `Dictionary<string, (object property, Type propertyType)> ` and creates `Dictionary<string, object> optionalProperties`



**If you want to make your own Popup Page**

This is the real power of this Plugin . If you look at the source for DualResponsePopupPage, or the SingleResponse version you'll notice that they are just simple Xaml Pages. Nothing fancy.

You can create the full thing yourself
1. Create Xaml Page with codebehind
2. Create your ViewModel that is associated with the popup, lets call ours `InformationPopupPage`
3. Ensure your ViewModel inherits from `PopupViewModel<TReturnable>` where `TReturnable` is what you want the popuppage to return to its caller
4. Ensure that your xaml page codebehind inherits from `PopupPage` (requirement to use rg plugins popup) and `IGenericViewModel<TViewModel>` where `TViewModel` is your Viewmodel, in our case it will be `IGenericViewModel<InformationPopupPage>`
5. You're ready to start using it the same as `DualResponsePopupPage`

or you can provide your own Xaml Page, with a codebehind that inherits from `PopupPage` and `IGenericViewModel<TViewModel>` where `TViewModel` is the plugin provided VM you wish to use.

to use this version, just call `TViewModel.GeneratePopup<YourXamlPopupPage>()`



<!-- LICENSE -->
## License
This project uses the MIT License

<!-- CONTACT -->
## Contact
My [Github](https://github.com/LuckyDucko),


