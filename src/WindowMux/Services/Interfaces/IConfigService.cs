using ScottReece.WindowMux.Models;

namespace ScottReece.WindowMux.Services.Interfaces;

/// <summary>
/// Provides access to application configuration.
/// </summary>
public interface IConfigService
{
    /// <summary>
    /// Gets all configured colors (first is always master).
    /// </summary>
    IReadOnlyList<ColorDefinition> Colors { get; }

    /// <summary>
    /// Gets the master color (first in list, shows all windows).
    /// </summary>
    ColorDefinition MasterColor { get; }

    /// <summary>
    /// Gets whether elevation is enabled (null = first run, not chosen).
    /// </summary>
    bool? ElevatedMode { get; }

    /// <summary>
    /// Gets whether this is the first run (user hasn't made elevation choice).
    /// </summary>
    bool IsFirstRun { get; }

    /// <summary>
    /// Checks if the given color ID is the master color.
    /// </summary>
    bool IsMasterColor(string colorId);

    /// <summary>
    /// Saves the user's elevation preference to config.
    /// </summary>
    void SetElevatedMode(bool elevated);

    /// <summary>
    /// Gets the saved window position.
    /// </summary>
    WindowPositionConfig? WindowPosition { get; }

    /// <summary>
    /// Saves the window position to config.
    /// </summary>
    void SaveWindowPosition(WindowPositionConfig position);
}
