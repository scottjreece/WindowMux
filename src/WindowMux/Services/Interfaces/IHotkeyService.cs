namespace ScottReece.WindowMux.Services.Interfaces;

/// <summary>
/// Manages global hotkeys for color assignment.
/// </summary>
public interface IHotkeyService
{
    /// <summary>
    /// Registers global hotkeys for the application.
    /// </summary>
    /// <param name="hwnd">The window handle to receive hotkey messages.</param>
    void RegisterHotkeys(IntPtr hwnd);

    /// <summary>
    /// Unregisters all global hotkeys.
    /// </summary>
    /// <param name="hwnd">The window handle that was used for registration.</param>
    void UnregisterHotkeys(IntPtr hwnd);

    /// <summary>
    /// Processes a hotkey message and returns the assigned color ID if applicable.
    /// </summary>
    string? ProcessHotkeyMessage(int hotkeyId);

    /// <summary>
    /// Event raised when an assignment hotkey is pressed.
    /// </summary>
    event EventHandler<string>? AssignmentHotkeyPressed;
}
