using ScottReece.WindowMux.Models;

namespace ScottReece.WindowMux.Services.Interfaces;

/// <summary>
/// Manages the registry of windows associated with each color.
/// </summary>
public interface IWindowRegistry
{
    /// <summary>
    /// Initializes the registry with the configured colors.
    /// </summary>
    void Initialize(IReadOnlyList<ColorDefinition> colors);

    /// <summary>
    /// Gets all window handles associated with a specific color ID.
    /// </summary>
    IReadOnlyList<IntPtr> GetWindowsForColor(string colorId);

    /// <summary>
    /// Sets the windows associated with a color ID (replaces existing).
    /// </summary>
    void SetWindowsForColor(string colorId, IEnumerable<IntPtr> windows);

    /// <summary>
    /// Gets all windows from all colors (for master color restore).
    /// </summary>
    IReadOnlyList<IntPtr> GetAllWindows();

    /// <summary>
    /// Removes windows that no longer exist from all color lists.
    /// </summary>
    void CleanupClosedWindows();
}
