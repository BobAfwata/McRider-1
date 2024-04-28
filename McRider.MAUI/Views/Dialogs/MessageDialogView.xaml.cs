namespace McRider.MAUI.Views.Dialogs;

public partial class MessageDialogView : StackLayout
{
    public MessageDialogView(
        Action<BaseDialogViewModel> successAction, Action<BaseDialogViewModel> dismissAction,
        string title, string message,
        string okButtonText = "Ok", string cancelButtonText = null)
    {
        InitializeComponent();

        if (!string.IsNullOrEmpty(message) && App.Current.Resources.TryGetValue(message, out var strValue) && !string.IsNullOrEmpty(strValue?.ToString()))
            message = strValue?.ToString();

        BindingContext = new MessageDialogViewModel(successAction, dismissAction)
        {
            Title = title ?? "Message Title",
            Message = message ?? "Message message...",
            SuccessButtonText = okButtonText,
            CancelButtonText = cancelButtonText,
        };
    }
}