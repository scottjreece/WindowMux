using ScottReece.WindowMux.Interop;
using ScottReece.WindowMux.Models;
using ScottReece.WindowMux.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace ScottReece.WindowMux.Services;

/// <summary>
/// Thread-safe registry of windows associated with each color.
/// </summary>
public sealed class WindowRegistry : IWindowRegistry
{
    private readonly Dictionary<string, HashSet<IntPtr>> _windowsByColor = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lock = new();
    private readonly ILogger<WindowRegistry> _logger;

    public WindowRegistry(ILogger<WindowRegistry> logger)
    {
        _logger = logger;
    }

    public void Initialize(IReadOnlyList<ColorDefinition> colors)
    {
        lock (_lock)
        {
            _windowsByColor.Clear();
            foreach (var color in colors)
            {
                _windowsByColor[color.Id] = new HashSet<IntPtr>();
            }

            _logger.LogInformation("Initialized registry with {Count} colors", colors.Count);
        }
    }

    public IReadOnlyList<IntPtr> GetWindowsForColor(string colorId)
    {
        lock (_lock)
        {
            if (_windowsByColor.TryGetValue(colorId, out var windows))
            {
                return windows.ToList();
            }

            return Array.Empty<IntPtr>();
        }
    }

    public void SetWindowsForColor(string colorId, IEnumerable<IntPtr> windows)
    {
        lock (_lock)
        {
            if (!_windowsByColor.ContainsKey(colorId))
            {
                _windowsByColor[colorId] = new HashSet<IntPtr>();
            }

            _windowsByColor[colorId] = new HashSet<IntPtr>(windows);
            _logger.LogInformation("Set {Count} windows for {Color}",
                _windowsByColor[colorId].Count, colorId);
        }
    }

    public IReadOnlyList<IntPtr> GetAllWindows()
    {
        lock (_lock)
        {
            var allWindows = new HashSet<IntPtr>();
            foreach (var set in _windowsByColor.Values)
            {
                foreach (var hwnd in set)
                {
                    allWindows.Add(hwnd);
                }
            }

            return allWindows.ToList();
        }
    }

    public void CleanupClosedWindows()
    {
        lock (_lock)
        {
            int totalRemoved = 0;
            foreach (var (colorId, windows) in _windowsByColor)
            {
                var closedWindows = windows
                    .Where(hwnd => !NativeMethods.IsWindow(hwnd))
                    .ToList();

                foreach (var hwnd in closedWindows)
                {
                    windows.Remove(hwnd);
                    totalRemoved++;
                }
            }

            if (totalRemoved > 0)
            {
                _logger.LogInformation("Cleaned up {Count} closed windows", totalRemoved);
            }
        }
    }
}
