using AsyncAwaitBestPractices.MVVM;

using Mopups.Animations;
using Mopups.PreBaked.PopupPages.SingleResponse;
using Mopups.Pages;
using Mopups.Services;


using Button = Microsoft.Maui.Controls.Button;
using ScrollView = Microsoft.Maui.Controls.ScrollView;
using Mopups.PreBaked.PopupPages.DualResponse;
using Mopups.PreBaked.Services;

namespace SampleMaui.CSharpMarkup;
public partial class PreBakedExample : ContentPage
{
    public PreBakedExample()
    {
        BuildContent();
    }

    private int PopupType { get; set; }

    protected void BuildContent()
    {
        BackgroundColor = Color.FromRgb(50, 55, 55);
        Title = "Popup Demo";
        Content = new ScrollView
        {
            VerticalOptions = LayoutOptions.FillAndExpand,
            Content = GenerateMainPageStackLayout()
        };
    }

    private StackLayout GenerateMainPageStackLayout()
    {
        var mainStackLayout = new StackLayout
        {
            Spacing = 20,
            Margin = new Thickness(10, 15)
        };
        mainStackLayout.Add(PremadePopupTypePicker());
        mainStackLayout.Add(GeneratePopupButton("Generate Popup", AttemptPopupPage()));
        return mainStackLayout;

        Picker PremadePopupTypePicker()
        {
            var PremadePopupPicker = new Picker
            {
                Items = { nameof(Mopups.PreBaked.PopupPages.SingleResponse),
                            nameof(Mopups.PreBaked.PopupPages.DualResponse),
                            nameof(Mopups.PreBaked.PopupPages.Login),
                            nameof(Mopups.PreBaked.PopupPages.Loader),
                            nameof(Mopups.PreBaked.PopupPages.EntryInput),
                            nameof(Mopups.PreBaked.PopupPages.TextInput)
                }
            };
            PremadePopupPicker.SelectedIndexChanged += OnPopupPickerSelectedIndexChanged;
            return PremadePopupPicker;

        }

    }


    void OnPopupPickerSelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = (Picker)sender;
        int selectedIndex = picker.SelectedIndex;
        PopupType = selectedIndex;
    }




    private static Button GeneratePopupButton(string buttonText, AsyncCommand buttonCommand)
    {
        return new Button
        {
            Text = buttonText,
            BackgroundColor = Color.FromArgb("#FF7DBBE6"),
            TextColor = Color.FromRgb(255, 255, 255),
            Command = buttonCommand,
        };
    }
    private bool LongRunningFunction(int MillisecondDelay, bool pointlessBoolean)
    {
        Thread.Sleep(6000);
        return pointlessBoolean;
    }

    private static List<string> LoadingReasons()
    {
        return new List<string>
            {
                "Twiddling Thumbs",
                "Rolling Eyes",
                "Checking Watch",
                "General Complaining",
                "Calling in late to work",
                "Waiting"
            };
    }

    private AsyncCommand AttemptPopupPage()
    {
        return new AsyncCommand(async () =>
        {
            try
            {
                var random = new Random();
                switch (PopupType)
                {
                    case 0: // single response
                        var exampleSingleResponse = await SingleResponseViewModel.AutoGenerateBasicPopup
                        (
                            buttonColour: Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
                            buttonTextColour: Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
                            buttonText: "Random Popup",
                            mainPopupColour: Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
                            popupInformation: "Popup Info",
                            displayImageName: "noDisplay",
                            heightRequest: random.Next(100, 400),
                            widthRequest: random.Next(100, 400));
                        Console.WriteLine(exampleSingleResponse);
                        break;
                    case 1: // dual response
                        var exampleDualResponse = await DualResponseViewModel.AutoGenerateBasicPopup(
                           Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
                           Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
                           "Left Button",
                           Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
                           Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
                           "Right Button",
                           Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
                           "Random Popup!",
                           "noDisplay",
                           heightRequest: random.Next(100, 400),
                           widthRequest: random.Next(100, 400));

                        Console.WriteLine(exampleDualResponse);
                        break;
                    case 2: // login
                        var exampleLoginResponse = await Mopups.PreBaked.PopupPages.Login.LoginViewModel.AutoGenerateBasicPopup(
                               Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
                               Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
                               "Left Button",
                               Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
                               Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
                               "Right Button",
                               Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
                               "Random Username",
                               "Random Username Placeholder",
                               "Random Password",
                               "Random Password Placeholder",
                               "noDisplay",
                            heightRequest: random.Next(100, 400),
                               widthRequest: random.Next(100, 400));
                        Console.WriteLine(String.Format("username: {0} Password {1}", exampleLoginResponse.username, exampleLoginResponse.password));
                        break;
                    case 3: //loader

                        var exampleLoaderResponse = await PreBakedMopupService.GetInstance().WrapReturnableFuncInLoader<int, bool, bool>(LongRunningFunction, 5000, true,
                                                                                                                Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
                                                                                                                Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
                                                                                                                LoadingReasons(),
                                                                                                                Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
                                                                                                                2000);
                        Console.WriteLine(exampleLoaderResponse);

                        break;
                    case 4: // entryinput
                        var exampleEntryInput = await Mopups.PreBaked.PopupPages.EntryInput.EntryInputViewModel.AutoGenerateBasicPopup(
                               Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
                               Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
                               "Left Button",
                               Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
                               Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
                               "Right Button",
                               Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
                               "Random Default Text",
                               "Random Default Placeholder",
                               heightRequest: random.Next(100, 400),
                               widthRequest: random.Next(100, 400));
                        Console.WriteLine(exampleEntryInput);

                        break;
                    case 5: // text input
                        var exampleTextInput = await Mopups.PreBaked.PopupPages.TextInput.TextInputViewModel.AutoGenerateBasicPopup(
                                                           Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
                               Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
                               "Left Button",
                               Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
                               Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
                               "Right Button",
                               Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
                               "Random Default Text",
                               "Random Default Placeholder",
                               heightRequest: random.Next(100, 400),
                               widthRequest: random.Next(100, 400));
                        Console.WriteLine(exampleTextInput);
                        break;
                }
            }
            catch (Exception)
            {
                throw;
            }
        });
    }
}
