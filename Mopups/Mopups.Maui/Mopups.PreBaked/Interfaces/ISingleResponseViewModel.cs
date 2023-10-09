using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

using Mopups.PreBaked.Interfaces;
using Mopups.PreBaked.PopupPages.SingleResponse;

using Mopups.Pages;

namespace Mopups.PreBaked.Interfaces
{
	public interface ISingleResponseViewModel : ISingleResponse
	{
		Task<bool> GeneratePopup(Dictionary<string, object> propertyDictionary);
		Task<bool> GeneratePopup<TPopupPage>(Dictionary<string, object> optionalProperties) where TPopupPage : PopupPage, IGenericViewModel<SingleResponseViewModel>, new();
		Task<bool> GeneratePopup();
		Task<bool> GeneratePopup<TPopupPage>() where TPopupPage : PopupPage, IGenericViewModel<SingleResponseViewModel>, new();
		Task<bool> GeneratePopup(ISingleResponse propertyInterface);
		Task<bool> GeneratePopup<TPopupPage>(ISingleResponse propertyInterface) where TPopupPage : PopupPage, IGenericViewModel<SingleResponseViewModel>, new();
		Dictionary<string, (object property, Type propertyType)> PullViewModelProperties();
	}

	public interface ISingleResponse
	{
		ICommand SingleButtonCommand { get; set; }
		string SingleButtonText { get; set; }
		Color SingleButtonColour { get; set; }
		Color SingleButtonTextColour { get; set; }
		string SingleDisplayImage { get; set; }
	}
}