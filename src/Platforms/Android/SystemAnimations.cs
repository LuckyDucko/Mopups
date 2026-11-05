using Android.Animation;

namespace Nkraft.Mopups.Animations;

internal static partial class SystemAnimations
{
    public static partial bool Enabled =>
        !OperatingSystem.IsAndroidVersionAtLeast(26) || ValueAnimator.AreAnimatorsEnabled();
}
