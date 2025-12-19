namespace ScottReece.WindowMux.Models;

/// <summary>
/// Defines a single color for workspace switching.
/// </summary>
public sealed class ColorDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int R { get; set; }
    public int G { get; set; }
    public int B { get; set; }

    public System.Drawing.Color ToColor() => System.Drawing.Color.FromArgb(R, G, B);
}
