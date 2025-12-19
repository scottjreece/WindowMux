namespace ScottReece.WindowMux.UI;

/// <summary>
/// Custom circular button control for color mode selection.
/// </summary>
public sealed class CircularButton : Control
{
    private bool _isHovered;
    private bool _isPressed;
    private bool _isActive;

    /// <summary>
    /// The color to display for this button.
    /// </summary>
    public Color ButtonColor { get; set; } = Color.Gray;

    /// <summary>
    /// The color ID this button represents.
    /// </summary>
    public string ColorId { get; set; } = string.Empty;

    /// <summary>
    /// The display name for the tooltip.
    /// </summary>
    public string ColorName { get; set; } = string.Empty;

    public bool IsActive
    {
        get => _isActive;
        set
        {
            _isActive = value;
            Invalidate();
        }
    }

    public CircularButton()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.SupportsTransparentBackColor, true);

        Size = new Size(32, 32);
        BackColor = Color.Transparent;
        Cursor = Cursors.Hand;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var buttonColor = ButtonColor;

        // Adjust color based on state
        if (_isPressed)
        {
            buttonColor = ControlPaint.Dark(buttonColor, 0.2f);
        }
        else if (_isHovered)
        {
            buttonColor = ControlPaint.Light(buttonColor, 0.2f);
        }

        // Draw shadow
        using (var shadowBrush = new SolidBrush(Color.FromArgb(50, 0, 0, 0)))
        {
            g.FillEllipse(shadowBrush, 2, 2, Width - 4, Height - 4);
        }

        // Draw main circle
        using (var brush = new SolidBrush(buttonColor))
        {
            g.FillEllipse(brush, 0, 0, Width - 4, Height - 4);
        }

        // Draw active indicator (inner ring)
        if (_isActive)
        {
            using var pen = new Pen(Color.White, 3);
            g.DrawEllipse(pen, 4, 4, Width - 12, Height - 12);
        }

        // Draw subtle border
        using (var pen = new Pen(Color.FromArgb(80, 0, 0, 0), 1))
        {
            g.DrawEllipse(pen, 0, 0, Width - 5, Height - 5);
        }
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        _isHovered = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _isHovered = false;
        _isPressed = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _isPressed = true;
            Invalidate();
        }

        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        _isPressed = false;
        Invalidate();
        base.OnMouseUp(e);
    }
}
