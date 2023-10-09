using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

using Mopups.PreBaked.Interfaces;
using Mopups.PreBaked.PopupPages.EntryInput;

using Mopups.Pages;

namespace Mopups.PreBaked.Interfaces
{
	public interface IEntryInputViewModel : IEntryInput
	{
		Task<string> GeneratePopup(Dictionary<string, object> propertyDictionary);
		Task<string> GeneratePopup<TPopupPage>(Dictionary<string, object> optionalProperties) where TPopupPage : PopupPage, IGenericViewModel<EntryInputViewModel>, new();
		Task<string> GeneratePopup();
		Task<string> GeneratePopup<TPopupPage>() where TPopupPage : PopupPage, IGenericViewModel<EntryInputViewModel>, new();
		Task<string> GeneratePopup(IEntryInput propertyInterface);
		Task<string> GeneratePopup<TPopupPage>(IEntryInput propertyInterface) where TPopupPage : PopupPage, IGenericViewModel<EntryInputViewModel>, new();
		Dictionary<string, (object property, Type propertyType)> PullViewModelProperties();
	}

	public interface IEntryInput
	{
		string TextInput { get; set; }
		string PlaceHolderInput { get; set; }
		ICommand LeftButtonCommand { get; set; }
		string LeftButtonText { get; set; }
		Color LeftButtonColour { get; set; }
		Color LeftButtonTextColour { get; set; }
		ICommand RightButtonCommand { get; set; }
		string RightButtonText { get; set; }
		Color RightButtonColour { get; set; }
		Color RightButtonTextColour { get; set; }
	}
}