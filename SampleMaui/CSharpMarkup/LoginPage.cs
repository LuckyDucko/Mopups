using Mopups.Pages;
using Mopups.Services;
using ScrollView = Microsoft.Maui.Controls.ScrollView;

namespace SampleMaui.CSharpMarkup;

public partial class LoginPage : PopupPage
{
    public Frame FrameContainer { get; set; }
    public Image DotNetBotImage { get; set; }

    public Entry UsernameEntry { get; set; }
    public Entry PasswordEntry { get; set; }

    public Button LoginButton { get; set; }
    protected void BuildContent()
    {
        try
        {
            this.Content = new ScrollView
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                BackgroundColor = Color.FromRgb(200.00, 0.00, 0.00),
                Content = GenerateLoginView()

            };
        }
        catch (Exception)
        {
            throw;
        }
    }

    private Frame GenerateLoginView()
    {
        FrameContainer = new Frame
        {
            Margin = new Microsoft.Maui.Thickness(1),
            Padding = new Microsoft.Maui.Thickness(0),
            BackgroundColor = Microsoft.Maui.Graphics.Colors.Gray,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Content = GenerateFrameContainerContent()
        };
        return FrameContainer;
    }

    private StackLayout GenerateFrameContainerContent()
    {
        var frameContainerContent = new StackLayout
        {
            Margin = new Microsoft.Maui.Thickness(1),
            Padding = new Microsoft.Maui.Thickness(1, 1),
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center

        };
        /*
        DotNetBotImage = new Image
        {

            Margin = new Microsoft.Maui.Thickness(1),
            BackgroundColor = Microsoft.Maui.Graphics.Colors.White,
            Scale = 10,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Source = ImageSource.FromFile("fluent_balloon.svg")
        };
        */
        UsernameEntry = new Entry
        {
            HorizontalOptions = LayoutOptions.Center,
            Placeholder = "Username",
            PlaceholderColor = Color.FromHex("#FF9CDAF1"),
            TextColor = Color.FromHex("#FF7DBBE6")
        };

        PasswordEntry = new Entry
        {
            HorizontalOptions = LayoutOptions.Center,
            IsPassword = true,
            Placeholder = "Password",
            PlaceholderColor = Color.FromHex("#FF9CDAF1"),
            TextColor = Color.FromHex("#FF7DBBE6")
        };

        LoginButton = new Button
        {
            Command = new Command(() => MopupService.Instance.PopAllAsync())
        };

        //frameContainerContent.Add(DotNetBotImage);
        frameContainerContent.Add(UsernameEntry);
        frameContainerContent.Add(PasswordEntry);
        frameContainerContent.Add(LoginButton);

        return frameContainerContent;
    }
}
