using System;
namespace Mopups.Animations;

public static class AnimationHelper
{
	public static bool SystemAnimationsEnabled
	{
		get
		{
#if __ANDROID__
			if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
			{
				return Android.Animation.ValueAnimator.AreAnimatorsEnabled();
			}
#endif
			return true;
		}
	}
}