using McRider.MAUI.Services;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace McRider.MAUI.Platforms.Windows;

public class WindowsScreenSelector: IScreenSelector
{
    public bool MoveCurrentToLandscapeScreen(IntPtr? windowHandle = null)
    {
        windowHandle ??= GetForegroundWindow();
        var landscapeScreen = Screen.AllScreens.FirstOrDefault(screen => screen.Bounds.Width > screen.Bounds.Height);

        if (landscapeScreen != null)
            MoveWindowToScreen(windowHandle.Value, landscapeScreen);

        return landscapeScreen != null;
    }

    public bool MoveCurrentToProtraitScreen(IntPtr? windowHandle = null)
    {
        windowHandle ??= GetForegroundWindow();
        var portraitScreen = Screen.AllScreens.FirstOrDefault(screen => screen.Bounds.Width < screen.Bounds.Height);

        if (portraitScreen != null)
            MoveWindowToScreen(windowHandle.Value, portraitScreen);

        return portraitScreen != null;

    }

    private void MoveWindowToScreen(IntPtr windowHandle, Screen screen)
    {
        var workingArea = screen.WorkingArea;
        MoveWindow(windowHandle, workingArea.Left, workingArea.Top, workingArea.Width, workingArea.Height, true);
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int Width, int Height, bool Repaint);

}
