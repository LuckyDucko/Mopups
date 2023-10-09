using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Mopups.PreBaked.Interfaces;

namespace Mopups.PreBaked.AbstractClasses
{
	public abstract class PopupViewModel<TReturnable> : BasePopupViewModel, IPopupViewModel<TReturnable>
	{
		/// <summary>
		/// This is what the result of the popupPage will be when it returns to its caller
		/// </summary>
		public TaskCompletionSource<TReturnable> Returnable { get; set; }

		/// <summary>
		/// This is the fallback value, incase of premature exists
		/// </summary>
		protected TReturnable BaseExitValue { get; set; }

		private string _mainPopupInformation;
		public string MainPopupInformation
		{
			get => _mainPopupInformation;
			set => SetValue(ref _mainPopupInformation, value);
		}

		
		private Color _mainPopupColour;
		public Color MainPopupColour
		{
			get => _mainPopupColour;
			set => SetValue(ref _mainPopupColour, value);
		}

		private int _widthRequest; //Ensure this is _name
		public int WidthRequest //Ensure this is Name
		{
			get => _widthRequest;
			set => SetValue(ref _widthRequest, value);
		}

		private int _heightRequest; //Ensure this is _name
		public int HeightRequest //Ensure this is Name
		{
			get => _heightRequest;
			set => SetValue(ref _heightRequest, value);
		}

		protected PopupViewModel(IPreBakedMopupService popupService, int heightRequest, int widthRequest) : base(popupService)
		{
			Returnable = new TaskCompletionSource<TReturnable>();
			HeightRequest = heightRequest;
			WidthRequest = widthRequest;
			BaseExitValue = default(TReturnable);
		}

		protected PopupViewModel(IPreBakedMopupService popupService) : base(popupService)
		{
			Returnable = new TaskCompletionSource<TReturnable>();
			HeightRequest = 0;
			WidthRequest = 0;
			BaseExitValue = default(TReturnable);
		}


		public virtual void SafeCloseModal<TPopupType>() where TPopupType : Mopups.Pages.PopupPage, new()
		{
			SafeCloseModal<TPopupType>(BaseExitValue);
		}

		/// <summary>
		/// awaits the async action to complete, and then
		/// passes the result to the sync SafeCloseModal
		/// </summary>
		/// <param name="asyncResult">still processing results</param>
		/// <returns></returns>
		public virtual async Task SafeCloseModal<TPopupType>(Task<TReturnable> asyncResult) where TPopupType : Mopups.Pages.PopupPage, new()
		{
			if (asyncResult.Status.Equals(TaskStatus.Created) || asyncResult.Status.Equals(TaskStatus.WaitingForActivation))
			{
				asyncResult.Start();
			}
			var buttonCommandResult = await asyncResult;
			SafeCloseModal<TPopupType>(buttonCommandResult);
		}

		/// <summary>
		/// returns the result of the popup into the PreBaked task, With
		/// fallback attempts if necessary.
		/// </summary>
		/// <param name="result">User Feedback/Processed Results</param>
		public virtual void SafeCloseModal<TPopupType>(TReturnable result) where TPopupType : Mopups.Pages.PopupPage, new()
		{
			try
			{
				var safeCloseAttempt = Returnable.TrySetResult(result);
				if (!safeCloseAttempt)
				{
					Returnable = new TaskCompletionSource<TReturnable>();
					Returnable.SetResult(result);
				}
			}
			catch (Exception)
			{
				Returnable = new TaskCompletionSource<TReturnable>();
				Returnable.SetResult(BaseExitValue);
			}
			finally
			{

				PopupService.PopAsync<TPopupType>();
			}
		}

		/// <summary>
		/// This is for use only when you wish for some form of reusable wrapper,
		/// used internally for property setting.
		/// </summary>
		/// <param name="optionalProperties"></param>
		public virtual void InitialiseOptionalProperties(Dictionary<string, object> optionalProperties)
		{
			foreach (KeyValuePair<string, object> property in optionalProperties)
			{
				GetType().GetProperty(property.Key).SetValue(this, property.Value, null);
			}
		}

		/// <summary>
		/// Allows you to gather the values of every property that is on the popupviewmodel
		/// into a key/value pair. Properties will need to be cast into their proper types
		/// </summary>
		/// <typeparam name="TViewModel"> Viwemodel Type you wish to iterate over</typeparam>
		/// <returns></returns>
		protected virtual Dictionary<string, (object property, Type propertyType)> PullViewModelProperties<TViewModel>() where TViewModel : BasePopupViewModel
		{
			var propertyDictionary = new Dictionary<string, (object property, Type propertyType)>();
			PropertyInfo[] properties = typeof(TViewModel).GetProperties();

			for (int propertyIndex = 0; propertyIndex < properties.Count(); propertyIndex++)
			{
				propertyDictionary.Add(properties[propertyIndex].Name, (properties[propertyIndex].GetValue(this, null), properties[propertyIndex].DeclaringType));
			}
			return propertyDictionary;
		}

		/// <summary>
		/// Converts the Dictionary provided by <see cref="PullViewModelProperties{TViewModel}"/> into one usable by popup generation functions
		/// </summary>
		/// <param name="viewModelProperties">user edited dictionary of viewModelProperties</param>
		/// <returns></returns>
		public Dictionary<string, object> FinalisePreparedProperties(Dictionary<string, (object property, Type propertyType)> viewModelProperties)
		{
			var propertyDictionary = new Dictionary<string, object>();
			foreach (var viewModelProperty in viewModelProperties)
			{
				propertyDictionary.Add(viewModelProperty.Key, viewModelProperty.Value.property);
			}
			return propertyDictionary;
		}

	}
}
