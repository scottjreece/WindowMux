namespace ScottReece.WindowMux.Services.Interfaces;

/// <summary>
/// Manages the active color mode state and orchestrates window visibility.
/// </summary>
public interface IModeStateMachine
{
    /// <summary>
    /// Gets the currently active color ID.
    /// </summary>
    string CurrentColorId { get; }

    /// <summary>
    /// Switches to a new color mode, updating window visibility.
    /// </summary>
    void SwitchMode(string colorId);

    /// <summary>
    /// Sets the overlay window handle so it can be excluded from operations.
    /// </summary>
    void SetOverlayHandle(IntPtr hwnd);

    /// <summary>
    /// Called when a new window is detected (no-op in current design).
    /// </summary>
    void HandleNewWindow(IntPtr hwnd);

    /// <summary>
    /// Called when a window is restored from minimized state (no-op in current design).
    /// </summary>
    void HandleWindowRestored(IntPtr hwnd);

    /// <summary>
    /// Event raised when the mode changes.
    /// </summary>
    event EventHandler<string>? ModeChanged;
}
