using ScottReece.WindowMux.Interop;
using ScottReece.WindowMux.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace ScottReece.WindowMux.Services;

/// <summary>
/// Manages global hotkeys for color switching (Alt+1, Alt+2, etc.).
/// </summary>
public sealed class HotkeyService : IHotkeyService
{
    private readonly ILogger<HotkeyService> _logger;
    private readonly IConfigService _configService;
    private int _registeredCount;

    public event EventHandler<string>? AssignmentHotkeyPressed;

    public HotkeyService(ILogger<HotkeyService> logger, IConfigService configService)
    {
        _logger = logger;
        _configService = configService;
    }

    public void RegisterHotkeys(IntPtr hwnd)
    {
        var colors = _configService.Colors;
        _registeredCount = Math.Min(colors.Count, 9); // Max 9 hotkeys (Alt+1 through Alt+9)

        uint[] vkCodes = {
            WindowStyles.VK_1, WindowStyles.VK_2, WindowStyles.VK_3,
            WindowStyles.VK_4, WindowStyles.VK_5, WindowStyles.VK_6,
            WindowStyles.VK_7, WindowStyles.VK_8, WindowStyles.VK_9
        };

        for (int i = 0; i < _registeredCount; i++)
        {
            int hotkeyId = i + 1; // IDs 1-9
            if (!NativeMethods.RegisterHotKey(hwnd, hotkeyId, 
                WindowStyles.MOD_ALT | WindowStyles.MOD_NOREPEAT, vkCodes[i]))
            {
                _logger.LogWarning("Failed to register Alt+{Number} hotkey", i + 1);
            }
            else
            {
                _logger.LogDebug("Registered Alt+{Number} for {Color}", i + 1, colors[i].Name);
            }
        }

        _logger.LogInformation("Registered {Count} hotkeys (Alt+1 through Alt+{Max})", 
            _registeredCount, _registeredCount);
    }

    public void UnregisterHotkeys(IntPtr hwnd)
    {
        for (int i = 1; i <= _registeredCount; i++)
        {
            NativeMethods.UnregisterHotKey(hwnd, i);
        }
        _logger.LogInformation("Unregistered {Count} hotkeys", _registeredCount);
    }

    public string? ProcessHotkeyMessage(int hotkeyId)
    {
        var colors = _configService.Colors;
        int index = hotkeyId - 1; // Convert to 0-based index

        if (index >= 0 && index < colors.Count)
        {
            string colorId = colors[index].Id;
            _logger.LogDebug("Hotkey Alt+{Number} pressed for {Color}", hotkeyId, colorId);
            AssignmentHotkeyPressed?.Invoke(this, colorId);
            return colorId;
        }

        return null;
    }
}
