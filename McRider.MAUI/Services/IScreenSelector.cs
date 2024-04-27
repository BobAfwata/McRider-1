using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McRider.MAUI.Services;

public interface IScreenSelector
{
    void MoveCurrentToLandscapeScreen(IntPtr? windowHandle = null);
    void MoveCurrentToProtraitScreen(IntPtr? windowHandle = null);
}
