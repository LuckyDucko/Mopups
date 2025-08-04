using System.ComponentModel;

namespace Mopups.PreBaked.Interfaces
{
    public interface IBasePopupViewModel
	{
		bool IsBusy { get; set; }
		event PropertyChangedEventHandler PropertyChanged;

        void RunOnAttachment<TPopupType>(TPopupType popupPage) where TPopupType : Pages.PopupPage;
    }
}