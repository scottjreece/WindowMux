using System.Security.Principal;
using System.Diagnostics;

namespace ScottReece.WindowMux.Elevation;

/// <summary>
/// Handles elevation detection and self-relaunch with admin privileges.
/// </summary>
public static class ElevationHelper
{
    /// <summary>
    /// Checks if the current process is running with administrator privileges.
    /// </summary>
    public static bool IsRunningElevated()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    /// <summary>
    /// Relaunches the application with elevated (administrator) privileges.
    /// </summary>
    /// <returns>True if relaunch was initiated, false if user declined UAC.</returns>
    public static bool RelaunchElevated()
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName,
                UseShellExecute = true,
                Verb = "runas"
            };

            if (string.IsNullOrEmpty(startInfo.FileName))
            {
                throw new InvalidOperationException("Could not determine executable path");
            }

            Process.Start(startInfo);
            return true;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            // User declined UAC prompt
            return false;
        }
    }
}
