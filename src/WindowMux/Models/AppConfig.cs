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
}
