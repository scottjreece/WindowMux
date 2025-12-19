using ScottReece.WindowMux.Elevation;
using ScottReece.WindowMux.Logging;
using ScottReece.WindowMux.Services;
using ScottReece.WindowMux.Services.Interfaces;
using ScottReece.WindowMux.UI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ScottReece.WindowMux;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        // Initialize WinForms first (needed for dialogs)
        ApplicationConfiguration.Initialize();

        // Configure logging
        var logPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ScottReece.WindowMux",
            "log.txt");

        // Build DI container
        var services = new ServiceCollection();
        ConfigureServices(services, logPath);
        using var serviceProvider = services.BuildServiceProvider();

        // Get config service to check elevation preference
        var configService = serviceProvider.GetRequiredService<IConfigService>();

        // Handle first run - ask user about elevation
        if (configService.IsFirstRun)
        {
            var choice = ShowFirstRunDialog();
            configService.SetElevatedMode(choice);
        }

        // Handle elevation if enabled
        if (configService.ElevatedMode == true && !ElevationHelper.IsRunningElevated())
        {
            if (ElevationHelper.RelaunchElevated())
            {
                return; // Exit this instance, elevated one will start
            }
            else
            {
                MessageBox.Show(
                    "Failed to obtain administrator privileges.\n\nThe application will run in non-elevated mode.\nSome windows from elevated apps may not be manageable.",
                    "Running Without Elevation",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        // Create and run the overlay form
        var form = serviceProvider.GetRequiredService<OverlayForm>();
        Application.Run(form);
    }

    private static bool ShowFirstRunDialog()
    {
        const string message = """
            Welcome to WindowMux!

            Would you like to run in Elevated (Administrator) mode?

            ELEVATED MODE (Yes):
            ✓ Can manage ALL windows including admin apps
            ✓ Works with Task Manager, admin terminals, etc.
            ✗ Requires UAC prompt on each launch

            NORMAL MODE (No):
            ✓ No UAC prompts
            ✓ Works with most applications
            ✗ Cannot manage windows from elevated apps

            You can change this later in appsettings.json.
            """;

        var result = MessageBox.Show(
            message,
            "WindowMux - First Run Setup",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        return result == DialogResult.Yes;
    }

    private static void ConfigureServices(IServiceCollection services, string logPath)
    {
        // Logging
        services.AddLogging(builder =>
        {
            builder.AddProvider(new FileLoggerProvider(logPath, LogLevel.Debug));
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        // Configuration (must be first - other services depend on it)
        services.AddSingleton<IConfigService, ConfigService>();

        // Services
        services.AddSingleton<IWindowFilterService, WindowFilterService>();
        services.AddSingleton<IWindowStateController, WindowStateController>();
        services.AddSingleton<IWindowRegistry, WindowRegistry>();
        services.AddSingleton<IModeStateMachine, ModeStateMachine>();
        services.AddSingleton<INewWindowMonitor, NewWindowMonitor>();
        services.AddSingleton<IHotkeyService, HotkeyService>();

        // UI
        services.AddSingleton<OverlayForm>();
    }
}
