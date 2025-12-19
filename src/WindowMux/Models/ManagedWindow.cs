namespace ScottReece.WindowMux.Models;

/// <summary>
/// Represents a window being managed by the workspace switcher.
/// </summary>
public sealed class ManagedWindow
{
    public IntPtr Handle { get; }
    public string Title { get; private set; }
    public string ClassName { get; }
    public string ProcessPath { get; }
    public int ProcessId { get; }

    public ManagedWindow(IntPtr handle, string title, string className, string processPath, int processId)
    {
        Handle = handle;
        Title = title;
        ClassName = className;
        ProcessPath = processPath;
        ProcessId = processId;
    }

    public void UpdateTitle(string newTitle)
    {
        Title = newTitle;
    }

    public override bool Equals(object? obj)
    {
        return obj is ManagedWindow other && Handle == other.Handle;
    }

    public override int GetHashCode()
    {
        return Handle.GetHashCode();
    }
}
