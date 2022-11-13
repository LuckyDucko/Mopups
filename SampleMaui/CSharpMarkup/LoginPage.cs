using Mopups.Animations;
using Mopups.Pages;
using Mopups.Services;
using System.Diagnostics.CodeAnalysis;

namespace SampleMaui.CSharpMarkup;

public partial class LoginPage : PopupPage
{
    public Frame FrameContainer { get; set; }
    public Image? DotNetBotImage { get; set; }

    public Entry UsernameEntry { get; set; }
    public Entry PasswordEntry { get; set; }

    public Button LoginButton { get; set; }

    [MemberNotNull(nameof(UsernameEntry))]
    [MemberNotNull(nameof(PasswordEntry))]
    [MemberNotNull(nameof(LoginButton))]
    [MemberNotNull(nameof(FrameContainer))]
    protected void BuildContent()
    {
        try
        {
            this.Content = GenerateLoginView();
            this.Background = Colors.WhiteSmoke;
            this.HasSystemPadding = false;
            this.BackgroundInputTransparent = true;
            this.CloseWhenBackgroundIsClicked = true;
            this.IsAnimationEnabled = true;
        }
        catch (Exception)
        {
            throw;
        }
    }

    [MemberNotNull(nameof(UsernameEntry))]
    [MemberNotNull(nameof(PasswordEntry))]
    [MemberNotNull(nameof(LoginButton))]
    [MemberNotNull(nameof(FrameContainer))]
    private Frame GenerateLoginView()
    {
        FrameContainer = new Frame
        {
            Margin = new Thickness(1),
            Padding = new Thickness(0),
            BackgroundColor = Colors.Gray,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Content = GenerateFrameContainerContent()
        };

        return FrameContainer;
    }

    [MemberNotNull(nameof(UsernameEntry))]
    [MemberNotNull(nameof(PasswordEntry))]
    [MemberNotNull(nameof(LoginButton))]
    private VerticalStackLayout GenerateFrameContainerContent()
    {
        var frameContainerContent = new VerticalStackLayout
        {
            Margin = new Thickness(1),
            Padding = new Thickness(1, 1),
        };

        UsernameEntry = new Entry
        {
            HorizontalOptions = LayoutOptions.Center,
            Placeholder = "Username",
            PlaceholderColor = Color.FromArgb("#FF9CDAF1"),
            TextColor = Color.FromArgb("#FF7DBBE6")
        };

        PasswordEntry = new Entry
        {
            HorizontalOptions = LayoutOptions.Center,
            IsPassword = true,
            Placeholder = "Password",
            PlaceholderColor = Color.FromArgb("#FF9CDAF1"),
            TextColor = Color.FromArgb("#FF7DBBE6")
        };

        LoginButton = new Button
        {
            Command = new Command(() => MopupService.Instance.PopAsync())
        };
        
        frameContainerContent.Add(DotNetBotImage);
        frameContainerContent.Add(UsernameEntry);
        frameContainerContent.Add(PasswordEntry);
        frameContainerContent.Add(LoginButton);

        return frameContainerContent;
    }

    protected override bool OnBackButtonPressed()
    {
        return Task.Run(() =>
        {
            MopupService.Instance.PopAllAsync();
            return base.OnBackButtonPressed();
        }).Result;
    }
}
