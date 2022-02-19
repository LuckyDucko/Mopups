
//using Demo.Pages;

using SampleMaui.CSharpMarkup;

using Application = Microsoft.Maui.Controls.Application;

[assembly: XamlCompilation(XamlCompilationOptions.Skip)]
namespace SampleMaui
{
    public partial class App : Application
    {
        public App()
        {
            MainPage = new MainPage();
        }
    }
}
