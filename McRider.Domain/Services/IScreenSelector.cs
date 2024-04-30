namespace McRider.Domain.Services;

public interface IScreenSelector
{
    bool MoveCurrentToLandscapeScreen(IntPtr? windowHandle = null);
    bool MoveCurrentToProtraitScreen(IntPtr? windowHandle = null);
}
