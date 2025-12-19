using ScottReece.WindowMux.Interop;
using ScottReece.WindowMux.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace ScottReece.WindowMux.Services;

/// <summary>
/// Filters windows to determine if they should be managed.
/// </summary>
public sealed class WindowFilterService : IWindowFilterService
{
    private readonly ILogger<WindowFilterService> _logger;

    // System window class names to exclude
    private static readonly HashSet<string> ExcludedClassNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Shell_TrayWnd",           // Taskbar
        "Shell_SecondaryTrayWnd",  // Secondary taskbar
        "Progman",                 // Desktop
        "WorkerW",                 // Desktop worker
        "DV2ControlHost",          // Start menu
        "Windows.UI.Core.CoreWindow", // UWP core windows (may want to manage some)
        "ApplicationFrameWindow",  // Store app frames - we'll handle these specially
        "XamlExplorerHostIslandWindow",
        "ForegroundStaging",
        "EdgeUiInputTopWndClass",
        "EdgeUiInputWndClass",
        "NarratorHelperWindow",
        "NotifyIconOverflowWindow",
        "TopLevelWindowForTouch",
        "InputApp",
    };

    public WindowFilterService(ILogger<WindowFilterService> logger)
    {
        _logger = logger;
    }

    public bool ShouldManage(IntPtr hwnd, IntPtr overlayHwnd)
    {
        // Never manage our own overlay
        if (hwnd == overlayHwnd)
            return false;

        // Must be visible
        if (!NativeMethods.IsWindowVisible(hwnd))
            return false;

        // Must be a top-level window (no owner)
        IntPtr owner = NativeMethods.GetWindow(hwnd, WindowStyles.GW_OWNER);
        if (owner != IntPtr.Zero)
            return false;

        // Check extended styles
        long exStyle = (long)NativeMethods.GetWindowLongPtr(hwnd, WindowStyles.GWL_EXSTYLE);

        // Exclude tool windows (they don't appear in taskbar/Alt+Tab)
        if ((exStyle & WindowStyles.WS_EX_TOOLWINDOW) != 0)
            return false;

        // Get class name and check exclusions
        string className = NativeMethods.GetClassName(hwnd);
        if (string.IsNullOrEmpty(className))
            return false;

        if (ExcludedClassNames.Contains(className))
        {
            _logger.LogTrace("Excluding window with class {ClassName}", className);
            return false;
        }

        // Check for ApplicationFrameWindow (UWP apps) - these are special
        // We want to manage the real app inside, not the frame itself sometimes
        // For now, include them as they appear in Alt+Tab

        // Optional: require a title (can be disabled for apps with empty titles)
        string title = NativeMethods.GetWindowText(hwnd);
        // Some legit apps have empty titles temporarily, so we're lenient here

        _logger.LogTrace("Window qualifies for management: {Title} [{ClassName}]", title, className);
        return true;
    }
}
