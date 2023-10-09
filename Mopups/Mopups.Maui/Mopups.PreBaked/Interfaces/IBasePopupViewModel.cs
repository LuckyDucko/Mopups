using System.ComponentModel;

namespace Mopups.PreBaked.Interfaces
{
	public interface IBasePopupViewModel
	{
		bool IsBusy { get; set; }
		event PropertyChangedEventHandler PropertyChanged;
	}
}