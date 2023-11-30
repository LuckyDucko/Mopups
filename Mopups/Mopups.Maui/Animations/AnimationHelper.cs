using System;
namespace Mopups.Animations;

public static class AnimationHelper
{
	public static bool SystemAnimationsEnabled
	{
		get
		{
#if __ANDROID__
			return Android.Animation.ValueAnimator.AreAnimatorsEnabled();
#endif
			return true;
		}
	}
}