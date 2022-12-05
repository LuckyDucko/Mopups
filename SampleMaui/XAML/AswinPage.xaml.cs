using Mopups.Pages;

namespace SampleMopups.XAML;

public partial class AswinPage : PopupPage
{
	public AswinPage()
	{
		InitializeComponent();
        BackgroundColor = Color.FromArgb("#80000000");
    }

    private void PopupPage_BackgroundClicked(object sender, EventArgs e)
    {
        Console.WriteLine(1);
    }

    private async void blahButton_Clicked(object sender, EventArgs e)
    {
        await blahButton.RelRotateTo(180,1000);
    }
}
