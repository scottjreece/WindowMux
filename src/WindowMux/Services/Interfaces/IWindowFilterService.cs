namespace ScottReece.WindowMux.Services.Interfaces;

/// <summary>
/// Filters windows to determine if they should be managed.
/// </summary>
public interface IWindowFilterService
{
    /// <summary>
    /// Determines if a window should be managed by the workspace switcher.
    /// </summary>
    /// <param name="hwnd">The window handle to check.</param>
    /// <param name="overlayHwnd">The overlay window to exclude.</param>
    /// <returns>True if the window should be managed.</returns>
    bool ShouldManage(IntPtr hwnd, IntPtr overlayHwnd);
}
