using System.Reflection;
using System.Windows.Input;

using Mopups.PreBaked.AbstractClasses;
using Mopups.PreBaked.Interfaces;

using Mopups.Pages;


namespace Mopups.PreBaked.PopupPages.EntryInput
{
	public class EntryInputViewModel : PopupViewModel<string>, IEntryInputViewModel
	{
		private string _textInput;
		public string TextInput
		{
			get => _textInput;
			set => SetValue(ref _textInput, value);
		}

		private string _placeHolderInput;
		public string PlaceHolderInput
		{
			get => _placeHolderInput;
			set => SetValue(ref _placeHolderInput, value);
		}

		private ICommand _leftButtonCommand;
		public ICommand LeftButtonCommand
		{
			get => _leftButtonCommand;
			set => SetValue(ref _leftButtonCommand, value);
		}

		private string _leftButtonText;
		public string LeftButtonText
		{
			get => _leftButtonText;
			set => SetValue(ref _leftButtonText, value);
		}

		private Color _leftButtonColour;
		public Color LeftButtonColour
		{
			get => _leftButtonColour;
			set => SetValue(ref _leftButtonColour, value);
		}

		private Color _leftButtonTextColour;
		public Color LeftButtonTextColour
		{
			get => _leftButtonTextColour;
			set => SetValue(ref _leftButtonTextColour, value);
		}

		private ICommand _rightButtonCommand;
		public ICommand RightButtonCommand
		{
			get => _rightButtonCommand;
			set => SetValue(ref _rightButtonCommand, value);
		}

		private string _rightButtonText;
		public string RightButtonText
		{
			get => _rightButtonText;
			set => SetValue(ref _rightButtonText, value);
		}

		private Color _rightButtonColour;
		public Color RightButtonColour
		{
			get => _rightButtonColour;
			set => SetValue(ref _rightButtonColour, value);
		}

		private Color _rightButtonTextColour;
		public Color RightButtonTextColour
		{
			get => _rightButtonTextColour;
			set => SetValue(ref _rightButtonTextColour, value);
		}

		public EntryInputViewModel(IPreBakedMopupService popupService) : base(popupService)
		{
		}

		public static EntryInputViewModel GenerateVM()
		{
			return new EntryInputViewModel(Services.PreBakedMopupService.GetInstance());
		}


		/// <summary>
		/// provides the EntryInputPopupPage Generic Type argument to
		/// <see cref="GeneratePopup{TPopupPage}(Dictionary{string, object})"/>
		/// </summary>
		/// <param name="propertyDictionary">Page Properties, for an example pull <seealso cref="PullViewModelProperties"/></param>
		/// <returns> Task that waits for user input</returns>
		public async Task<string> GeneratePopup(Dictionary<string, object> propertyDictionary)
		{
			return await GeneratePopup<EntryInputPopupPage>(propertyDictionary);
		}

		/// <summary>
		/// Attaches properties through the use of reflection. <see cref="PopupViewModel{TReturnable}.InitialiseOptionalProperties(Dictionary{string, object})"/>
		/// </summary>
		/// <typeparam name="TPopupPage">User defined page that uses the EntryInputViewModel ViewModel</typeparam>
		/// <param name="propertyDictionary">Page Properties, for an example pull <seealso cref="PullViewModelProperties"/></param>
		/// <returns> Task that waits for user input</returns>
		public async Task<string> GeneratePopup<TPopupPage>(Dictionary<string, object> optionalProperties) where TPopupPage : Mopups.Pages.PopupPage, IGenericViewModel<EntryInputViewModel>, new()
		{
			InitialiseOptionalProperties(optionalProperties);
			return await Services.PreBakedMopupService.GetInstance().PushAsync<EntryInputViewModel, TPopupPage, string>(this);
		}

		/// <returns> Task that waits for user input</returns>
		public async Task<string> GeneratePopup()
		{
			return await GeneratePopup<EntryInputPopupPage>();
		}

		/// <returns> Task that waits for user input</returns>
		public async Task<string> GeneratePopup<TPopupPage>() where TPopupPage : Mopups.Pages.PopupPage, IGenericViewModel<EntryInputViewModel>, new()
		{
			return await Services.PreBakedMopupService.GetInstance().PushAsync<EntryInputViewModel, TPopupPage, string>(this);
		}

		public async Task<string> GeneratePopup(IEntryInput propertyInterface)
		{
			return await GeneratePopup<EntryInputPopupPage>(propertyInterface);
		}

		public async Task<string> GeneratePopup<TPopupPage>(IEntryInput propertyInterface) where TPopupPage : PopupPage, IGenericViewModel<EntryInputViewModel>, new()
		{
			PropertyInfo[] properties = typeof(IEntryInput).GetProperties();
			for (int propertyIndex = 0; propertyIndex < properties.Count(); propertyIndex++)
			{
				GetType().GetProperty(properties[propertyIndex].Name).SetValue(this, properties[propertyIndex].GetValue(propertyInterface, null), null);
			}
			return await Services.PreBakedMopupService.GetInstance().PushAsync<EntryInputViewModel, TPopupPage, string>(this);
		}


		/// <summary>
		/// Provides a base dictionary, along with types that you can use to Initialise properties
		/// </summary>
		/// <returns>All Properties contained within the Viewmodel, with their names, current values, and types</returns>
		public virtual Dictionary<string, (object property, Type propertyType)> PullViewModelProperties()
		{
			return base.PullViewModelProperties<EntryInputViewModel>();
		}



		/// <summary>
		/// provides the EntryInputPopupPage Generic Type argument to
		/// <see cref="AutoGenerateBasicPopup{TPopupPage}(Color, Color, string, Color, Color, string, Color, string, string, int, int)"/>
		/// </summary>
		public static async Task<string> AutoGenerateBasicPopup(Color leftButtonColour, Color leftButtonTextColour, string leftButtonText, Color rightButtonColour, Color rightButtonTextColour, string rightButtonText, Color mainPopupColour, string defaultTextInput, string defaultPlaceHolder, int heightRequest = 0, int widthRequest = 0)
		{
			return await AutoGenerateBasicPopup<EntryInputPopupPage>(leftButtonColour, leftButtonTextColour, leftButtonText, rightButtonColour, rightButtonTextColour, rightButtonText, mainPopupColour, defaultTextInput, defaultPlaceHolder, heightRequest, widthRequest);
		}

		public static async Task<string> AutoGenerateBasicPopup<TPopupPage>(Color leftButtonColour, Color leftButtonTextColour, string leftButtonText, Color rightButtonColour, Color rightButtonTextColour, string rightButtonText, Color mainPopupColour, string defaultTextInput, string defaultPlaceHolder, int heightRequest = 0, int widthRequest = 0) where TPopupPage : Mopups.Pages.PopupPage, IGenericViewModel<EntryInputViewModel>, new()
		{
			var AutoGeneratePopupViewModel = new EntryInputViewModel(Services.PreBakedMopupService.GetInstance());
			ICommand leftButtonCommand = new Command(() => AutoGeneratePopupViewModel.SafeCloseModal<TPopupPage>("No Text Available"));
			ICommand rightButtonCommand = new Command(() => AutoGeneratePopupViewModel.SafeCloseModal<TPopupPage>(AutoGeneratePopupViewModel.TextInput));

			var propertyDictionary = new Dictionary<string, object>
			{
				{ "LeftButtonCommand", leftButtonCommand },
				{ "LeftButtonColour", leftButtonColour },
				{ "LeftButtonText", leftButtonText ?? "Yes" },
				{ "LeftButtonTextColour", leftButtonTextColour },

				{ "RightButtonCommand", rightButtonCommand },
				{ "RightButtonColour", rightButtonColour },
				{ "RightButtonText", rightButtonText ?? "No" },
				{ "RightButtonTextColour", rightButtonTextColour },

				{ "HeightRequest", heightRequest },
				{ "WidthRequest", widthRequest },
				{ "PlaceHolderInput", defaultPlaceHolder},
				{ "MainPopupColour", mainPopupColour },
				{ "TextInput", defaultTextInput }
			};
			return await AutoGeneratePopupViewModel.GeneratePopup<TPopupPage>(propertyDictionary);
		}
	}
}

