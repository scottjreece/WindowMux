namespace ScottReece.WindowMux.Services.Interfaces;

/// <summary>
/// Controls window state (minimize/restore).
/// </summary>
public interface IWindowStateController
{
    /// <summary>
    /// Restores a window to its previous position and size.
    /// </summary>
    void RestoreWindow(IntPtr hwnd);

    /// <summary>
    /// Minimizes a window to the taskbar.
    /// </summary>
    void MinimizeWindow(IntPtr hwnd);

    /// <summary>
    /// Checks if a window is currently minimized.
    /// </summary>
    bool IsMinimized(IntPtr hwnd);
}
