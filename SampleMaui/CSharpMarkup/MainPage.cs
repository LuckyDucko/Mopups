using System.Reflection;
using AsyncAwaitBestPractices.MVVM;
using Mopups.Animations.Base;
using Mopups.Pages;
using Mopups.Services;

using SampleMopups.XAML;

using Button = Microsoft.Maui.Controls.Button;
using ScrollView = Microsoft.Maui.Controls.ScrollView;

namespace SampleMaui.CSharpMarkup;

public partial class MainPage : ContentPage
{
    public Picker AnimationPicker { get; set; }

    public Slider AnimationLengthInSlider { get; set; }

    public Picker EasingInPicker { get; set; }

    public Slider AnimationLengthOutSlider { get; set; }

    public Picker EasingOutPicker { get; set; }

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
        mainStackLayout.Add(AnimationPicker = PopupAnimationPicker());
        mainStackLayout.Add(new Label() { Text = "In" });
        mainStackLayout.Add(EasingInPicker = EasingAnimationPicker());
        mainStackLayout.Add(AnimationLengthInSlider = PopupAnimationLengthSlider());
        mainStackLayout.Add(new Label() { Text = "Out" });
        mainStackLayout.Add(EasingOutPicker = EasingAnimationPicker());
        mainStackLayout.Add(AnimationLengthOutSlider = PopupAnimationLengthSlider());
        mainStackLayout.Add(GeneratePopupButton("Open Login Popup", GenerateSimpleCommandForPopup<LoginPage>()));
        mainStackLayout.Add(GeneratePopupButton("Open Aswins Popup", GenerateSimpleCommandForPopup<AswinPage>()));
        var newButton = new Button
                        {
                            Text = "Switch To Prebaked Examples",
                            BackgroundColor = Color.FromArgb("#FF7DBBE6"),
                            TextColor = Color.FromRgb(255, 255, 255),
                            Command = new AsyncCommand(async () =>
                                                       {
                                                           await Navigation.PushAsync(new PreBakedExample());
                                                       })
                        };
        mainStackLayout.Add(newButton);

        return mainStackLayout;

        Picker PopupAnimationPicker()
        {
            return new Picker
                   {
                       ItemsSource = typeof(BaseAnimation)
                                     .Assembly.GetTypes()
                                     .Where( t => t.IsSubclassOf( typeof(BaseAnimation) ) && !t.IsAbstract )
                                     .Select( t => (BaseAnimation)Activator.CreateInstance( t ) )
                                     .ToList(),
                       ItemDisplayBinding = new Binding( ".", BindingMode.OneWay, ObjectTypeNameConverter.Instance ),
                       SelectedIndex = 0
                   };
        }

        Picker EasingAnimationPicker()
        {
            return new Picker
                   {
                       ItemsSource = typeof(Easing).GetFields( BindingFlags.Public | BindingFlags.Static ),
                       ItemDisplayBinding = new Binding( "Name", BindingMode.OneWay ),
                       SelectedIndex = 0
                   };
        }

        Slider PopupAnimationLengthSlider()
        {
            return new Slider
                   {
                       Minimum = 0,
                       Maximum = 2000,
                       FlowDirection = FlowDirection.LeftToRight,
                   };
        }

    }

    private static Button GeneratePopupButton( string buttonText, AsyncCommand buttonCommand )
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
        return new AsyncCommand(
            async () =>
            {
                try
                {
                    var animation = (BaseAnimation)this.AnimationPicker.SelectedItem;

                    animation.DurationIn = Convert.ToUInt32(AnimationLengthInSlider.Value);
                    animation.DurationOut = Convert.ToUInt32(AnimationLengthOutSlider.Value);
                    animation.EasingIn = (Easing)((FieldInfo)EasingInPicker.SelectedItem).GetValue(null);
                    animation.EasingOut = (Easing)((FieldInfo)EasingOutPicker.SelectedItem).GetValue(null);

                    await MopupService.Instance.PushAsync(new TPopupPage { Animation = animation });
                }
                catch (Exception)
                {
                    throw;
                }
            });
    }




}
