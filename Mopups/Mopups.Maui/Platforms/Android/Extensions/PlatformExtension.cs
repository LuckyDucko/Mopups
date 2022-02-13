

using Mopups.Pages;

namespace Mopups.Droid.Extension
{
    internal static class PlatformExtension
    {
        public static IViewHandler GetOrCreateHandler(this VisualElement bindable)
        {
            try
            {
                return bindable.Handler ??= new PopupPageHandler();
            }
            catch (System.Exception ex)
            {
                throw;
            }
        }
    }
}
