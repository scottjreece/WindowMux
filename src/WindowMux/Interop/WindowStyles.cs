namespace ScottReece.WindowMux.Interop;

/// <summary>
/// Windows API constants for window styles and commands.
/// </summary>
public static class WindowStyles
{
    // GetWindowLongPtr index
    public const int GWL_EXSTYLE = -20;
    public const int GWL_STYLE = -16;

    // Extended window styles
    public const long WS_EX_TOOLWINDOW = 0x00000080L;
    public const long WS_EX_APPWINDOW = 0x00040000L;
    public const long WS_EX_NOACTIVATE = 0x08000000L;
    public const long WS_EX_TOPMOST = 0x00000008L;

    // Window styles
    public const long WS_VISIBLE = 0x10000000L;
    public const long WS_POPUP = 0x80000000L;
    public const long WS_CHILD = 0x40000000L;

    // ShowWindow commands
    public const int SW_HIDE = 0;
    public const int SW_SHOW = 5;
    public const int SW_MINIMIZE = 6;
    public const int SW_RESTORE = 9;

    // GetWindow commands
    public const uint GW_OWNER = 4;

    // GetAncestor flags
    public const uint GA_ROOT = 2;
    public const uint GA_ROOTOWNER = 3;

    // WinEvent constants
    public const uint EVENT_OBJECT_CREATE = 0x8000;
    public const uint EVENT_OBJECT_SHOW = 0x8002;
    public const uint EVENT_SYSTEM_MINIMIZEEND = 0x0017;
    public const uint EVENT_SYSTEM_FOREGROUND = 0x0003;
    public const uint WINEVENT_OUTOFCONTEXT = 0x0000;
    public const uint WINEVENT_SKIPOWNPROCESS = 0x0002;
    public const int OBJID_WINDOW = 0;

    // Hotkey modifiers
    public const uint MOD_ALT = 0x0001;
    public const uint MOD_CONTROL = 0x0002;
    public const uint MOD_NOREPEAT = 0x4000;

    // Virtual key codes for number keys 1-9
    public const uint VK_1 = 0x31;
    public const uint VK_2 = 0x32;
    public const uint VK_3 = 0x33;
    public const uint VK_4 = 0x34;
    public const uint VK_5 = 0x35;
    public const uint VK_6 = 0x36;
    public const uint VK_7 = 0x37;
    public const uint VK_8 = 0x38;
    public const uint VK_9 = 0x39;

    // Windows messages
    public const int WM_HOTKEY = 0x0312;
}
