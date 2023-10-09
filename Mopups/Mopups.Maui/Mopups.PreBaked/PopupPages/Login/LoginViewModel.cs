using Mopups.PreBaked.AbstractClasses;
using System.Windows.Input;
using Mopups.PreBaked.Interfaces;
using Mopups.Pages;
using System.Reflection;

namespace Mopups.PreBaked.PopupPages.Login
{
    public class LoginViewModel : PopupViewModel<(string username, string password)>, ILoginViewModel
	{
		private string _username;
		public string Username
		{
			get => _username;
			set => SetValue(ref _username, value);
		}

		private string _usernamePlaceholder;
		public string UsernamePlaceholder
		{
			get => _usernamePlaceholder;
			set => SetValue(ref _usernamePlaceholder, value);
		}

		private Color _usernamePlaceholderColour; //Ensure this is _name
		public Color UsernamePlaceholderColour //Ensure this is Name
		{
			get => _usernamePlaceholderColour;
			set => SetValue(ref _usernamePlaceholderColour, value);
		}

		private Color _usernameTextColour; //Ensure this is _name
		public Color UsernameTextColour //Ensure this is Name
		{
			get => _usernameTextColour;
			set => SetValue(ref _usernameTextColour, value);
		}

		private Color _usernameBackgroundColour; //Ensure this is _name
		public Color UsernameBackgroundColour //Ensure this is Name
		{
			get => _usernameBackgroundColour;
			set => SetValue(ref _usernameBackgroundColour, value);
		}

		private string _password;
		public string Password
		{
			get => _password;
			set => SetValue(ref _password, value);
		}

		private string _passwordPlaceholder;
		public string PasswordPlaceholder
		{
			get => _passwordPlaceholder;
			set => SetValue(ref _passwordPlaceholder, value);
		}

		private Color _passwordPlaceholderColour; 
		public Color PasswordPlaceholderColour 
		{
			get => _passwordPlaceholderColour;
			set => SetValue(ref _passwordPlaceholderColour, value);
		}

		private Color _passwordTextColour; 
		public Color PasswordTextColour
		{
			get => _passwordTextColour;
			set => SetValue(ref _passwordTextColour, value);
		}

		private Color _passwordBackgroundColour; 
		public Color PasswordBackgroundColour 
		{
			get => _passwordBackgroundColour;
			set => SetValue(ref _passwordBackgroundColour, value);
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

		private string _pictureSource;
		public string PictureSource
		{
			get => _pictureSource;
			set => SetValue(ref _pictureSource, value);
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

		public LoginViewModel(IPreBakedMopupService popupService) : base(popupService)
		{
		}


		public static LoginViewModel GenerateVM()
		{
			return new LoginViewModel(Services.PreBakedMopupService.GetInstance());
		}

		/// <summary>
		/// provides the LoginPopupPage Generic Type argument to
		/// <see cref="GeneratePopup{TPopupPage}(Dictionary{string, object})"/>
		/// </summary>
		/// <param name="propertyDictionary">Page Properties, for an example pull <seealso cref="PullViewModelProperties"/></param>
		/// <returns> Task that waits for user input</returns>
		public async Task<(string username, string password)> GeneratePopup(Dictionary<string, object> propertyDictionary)
		{
			return await GeneratePopup<LoginPopupPage>(propertyDictionary);
		}

		/// <summary>
		/// Attaches properties through the use of reflection. <see cref="PopupViewModel{TReturnable}.InitialiseOptionalProperties(Dictionary{string, object})"/>
		/// </summary>
		/// <typeparam name="TPopupPage">User defined page that uses the LoginViewModel ViewModel</typeparam>
		/// <param name="propertyDictionary">Page Properties, for an example pull <seealso cref="PullViewModelProperties"/></param>
		/// <returns> Task that waits for user input</returns>
		public async Task<(string username, string password)> GeneratePopup<TPopupPage>(Dictionary<string, object> optionalProperties) where TPopupPage : Mopups.Pages.PopupPage, IGenericViewModel<LoginViewModel>, new()
		{
			InitialiseOptionalProperties(optionalProperties);
			return await Services.PreBakedMopupService.GetInstance().PushAsync<LoginViewModel, TPopupPage, (string username, string password)>(this);
		}

		/// <returns> Task that waits for user input</returns>
		public async Task<(string username, string password)> GeneratePopup()
		{
			return await GeneratePopup<LoginPopupPage>();
		}

		/// <returns> Task that waits for user input</returns>
		public async Task<(string username, string password)> GeneratePopup<TPopupPage>() where TPopupPage : Mopups.Pages.PopupPage, IGenericViewModel<LoginViewModel>, new()
		{
			return await Services.PreBakedMopupService.GetInstance().PushAsync<LoginViewModel, TPopupPage, (string username, string password)>(this);
		}

		public async Task<(string username, string password)> GeneratePopup(ILogin propertyInterface)
		{
			return await GeneratePopup<LoginPopupPage>(propertyInterface);
		}

		public async Task<(string username, string password)> GeneratePopup<TPopupPage>(ILogin propertyInterface) where TPopupPage : PopupPage, IGenericViewModel<LoginViewModel>, new()
		{
			PropertyInfo[] properties = typeof(ILogin).GetProperties();
			for (int propertyIndex = 0; propertyIndex < properties.Count(); propertyIndex++)
			{
				GetType().GetProperty(properties[propertyIndex].Name).SetValue(this, properties[propertyIndex].GetValue(propertyInterface, null), null);
			}
			return await Services.PreBakedMopupService.GetInstance().PushAsync<LoginViewModel, TPopupPage, (string username, string password)>(this);
		}


		/// <summary>
		/// Provides a base dictionary, along with types that you can use to Initialise properties
		/// </summary>
		/// <returns>All Properties contained within the Viewmodel, with their names, current values, and types</returns>
		public virtual Dictionary<string, (object property, Type propertyType)> PullViewModelProperties()
		{
			return base.PullViewModelProperties<LoginViewModel>();
		}

		/// <summary>
		/// provides the LoginPopupPage Generic Type argument to
		/// <see cref="AutoGenerateBasicPopup{TPopupPage}(Color, Color, string, Color, Color, string, Color, string, string, string, string, string, int, int)"/>
		/// </summary>
		public static async Task<(string username, string password)> AutoGenerateBasicPopup(Color leftButtonColour, Color leftButtonTextColour, string leftButtonText, Color rightButtonColour, Color rightButtonTextColour, string rightButtonText, Color mainPopupColour, string username, string usernamePlaceHolder, string password, string passwordPlaceHolder, string PictureSource, int heightRequest = 0, int widthRequest = 0)
		{
			return await AutoGenerateBasicPopup<LoginPopupPage>(leftButtonColour, leftButtonTextColour, leftButtonText, rightButtonColour, rightButtonTextColour, rightButtonText, mainPopupColour, username, usernamePlaceHolder, password, passwordPlaceHolder, PictureSource, heightRequest, widthRequest);
		}

		public static async Task<(string username, string password)> AutoGenerateBasicPopup<TPopupPage>(Color leftButtonColour, Color leftButtonTextColour, string leftButtonText, Color rightButtonColour, Color rightButtonTextColour, string rightButtonText, Color mainPopupColour, string username, string usernamePlaceHolder, string password, string passwordPlaceHolder, string PictureSource, int heightRequest = 0, int widthRequest = 0) where TPopupPage : Mopups.Pages.PopupPage, IGenericViewModel<LoginViewModel>, new()
		{
			var AutoGeneratePopupViewModel = new LoginViewModel(Services.PreBakedMopupService.GetInstance());
			ICommand leftButtonCommand = new Command(() => AutoGeneratePopupViewModel.SafeCloseModal<TPopupPage>(("invalid", "invalid")));
			ICommand rightButtonCommand = new Command(() => AutoGeneratePopupViewModel.SafeCloseModal<TPopupPage>((AutoGeneratePopupViewModel.Username, AutoGeneratePopupViewModel.Password)));

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
				{ "MainPopupColour", mainPopupColour },
				{ "Username", username},
				{ "UsernamePlaceholder" , usernamePlaceHolder},
				{ "Password" , password},
				{ "PasswordPlaceholder" , passwordPlaceHolder},
				{ "PictureSource" , PictureSource}
			};
			return await AutoGeneratePopupViewModel.GeneratePopup<TPopupPage>(propertyDictionary);
		}

	}
}

