using AsyncAwaitBestPractices.MVVM;

using Mopups.Animations;

using Mopups.Pages;
using Mopups.Services;

using SampleMopups.XAML;

using Button = Microsoft.Maui.Controls.Button;
using ScrollView = Microsoft.Maui.Controls.ScrollView;

namespace SampleMaui.CSharpMarkup;

public partial class MainPage : ContentPage
{
    public double AnimationLength { get; set; }
    public int AnimationType { get; set; }
    public int AnimationEasing { get; set; }

    protected void BuildContent()
    {
        BackgroundColor = Color.FromRgb(255, 255, 255);
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
        mainStackLayout.Add(PopupAnimationPicker());
        mainStackLayout.Add(EasingAnimationPicker());
        mainStackLayout.Add(PopupAnimationLengthSlider());
        mainStackLayout.Add(GeneratePopupButton("Open Login Popup", GenerateSimpleCommandForPopup<LoginPage>()));
        mainStackLayout.Add(GeneratePopupButton("Open Aswins Popup", GenerateSimpleCommandForPopup<AswinPage>()));
        var newButton = new Button
        {
            Text = "New page test",
            BackgroundColor = Color.FromArgb("#FF7DBBE6"),
            TextColor = Color.FromRgb(255, 255, 255),
            Command = new AsyncCommand(async () =>
            {
                await Navigation.PushAsync(new TestPage());
            })
        };
        mainStackLayout.Add(newButton);

        return mainStackLayout;

        Picker PopupAnimationPicker()
        {
            var animationPicker = new Picker
            {
                Items = { "FadeAnimation", "MoveAnimation", "ScaleAnimation" }
            };

            animationPicker.SelectedIndexChanged += OnAnimationPickerSelectedIndexChanged;

            return animationPicker;
        }

        Picker EasingAnimationPicker()
        {
            var animationPicker = new Picker
            {
                Items = { "Linear", "BounceIn", "BounceOut", "CubicIn", "CubicOut", "SinIn", "SinInOut", "SinOut", "SpringIn", "SpringOut" }
            };

            animationPicker.SelectedIndexChanged += OnEasingPickerSelectedIndexChanged;

            return animationPicker;

        }
        Slider PopupAnimationLengthSlider()
        {
            var animationSlider = new Slider
            {
                Minimum = 0,
                Maximum = 2000,
                FlowDirection = FlowDirection.LeftToRight,

            };

            animationSlider.ValueChanged += OnSliderValueChanged;

            return animationSlider;
        }

    }

    void OnEasingPickerSelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = (Picker)sender;
        int selectedIndex = picker.SelectedIndex;
        AnimationType = selectedIndex;
    }

    void OnAnimationPickerSelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = (Picker)sender;
        int selectedIndex = picker.SelectedIndex;
        AnimationType = selectedIndex;
    }

    void OnSliderValueChanged(object sender, ValueChangedEventArgs args)
    {
        AnimationLength = args.NewValue;
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

    private AsyncCommand GenerateSimpleCommandForPopup<TPopupPage>() where TPopupPage : PopupPage, new()
    {
        return new AsyncCommand(async () =>
        {
            try
            {
                var page = new TPopupPage();
                var easing = Easing.Linear;

                switch (AnimationEasing)
                {
                    case 0:
                        easing = Easing.Linear;
                        break;
                    case 1:
                        easing = Easing.BounceIn;
                        break;
                    case 2:
                        easing = Easing.BounceOut;
                        break;
                    case 3:
                        easing = Easing.CubicIn;
                        break;
                    case 4:
                        easing = Easing.CubicOut;
                        break;
                    case 5:
                        easing = Easing.SinIn;
                        break;
                    case 6:
                        easing = Easing.SinInOut;
                        break;
                    case 7:
                        easing = Easing.SinOut;
                        break;
                    case 8:
                        easing = Easing.SpringIn;
                        break;
                    case 9:
                        easing = Easing.SpringOut;
                        break;
                }

                switch (AnimationType)
                {
                    case 1:
                        page.Animation = new FadeAnimation() { DurationIn = Convert.ToUInt32(AnimationLength), DurationOut = Convert.ToUInt32(AnimationLength), EasingIn = easing, EasingOut = easing };
                        break;
                    case 2:
                        page.Animation = new MoveAnimation() { DurationIn = Convert.ToUInt32(AnimationLength), DurationOut = Convert.ToUInt32(AnimationLength), EasingIn = easing, EasingOut = easing };
                        break;
                    case 3:
                        page.Animation = new ScaleAnimation() { DurationIn = Convert.ToUInt32(AnimationLength), DurationOut = Convert.ToUInt32(AnimationLength), EasingIn = easing, EasingOut = easing };
                        break;
                    default:
                        break;
                }

                await MopupService.Instance.PushAsync(page);
            }
            catch (Exception)
            {
                throw;
            }
        });
    }
}
