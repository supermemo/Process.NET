using Process.NET.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Process.NET.Windows
{
  public static class WindowStyling
  {
    private const int GWL_STYLE = -16;
    private const int GWL_EXSTYLE = -20;
    
    private const int SWP_NOSIZE = 0x1;
    private const int SWP_NOMOVE = 0x2;
    private const int SWP_FRAMECHANGED = 0x20;

    public static void MakeWindowTitleless(IntPtr MainWindowHandle)
    {
      // Style
      int style = (int)User32.GetWindowLong32(MainWindowHandle, GWL_STYLE);

      style = style & ~(int)WindowStyles.WS_CAPTION;
      style = style & ~(int)WindowStyles.WS_SYSMENU;
      //style = style & ~(int)WindowStyles.WS_THICKFRAME;
      style = style & ~(int)WindowStyles.WS_MINIMIZE;
      style = style & ~(int)WindowStyles.WS_MAXIMIZEBOX;

      User32.SetWindowLong32(MainWindowHandle, GWL_STYLE, new IntPtr(style));

      // ExStyle
      //style = (int)Kernel32.GetWindowLong32(MainWindowHandle, GWL_EXSTYLE);

      //User32.SetWindowLong32(MainWindowHandle, GWL_EXSTYLE, new IntPtr(style | (int)WindowStyles.WS_EX_DLGMODALFRAME));
      //User32.SetWindowPos(MainWindowHandle, new IntPtr(0), 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_FRAMECHANGED);
    }
  }
}
