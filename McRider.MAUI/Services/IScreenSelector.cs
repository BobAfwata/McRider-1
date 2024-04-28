using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McRider.MAUI.Services;

public interface IScreenSelector
{
    bool MoveCurrentToLandscapeScreen(IntPtr? windowHandle = null);
    bool MoveCurrentToProtraitScreen(IntPtr? windowHandle = null);
}
