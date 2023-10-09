using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

using Mopups.PreBaked.Interfaces;
using Mopups.PreBaked.PopupPages.Login;

using Mopups.Pages;

namespace Mopups.PreBaked.Interfaces
{
	public interface ILoginViewModel : ILogin
	{
		Task<(string username, string password)> GeneratePopup(Dictionary<string, object> propertyDictionary);
		Task<(string username, string password)> GeneratePopup<TPopupPage>(Dictionary<string, object> optionalProperties) where TPopupPage : PopupPage, IGenericViewModel<LoginViewModel>, new();
		Task<(string username, string password)> GeneratePopup();
		Task<(string username, string password)> GeneratePopup<TPopupPage>() where TPopupPage : PopupPage, IGenericViewModel<LoginViewModel>, new();
		Task<(string username, string password)> GeneratePopup(ILogin propertyInterface);
		Task<(string username, string password)> GeneratePopup<TPopupPage>(ILogin propertyInterface) where TPopupPage : PopupPage, IGenericViewModel<LoginViewModel>, new();
		Dictionary<string, (object property, Type propertyType)> PullViewModelProperties();
	}

	public interface ILogin
	{
		string Username { get; set; }
		string UsernamePlaceholder { get; set; }
		Color UsernamePlaceholderColour { get; set; }
		Color UsernameTextColour { get; set; }
		Color UsernameBackgroundColour { get; set; }
		string Password { get; set; }
		string PasswordPlaceholder { get; set; }
		Color PasswordPlaceholderColour { get; set; }
		Color PasswordTextColour { get; set; }
		Color PasswordBackgroundColour { get; set; }
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