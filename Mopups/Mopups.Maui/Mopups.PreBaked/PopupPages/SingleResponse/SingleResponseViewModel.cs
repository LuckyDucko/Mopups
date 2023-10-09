using System.Windows.Input;
using Mopups.PreBaked.AbstractClasses;
using Mopups.PreBaked.Interfaces;
using Mopups.Pages;
using System.Reflection;

namespace Mopups.PreBaked.PopupPages.SingleResponse
{
    public class SingleResponseViewModel : PopupViewModel<bool>, ISingleResponseViewModel
	{
		private ICommand _singleButtonCommand;
		public ICommand SingleButtonCommand
		{
			get => _singleButtonCommand;
			set => SetValue(ref _singleButtonCommand, value);
		}

		private string _singleButtonText;
		public string SingleButtonText
		{
			get => _singleButtonText;
			set => SetValue(ref _singleButtonText, value);
		}

		private Color _singleButtonColour;
		public Color SingleButtonColour
		{
			get => _singleButtonColour;
			set => SetValue(ref _singleButtonColour, value);
		}

		private Color _singleButtonTextColour;
		public Color SingleButtonTextColour
		{
			get => _singleButtonTextColour;
			set => SetValue(ref _singleButtonTextColour, value);
		}

		private string _singleDisplayImage;
		public string SingleDisplayImage
		{
			get => _singleDisplayImage;
			set => SetValue(ref _singleDisplayImage, value);
		}



		public SingleResponseViewModel(IPreBakedMopupService popupService) : base(popupService)
		{
		}


		public static SingleResponseViewModel GenerateVM()
		{
			return new SingleResponseViewModel(Services.PreBakedMopupService.GetInstance());
		}

		/// <summary>
		/// provides the SingleResponsePopupPage Generic Type argument to
		/// <see cref="GeneratePopup{TPopupPage}(Dictionary{string, object})"/>
		/// </summary>
		/// <param name="propertyDictionary">Page Properties, for an example pull <seealso cref="PullViewModelProperties"/></param>
		/// <returns> Task that waits for user input</returns>
		public async Task<bool> GeneratePopup(Dictionary<string, object> propertyDictionary)
		{
			return await GeneratePopup<SingleResponsePopupPage>(propertyDictionary);
		}

		/// <summary>
		/// Attaches properties through the use of reflection. <see cref="PopupViewModel{TReturnable}.InitialiseOptionalProperties(Dictionary{string, object})"/>
		/// </summary>
		/// <typeparam name="TPopupPage">User defined page that uses the SingleResponseViewModel ViewModel</typeparam>
		/// <param name="propertyDictionary">Page Properties, for an example pull <seealso cref="PullViewModelProperties"/></param>
		/// <returns> Task that waits for user input</returns>
		public async Task<bool> GeneratePopup<TPopupPage>(Dictionary<string, object> optionalProperties) where TPopupPage : Mopups.Pages.PopupPage, IGenericViewModel<SingleResponseViewModel>, new()
		{
			InitialiseOptionalProperties(optionalProperties);

            return await Services.PreBakedMopupService.GetInstance().PushAsync<SingleResponseViewModel, TPopupPage, bool>(this);
		}


		/// <returns> Task that waits for user input</returns>
		public async Task<bool> GeneratePopup()
		{
			return await GeneratePopup<SingleResponsePopupPage>();
		}

		/// <returns> Task that waits for user input</returns>
		public async Task<bool> GeneratePopup<TPopupPage>() where TPopupPage : Mopups.Pages.PopupPage, IGenericViewModel<SingleResponseViewModel>, new()
		{
			return await Services.PreBakedMopupService.GetInstance().PushAsync<SingleResponseViewModel, TPopupPage, bool>(this);
		}

		public async Task<bool> GeneratePopup(ISingleResponse propertyInterface)
		{
			return await GeneratePopup<SingleResponsePopupPage>(propertyInterface);
		}

		public async Task<bool> GeneratePopup<TPopupPage>(ISingleResponse propertyInterface) where TPopupPage : PopupPage, IGenericViewModel<SingleResponseViewModel>, new()
		{
			PropertyInfo[] properties = typeof(ISingleResponse).GetProperties();
			for (int propertyIndex = 0; propertyIndex < properties.Count(); propertyIndex++)
			{
				GetType().GetProperty(properties[propertyIndex].Name).SetValue(this, properties[propertyIndex].GetValue(propertyInterface, null), null);
			}
			return await Services.PreBakedMopupService.GetInstance().PushAsync<SingleResponseViewModel, TPopupPage, bool>(this);
		}




		/// <summary>
		/// Provides a base dictionary, along with types that you can use to Initialise properties
		/// </summary>
		/// <returns>All Properties contained within the Viewmodel, with their names, current values, and types</returns>
		public virtual Dictionary<string, (object property, Type propertyType)> PullViewModelProperties()
		{
			return base.PullViewModelProperties<SingleResponseViewModel>();
		}

		/// <summary>
		/// provides the SingleResponsePopupPage Generic Type argument to
		/// <see cref="AutoGenerateBasicPopup{TPopupPage}(Color, Color, string, Color, string, string, int, int)"/>
		/// <returns> a task awaiting user interaction</returns>
		public static async Task<bool> AutoGenerateBasicPopup(Color buttonColour, Color buttonTextColour, string buttonText, Color mainPopupColour, string popupInformation, string displayImageName, int heightRequest = 0, int widthRequest = 0)
		{
			return await AutoGenerateBasicPopup<SingleResponsePopupPage>(buttonColour, buttonTextColour, buttonText, mainPopupColour, popupInformation, displayImageName, heightRequest, widthRequest);
		}

		/// <summary>
		/// Attached basic properties defined.
		/// Left button will return false
		/// Right button will return true
		/// </summary>
		/// <typeparam name="TPopupPage">User defined page that uses the SingleResponseViewModel ViewModel</typeparam>
		/// <returns> a task awaiting user interaction</returns>
		public static async Task<bool> AutoGenerateBasicPopup<TPopupPage>(Color buttonColour, Color buttonTextColour, string buttonText, Color mainPopupColour, string popupInformation, string displayImageName, int heightRequest = 0, int widthRequest = 0) where TPopupPage : Mopups.Pages.PopupPage, IGenericViewModel<SingleResponseViewModel>, new()
		{
			var AutoGeneratePopupViewModel = new SingleResponseViewModel(Services.PreBakedMopupService.GetInstance());
			ICommand singleButtonCommand = new Command(() => AutoGeneratePopupViewModel.SafeCloseModal<TPopupPage>(true));

			var propertyDictionary = new Dictionary<string, object>
			{
				{ "SingleButtonCommand", singleButtonCommand },
				{ "SingleButtonColour", buttonColour },
				{ "SingleButtonText", buttonText ?? "Yes" },
				{ "SingleButtonTextColour", buttonTextColour },
				{ "HeightRequest", heightRequest },
				{ "WidthRequest", widthRequest },
				{ "MainPopupInformation", popupInformation ?? "An Error has occured, try again" },
				{ "MainPopupColour", mainPopupColour },
				{ "SingleDisplayImage", displayImageName ?? "NoSource.png" }
			};
			return await AutoGeneratePopupViewModel.GeneratePopup<TPopupPage>(propertyDictionary);
		}




	}
}