//using Demo.Pages;
using SampleMaui.CSharpMarkup;

[assembly: XamlCompilation(XamlCompilationOptions.Skip)]
namespace SampleMaui;

public partial class App : Application
{
    public App()
    {
        MainPage = new NavigationPage(new MainPage());
    }
}
