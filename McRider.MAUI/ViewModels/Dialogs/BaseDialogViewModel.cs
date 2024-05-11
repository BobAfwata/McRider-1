namespace McRider.MAUI.ViewModels.Dialogs;

public abstract partial class BaseDialogViewModel : BaseViewModel
{
    [ObservableProperty]
    Action<BaseDialogViewModel> _successAction;

    [ObservableProperty]
    Action<BaseDialogViewModel> _dismissAction;

    [ObservableProperty]
    bool _showActionButtons = true;

    public BaseDialogViewModel(Action<BaseDialogViewModel> successAction, Action<BaseDialogViewModel> dismissAction)
    {
        _successAction = successAction;
        _dismissAction = dismissAction;

        Title = "McRider Dialog";
        Revalidate = new Action(() => OnPropertyChanged(nameof(IsValid))).Debounce(90);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(IsValid))
            Revalidate?.Invoke();

        base.OnPropertyChanged(e);
    }

    public abstract bool IsValid { get; }

    public Action Revalidate { get; private set; }

    [ObservableProperty]
    public string _cancelButtonText = "Cancel";

    [ObservableProperty]
    public string _successButtonText = "Ok";

    [RelayCommand]
    public virtual void OnSuccess() => _successAction?.Invoke(this); 

    [RelayCommand]
    public virtual void OnDismiss() => _dismissAction?.Invoke(this);
}
