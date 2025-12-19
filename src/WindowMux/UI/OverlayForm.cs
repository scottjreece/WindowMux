using ScottReece.WindowMux.Interop;
using ScottReece.WindowMux.Models;
using ScottReece.WindowMux.Services.Interfaces;

namespace ScottReece.WindowMux.UI;

/// <summary>
/// Main overlay window with color mode buttons.
/// </summary>
public sealed class OverlayForm : Form
{
    private readonly IModeStateMachine _modeStateMachine;
    private readonly IHotkeyService _hotkeyService;
    private readonly INewWindowMonitor _windowMonitor;
    private readonly IConfigService _configService;

    private readonly List<CircularButton> _colorButtons = new();
    private readonly Button _closeButton;
    private readonly ToolTip _buttonToolTip = new();

    private Point _dragStartPoint;
    private bool _isDragging;

    private const int ButtonSize = 32;
    private const int ButtonSpacing = 36;
    private const int DragHandleWidth = 24;
    private const int FormPadding = 8;

    public OverlayForm(
        IModeStateMachine modeStateMachine,
        IHotkeyService hotkeyService,
        INewWindowMonitor windowMonitor,
        IConfigService configService)
    {
        _modeStateMachine = modeStateMachine;
        _hotkeyService = hotkeyService;
        _windowMonitor = windowMonitor;
        _configService = configService;

        InitializeForm();

        // Create color buttons dynamically from config
        // ShowAlways required because our form uses WS_EX_NOACTIVATE
        _buttonToolTip.ShowAlways = true;
        int x = FormPadding;
        foreach (var color in _configService.Colors)
        {
            var button = CreateButton(color, x);
            _colorButtons.Add(button);
            Controls.Add(button);
            _buttonToolTip.SetToolTip(button, color.Name);
            x += ButtonSpacing;
        }

        // Add drag handle space, then close button
        x += DragHandleWidth;
        _closeButton = CreateCloseButton(x);
        Controls.Add(_closeButton);

        // Set initial active state
        UpdateActiveButton(_modeStateMachine.CurrentColorId);

        // Subscribe to mode changes
        _modeStateMachine.ModeChanged += OnModeChanged;
    }

    private void InitializeForm()
    {
        Text = "WindowMux";
        FormBorderStyle = FormBorderStyle.None;
        TopMost = true;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;

        // Calculate form width based on number of colors
        int colorCount = _configService.Colors.Count;
        int formWidth = FormPadding + (colorCount * ButtonSpacing) + DragHandleWidth + ButtonSize + FormPadding;

        // Position in top-right corner with some margin
        var screenBounds = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1920, 1080);
        Location = new Point(screenBounds.Right - formWidth - 10, screenBounds.Top + 10);

        Size = new Size(formWidth, 48);
        BackColor = Color.FromArgb(40, 44, 52); // Dark background

        // Make corners rounded
        Region = CreateRoundedRegion(Width, Height, 24);

        // Enable drag
        MouseDown += OnFormMouseDown;
        MouseMove += OnFormMouseMove;
        MouseUp += OnFormMouseUp;
    }

    private static Region CreateRoundedRegion(int width, int height, int radius)
    {
        using var path = new System.Drawing.Drawing2D.GraphicsPath();
        path.AddArc(0, 0, radius, radius, 180, 90);
        path.AddArc(width - radius, 0, radius, radius, 270, 90);
        path.AddArc(width - radius, height - radius, radius, radius, 0, 90);
        path.AddArc(0, height - radius, radius, radius, 90, 90);
        path.CloseAllFigures();
        return new Region(path);
    }

    private CircularButton CreateButton(ColorDefinition colorDef, int x)
    {
        var button = new CircularButton
        {
            ColorId = colorDef.Id,
            ColorName = colorDef.Name,
            ButtonColor = colorDef.ToColor(),
            Location = new Point(x, 8),
            Size = new Size(ButtonSize, ButtonSize)
        };

        button.Click += (_, _) => OnColorButtonClick(colorDef.Id);
        return button;
    }

    private Button CreateCloseButton(int x)
    {
        var button = new Button
        {
            Location = new Point(x, 8),
            Size = new Size(ButtonSize, ButtonSize),
            FlatStyle = FlatStyle.Flat,
            Text = "✕",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            ForeColor = Color.FromArgb(200, 200, 200),
            BackColor = Color.FromArgb(60, 64, 72),
            Cursor = Cursors.Hand
        };

        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = Color.FromArgb(200, 60, 60);
        button.FlatAppearance.MouseDownBackColor = Color.FromArgb(180, 40, 40);

        button.Click += (_, _) => Application.Exit();
        return button;
    }

    private void OnColorButtonClick(string colorId)
    {
        _modeStateMachine.SwitchMode(colorId);
    }

    private void OnModeChanged(object? sender, string colorId)
    {
        if (InvokeRequired)
        {
            Invoke(() => UpdateActiveButton(colorId));
        }
        else
        {
            UpdateActiveButton(colorId);
        }
    }

    private void UpdateActiveButton(string colorId)
    {
        foreach (var button in _colorButtons)
        {
            button.IsActive = string.Equals(button.ColorId, colorId, StringComparison.OrdinalIgnoreCase);
        }
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        // Set overlay handle so it can be excluded
        _modeStateMachine.SetOverlayHandle(Handle);

        // Register Alt+1, Alt+2, etc. hotkeys
        _hotkeyService.RegisterHotkeys(Handle);

        // Start window monitor
        _windowMonitor.Start(Handle);
        _windowMonitor.NewWindowDetected += OnNewWindowDetected;
        _windowMonitor.WindowRestored += OnWindowRestored;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _windowMonitor.NewWindowDetected -= OnNewWindowDetected;
        _windowMonitor.WindowRestored -= OnWindowRestored;
        _windowMonitor.Stop();
        _hotkeyService.UnregisterHotkeys(Handle);
        _buttonToolTip.Dispose();

        base.OnFormClosing(e);
    }

    private void OnNewWindowDetected(object? sender, IntPtr hwnd)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => _modeStateMachine.HandleNewWindow(hwnd));
        }
        else
        {
            _modeStateMachine.HandleNewWindow(hwnd);
        }
    }

    private void OnWindowRestored(object? sender, IntPtr hwnd)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => _modeStateMachine.HandleWindowRestored(hwnd));
        }
        else
        {
            _modeStateMachine.HandleWindowRestored(hwnd);
        }
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WindowStyles.WM_HOTKEY)
        {
            int hotkeyId = m.WParam.ToInt32();
            var colorId = _hotkeyService.ProcessHotkeyMessage(hotkeyId);
            if (colorId != null)
            {
                _modeStateMachine.SwitchMode(colorId);
            }
        }

        base.WndProc(ref m);
    }

    // Drag handling
    private void OnFormMouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _isDragging = true;
            _dragStartPoint = e.Location;
        }
    }

    private void OnFormMouseMove(object? sender, MouseEventArgs e)
    {
        if (_isDragging)
        {
            Location = new Point(
                Location.X + e.X - _dragStartPoint.X,
                Location.Y + e.Y - _dragStartPoint.Y
            );
        }
    }

    private void OnFormMouseUp(object? sender, MouseEventArgs e)
    {
        _isDragging = false;
    }

    // Prevent stealing focus
    protected override bool ShowWithoutActivation => true;

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            // WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW to prevent focus stealing and taskbar icon
            cp.ExStyle |= 0x08000000 | 0x00000080;
            return cp;
        }
    }
}
