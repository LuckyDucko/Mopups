using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mopups.PreBaked.Interfaces
{
	public interface IPopupViewModel<TReturnable>
	{
		TaskCompletionSource<TReturnable> Returnable { get; set; }
		string MainPopupInformation { get; set; }
		Color MainPopupColour { get; set; }
		int WidthRequest { get; set; }
		int HeightRequest { get; set; }

		void InitialiseOptionalProperties(Dictionary<string, object> optionalProperties);
		void SafeCloseModal<TPopupPage>() where TPopupPage : Mopups.Pages.PopupPage, new();
		Task SafeCloseModal<TPopupPage>(Task<TReturnable> buttonCommand) where TPopupPage : Mopups.Pages.PopupPage, new();
		void SafeCloseModal<TPopupPage>(TReturnable result) where TPopupPage : Mopups.Pages.PopupPage, new();
	}
}