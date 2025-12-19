namespace ScottReece.WindowMux.Services.Interfaces;

/// <summary>
/// Monitors for newly created windows.
/// </summary>
public interface INewWindowMonitor
{
    /// <summary>
    /// Starts monitoring for new windows.
    /// </summary>
    /// <param name="overlayHwnd">The overlay window to exclude from detection.</param>
    void Start(IntPtr overlayHwnd);

    /// <summary>
    /// Stops monitoring for new windows.
    /// </summary>
    void Stop();

    /// <summary>
    /// Event raised when a new manageable window is detected.
    /// </summary>
    event EventHandler<IntPtr>? NewWindowDetected;

    /// <summary>
    /// Event raised when a window is restored from minimized state.
    /// </summary>
    event EventHandler<IntPtr>? WindowRestored;
}
