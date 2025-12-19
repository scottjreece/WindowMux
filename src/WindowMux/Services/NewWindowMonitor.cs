using ScottReece.WindowMux.Interop;
using ScottReece.WindowMux.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace ScottReece.WindowMux.Services;

/// <summary>
/// Monitors for newly created windows using SetWinEventHook.
/// </summary>
public sealed class NewWindowMonitor : INewWindowMonitor, IDisposable
{
    private readonly IWindowFilterService _filterService;
    private readonly ILogger<NewWindowMonitor> _logger;

    private IntPtr _overlayHwnd;
    private IntPtr _showHook;
    private IntPtr _restoreHook;
    private NativeMethods.WinEventProc? _showHookProc;
    private NativeMethods.WinEventProc? _restoreHookProc;
    private readonly HashSet<IntPtr> _recentlyProcessed = new();
    private readonly object _lock = new();

    public event EventHandler<IntPtr>? NewWindowDetected;
    public event EventHandler<IntPtr>? WindowRestored;

    public NewWindowMonitor(IWindowFilterService filterService, ILogger<NewWindowMonitor> logger)
    {
        _filterService = filterService;
        _logger = logger;
    }

    public void Start(IntPtr overlayHwnd)
    {
        _overlayHwnd = overlayHwnd;

        // Keep references to prevent garbage collection
        _showHookProc = ShowEventCallback;
        _restoreHookProc = RestoreEventCallback;

        // Hook for new windows (EVENT_OBJECT_SHOW)
        _showHook = NativeMethods.SetWinEventHook(
            WindowStyles.EVENT_OBJECT_SHOW,
            WindowStyles.EVENT_OBJECT_SHOW,
            IntPtr.Zero,
            _showHookProc,
            0, // All processes
            0, // All threads
            WindowStyles.WINEVENT_OUTOFCONTEXT | WindowStyles.WINEVENT_SKIPOWNPROCESS
        );

        if (_showHook == IntPtr.Zero)
        {
            _logger.LogError("Failed to set show WinEventHook");
            throw new InvalidOperationException("Failed to install window monitoring hook");
        }

        // Hook for restored windows (EVENT_SYSTEM_MINIMIZEEND)
        _restoreHook = NativeMethods.SetWinEventHook(
            WindowStyles.EVENT_SYSTEM_MINIMIZEEND,
            WindowStyles.EVENT_SYSTEM_MINIMIZEEND,
            IntPtr.Zero,
            _restoreHookProc,
            0,
            0,
            WindowStyles.WINEVENT_OUTOFCONTEXT | WindowStyles.WINEVENT_SKIPOWNPROCESS
        );

        if (_restoreHook == IntPtr.Zero)
        {
            _logger.LogWarning("Failed to set restore WinEventHook");
            // Non-fatal, continue with show hook only
        }

        _logger.LogInformation("Window monitor started");
    }

    public void Stop()
    {
        if (_showHook != IntPtr.Zero)
        {
            NativeMethods.UnhookWinEvent(_showHook);
            _showHook = IntPtr.Zero;
            _showHookProc = null;
        }

        if (_restoreHook != IntPtr.Zero)
        {
            NativeMethods.UnhookWinEvent(_restoreHook);
            _restoreHook = IntPtr.Zero;
            _restoreHookProc = null;
        }

        _logger.LogInformation("Window monitor stopped");
    }

    private void ShowEventCallback(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild,
        uint dwEventThread, uint dwmsEventTime)
    {
        // Only care about window object events
        if (idObject != WindowStyles.OBJID_WINDOW || idChild != 0)
            return;

        if (hwnd == IntPtr.Zero || hwnd == _overlayHwnd)
            return;

        // Debounce: avoid processing the same window multiple times
        lock (_lock)
        {
            if (_recentlyProcessed.Contains(hwnd))
                return;

            // Check if this is a manageable window
            if (!_filterService.ShouldManage(hwnd, _overlayHwnd))
                return;

            _recentlyProcessed.Add(hwnd);

            // Clean up old entries periodically
            if (_recentlyProcessed.Count > 100)
            {
                var toRemove = _recentlyProcessed
                    .Where(h => !NativeMethods.IsWindow(h))
                    .ToList();
                foreach (var h in toRemove)
                    _recentlyProcessed.Remove(h);
            }
        }

        _logger.LogDebug("New window detected: {Handle}", hwnd);
        NewWindowDetected?.Invoke(this, hwnd);
    }

    private void RestoreEventCallback(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild,
        uint dwEventThread, uint dwmsEventTime)
    {
        if (hwnd == IntPtr.Zero || hwnd == _overlayHwnd)
            return;

        // Check if this is a manageable window
        if (!_filterService.ShouldManage(hwnd, _overlayHwnd))
            return;

        _logger.LogDebug("Window restored: {Handle}", hwnd);
        WindowRestored?.Invoke(this, hwnd);
    }

    public void Dispose()
    {
        Stop();
    }
}
