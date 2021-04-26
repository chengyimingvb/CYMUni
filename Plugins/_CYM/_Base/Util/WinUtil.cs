//------------------------------------------------------------------------------
// BaseWinUtils.cs
// Copyright 2019 2019/4/14 
// Created by CYM on 2019/4/14
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace CYM
{
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
    public enum GWLWindowStyles : long
    {
        GWL_EXSTYLE = -20,          //Sets a new extended window style.
        GWL_HINSTANCE = -6,         //Sets a new application instance handle.
        GWL_ID = -12,               //Sets a new identifier of the child window. The window cannot be a top-level window.
        GWL_STYLE = -16,            //Sets a new window style.
        GWL_USERDATA = -21,         //Sets the user data associated with the window. This data is intended for use by the application that created the window. Its value is initially zero.
        GWL_WNDPROC = -4            //Sets a new address for the window procedure. You cannot change this attribute if the window does not belong to the same process as the calling thread.
    }

    public enum WindowStyles : long
    {
        WS_BORDER = 0x00800000,         //The window has a thin-line border.
        WS_CAPTION = 0x00C00000,        //The window has a title bar (includes the WS_BORDER style).
        WS_CHILD = 0x40000000,          //The window is a child window. A window with this style cannot have a menu bar. This style cannot be used with the WS_POPUP style.
        WS_CHILDWINDOW = 0x40000000,    //Same as the WS_CHILD style.
        WS_CLIPCHILDREN = 0x02000000,   //Excludes the area occupied by child windows when drawing occurs within the parent window. This style is used when creating the parent window.
        WS_CLIPSIBLINGS = 0x04000000,   //Clips child windows relative to each other; that is, when a particular child window receives a WM_PAINT message, the WS_CLIPSIBLINGS style clips all other overlapping child windows out of the region of the child window to be updated
        WS_DISABLED = 0x08000000,       //The window is initially disabled. A disabled window cannot receive input from the user.
        WS_DLGFRAME = 0x00400000,       //The window has a border of a style typically used with dialog boxes. A window with this style cannot have a title bar.
        WS_GROUP = 0x00020000,          //The window is the first control of a group of controls. The group consists of this first control and all controls defined after it, up to the next control with the WS_GROUP style.
        WS_HSCROLL = 0x00100000,        //The window has a horizontal scroll bar.
        WS_ICONIC = 0x20000000,         //The window is initially minimized. Same as the WS_MINIMIZE style.
        WS_MAXIMIZE = 0x01000000,       //The window is initially maximized.
        WS_MAXIMIZEBOX = 0x00010000,    //The window has a maximize button. Cannot be combined with the WS_EX_CONTEXTHELP style.
        WS_MINIMIZE = 0x20000000,       //The window is initially minimized. Same as the WS_ICONIC style.
        WS_MINIMIZEBOX = 0x00020000,    //The window has a minimize button. Cannot be combined with the WS_EX_CONTEXTHELP style. The WS_SYSMENU style must also be specified.
        WS_OVERLAPPED = 0x00000000,     //The window is an overlapped window. An overlapped window has a title bar and a border. Same as the WS_TILED style.
        WS_OVERLAPPEDWINDOW = 0x00000000 | 0x00C00000 | 0x00080000 | 0x00040000 | 0x00020000 | 0x00010000,  //The window is an overlapped window. Same as the WS_TILEDWINDOW style.
        WS_POPUP = 0x80000000,          //The windows is a pop-up window. This style cannot be used with the WS_CHILD style.
        WS_POPUPWINDOW = 0x80000000 | 0x00800000 | 0x00080000,  //The window is a pop-up window. The WS_CAPTION and WS_POPUPWINDOW styles must be combined to make the window menu visible.
        WS_SIZEBOX = 0x00040000,        //The window has a sizing border. Same as the WS_THICKFRAME style.
        WS_SYSMENU = 0x00080000,        //The window has a window menu on its title bar. The WS_CAPTION style must also be specified.
        WS_TABSTOP = 0x00010000,        //The window is a control that can receive the keyboard focus when the user presses the TAB key. Pressing the TAB key changes the keyboard focus to the next control with the WS_TABSTOP style.
        WS_THICKFRAME = 0x00040000,     //The window has a sizing border. Same as the WS_SIZEBOX style.
        WS_TILED = 0x00000000,          //The window is an overlapped window. An overlapped window has a title bar and a border. Same as the WS_OVERLAPPED style.
        WS_TILEDWINDOW = 0x00000000 | 0x00C00000 | 0x00080000 | 0x00040000 | 0x00020000 | 0x00010000,   //The window is an overlapped window. Same as the WS_OVERLAPPEDWINDOW style.
        WS_VISIBLE = 0x10000000,        //The window is initially visible.
        WS_VSCROLL = 0x00200000         //The window has a vertical scroll bar.
    }

    public enum ExtendedWindowStyles : long
    {
        WS_EX_ACCEPTFILES = 0x00000010,         //The window accepts drag-drop files.
        WS_EX_APPWINDOW = 0x00040000,           //Forces a top-level window onto the taskbar when the window is visible.
        WS_EX_CLIENTEDGE = 0x00000200,          //The window has a border with a sunken edge.
        WS_EX_COMPOSITED = 0x02000000,          //Paints all descendants of a window in bottom-to-top painting order using double-buffering.
        WS_EX_CONTEXTHELP = 0x00000400,         //The title bar of the window includes a question mark. When the user clicks the question mark, the cursor changes to a question mark with a pointer. If the user then clicks a child window, the child receives a WM_HELP message
        WS_EX_CONTROLPARENT = 0x00010000,       //The window itself contains child windows that should take part in dialog box navigation. 
        WS_EX_DLGMODALFRAME = 0x00000001,       //The window has a double border; the window can, optionally, be created with a title bar by specifying the WS_CAPTION style in the dwStyle parameter.
        WS_EX_LAYERED = 0x00080000,             //The window is a layered window.
        WS_EX_LAYOUTRTL = 0x00400000,           //If the shell language is Hebrew, Arabic, or another language that supports reading order alignment, the horizontal origin of the window is on the right edge. Increasing horizontal values advance to the left.
        WS_EX_LEFT = 0x00000000,                //The window has generic left-aligned properties. This is the default.
        WS_EX_LEFTSCROLLBAR = 0x00004000,       //If the shell language is Hebrew, Arabic, or another language that supports reading order alignment, the vertical scroll bar (if present)
        WS_EX_LTRREADING = 0x00000000,          //The window text is displayed using left-to-right reading-order properties. This is the default.
        WS_EX_MDICHILD = 0x00000040,            //The window is a MDI child window.
        WS_EX_NOACTIVATE = 0x08000000,          //A top-level window created with this style does not become the foreground window when the user clicks it. 
        WS_EX_NOINHERITLAYOUT = 0x00100000,     //The window does not pass its window layout to its child windows.
        WS_EX_NOPARENTNOTIFY = 0x00000004,      //The child window created with this style does not send the WM_PARENTNOTIFY message to its parent window when it is created or destroyed.
        WS_EX_NOREDIRECTIONBITMAP = 0x00200000, //The window does not render to a redirection surface. This is for windows that do not have visible content or that use mechanisms other than surfaces to provide their visual.
        WS_EX_OVERLAPPEDWINDOW = 0x00000300,    //The window is an overlapped window.
        WS_EX_PALETTEWINDOW = 0x00000188,       //The window is palette window, which is a modeless dialog box that presents an array of commands.
        WS_EX_RIGHT = 0x00001000,               //The window has generic "right-aligned" properties. This depends on the window class
        WS_EX_RIGHTSCROLLBAR = 0x00000000,      //The vertical scroll bar (if present) is to the right of the client area. This is the default.
        WS_EX_RTLREADING = 0x00002000,          //If the shell language is Hebrew, Arabic, or another language that supports reading-order alignment, the window text is displayed using right-to-left reading-order properties.
        WS_EX_STATICEDGE = 0x00020000,          //The window has a three-dimensional border style intended to be used for items that do not accept user input.
        WS_EX_TOOLWINDOW = 0x00000080,          //The window is intended to be used as a floating toolbar. A tool window has a title bar that is shorter than a normal title bar, and the window title is drawn using a smaller font.
        WS_EX_TOPMOST = 0x00000008,             //The window should be placed above all non-topmost windows and should stay above them, even when the window is deactivated. To add or remove this style, use the SetWindowPos function.
        WS_EX_TRANSPARENT = 0x00000020,         //The window should not be painted until siblings beneath the window (that were created by the same thread) have been painted. The window appears transparent because the bits of underlying sibling windows have already been painted.
        WS_EX_WINDOWEDGE = 0x00000100           //The window has a border with a raised edge.
    }
    public enum WindowToolsFlags : long
    {
        SWP_NOSIZE = 0x0001,        //Binary: 0000 0000 0000 0001
        SWP_NOMOVE = 0x0002,        //Binary: 0000 0000 0000 0010
        SWP_NOZORDER = 0x0004,      //Binary: 0000 0000 0000 0100
        SWP_DRAWFRAME = 0x0020,     //Binary: 0000 0000 0010 0000
        SWP_HIDEWINDOW = 0x0080,    //Binary: 0000 0000 1000 0000
        SWP_NOACTIVATE = 0x0010,    //Binary: 0000 0000 0001 0000
        SWP_SHOWWINDOW = 0x0040     //Binary: 0000 0000 0100 0000
    }

    public enum WindowToolsAlphaFlag : long
    {
        LWA_ALPHA = 0x00000002,       //Binary: 0000 0000 0000 0000 0000 0000 0000 0010
        LWA_COLORKEY = 0x00000001     //Binary: 0000 0000 0000 0000 0000 0000 0000 0001
    }
#endif


    public class WinUtil
    {
        public static void MessageBox(String message, String title)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            MessageBox(IntPtr.Zero, message, title, 0);
#endif
        }

        public static void DisableSysMenuButton()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN    
            if (!UnityEngine.Application.isEditor)
            {
                IntPtr hWindow = GetForegroundWindow();                                                       
                IntPtr hMenu = GetSystemMenu(hWindow, false);
                int count = GetMenuItemCount(hMenu);
                RemoveMenu(hMenu, count - 1, MF_BYPOSITION);
                RemoveMenu(hMenu, count - 2, MF_BYPOSITION);
            }
#endif
        }

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        private const int MF_BYPOSITION = 0x00000400;
        /// <summary>
        /// 取得指定窗口的系统菜单的句柄。
        /// </summary>
        /// <param name="hwnd">指向要获取系统菜单窗口的 <see cref="IntPtr"/> 句柄。</param>
        /// <param name="bRevert">获取系统菜单的方式。设置为 <b>true</b>，表示接收原始的系统菜单，否则设置为 <b>false</b> 。</param>
        /// <returns>指向要获取的系统菜单的 <see cref="IntPtr"/> 句柄。</returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetSystemMenu(IntPtr hwnd, bool bRevert);


        // <summary>
        /// 获取指定的菜单中条目（菜单项）的数量。
        /// </summary>
        /// <param name="hMenu">指向要获取菜单项数量的系统菜单的 <see cref="IntPtr"/> 句柄。</param>
        /// <returns>菜单中的条目数量</returns>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetMenuItemCount(IntPtr hMenu);


        /// <summary>
        /// 删除指定的菜单条目。
        /// </summary>
        /// <param name="hMenu">指向要移除的菜单的 <see cref="IntPtr"/> 。</param>
        /// <param name="uPosition">欲改变的菜单条目的标识符。</param>
        /// <param name="uFlags"></param>
        /// <returns>非零表示成功，零表示失败。</returns>
        /// <remarks>
        /// 如果在 <paramref name="uFlags"/> 中使用了<see cref="MF_BYPOSITION"/> ，则在 <paramref name="uPosition"/> 参数表示菜单项的索引；
        /// 如果在 <paramref name="uFlags"/> 中使用了 <b>MF_BYCOMMAND</b>，则在 <paramref name="uPosition"/> 中使用菜单项的ID。
        /// </remarks>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int RemoveMenu(IntPtr hMenu, int uPosition, int uFlags);

        [DllImport("user32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
        static extern int MessageBox(IntPtr handle, String message, String title, int type);//具体方法
        [DllImport("user32.dll")]
        static extern IntPtr SetWindowLong(IntPtr hwnd, int _nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("user32.dll")]
        static extern long GetWindowLong(IntPtr hwnd, long nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        static extern long GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        static extern bool SetWindowText(IntPtr hwnd, string lpString);

        [DllImport("user32.dll")]
        static extern IntPtr SetWindowLong(IntPtr hwnd, long nIndex, long dwNewLong);

        [DllImport("user32.dll")]
        static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

#endif
    }
}