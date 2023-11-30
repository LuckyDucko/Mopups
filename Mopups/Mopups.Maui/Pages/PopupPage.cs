using AsyncAwaitBestPractices;
using Mopups.Animations;
using Mopups.Animations.Base;
using Mopups.Enums;
using Mopups.Services;
using System.Windows.Input;

namespace Mopups.Pages;

public partial class PopupPage : ContentPage
{



    public event EventHandler? BackgroundClicked;

    internal Task? AppearingTransactionTask { get; set; }

    internal Task? DisappearingTransactionTask { get; set; }

    public static readonly BindableProperty IsAnimationEnabledProperty = BindableProperty.Create(nameof(IsAnimationEnabled), typeof(bool), typeof(PopupPage), true);

    public bool IsAnimationEnabled
    {
        get => (bool)GetValue(IsAnimationEnabledProperty) && AnimationHelper.SystemAnimationsEnabled;
        set => SetValue(IsAnimationEnabledProperty, value);
    }

    public static readonly BindableProperty HasSystemPaddingProperty = BindableProperty.Create(nameof(HasSystemPadding), typeof(bool), typeof(PopupPage), true);

    public bool HasSystemPadding
    {
        get => (bool)GetValue(HasSystemPaddingProperty);
        set => SetValue(HasSystemPaddingProperty, value);
    }

    public static readonly BindableProperty AnimationProperty = BindableProperty.Create(nameof(Animation), typeof(IPopupAnimation), typeof(PopupPage), new ScaleAnimation());

    public IPopupAnimation Animation
    {
        get => (IPopupAnimation)GetValue(AnimationProperty);
        set => SetValue(AnimationProperty, value);
    }

    public static readonly BindableProperty SystemPaddingProperty = BindableProperty.Create(nameof(SystemPadding), typeof(Thickness), typeof(PopupPage), default(Thickness), BindingMode.OneWayToSource);

    public Thickness SystemPadding
    {
        get => (Thickness)GetValue(SystemPaddingProperty);
        internal set => SetValue(SystemPaddingProperty, value);
    }

    public static readonly BindableProperty SystemPaddingSidesProperty = BindableProperty.Create(nameof(SystemPaddingSides), typeof(PaddingSide), typeof(PopupPage), PaddingSide.All);

    public PaddingSide SystemPaddingSides
    {
        get => (PaddingSide)GetValue(SystemPaddingSidesProperty);
        set => SetValue(SystemPaddingSidesProperty, value);
    }

    public static readonly BindableProperty CloseWhenBackgroundIsClickedProperty = BindableProperty.Create(nameof(CloseWhenBackgroundIsClicked), typeof(bool), typeof(PopupPage), true);

    public bool CloseWhenBackgroundIsClicked
    {
        get => (bool)GetValue(CloseWhenBackgroundIsClickedProperty);
        set => SetValue(CloseWhenBackgroundIsClickedProperty, value);
    }

    public static readonly BindableProperty BackgroundInputTransparentProperty = BindableProperty.Create(nameof(BackgroundInputTransparent), typeof(bool), typeof(PopupPage), false);

    public bool BackgroundInputTransparent
    {
        get => (bool)GetValue(BackgroundInputTransparentProperty);
        set => SetValue(BackgroundInputTransparentProperty, value);
    }

    public static readonly BindableProperty HasKeyboardOffsetProperty = BindableProperty.Create(nameof(HasKeyboardOffset), typeof(bool), typeof(PopupPage), true);

    public bool HasKeyboardOffset
    {
        get => (bool)GetValue(HasKeyboardOffsetProperty);
        set => SetValue(HasKeyboardOffsetProperty, value);
    }

    public static readonly BindableProperty KeyboardOffsetProperty = BindableProperty.Create(nameof(KeyboardOffset), typeof(double), typeof(PopupPage), 0d, BindingMode.OneWayToSource);

    public double KeyboardOffset
    {
        get => (double)GetValue(KeyboardOffsetProperty);
        private set => SetValue(KeyboardOffsetProperty, value);
    }

    public static readonly BindableProperty BackgroundClickedCommandProperty = BindableProperty.Create(nameof(BackgroundClickedCommand), typeof(ICommand), typeof(PopupPage));

    public ICommand BackgroundClickedCommand
    {
        get => (ICommand)GetValue(BackgroundClickedCommandProperty);
        set => SetValue(BackgroundClickedCommandProperty, value);
    }

    public static readonly BindableProperty BackgroundClickedCommandParameterProperty = BindableProperty.Create(nameof(BackgroundClickedCommandParameter), typeof(object), typeof(PopupPage));

    public object BackgroundClickedCommandParameter
    {
        get => GetValue(BackgroundClickedCommandParameterProperty);
        set => SetValue(BackgroundClickedCommandParameterProperty, value);
    }

    public static readonly BindableProperty DisableAndroidAccessibilityHandlingProperty = BindableProperty.Create(nameof(DisableAndroidAccessibilityHandling), typeof(bool), typeof(PopupPage), false);

    public bool DisableAndroidAccessibilityHandling
    {
        get => (bool)GetValue(DisableAndroidAccessibilityHandlingProperty);
        set => SetValue(DisableAndroidAccessibilityHandlingProperty, value);
    }

    public PopupPage()
    {
        //BackgroundColor = Color.FromArgb("#80000000");
    }

    protected override bool OnBackButtonPressed()
    {
        return false;
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
                ForceLayout();
                break;
                //case nameof(IsAnimating):
                //    IsAnimationEnabled = IsAnimating;
                //    break;
                //case nameof(IsAnimationEnabled):
                //    IsAnimating = IsAnimationEnabled;
                //    break;
        }
    }

    protected override Size ArrangeOverride(Rect bounds)
    {
        return base.ArrangeOverride(bounds);
    }

    protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
    {
        return base.MeasureOverride(widthConstraint, heightConstraint);
    }



    /// <summary>
    /// LayoutChildren is not working.. Maui Bug???
    /// </summary>


    //protected override void LayoutChildren(double x, double y, double width, double height)
    //{
    //    if (HasSystemPadding)
    //    {
    //        var systemPadding = SystemPadding;
    //        var systemPaddingSide = SystemPaddingSides;
    //        var left = 0d;
    //        var top = 0d;
    //        var right = 0d;
    //        var bottom = 0d;

    //        if (systemPaddingSide.HasFlag(PaddingSide.Left))
    //            left = systemPadding.Left;
    //        if (systemPaddingSide.HasFlag(PaddingSide.Top))
    //            top = systemPadding.Top;
    //        if (systemPaddingSide.HasFlag(PaddingSide.Right))
    //            right = systemPadding.Right;
    //        if (systemPaddingSide.HasFlag(PaddingSide.Bottom))
    //            bottom = systemPadding.Bottom;

    //        x += left;
    //        y += top;
    //        width -= left + right;

    //        if (HasKeyboardOffset)
    //            height -= top + Math.Max(bottom, KeyboardOffset);
    //        else
    //            height -= top + bottom;
    //    }
    //    else if (HasKeyboardOffset)
    //    {
    //        height -= KeyboardOffset;
    //    }

    //    base.LayoutChildren(x, y, width, height);
    //}


    #region Animation Methods

    internal void PreparingAnimation()
    {
        if (IsAnimationEnabled)
            Animation?.Preparing(Content, this);
    }

    internal void DisposingAnimation()
    {
        if (IsAnimationEnabled)
            Animation?.Disposing(Content, this);
    }

    internal async Task AppearingAnimation()
    {
        OnAppearingAnimationBegin();
        await OnAppearingAnimationBeginAsync();

        if (IsAnimationEnabled && Animation != null)
            await Animation.Appearing(Content, this);

        OnAppearingAnimationEnd();
        await OnAppearingAnimationEndAsync();
    }

    internal async Task DisappearingAnimation()
    {
        OnDisappearingAnimationBegin();
        await OnDisappearingAnimationBeginAsync();

        if (IsAnimationEnabled && Animation != null)
            await Animation.Disappearing(Content, this);

        OnDisappearingAnimationEnd();
        await OnDisappearingAnimationEndAsync();
    }

    #endregion

    #region Override Animation Methods

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

    protected virtual Task OnAppearingAnimationBeginAsync()
    {
        return Task.FromResult(0);
    }

    protected virtual Task OnAppearingAnimationEndAsync()
    {
        return Task.FromResult(0);
    }

    protected virtual Task OnDisappearingAnimationBeginAsync()
    {
        return Task.FromResult(0);
    }

    protected virtual Task OnDisappearingAnimationEndAsync()
    {
        return Task.FromResult(0);
    }

    #endregion

    protected virtual bool OnBackgroundClicked()
    {
        return CloseWhenBackgroundIsClicked;
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
            MopupService.Instance.RemovePageAsync(this).SafeFireAndForget();
        }
    }
}
