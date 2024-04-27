using CommunityToolkit.Maui.Views;

namespace McRider.MAUI.Views.Dialogs;

public partial class DialogWrapper : Popup
{
    ILogger logger = App.ServiceProvider.GetService<ILogger<DialogWrapper>>();

    public DialogWrapper(View view, BaseDialogViewModel vm)
    {
        InitializeComponent();

#if IOS
        this.VerticalOptions = Microsoft.Maui.Primitives.LayoutAlignment.Start;
#endif

        InitializeSize(view, vm);
    }

    private void InitializeSize(View view, BaseDialogViewModel vm)
    {
        if (vm != null && string.IsNullOrEmpty(vm.SuccessButtonText))
            vm.SuccessButtonText = "Ok";

        var width = 0.75 * DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
        var height = 0.75 * DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;
        var maxBodyHeight = height - Math.Max(100, headerStack.Height) - (vm?.ShowActionButtons == true ? Math.Max(100, footerStack.Height) : 0);

        wrapperBorder.MaximumWidthRequest = Math.Min(400, width);
        bodyScroll.MaximumHeightRequest = Math.Min(600, maxBodyHeight);

        Opened += DialogWrapper_Opened;
        Closed += DialogWrapper_Closed;

        bodyStack.Clear();
        bodyStack.Children.Add(view);

        BindingContext = vm;
    }

    private void DialogWrapper_Closed(object sender, CommunityToolkit.Maui.Core.PopupClosedEventArgs e)
    {
        if (BindingContext is BaseDialogViewModel vm)
            vm.OnDismiss();
    }

    private void DialogWrapper_Opened(object sender, CommunityToolkit.Maui.Core.PopupOpenedEventArgs e)
    {
        if (BindingContext is BaseViewModel vm)
            _ = vm.Initialize();
    }

    private void CloseImageButton_Clicked(object sender, EventArgs e)
    {
        this.TryClose(false);
    }

    private void SuccessButton_Clicked(object sender, EventArgs e)
    {
        if (BindingContext is BaseDialogViewModel vm)
        {
            vm.OnSuccess();
            this.TryClose(true);
        }
        else
        {
            this.TryClose(false);
        }
    }

    private void TryClose(bool ok)
    {
        try
        {
            CloseAsync(ok).Wait();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error Closing Popup!");
        }
    }
}