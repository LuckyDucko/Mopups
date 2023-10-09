using System.Collections.Generic;


namespace Mopups.PreBaked.Interfaces
{
    public interface ILoaderViewModel
    {
        Color LoaderColour { get; set; }
        Color TextColour { get; set; }
        List<string> ReasonsForLoader { get; set; }
        int MillisecondsBetweenReasonSwitch { get; set; }
        void SafeCloseModal<TPopupType>() where TPopupType : Mopups.Pages.PopupPage, new();
    }
}