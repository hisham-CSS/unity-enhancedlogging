#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Simplified build-integrated symbol manager that automatically configures logging symbols
/// based on build type without requiring manual menu interactions.
/// 
/// Symbol Configuration:
/// - Editor: All symbols (ENABLE_CONSOLE_LOGGING, ENABLE_FILE_LOGGING, ENABLE_SCREEN_LOGGING)
/// - Development Build: File + Screen logging (ENABLE_FILE_LOGGING, ENABLE_SCREEN_LOGGING)
/// - Release Build: File logging only (ENABLE_FILE_LOGGING) or none based on user preference
/// </summary>
[InitializeOnLoad]
public static class LoggerSymbolManager
{
    // Logging symbols
    private const string CONSOLE_LOGGING_SYMBOL = "ENABLE_CONSOLE_LOGGING";
    private const string FILE_LOGGING_SYMBOL = "ENABLE_FILE_LOGGING";
    private const string SCREEN_LOGGING_SYMBOL = "ENABLE_SCREEN_LOGGING";

    // User preference key for disabling file logging in release builds
    public const string DISABLE_FILE_LOGGING_PREF = "DisableFileLoggingInRelease";

    static LoggerSymbolManager()
    {
        // Set editor symbols on startup
        SetEditorSymbols();
    }

    /// <summary>
    /// Sets symbols for editor (all logging enabled)
    /// </summary>
    private static void SetEditorSymbols()
    {
        var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        var editorSymbols = new string[]
        {
            CONSOLE_LOGGING_SYMBOL,
            FILE_LOGGING_SYMBOL,
            SCREEN_LOGGING_SYMBOL
        };

        SetSymbolsForBuild(namedBuildTarget, editorSymbols);
        Debug.Log("[LoggerSymbolManager] Editor symbols configured: Console, File, Screen logging enabled");
    }

    /// <summary>
    /// Determines which symbols should be active for a given build configuration
    /// </summary>
    public static string[] GetSymbolsForBuild(bool isDevelopmentBuild, bool disableFileLoggingInRelease)
    {
        var symbols = new System.Collections.Generic.List<string>();

        if (isDevelopmentBuild)
        {
            // Development builds: File + Screen logging (no console - that's editor only)
            symbols.Add(FILE_LOGGING_SYMBOL);
            symbols.Add(SCREEN_LOGGING_SYMBOL);
        }
        else
        {
            // Release builds: File logging only (unless disabled by user preference)
            if (!disableFileLoggingInRelease)
            {
                symbols.Add(FILE_LOGGING_SYMBOL);
            }
            // Screen logging is never included in release builds
            // Console logging is never included in any builds (editor only)
        }

        return symbols.ToArray();
    }

    /// <summary>
    /// Sets the logging symbols for a specific build target, preserving other symbols
    /// </summary>
    public static void SetSymbolsForBuild(NamedBuildTarget namedBuildTarget, string[] requiredLoggingSymbols)
    {
        var currentSymbols = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
        var symbolList = new System.Collections.Generic.List<string>(currentSymbols.Split(';'));

        // Remove empty entries and existing logging symbols
        symbolList.RemoveAll(s => string.IsNullOrWhiteSpace(s) ||
                                 s.Trim() == CONSOLE_LOGGING_SYMBOL ||
                                 s.Trim() == FILE_LOGGING_SYMBOL ||
                                 s.Trim() == SCREEN_LOGGING_SYMBOL);

        // Add required logging symbols
        symbolList.AddRange(requiredLoggingSymbols);

        string newSymbols = string.Join(";", symbolList.ToArray());
        PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, newSymbols);
    }

    /// <summary>
    /// User preference: Toggle file logging in release builds
    /// </summary>
    [MenuItem("Cat Splat Tools/Logger/Toggle File Logging in Release Builds")]
    public static void ToggleFileLoggingInRelease()
    {
        bool currentSetting = EditorPrefs.GetBool(DISABLE_FILE_LOGGING_PREF, false);
        EditorPrefs.SetBool(DISABLE_FILE_LOGGING_PREF, !currentSetting);

        string status = !currentSetting ? "DISABLED" : "ENABLED";
        Debug.Log($"[LoggerSymbolManager] File logging in release builds: {status}");

        // Update editor symbols to reflect current preference
        SetEditorSymbols();
    }

    /// <summary>
    /// Utility: Check current logging configuration
    /// </summary>
    [MenuItem("Cat Splat Tools/Logger/Check Current Configuration")]
    public static void CheckCurrentConfiguration()
    {
        var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        var currentSymbols = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
        var symbolList = currentSymbols.Split(';');

        bool isDevelopmentBuild = EditorUserBuildSettings.development;
        bool disableFileInRelease = EditorPrefs.GetBool(DISABLE_FILE_LOGGING_PREF, false);

        Debug.Log($"[LoggerSymbolManager] Current Configuration:");
        Debug.Log($"Build Type: {(isDevelopmentBuild ? "Development" : "Release")}");
        Debug.Log($"Disable File Logging in Release: {disableFileInRelease}");
        Debug.Log($"Console Logging: {System.Array.Exists(symbolList, s => s.Trim() == CONSOLE_LOGGING_SYMBOL)} (Editor only)");
        Debug.Log($"File Logging: {System.Array.Exists(symbolList, s => s.Trim() == FILE_LOGGING_SYMBOL)}");
        Debug.Log($"Screen Logging: {System.Array.Exists(symbolList, s => s.Trim() == SCREEN_LOGGING_SYMBOL)}");
        Debug.Log($"All Symbols: {currentSymbols}");

        // Show what will happen in next build
        var nextBuildSymbols = GetSymbolsForBuild(isDevelopmentBuild, disableFileInRelease);
        Debug.Log($"Next Build Will Have: {(nextBuildSymbols.Length > 0 ? string.Join(", ", nextBuildSymbols) : "No logging symbols")}");
    }

    /// <summary>
    /// Utility: Force refresh editor symbols (useful for testing)
    /// </summary>
    [MenuItem("Cat Splat Tools/Logger/Refresh Editor Symbols")]
    public static void RefreshEditorSymbols()
    {
        SetEditorSymbols();
        Debug.Log("[LoggerSymbolManager] Editor symbols refreshed");
    }
}

/// <summary>
/// Handle build target changes to update editor symbols
/// This ensures editor symbols are correct when switching platforms
/// </summary>
public class LoggerBuildTargetChangeHandler : IActiveBuildTargetChanged
{
    public int callbackOrder => 0;

    public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
    {
        Debug.Log($"[LoggerBuildTargetChangeHandler] Build target changed from {previousTarget} to {newTarget}");

        // Refresh editor symbols for new target
        LoggerSymbolManager.RefreshEditorSymbols();
    }    
}

public class LoggerBuildProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder => -100;

    public void OnPreprocessBuild(BuildReport report)
    {
        var buildTarget = report.summary.platform;
        var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(BuildPipeline.GetBuildTargetGroup(buildTarget));

        bool isDevelopmentBuild = EditorUserBuildSettings.development;
        bool disableFileLoggingInRelease = EditorPrefs.GetBool(LoggerSymbolManager.DISABLE_FILE_LOGGING_PREF, false);

        var requiredSymbols = LoggerSymbolManager.GetSymbolsForBuild(isDevelopmentBuild, disableFileLoggingInRelease);
        LoggerSymbolManager.SetSymbolsForBuild(namedBuildTarget, requiredSymbols);

        string buildType = isDevelopmentBuild ? "Development" : "Release";
        Debug.Log($"[LoggerBuildProcessor] Configured {buildType} build with symbols: {string.Join(", ", requiredSymbols)}");
    }
}

#endif

