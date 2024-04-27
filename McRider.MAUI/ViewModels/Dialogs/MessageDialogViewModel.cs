namespace McRider.MAUI.ViewModels.Dialogs;

public partial class MessageDialogViewModel : BaseDialogViewModel
{
    public MessageDialogViewModel(Action<BaseDialogViewModel> successAction, Action<BaseDialogViewModel> dismissAction)
        : base(successAction, dismissAction) { }

    public MessageDialogViewModel()
        : this(null, null) { }

    [ObservableProperty]
    string _message;

    public override bool IsValid => true;
}
