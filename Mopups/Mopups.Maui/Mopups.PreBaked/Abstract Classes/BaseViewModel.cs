using Mopups.PreBaked.Interfaces;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;




namespace Mopups.PreBaked.AbstractClasses
{
	public abstract class BasePopupViewModel : BindableObject, INotifyPropertyChanged, IBasePopupViewModel
	{
		protected IPreBakedMopupService PopupService { get; set; }

		private bool _isBusy;
		public bool IsBusy
		{
			get => _isBusy;
			set => SetValue(ref _isBusy, value);
		}


		protected BasePopupViewModel(IPreBakedMopupService popupService)
		{
			PopupService = popupService;
		}

		public virtual void RunOnAttachment<TPopupType>(TPopupType popupPage) where TPopupType : Mopups.Pages.PopupPage
		{

		}

		protected virtual void SetValue<T>(ref T backingField, T value, [CallerMemberName] string propertyName = null)
		{
			if (EqualityComparer<T>.Default.Equals(backingField, value))
			{
				return;
			}
			backingField = value;
			OnPropertyChanged(propertyName);
		}
	}
}
