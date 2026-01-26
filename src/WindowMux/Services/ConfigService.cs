using System.Text.Json;
using ScottReece.WindowMux.Models;
using ScottReece.WindowMux.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace ScottReece.WindowMux.Services;

/// <summary>
/// Loads and provides access to application configuration.
/// </summary>
public sealed class ConfigService : IConfigService
{
    private readonly ILogger<ConfigService> _logger;
    private readonly AppConfig _config;

    public IReadOnlyList<ColorDefinition> Colors => _config.Colors;
    public ColorDefinition MasterColor => _config.Colors[0];
    public bool? ElevatedMode => _config.ElevatedMode;
    public bool IsFirstRun => _config.ElevatedMode == null;

    public ConfigService(ILogger<ConfigService> logger)
    {
        _logger = logger;
        _config = LoadConfig();
    }

    public bool IsMasterColor(string colorId)
    {
        return string.Equals(MasterColor.Id, colorId, StringComparison.OrdinalIgnoreCase);
    }

    public void SetElevatedMode(bool elevated)
    {
        _config.ElevatedMode = elevated;
        SaveConfig(GetConfigPath(), _config);
        _logger.LogInformation("Saved elevation preference: {Elevated}", elevated);
    }

    public WindowPositionConfig? WindowPosition => _config.WindowPosition;

    public void SaveWindowPosition(WindowPositionConfig position)
    {
        _config.WindowPosition = position;
        SaveConfig(GetConfigPath(), _config);
        _logger.LogInformation("Saved window position: Corner={Corner}, OffsetX={X}, OffsetY={Y}", 
            position.Corner, position.OffsetX, position.OffsetY);
    }

    private AppConfig LoadConfig()
    {
        var configPath = GetConfigPath();
        _logger.LogInformation("Loading config from {Path}", configPath);

        if (!File.Exists(configPath))
        {
            _logger.LogInformation("Config file not found, creating default");
            var defaultConfig = CreateDefaultConfig();
            SaveConfig(configPath, defaultConfig);
            return defaultConfig;
        }

        try
        {
            var json = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<AppConfig>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (config?.Colors == null || config.Colors.Count == 0)
            {
                _logger.LogWarning("Config has no colors, using defaults");
                return CreateDefaultConfig();
            }

            _logger.LogInformation("Loaded {Count} colors from config", config.Colors.Count);
            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load config, using defaults");
            return CreateDefaultConfig();
        }
    }

    private static string GetConfigPath()
    {
        return Path.Combine(AppContext.BaseDirectory, "appsettings.json");
    }

    private void SaveConfig(string path, AppConfig config)
    {
        try
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(path, json);
            _logger.LogInformation("Saved config to {Path}", path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save config");
        }
    }

    private static AppConfig CreateDefaultConfig()
    {
        return new AppConfig
        {
            ElevatedMode = null, // First run - user hasn't chosen yet
            Colors = new List<ColorDefinition>
            {
                new() { Id = "green", Name = "Green", R = 46, G = 204, B = 113 },
                new() { Id = "red", Name = "Red", R = 231, G = 76, B = 60 },
                new() { Id = "blue", Name = "Blue", R = 52, G = 152, B = 219 },
                new() { Id = "yellow", Name = "Yellow", R = 241, G = 196, B = 15 },
                new() { Id = "orange", Name = "Orange", R = 230, G = 126, B = 34 },
                new() { Id = "purple", Name = "Purple", R = 155, G = 89, B = 182 }
            }
        };
    }
}
