using AsyncAwaitBestPractices.MVVM;
using Mopups.Pages;
using Mopups.Services;

using RGPopupSample;

using Button = Microsoft.Maui.Controls.Button;
using ScrollView = Microsoft.Maui.Controls.ScrollView;

namespace SampleMaui.CSharpMarkup;

public partial class MainPage : ContentPage
{
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
        mainStackLayout.Add(GeneratePopupButton("Open Popup", GenerateSimpleCommandForPopup<PopupTest>()));
        return mainStackLayout;
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

    private static AsyncCommand GenerateSimpleCommandForPopup<TPopupPage>() where TPopupPage : PopupPage, new()
    {
        return new AsyncCommand(async () =>
        {
            try
            {
                var page = new TPopupPage();
                await MopupService.Instance.PushAsync(page);
            }
            catch (Exception)
            {
                throw;
            }
        });
    }
}
