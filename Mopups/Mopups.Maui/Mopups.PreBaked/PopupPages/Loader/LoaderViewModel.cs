using Mopups.PreBaked.AbstractClasses;
using Mopups.PreBaked.Interfaces;

namespace Mopups.PreBaked.PopupPages.Loader
{
	public class LoaderViewModel : PopupViewModel<bool>, ILoaderViewModel
	{
		private Color _loaderColour;
		public Color LoaderColour
		{
			get => _loaderColour;
			set => SetValue(ref _loaderColour, value);
		}

		private Color _textColour;
		public Color TextColour
		{
			get => _textColour;
			set => SetValue(ref _textColour, value);
		}

		private List<string> _reasonsForLoader;
		public List<string> ReasonsForLoader
		{
			get => _reasonsForLoader;
			set => SetValue(ref _reasonsForLoader, value);
		}

		private int _millisecondsBetweenReasonSwitch;
		public int MillisecondsBetweenReasonSwitch
		{
			get => _millisecondsBetweenReasonSwitch;
			set => SetValue(ref _millisecondsBetweenReasonSwitch, value);
		}

		private CancellationTokenSource TextColourToken { get; set; }

		public LoaderViewModel(IPreBakedMopupService popupService, List<string> reasonsForLoader) : base(popupService)
		{
			TextColourToken = new CancellationTokenSource();
			ReasonsForLoader = reasonsForLoader;
			MainPopupInformation = ReasonsForLoader.Last();
			if (ReasonsForLoader?.Count > 1)
			{
				Task.Run(() => InformationSwitch(TextColourToken));
			}

		}

		public LoaderViewModel(IPreBakedMopupService popupService, List<string> reasonsForLoader, int millisecondsBetweenReasons) : base(popupService)
		{
			TextColourToken = new CancellationTokenSource();
			ReasonsForLoader = reasonsForLoader;
			MainPopupInformation = ReasonsForLoader.Last();
			MillisecondsBetweenReasonSwitch = millisecondsBetweenReasons;
			if (ReasonsForLoader?.Count > 1)
			{
				Task.Run(() => InformationSwitch(TextColourToken)).ConfigureAwait(false);
			}
		}
		private void InformationSwitch(CancellationTokenSource TextToken)
		{
			while (!TextToken.IsCancellationRequested)
			{
				Thread.Sleep(MillisecondsBetweenReasonSwitch);
				for (int i = 1; i < 10; i++)
				{
					Thread.Sleep(50);
					MainThread.BeginInvokeOnMainThread(() => TextColour = TextColour.WithLuminosity((float)(i * 0.1)));
				}

				int SelectionFromUnchosen = new Random().Next(ReasonsForLoader.Count() - 2);
				MainThread.BeginInvokeOnMainThread(() => MainPopupInformation = ReasonsForLoader
																			.OrderBy(pushChosenToBack)
																			.ElementAt(SelectionFromUnchosen));
				for (int i = 10; i > 0; i--)
				{
					Thread.Sleep(50);
					MainThread.BeginInvokeOnMainThread(() => TextColour = TextColour.WithLuminosity((float)(i * 0.1)));
				}
			}
			bool pushChosenToBack(string reasons) => reasons.Equals(MainPopupInformation);
		}

		public override void SafeCloseModal<TPopupPage>()
		{
			TextColourToken.Cancel();
			base.SafeCloseModal<TPopupPage>(true);
		}
	}
}
