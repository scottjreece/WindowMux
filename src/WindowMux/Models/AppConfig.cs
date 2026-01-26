namespace ScottReece.WindowMux.Models;

/// <summary>
/// Application configuration.
/// </summary>
public sealed class AppConfig
{
    public List<ColorDefinition> Colors { get; set; } = new();
    
    /// <summary>
    /// Whether to run in elevated (administrator) mode.
    /// Null means first run - user hasn't made a choice yet.
    /// </summary>
    public bool? ElevatedMode { get; set; }

    /// <summary>
    /// Saved window position.
    /// </summary>
    public WindowPositionConfig? WindowPosition { get; set; }
}

public class WindowPositionConfig
{
    /// <summary>
    /// 0=TopLeft, 1=TopRight, 2=BottomRight, 3=BottomLeft
    /// </summary>
    public int Corner { get; set; }
    
    public int OffsetX { get; set; }
    public int OffsetY { get; set; }
}
