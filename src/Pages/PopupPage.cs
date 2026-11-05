using System.Diagnostics;
using System.Windows.Input;
using Nkraft.CrossUtility.Extensions;
using Nkraft.Mopups.Animations;
using Nkraft.Mopups.Enums;
using Nkraft.Mopups.Services;

namespace Nkraft.Mopups.Pages;

public class PopupPage : ContentPage
{
    public static readonly BindableProperty IsAnimationEnabledProperty = BindableProperty.Create(
        nameof(IsAnimationEnabled), typeof(bool), typeof(PopupPage), true);

    public static readonly BindableProperty AnimationProperty = BindableProperty.Create(
        nameof(Animation), typeof(IPopupAnimation), typeof(PopupPage),
        defaultValueCreator: _ => new ScaleAnimation());

    public static readonly BindableProperty HasSystemPaddingProperty = BindableProperty.Create(
        nameof(HasSystemPadding), typeof(bool), typeof(PopupPage), true);

    public static readonly BindableProperty SystemPaddingProperty = BindableProperty.Create(
        nameof(SystemPadding), typeof(Thickness), typeof(PopupPage), default(Thickness), BindingMode.OneWayToSource);

    public static readonly BindableProperty SystemPaddingSidesProperty = BindableProperty.Create(
        nameof(SystemPaddingSides), typeof(PaddingSide), typeof(PopupPage), PaddingSide.All);

    public static readonly BindableProperty HasKeyboardOffsetProperty = BindableProperty.Create(
        nameof(HasKeyboardOffset), typeof(bool), typeof(PopupPage), true);

    public static readonly BindableProperty KeyboardOffsetProperty = BindableProperty.Create(
        nameof(KeyboardOffset), typeof(double), typeof(PopupPage), 0d, BindingMode.OneWayToSource);

    public static readonly BindableProperty CloseWhenBackgroundIsClickedProperty = BindableProperty.Create(
        nameof(CloseWhenBackgroundIsClicked), typeof(bool), typeof(PopupPage), true);

    public static readonly BindableProperty BackgroundInputTransparentProperty = BindableProperty.Create(
        nameof(BackgroundInputTransparent), typeof(bool), typeof(PopupPage), false);

    public static readonly BindableProperty BackgroundClickedCommandProperty = BindableProperty.Create(
        nameof(BackgroundClickedCommand), typeof(ICommand), typeof(PopupPage));

    public static readonly BindableProperty BackgroundClickedCommandParameterProperty = BindableProperty.Create(
        nameof(BackgroundClickedCommandParameter), typeof(object), typeof(PopupPage));

    public static readonly BindableProperty DisableAndroidAccessibilityHandlingProperty = BindableProperty.Create(
        nameof(DisableAndroidAccessibilityHandling), typeof(bool), typeof(PopupPage), false);

    private static readonly TimeSpan AnimationTimeout = TimeSpan.FromSeconds(5);

    private IPopupAnimation? _runningAnimation;

    public event EventHandler? BackgroundClicked;

    public bool IsAnimationEnabled
    {
        get => (bool)GetValue(IsAnimationEnabledProperty);
        set => SetValue(IsAnimationEnabledProperty, value);
    }

    public IPopupAnimation? Animation
    {
        get => (IPopupAnimation?)GetValue(AnimationProperty);
        set => SetValue(AnimationProperty, value);
    }

    public bool HasSystemPadding
    {
        get => (bool)GetValue(HasSystemPaddingProperty);
        set => SetValue(HasSystemPaddingProperty, value);
    }

    public Thickness SystemPadding
    {
        get => (Thickness)GetValue(SystemPaddingProperty);
        private set => SetValue(SystemPaddingProperty, value);
    }

    public PaddingSide SystemPaddingSides
    {
        get => (PaddingSide)GetValue(SystemPaddingSidesProperty);
        set => SetValue(SystemPaddingSidesProperty, value);
    }

    public bool HasKeyboardOffset
    {
        get => (bool)GetValue(HasKeyboardOffsetProperty);
        set => SetValue(HasKeyboardOffsetProperty, value);
    }

    public double KeyboardOffset
    {
        get => (double)GetValue(KeyboardOffsetProperty);
        private set => SetValue(KeyboardOffsetProperty, value);
    }

    public bool CloseWhenBackgroundIsClicked
    {
        get => (bool)GetValue(CloseWhenBackgroundIsClickedProperty);
        set => SetValue(CloseWhenBackgroundIsClickedProperty, value);
    }

    public bool BackgroundInputTransparent
    {
        get => (bool)GetValue(BackgroundInputTransparentProperty);
        set => SetValue(BackgroundInputTransparentProperty, value);
    }

    public ICommand? BackgroundClickedCommand
    {
        get => (ICommand?)GetValue(BackgroundClickedCommandProperty);
        set => SetValue(BackgroundClickedCommandProperty, value);
    }

    public object? BackgroundClickedCommandParameter
    {
        get => GetValue(BackgroundClickedCommandParameterProperty);
        set => SetValue(BackgroundClickedCommandParameterProperty, value);
    }

    public bool DisableAndroidAccessibilityHandling
    {
        get => (bool)GetValue(DisableAndroidAccessibilityHandlingProperty);
        set => SetValue(DisableAndroidAccessibilityHandlingProperty, value);
    }

    public PopupPage()
    {
        BackgroundColor = Colors.Transparent;
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        switch (propertyName)
        {
            case nameof(HasSystemPadding):
            case nameof(HasKeyboardOffset):
            case nameof(SystemPaddingSides):
            case nameof(SystemPadding):
            case nameof(KeyboardOffset):
                ApplySystemPadding();
                break;
        }
    }

    protected virtual void OnAppearingAnimationBegin()
    {
    }

    protected virtual void OnAppearingAnimationEnd()
    {
    }

    protected virtual void OnDisappearingAnimationBegin()
    {
    }

    protected virtual void OnDisappearingAnimationEnd()
    {
    }

    protected virtual Task OnAppearingAnimationBeginAsync() => Task.CompletedTask;

    protected virtual Task OnAppearingAnimationEndAsync() => Task.CompletedTask;

    protected virtual Task OnDisappearingAnimationBeginAsync() => Task.CompletedTask;

    protected virtual Task OnDisappearingAnimationEndAsync() => Task.CompletedTask;

    /// <summary>
    /// Whether a tap on the background closes the popup; by default <see cref="CloseWhenBackgroundIsClicked"/>.
    /// </summary>
    protected virtual bool OnBackgroundClicked() => CloseWhenBackgroundIsClicked;

    /// <summary>The platform reports the insets it lays the page out under, in device-independent units.</summary>
    internal void UpdateInsets(Thickness systemPadding, double keyboardOffset)
    {
        SystemPadding = systemPadding;
        KeyboardOffset = keyboardOffset;
    }

    internal void SendBackgroundClick()
    {
        BackgroundClicked?.Invoke(this, EventArgs.Empty);
        if (BackgroundClickedCommand?.CanExecute(BackgroundClickedCommandParameter) == true)
        {
            BackgroundClickedCommand.Execute(BackgroundClickedCommandParameter);
        }

        if (OnBackgroundClicked())
        {
            MopupService.Instance.RemovePageAsync(this)
                .FireAndForget(ex => Trace.TraceError("Closing a popup on a background tap failed: {0}", ex));
        }
    }

    /// <summary>
    /// Before the page goes on screen: the animation that will show it sets its starting state, and stays the one
    /// to restore the page in <see cref="RestoreAnimation"/>, whatever <see cref="Animation"/> is set to by then.
    /// </summary>
    internal void PrepareAnimation(bool animate)
    {
        _runningAnimation = animate ? Animation : null;
        _runningAnimation?.Preparing(Content, this);
    }

    internal async Task AppearAsync()
    {
        OnAppearingAnimationBegin();
        await OnAppearingAnimationBeginAsync();

        if (_runningAnimation is { } animation)
        {
            await RunAsync(animation.AppearingAsync(Content, this));
        }

        OnAppearingAnimationEnd();
        await OnAppearingAnimationEndAsync();
    }

    internal async Task DisappearAsync(bool animate)
    {
        OnDisappearingAnimationBegin();
        await OnDisappearingAnimationBeginAsync();

        if (animate && Animation is { } animation)
        {
            _runningAnimation ??= animation;
            await RunAsync(animation.DisappearingAsync(Content, this));
        }

        OnDisappearingAnimationEnd();
        await OnDisappearingAnimationEndAsync();
    }

    /// <summary>
    /// After the page is off screen: puts back what the animation changed, so the page can be pushed again.
    /// </summary>
    internal void RestoreAnimation()
    {
        _runningAnimation?.Disposing(Content, this);
        _runningAnimation = null;
    }

    /// <summary>
    /// An animation that never ends (the app went to the background and its ticker with it) would otherwise keep the
    /// popup in the stack for good; after <see cref="AnimationTimeout"/> the navigation goes on without it.
    /// </summary>
    private static async Task RunAsync(Task animation)
    {
        try
        {
            await animation.WaitAsync(AnimationTimeout);
        }
        catch (TimeoutException)
        {
            Trace.TraceWarning("A popup animation didn't finish within {0}; going on without it.", AnimationTimeout);
        }
    }

    private void ApplySystemPadding()
    {
        if (!HasSystemPadding && !HasKeyboardOffset)
            return;

        var system = HasSystemPadding ? SystemPadding : default;
        var sides = SystemPaddingSides;

        var left = sides.HasFlag(PaddingSide.Left) ? system.Left : 0;
        var top = sides.HasFlag(PaddingSide.Top) ? system.Top : 0;
        var right = sides.HasFlag(PaddingSide.Right) ? system.Right : 0;
        var bottom = sides.HasFlag(PaddingSide.Bottom) ? system.Bottom : 0;

        if (HasKeyboardOffset)
        {
            bottom = Math.Max(bottom, KeyboardOffset);
        }

        Padding = new Thickness(left, top, right, bottom);
    }
}
