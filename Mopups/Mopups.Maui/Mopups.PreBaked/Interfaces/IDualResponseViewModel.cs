using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

using Mopups.PreBaked.PopupPages.DualResponse;

using Mopups.Pages;

namespace Mopups.PreBaked.Interfaces
{
	public interface IDualResponseViewModel : IDualResponse
	{
		Task<bool> GeneratePopup(Dictionary<string, object> propertyDictionary);
		Task<bool> GeneratePopup<TPopupPage>(Dictionary<string, object> optionalProperties) where TPopupPage : PopupPage, IGenericViewModel<DualResponseViewModel>, new();
		Task<bool> GeneratePopup();
		Task<bool> GeneratePopup<TPopupPage>() where TPopupPage : PopupPage, IGenericViewModel<DualResponseViewModel>, new();
		Task<bool> GeneratePopup(IDualResponse propertyInterface);
		Task<bool> GeneratePopup<TPopupPage>(IDualResponse propertyInterface) where TPopupPage : PopupPage, IGenericViewModel<DualResponseViewModel>, new();
		Dictionary<string, (object property, Type propertyType)> PullViewModelProperties();
	}

	public interface IDualResponse
	{
		ICommand LeftButtonCommand { get; set; }
		string LeftButtonText { get; set; }
		Color LeftButtonColour { get; set; }
		Color LeftButtonTextColour { get; set; }
		ICommand RightButtonCommand { get; set; }
		string RightButtonText { get; set; }
		string PictureSource { get; set; }
		Color RightButtonColour { get; set; }
		Color RightButtonTextColour { get; set; }
	}
}