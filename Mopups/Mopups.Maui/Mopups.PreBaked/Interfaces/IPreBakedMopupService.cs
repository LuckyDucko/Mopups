using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Mopups.PreBaked.AbstractClasses;

using Mopups.Pages;

namespace Mopups.PreBaked.Interfaces
{
	public interface IPreBakedMopupService
	{
		TPopupPage AttachViewModel<TPopupPage, TViewModel>(TPopupPage popupPage, TViewModel viewModel)
			where TPopupPage : PopupPage, IGenericViewModel<TViewModel>
			where TViewModel : BasePopupViewModel;
		TPopupPage CreatePopupPage<TPopupPage>() where TPopupPage : PopupPage, new();
		Task ForceMinimumWaitTime(Task returnableTask, int millisecondsDelay);
		Task<TAsyncActionResult> ForceMinimumWaitTime<TAsyncActionResult>(Task<TAsyncActionResult> returnableTask, int millisecondsDelay);
		void PopAsync<TPopupType>() where TPopupType : PopupPage, new();
		void PopAsync<TPopupType>(Action<Exception> exceptionActionForSafeFireAndForget) where TPopupType : PopupPage, new();
		Task<TReturnable> PushAsync<TViewModel, TPopupPage, TReturnable>(TViewModel modalViewModel)
			where TViewModel : PopupViewModel<TReturnable>
			where TPopupPage : PopupPage, IGenericViewModel<TViewModel>, new();
		Task<TSyncActionResult> WrapReturnableFuncInLoader<TSyncActionResult>(Func<TSyncActionResult> action, Color loaderColour, Color loaderPopupColour, List<string> reasonsForLoader, Color textColour, int millisecondsBetweenReasons = 2000);
		Task<TSyncActionResult> WrapReturnableFuncInLoader<TSyncActionResult, TPopupPage>(Func<TSyncActionResult> action, Color loaderColour, Color loaderPopupColour, List<string> reasonsForLoader, Color textColour, int millisecondsBetweenReasons = 2000) where TPopupPage : PopupPage, IGenericViewModel<PopupPages.Loader.LoaderViewModel>, new();
		Task<TSyncActionResult> WrapReturnableFuncInLoader<TArgument1, TSyncActionResult>(Func<TArgument1, TSyncActionResult> action, TArgument1 argument1, Color loaderColour, Color loaderPopupColour, List<string> reasonsForLoader, Color textColour, int millisecondsBetweenReasons = 2000);
		Task<TSyncActionResult> WrapReturnableFuncInLoader<TArgument1, TSyncActionResult, TPopupPage>(Func<TArgument1, TSyncActionResult> action, TArgument1 argument1, Color loaderColour, Color loaderPopupColour, List<string> reasonsForLoader, Color textColour, int millisecondsBetweenReasons = 2000) where TPopupPage : PopupPage, IGenericViewModel<PopupPages.Loader.LoaderViewModel>, new();
		Task<TSyncActionResult> WrapReturnableFuncInLoader<TArgument1, TArgument2, TSyncActionResult>(Func<TArgument1, TArgument2, TSyncActionResult> action, TArgument1 argument1, TArgument2 argument2, Color loaderColour, Color loaderPopupColour, List<string> reasonsForLoader, Color textColour, int millisecondsBetweenReasons = 2000);
		Task<TSyncActionResult> WrapReturnableFuncInLoader<TArgument1, TArgument2, TSyncActionResult, TPopupPage>(Func<TArgument1, TArgument2, TSyncActionResult> action, TArgument1 argument1, TArgument2 argument2, Color loaderColour, Color loaderPopupColour, List<string> reasonsForLoader, Color textColour, int millisecondsBetweenReasons = 2000) where TPopupPage : PopupPage, IGenericViewModel<PopupPages.Loader.LoaderViewModel>, new();
		Task<TSyncActionResult> WrapReturnableFuncInLoader<TArgument1, TArgument2, TArgument3, TSyncActionResult>(Func<TArgument1, TArgument2, TArgument3, TSyncActionResult> action, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3, Color loaderColour, Color loaderPopupColour, List<string> reasonsForLoader, Color textColour, int millisecondsBetweenReasons = 2000);
		Task<TSyncActionResult> WrapReturnableFuncInLoader<TArgument1, TArgument2, TArgument3, TSyncActionResult, TPopupPage>(Func<TArgument1, TArgument2, TArgument3, TSyncActionResult> action, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3, Color loaderColour, Color loaderPopupColour, List<string> reasonsForLoader, Color textColour, int millisecondsBetweenReasons = 2000) where TPopupPage : PopupPage, IGenericViewModel<PopupPages.Loader.LoaderViewModel>, new();
		Task<TSyncActionResult> WrapReturnableFuncInLoader<TArgument1, TArgument2, TArgument3, TArgument4, TSyncActionResult>(Func<TArgument1, TArgument2, TArgument3, TArgument4, TSyncActionResult> action, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3, TArgument4 argument4, Color loaderColour, Color loaderPopupColour, List<string> reasonsForLoader, Color textColour, int millisecondsBetweenReasons = 2000);
		Task<TSyncActionResult> WrapReturnableFuncInLoader<TArgument1, TArgument2, TArgument3, TArgument4, TSyncActionResult, TPopupPage>(Func<TArgument1, TArgument2, TArgument3, TArgument4, TSyncActionResult> action, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3, TArgument4 argument4, Color loaderColour, Color loaderPopupColour, List<string> reasonsForLoader, Color textColour, int millisecondsBetweenReasons = 2000) where TPopupPage : PopupPage, IGenericViewModel<PopupPages.Loader.LoaderViewModel>, new();
		Task<TSyncActionResult> WrapReturnableFuncInLoader<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5, TSyncActionResult>(Func<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5, TSyncActionResult> action, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3, TArgument4 argument4, TArgument5 argument5, Color loaderColour, Color loaderPopupColour, List<string> reasonsForLoader, Color textColour, int millisecondsBetweenReasons = 2000);
		Task<TSyncActionResult> WrapReturnableFuncInLoader<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5, TSyncActionResult, TPopupPage>(Func<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5, TSyncActionResult> action, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3, TArgument4 argument4, TArgument5 argument5, Color loaderColour, Color loaderPopupColour, List<string> reasonsForLoader, Color textColour, int millisecondsBetweenReasons = 2000) where TPopupPage : PopupPage, IGenericViewModel<PopupPages.Loader.LoaderViewModel>, new();
		Task<TSyncActionResult> WrapReturnableFuncInLoader<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5, TArgument6, TSyncActionResult>(Func<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5, TArgument6, TSyncActionResult> action, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3, TArgument4 argument4, TArgument5 argument5, TArgument6 argument6, Color loaderColour, Color loaderPopupColour, List<string> reasonsForLoader, Color textColour, int millisecondsBetweenReasons = 2000);
		Task<TSyncActionResult> WrapReturnableFuncInLoader<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5, TArgument6, TSyncActionResult, TPopupPage>(Func<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5, TArgument6, TSyncActionResult> action, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3, TArgument4 argument4, TArgument5 argument5, TArgument6 argument6, Color loaderColour, Color loaderPopupColour, List<string> reasonsForLoader, Color textColour, int millisecondsBetweenReasons = 2000) where TPopupPage : PopupPage, IGenericViewModel<PopupPages.Loader.LoaderViewModel>, new();
		Task<TAsyncActionResult> WrapReturnableTaskInLoader<TAsyncActionResult, TPopupPage>(Task<TAsyncActionResult> action, Color loaderColour, Color loaderPopupColour, List<string> reasonsForLoader, Color textColour, int millisecondsBetweenReasons = 2000) where TPopupPage : PopupPage, IGenericViewModel<PopupPages.Loader.LoaderViewModel>, new();
		Task WrapTaskInLoader<TPopupPage>(Task action, Color loaderColour, Color loaderPopupColour, List<string> reasonsForLoader, Color textColour, int millisecondsBetweenReasons = 2000) where TPopupPage : PopupPage, IGenericViewModel<PopupPages.Loader.LoaderViewModel>, new();
	}
}