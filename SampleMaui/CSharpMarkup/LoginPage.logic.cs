using Mopups.Pages;

namespace SampleMaui.CSharpMarkup;

public partial class LoginPage : PopupPage
{
    public LoginPage()
    {
        BuildContent();
    }

    protected override bool OnBackgroundClicked()
    {
        return base.OnBackgroundClicked();
    }
}
