using Mopups.Pages;

namespace SampleMopups.XAML;

public partial class AswinPage : PopupPage
{
	public AswinPage()
	{
		InitializeComponent();
	}

    private void PopupPage_BackgroundClicked(object sender, EventArgs e)
    {
    }

    private async void blahButton_Clicked(object sender, EventArgs e)
    {
        await blahButton.RelRotateTo(180,1000);
    }
}
