using ScottReece.WindowMux.Interop;
using ScottReece.WindowMux.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace ScottReece.WindowMux.Services;

/// <summary>
/// Manages the active color mode and orchestrates window visibility.
/// </summary>
public sealed class ModeStateMachine : IModeStateMachine
{
    private readonly IWindowRegistry _registry;
    private readonly IWindowStateController _stateController;
    private readonly IWindowFilterService _filterService;
    private readonly IConfigService _configService;
    private readonly ILogger<ModeStateMachine> _logger;

    private string _currentColorId;
    private IntPtr _overlayHwnd = IntPtr.Zero;

    public event EventHandler<string>? ModeChanged;

    public string CurrentColorId => _currentColorId;

    public ModeStateMachine(
        IWindowRegistry registry,
        IWindowStateController stateController,
        IWindowFilterService filterService,
        IConfigService configService,
        ILogger<ModeStateMachine> logger)
    {
        _registry = registry;
        _stateController = stateController;
        _filterService = filterService;
        _configService = configService;
        _logger = logger;

        // Initialize with master color
        _currentColorId = _configService.MasterColor.Id;

        // Initialize registry with configured colors
        _registry.Initialize(_configService.Colors);
    }

    public void SetOverlayHandle(IntPtr hwnd)
    {
        _overlayHwnd = hwnd;
        _logger.LogDebug("Overlay handle set to {Handle}", hwnd);
    }

    public void SwitchMode(string newColorId)
    {
        if (string.Equals(_currentColorId, newColorId, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug("Already in {Color} mode, no change needed", newColorId);
            return;
        }

        _logger.LogInformation("Switching from {OldColor} to {NewColor}", _currentColorId, newColorId);

        // Clean up any closed windows first
        _registry.CleanupClosedWindows();

        // Step 1: Capture all currently visible (non-minimized) windows for the OLD color
        var visibleWindows = GetVisibleManagedWindows();
        _registry.SetWindowsForColor(_currentColorId, visibleWindows);
        _logger.LogDebug("Captured {Count} visible windows for {Color}", visibleWindows.Count, _currentColorId);

        // Update current color
        _currentColorId = newColorId;

        // Step 2: Apply window states for the new color
        if (_configService.IsMasterColor(newColorId))
        {
            // Master color: restore ALL windows from ALL colors
            RestoreAllWindows();
        }
        else
        {
            // Regular color: minimize all, then restore only windows for the new color
            MinimizeAllWindows();
            RestoreWindowsForColor(newColorId);
        }

        // Ensure overlay stays on top
        EnsureOverlayTopmost();

        ModeChanged?.Invoke(this, newColorId);
        _logger.LogInformation("Mode switched to {Color}", newColorId);
    }

    private List<IntPtr> GetVisibleManagedWindows()
    {
        var visibleWindows = new List<IntPtr>();

        NativeMethods.EnumWindows((hwnd, _) =>
        {
            if (hwnd == _overlayHwnd)
                return true;

            if (!_filterService.ShouldManage(hwnd, _overlayHwnd))
                return true;

            if (!_stateController.IsMinimized(hwnd))
            {
                visibleWindows.Add(hwnd);
            }

            return true;
        }, IntPtr.Zero);

        return visibleWindows;
    }

    private void RestoreAllWindows()
    {
        var allWindows = _registry.GetAllWindows();
        _logger.LogDebug("Restoring all {Count} windows", allWindows.Count);

        foreach (var hwnd in allWindows)
        {
            if (NativeMethods.IsWindow(hwnd) && hwnd != _overlayHwnd)
            {
                _stateController.RestoreWindow(hwnd);
            }
        }
    }

    private void MinimizeAllWindows()
    {
        NativeMethods.EnumWindows((hwnd, _) =>
        {
            if (hwnd == _overlayHwnd)
                return true;

            if (!_filterService.ShouldManage(hwnd, _overlayHwnd))
                return true;

            if (!_stateController.IsMinimized(hwnd))
            {
                _stateController.MinimizeWindow(hwnd);
            }

            return true;
        }, IntPtr.Zero);

        _logger.LogDebug("Minimized all windows");
    }

    private void RestoreWindowsForColor(string colorId)
    {
        var windows = _registry.GetWindowsForColor(colorId);
        _logger.LogDebug("Restoring {Count} windows for {Color}", windows.Count, colorId);

        foreach (var hwnd in windows)
        {
            if (NativeMethods.IsWindow(hwnd) && hwnd != _overlayHwnd)
            {
                _stateController.RestoreWindow(hwnd);
            }
        }
    }

    public void HandleNewWindow(IntPtr hwnd)
    {
        _logger.LogDebug("New window detected: {Handle}, will be tracked on mode switch", hwnd);
    }

    public void HandleWindowRestored(IntPtr hwnd)
    {
        _logger.LogDebug("Window restored: {Handle}, will be tracked on mode switch", hwnd);
    }

    private void EnsureOverlayTopmost()
    {
        if (_overlayHwnd == IntPtr.Zero)
            return;

        NativeMethods.SetWindowPos(
            _overlayHwnd,
            NativeMethods.HWND_TOPMOST,
            0, 0, 0, 0,
            NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOACTIVATE);

        _logger.LogDebug("Re-asserted topmost for overlay");
    }
}
