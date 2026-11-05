namespace Nkraft.Mopups.Animations;

internal static partial class SystemAnimations
{
    /// <summary>
    /// Whether the system plays animations at all. Where it doesn't, a popup's animation would never report its end,
    /// so the popup skips it.
    /// </summary>
    public static partial bool Enabled { get; }
}
