using AsyncAwaitBestPractices;

using Mopups.PreBaked.AbstractClasses;
using Mopups.PreBaked.Interfaces;
using Mopups.PreBaked.PopupPages.Loader;

using Mopups.Interfaces;
using Mopups.Pages;
using Mopups.Services;

namespace Mopups.PreBaked.Services
{
	public class PreBakedMopupService : IPreBakedMopupService
	{
		private static volatile PreBakedMopupService s_internalMopupService;
		private static IPopupNavigation s_popupNavigation;
		private static readonly object s_threadPadlock = new object();

		private PreBakedMopupService(IPopupNavigation popupNavigation = null)
		{
			s_popupNavigation = popupNavigation ?? MopupService.Instance;
		}

		public static PreBakedMopupService GetInstance(IPopupNavigation popupNavigation = null)
		{
			if (s_popupNavigation == null)
			{
				lock (s_threadPadlock)
				{
					if (s_popupNavigation == null)
					{
						s_internalMopupService = new PreBakedMopupService(popupNavigation);
					}
				}
			}
			return s_internalMopupService;
		}

		public void PopAsync<TPopupType>() where TPopupType : PopupPage, new()
		{
			lock (s_threadPadlock)
			{
				var PotentialPages = s_popupNavigation.PopupStack.Where((PopupPage PageOnPopupStack) => PageOnPopupStack.GetType().IsEquivalentTo(typeof(TPopupType)));
				if (PotentialPages.Any())
				{
					s_popupNavigation.RemovePageAsync(PotentialPages.First()).SafeFireAndForget(onException: (Exception ex) => Console.WriteLine(ex));
				}
			}
		}

		/// <summary>
		/// Added the ability to specify an action when an exception happens when popping.
		/// An example of this is issues on detach, such as AiForms.Effects on popupPages
		/// </summary>
		/// <typeparam name="TPopupType"></typeparam>
		/// <param name="exceptionActionForSafeFireAndForget"></param>
		public void PopAsync<TPopupType>(Action<Exception> exceptionActionForSafeFireAndForget) where TPopupType : PopupPage, new()
		{
			lock (s_threadPadlock)
			{
				var PotentialPages = s_popupNavigation.PopupStack.Where((PopupPage PageOnPopupStack) => PageOnPopupStack.GetType().IsEquivalentTo(typeof(TPopupType)));
				if (PotentialPages.Any())
				{
					s_popupNavigation.RemovePageAsync(PotentialPages.First()).SafeFireAndForget(exceptionActionForSafeFireAndForget);
				}
			}
		}


		public TPopupPage CreatePopupPage<TPopupPage>()
			where TPopupPage : PopupPage, new()
		{
			return new TPopupPage();
		}

		public TPopupPage AttachViewModel<TPopupPage, TViewModel>(TPopupPage popupPage, TViewModel viewModel)
			where TPopupPage : PopupPage, IGenericViewModel<TViewModel>
			where TViewModel : BasePopupViewModel
		{
			popupPage.SetViewModel(viewModel);
			viewModel.RunOnAttachment<TPopupPage>(popupPage);
			return popupPage;
		}

		public async Task<TReturnable> PushAsync<TViewModel, TPopupPage, TReturnable>(TViewModel modalViewModel)
			where TPopupPage : PopupPage, IGenericViewModel<TViewModel>, new()
			where TViewModel : PopupViewModel<TReturnable>
		{
			TPopupPage popupModal = AttachViewModel(CreatePopupPage<TPopupPage>(), modalViewModel);
			await s_popupNavigation.PushAsync(popupModal);
			return await modalViewModel.Returnable.Task;
		}

		/// <summary>
		/// Used to provide a minimum wait time to any task.
		/// </summary>
		/// <param name="returnableTask">The task you wish to await</param>
		/// <param name="millisecondsDelay">Delay you wish to add</param>
		/// <returns></returns>
		public async Task ForceMinimumWaitTime(Task returnableTask, int millisecondsDelay)
		{
			Task initialTime = Task.Delay(millisecondsDelay);
			await Task.WhenAll(initialTime, returnableTask);
		}

		/// <summary>
		/// Used to provide a minimum wait time to any task with a return value
		/// </summary>
		/// <typeparam name="TAsyncActionResult">the type for what the task returns</typeparam>
		/// <param name="returnableTask">The task you wish to await</param>
		/// <param name="millisecondsDelay">Delay you wish to add</param>
		/// <returns></returns>
		public async Task<TAsyncActionResult> ForceMinimumWaitTime<TAsyncActionResult>(Task<TAsyncActionResult> returnableTask, int millisecondsDelay)
		{
			Task initialTime = Task.Delay(millisecondsDelay);
			await Task.WhenAll(initialTime, returnableTask);
			return returnableTask.Result;
		}

		public async Task WrapTaskInLoader(Task action, Color loaderColour, Color loaderPopupColour, List<string> reasonsForLoader, Color textColour, int millisecondsBetweenReasons = 2000)
		{
			await WrapTaskInLoader<LoaderPopupPage>(action, loaderColour, loaderPopupColour, reasonsForLoader, textColour, millisecondsBetweenReasons);
		}

		public async Task WrapTaskInLoader<TPopupPage>(Task action, Color loaderColour, Color loaderPopupColour, List<string> reasonsForLoader, Color textColour, int millisecondsBetweenReasons = 2000)
			where TPopupPage : PopupPage, IGenericViewModel<LoaderViewModel>, new()
		{
			Task PaddedTaskTime = ForceMinimumWaitTime(action, 1000);
			ConstructLoaderAndDisplay<TPopupPage>(PaddedTaskTime, loaderColour, loaderPopupColour, reasonsForLoader, textColour, millisecondsBetweenReasons);
			await PaddedTaskTime;
		}



        public async Task<TAsyncActionResult> WrapReturnableTaskInLoader<TAsyncActionResult>(Task<TAsyncActionResult> action, Color loaderColour, Color loaderPopupColour, List<string> reasonsForLoader, Color textColour, int millisecondsBetweenReasons = 2000)
		{
			return await WrapReturnableTaskInLoader<TAsyncActionResult, LoaderPopupPage>(action, loaderColour, loaderPopupColour, reasonsForLoader, textColour, millisecondsBetweenReasons);
		}

		public async Task<TAsyncActionResult> WrapReturnableTaskInLoader<TAsyncActionResult, TPopupPage>(Task<TAsyncActionResult> action, Color loaderColour, Color loaderPopupColour, List<string> reasonsForLoader, Color textColour, int millisecondsBetweenReasons = 2000)
			where TPopupPage : PopupPage, IGenericViewModel<LoaderViewModel>, new()
		{
			ConstructLoaderAndDisplay<TPopupPage>(action, loaderColour, loaderPopupColour, reasonsForLoader, textColour, millisecondsBetweenReasons);
			await action;
			return action.Result;
		}

		public async Task<TSyncActionResult> WrapReturnableFuncInLoader<TSyncActionResult>(Func<TSyncActionResult> action, Color loaderColour, Color loaderPopupColour, List<string> reasonsForLoader, Color textColour, int millisecondsBetweenReasons = 2000)
		{
			return await WrapReturnableFuncInLoader<TSyncActionResult, LoaderPopupPage>(action, loaderColour, loaderPopupColour, reasonsForLoader, textColour, millisecondsBetweenReasons);
		}

		public async Task<TSyncActionResult> WrapReturnableFuncInLoader<TSyncActionResult, TPopupPage>(Func<TSyncActionResult> action, Color loaderColour, Color loaderPopupColour, List<string> reasonsForLoader, Color textColour, int millisecondsBetweenReasons = 2000)
			where TPopupPage : PopupPage, IGenericViewModel<LoaderViewModel>, new()
		{
			Task<TSyncActionResult> actionResult = Task.Run(action);
			return await WrapReturnableTaskInLoader<TSyncActionResult, TPopupPage>(actionResult, loaderColour, loaderPopupColour, reasonsForLoader, textColour, millisecondsBetweenReasons);
		}


		public async Task<TSyncActionResult> WrapReturnableFuncInLoader<TArgument1, TSyncActionResult>(Func<TArgument1, TSyncActionResult> action, TArgument1 argument1, Color loaderColour, Color loaderPopupColour, List<string> reasonsForLoader, Color textColour, int millisecondsBetweenReasons = 2000)
		{
			return await WrapReturnableFuncInLoader<TArgument1, TSyncActionResult, LoaderPopupPage>(action, argument1, loaderColour, loaderPopupColour, reasonsForLoader, textColour, millisecondsBetweenReasons);
		}

		public async Task<TSyncActionResult> WrapReturnableFuncInLoader<TArgument1, TSyncActionResult, TPopupPage>(Func<TArgument1, TSyncActionResult> action, TArgument1 argument1, Color loaderColour, Color loaderPopupColour, List<string> reasonsForLoader, Color textColour, int millisecondsBetweenReasons = 2000)
			where TPopupPage : PopupPage, IGenericViewModel<LoaderViewModel>, new()
		{
			Task<TSyncActionResult> actionResult = Task.Run(() => action.Invoke(argument1));
			return await WrapReturnableTaskInLoader<TSyncActionResult, TPopupPage>(actionResult, loaderColour, loaderPopupColour, reasonsForLoader, textColour, millisecondsBetweenReasons);
		}


		public async Task<TSyncActionResult> WrapReturnableFuncInLoader<TArgument1, TArgument2, TSyncActionResult>(Func<TArgument1, TArgument2, TSyncActionResult> action, TArgument1 argument1, TArgument2 argument2, Color loaderColour, Color loaderPopupColour, List<string> reasonsForLoader, Color textColour, int millisecondsBetweenReasons = 2000)
		{
			return await WrapReturnableFuncInLoader<TArgument1, TArgument2, TSyncActionResult, LoaderPopupPage>(action, argument1, argument2, loaderColour, loaderPopupColour, reasonsForLoader, textColour, millisecondsBetweenReasons);
		}

		public async Task<TSyncActionResult> WrapReturnableFuncInLoader<TArgument1, TArgument2, TSyncActionResult, TPopupPage>(Func<TArgument1, TArgument2, TSyncActionResult> action, TArgument1 argument1, TArgument2 argument2, Color loaderColour, Color loaderPopupColour, List<string> reasonsForLoader, Color textColour, int millisecondsBetweenReasons = 2000)
			where TPopupPage : PopupPage, IGenericViewModel<LoaderViewModel>, new()
		{
			Task<TSyncActionResult> actionResult = Task.Run(() => action.Invoke(argument1, argument2));
			return await WrapReturnableTaskInLoader<TSyncActionResult, TPopupPage>(actionResult, loaderColour, loaderPopupColour, reasonsForLoader, textColour, millisecondsBetweenReasons);
		}

		public async Task<TSyncActionResult> WrapReturnableFuncInLoader<TArgument1, TArgument2, TArgument3, TSyncActionResult>(Func<TArgument1, TArgument2, TArgument3, TSyncActionResult> action, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3, Color loaderColour, Color loaderPopupColour, List<string> reasonsForLoader, Color textColour, int millisecondsBetweenReasons = 2000)
		{
			return await WrapReturnableFuncInLoader<TArgument1, TArgument2, TArgument3, TSyncActionResult, LoaderPopupPage>(action, argument1, argument2, argument3, loaderColour, loaderPopupColour, reasonsForLoader, textColour, millisecondsBetweenReasons);
		}

		public async Task<TSyncActionResult> WrapReturnableFuncInLoader<TArgument1, TArgument2, TArgument3, TSyncActionResult, TPopupPage>(Func<TArgument1, TArgument2, TArgument3, TSyncActionResult> action, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3, Color loaderColour, Color loaderPopupColour, List<string> reasonsForLoader, Color textColour, int millisecondsBetweenReasons = 2000)
			where TPopupPage : PopupPage, IGenericViewModel<LoaderViewModel>, new()
		{
			Task<TSyncActionResult> actionResult = Task.Run(() => action.Invoke(argument1, argument2, argument3));
			return await WrapReturnableTaskInLoader<TSyncActionResult, TPopupPage>(actionResult, loaderColour, loaderPopupColour, reasonsForLoader, textColour, millisecondsBetweenReasons);
		}

		public async Task<TSyncActionResult> WrapReturnableFuncInLoader<TArgument1, TArgument2, TArgument3, TArgument4, TSyncActionResult>(Func<TArgument1, TArgument2, TArgument3, TArgument4, TSyncActionResult> action, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3, TArgument4 argument4, Color loaderColour, Color loaderPopupColour, List<string> reasonsForLoader, Color textColour, int millisecondsBetweenReasons = 2000)
		{
			return await WrapReturnableFuncInLoader<TArgument1, TArgument2, TArgument3, TArgument4, TSyncActionResult, LoaderPopupPage>(action, argument1, argument2, argument3, argument4, loaderColour, loaderPopupColour, reasonsForLoader, textColour, millisecondsBetweenReasons);
		}

		public async Task<TSyncActionResult> WrapReturnableFuncInLoader<TArgument1, TArgument2, TArgument3, TArgument4, TSyncActionResult, TPopupPage>(Func<TArgument1, TArgument2, TArgument3, TArgument4, TSyncActionResult> action, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3, TArgument4 argument4, Color loaderColour, Color loaderPopupColour, List<string> reasonsForLoader, Color textColour, int millisecondsBetweenReasons = 2000)
			where TPopupPage : PopupPage, IGenericViewModel<LoaderViewModel>, new()
		{
			Task<TSyncActionResult> actionResult = Task.Run(() => action.Invoke(argument1, argument2, argument3, argument4));
			return await WrapReturnableTaskInLoader<TSyncActionResult, TPopupPage>(actionResult, loaderColour, loaderPopupColour, reasonsForLoader, textColour, millisecondsBetweenReasons);
		}

		public async Task<TSyncActionResult> WrapReturnableFuncInLoader<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5, TSyncActionResult>(Func<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5, TSyncActionResult> action, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3, TArgument4 argument4, TArgument5 argument5, Color loaderColour, Color loaderPopupColour, List<string> reasonsForLoader, Color textColour, int millisecondsBetweenReasons = 2000)
		{
			return await WrapReturnableFuncInLoader<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5, TSyncActionResult, LoaderPopupPage>(action, argument1, argument2, argument3, argument4, argument5, loaderColour, loaderPopupColour, reasonsForLoader, textColour, millisecondsBetweenReasons);
		}

		public async Task<TSyncActionResult> WrapReturnableFuncInLoader<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5, TSyncActionResult, TPopupPage>(Func<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5, TSyncActionResult> action, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3, TArgument4 argument4, TArgument5 argument5, Color loaderColour, Color loaderPopupColour, List<string> reasonsForLoader, Color textColour, int millisecondsBetweenReasons = 2000)
			where TPopupPage : PopupPage, IGenericViewModel<LoaderViewModel>, new()
		{
			Task<TSyncActionResult> actionResult = Task.Run(() => action.Invoke(argument1, argument2, argument3, argument4, argument5));
			return await WrapReturnableTaskInLoader<TSyncActionResult, TPopupPage>(actionResult, loaderColour, loaderPopupColour, reasonsForLoader, textColour, millisecondsBetweenReasons);
		}

		public async Task<TSyncActionResult> WrapReturnableFuncInLoader<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5, TArgument6, TSyncActionResult>(Func<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5, TArgument6, TSyncActionResult> action, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3, TArgument4 argument4, TArgument5 argument5, TArgument6 argument6, Color loaderColour, Color loaderPopupColour, List<string> reasonsForLoader, Color textColour, int millisecondsBetweenReasons = 2000)
		{
			return await WrapReturnableFuncInLoader<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5, TArgument6, TSyncActionResult, LoaderPopupPage>(action, argument1, argument2, argument3, argument4, argument5, argument6, loaderColour, loaderPopupColour, reasonsForLoader, textColour, millisecondsBetweenReasons);
		}

		public async Task<TSyncActionResult> WrapReturnableFuncInLoader<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5, TArgument6, TSyncActionResult, TPopupPage>(Func<TArgument1, TArgument2, TArgument3, TArgument4, TArgument5, TArgument6, TSyncActionResult> action, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3, TArgument4 argument4, TArgument5 argument5, TArgument6 argument6, Color loaderColour, Color loaderPopupColour, List<string> reasonsForLoader, Color textColour, int millisecondsBetweenReasons = 2000)
			where TPopupPage : PopupPage, IGenericViewModel<LoaderViewModel>, new()
		{
			Task<TSyncActionResult> actionResult = Task.Run(() => action.Invoke(argument1, argument2, argument3, argument4, argument5, argument6));
			return await WrapReturnableTaskInLoader<TSyncActionResult, TPopupPage>(actionResult, loaderColour, loaderPopupColour, reasonsForLoader, textColour, millisecondsBetweenReasons);
		}


		private LoaderViewModel ConstructLoaderModal(Color loaderColour, Color loaderPopupColour, List<string> reasonsForLoader, Color textColour, int millisecondsBetweenReasons)
		{
			return new LoaderViewModel(this, reasonsForLoader)
			{
				LoaderColour = loaderColour,
				MainPopupColour = loaderPopupColour,
				TextColour = textColour,
				MillisecondsBetweenReasonSwitch = millisecondsBetweenReasons,
			};
		}

		private void ConstructLoaderAndDisplay<TPopupPage>(Task action, Color loaderColour, Color loaderPopupColour, List<string> reasonsForLoader, Color textColour, int millisecondsBetweenReasons) where TPopupPage : PopupPage, IGenericViewModel<LoaderViewModel>, new()
		{
			LoaderViewModel loaderWaiting = ConstructLoaderModal(loaderColour, loaderPopupColour, reasonsForLoader, textColour, millisecondsBetweenReasons);

			action.GetAwaiter().OnCompleted(() => MainThread.BeginInvokeOnMainThread(() => loaderWaiting.SafeCloseModal<TPopupPage>()));
			if (!action.IsCompleted)
			{
				LoaderAttachAndPush<TPopupPage>(loaderWaiting).SafeFireAndForget();
			}
		}

		private async Task LoaderAttachAndPush<TPopupPage>(LoaderViewModel loaderWaiting) where TPopupPage : PopupPage, IGenericViewModel<LoaderViewModel>, new()
		{
			var popupModal = AttachViewModel(CreatePopupPage<TPopupPage>(), loaderWaiting);
			await MainThread.InvokeOnMainThreadAsync(() => s_popupNavigation.PushAsync(popupModal));
		}
	}
}
