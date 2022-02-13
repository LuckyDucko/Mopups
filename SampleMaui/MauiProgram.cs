
using Microsoft.Maui.LifecycleEvents;

using Mopups.Interfaces;
using Mopups.Services;

using Mopups;
using Mopups.Pages;
using Mopups.Droid.Implementation;

namespace SampleMaui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                })
                .ConfigureLifecycleEvents(lifecycle =>
                {
                    
#if ANDROID
                    lifecycle.AddAndroid(d => d.OnBackPressed(activity => AndroidMopups.SendBackPressed(activity.OnBackPressed)));
                
#endif
                })
                .ConfigureMauiHandlers(handlers =>
                {

                    handlers.AddHandler(typeof(PopupPage), typeof(PopupPageHandler));
                });

            //Work out how to register this as a singleton
            return builder.Build();
        }
    }
}
