using ScottReece.WindowMux.Interop;
using ScottReece.WindowMux.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace ScottReece.WindowMux.Services;

/// <summary>
/// Controls window state using minimize/restore.
/// </summary>
public sealed class WindowStateController : IWindowStateController
{
    private readonly ILogger<WindowStateController> _logger;

    public WindowStateController(ILogger<WindowStateController> logger)
    {
        _logger = logger;
    }

    public void RestoreWindow(IntPtr hwnd)
    {
        if (!NativeMethods.IsWindow(hwnd))
        {
            _logger.LogWarning("Attempted to restore invalid window handle: {Handle}", hwnd);
            return;
        }

        // Use SW_RESTORE to restore window to its previous position
        bool result = NativeMethods.ShowWindow(hwnd, WindowStyles.SW_RESTORE);
        _logger.LogDebug("Restored window {Handle}, result: {Result}", hwnd, result);
    }

    public void MinimizeWindow(IntPtr hwnd)
    {
        if (!NativeMethods.IsWindow(hwnd))
        {
            _logger.LogWarning("Attempted to minimize invalid window handle: {Handle}", hwnd);
            return;
        }

        bool result = NativeMethods.ShowWindow(hwnd, WindowStyles.SW_MINIMIZE);
        _logger.LogDebug("Minimized window {Handle}, result: {Result}", hwnd, result);
    }

    public bool IsMinimized(IntPtr hwnd)
    {
        if (!NativeMethods.IsWindow(hwnd))
            return false;

        return NativeMethods.IsIconic(hwnd);
    }
}
