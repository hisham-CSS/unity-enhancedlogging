using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

public static class Logger
{
    // Configuration
    public static bool EnableFileLogging { get; set; } = false;
    public static bool EnableScreenLogging { get; set; } = false;
    public static string LogFilePath { get; set; } = Path.Combine(Application.persistentDataPath, "game_log.txt");
    public static int MaxScreenLogLines { get; set; } = 50;
    public static float ScreenLogDisplayTime { get; set; } = 5.0f;
    
    // Screen logging components
    private static readonly Queue<LogEntry> _screenLogs = new Queue<LogEntry>();
    private static UIDocument _logUIDocument;
    private static ScrollView _logScrollView;
    private static VisualElement _logContent;
    private static bool _screenLogInitialized = false;

    // File logging
    private static StreamWriter _logFileWriter;
    private static bool _fileLogInitialized = false;
    
    // Cleanup tracking
    private static bool _cleanupRegistered = false;

    // Log entry structure for screen logging
    private struct LogEntry
    {
        public string message;
        public LogType logType;
        public DateTime timestamp;
        public float displayTime;
    }

    #region API Methods
    public static void Log(string log, bool verbose, LogType logType = LogType.Log, Exception e = null)
    {
        if (!verbose) return;
        
        // Ensure cleanup is registered
        if (!_cleanupRegistered)
        {
            Initialize();
        }

        // Console logging (stripped in release builds)
        LogToConsole(log, logType, e);

        // File logging (retained in release builds unless explicitly disabled)
        LogToFile(log, logType, e);

        // Screen logging (stripped in release builds)
        LogToScreen(log, logType, e);
    }
    // Console-only logging (development only)
    [System.Diagnostics.Conditional("ENABLE_CONSOLE_LOGGING")]
    public static void LogConsoleOnly(string message, LogType logType = LogType.Log)
    {
        LogToConsole(message, logType, null);
    }
    // File-only logging (available in release builds)
    [System.Diagnostics.Conditional("ENABLE_FILE_LOGGING")]
    public static void LogFileOnly(string message, LogType logType = LogType.Log)
    {
        LogToFile(message, logType, null);
    }
    // Screen-only logging (development only)
    [System.Diagnostics.Conditional("ENABLE_SCREEN_LOGGING")]
    public static void LogScreenOnly(string message, LogType logType = LogType.Log)
    {
        LogToScreen(message, logType);
    }
    // Convenience methods for different log levels
    public static void LogInfo(string message, bool verbose = true)
    {
        Log(message, verbose, LogType.Log);
    }
    public static void LogWarning(string message, bool verbose = true)
    {
        Log(message, verbose, LogType.Warning);
    }
    public static void LogError(string message, bool verbose = true)
    {
        Log(message, verbose, LogType.Error);
    }
    public static void LogException(Exception exception, string message = null, bool verbose = true)
    {
        string logMessage = string.IsNullOrEmpty(message) ? "Exception occurred" : message;
        Log(logMessage, verbose, LogType.Exception, exception);
    }
    public static void LogAssert(string message, bool verbose = true)
    {
        Log(message, verbose, LogType.Assert);
    }
    // Configuration methods that work regardless of symbol (for package setup)
    public static void ConfigureLogging(bool fileLogging = false, bool screenLogging = false, string customLogPath = null)
    {
        EnableFileLogging = fileLogging;
        EnableScreenLogging = screenLogging;

        if (!string.IsNullOrEmpty(customLogPath))
        {
            LogFilePath = customLogPath;
        }

        Log($"Logging configured - File: {fileLogging}, Screen: {screenLogging}", true, LogType.Log);
    }
    // Check which logging types are enabled
    public static LoggingCapabilities GetLoggingCapabilities()
    {
        return new LoggingCapabilities
        {
            ConsoleLogging = IsConsoleLoggingEnabled(),
            FileLogging = IsFileLoggingEnabled(),
            ScreenLogging = IsScreenLoggingEnabled()
        };
    }
    public static bool IsConsoleLoggingEnabled()
    {
        #if ENABLE_CONSOLE_LOGGING
        return true;
        #else
        return false;
        #endif
    }
    public static bool IsFileLoggingEnabled()
    {
        #if ENABLE_FILE_LOGGING
        return true;
        #else
        return false;
        #endif
    }
    public static bool IsScreenLoggingEnabled()
    {
        #if ENABLE_SCREEN_LOGGING
        return true;
        #else
        return false;
        #endif
    }
    [System.Diagnostics.Conditional("ENABLE_SCREEN_LOGGING")]
    public static void SetScreenLogVisible(bool visible)
    {
        if (_logUIDocument?.rootVisualElement == null) return;

        var root = _logUIDocument.rootVisualElement.Q("screen-logger");
        if (root != null)
        {
            root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
    [System.Diagnostics.Conditional("ENABLE_SCREEN_LOGGING")]
    public static void ToggleScreenLogging() => SetScreenLogVisible(!IsScreenLogVisible());
    [System.Diagnostics.Conditional("ENABLE_FILE_LOGGING")]
    public static void FlushLogFile()
    {
        _logFileWriter?.Flush();
    }
    #endregion

    #region Initialization
    [UnityEngine.RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        if (_cleanupRegistered) return;

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.playModeStateChanged += (state) =>
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                Cleanup();
            }
        };
        #else
        UnityEngine.Application.quitting += Cleanup;
        #endif

        _cleanupRegistered = true;

        // Log initialization info based on available symbols
        LogInitializationInfo();
    }
    private static void LogInitializationInfo()
    {
        #if ENABLE_CONSOLE_LOGGING
        LogConsoleOnly("Logging system initialized".Color(Color.green));
        #endif

        #if ENABLE_FILE_LOGGING
        LogToFile("Logging system initialized with file logging", LogType.Log);
        #endif

        // Show which logging types are enabled
        string enabledTypes = GetEnabledLoggingTypes();

        #if ENABLE_CONSOLE_LOGGING
        LogConsoleOnly($"Enabled logging types: {enabledTypes}".Color(Color.cyan));
        #endif

        #if ENABLE_FILE_LOGGING
        LogToFile($"Enabled logging types: {enabledTypes}", LogType.Log);
        #endif
    }
    private static void InitializeFileLogging()
    {
        if (_fileLogInitialized) return;

        try
        {
            string directory = Path.GetDirectoryName(LogFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            _logFileWriter = new StreamWriter(LogFilePath, append: true);
            _logFileWriter.WriteLine($"\n=== Log Session Started: {DateTime.Now} ===");
            _logFileWriter.WriteLine($"Unity Version: {Application.unityVersion}");
            _logFileWriter.WriteLine($"Platform: {Application.platform}");
            _logFileWriter.WriteLine($"Product Name: {Application.productName}");
            _logFileWriter.WriteLine($"Version: {Application.version}");
            _logFileWriter.WriteLine($"Development Build: {Debug.isDebugBuild}");
            _logFileWriter.WriteLine($"Logging Capabilities: {GetLoggingCapabilities()}");
            _logFileWriter.Flush();

            _fileLogInitialized = true;

            #if ENABLE_CONSOLE_LOGGING
            Debug.Log($"File logging initialized: {LogFilePath}");
            #endif
        }
        catch (Exception ex)
        {
            #if ENABLE_CONSOLE_LOGGING
            Debug.LogError($"Failed to initialize file logging: {ex.Message}");
            #endif
        }
    }
    [System.Diagnostics.Conditional("ENABLE_SCREEN_LOGGING")]
    private static void InitializeScreenLogging()
    {
        if (_screenLogInitialized) return;

        try
        {
            // Create UI Document GameObject
            GameObject uiDocumentObject = new GameObject("ScreenLogger");
            UnityEngine.Object.DontDestroyOnLoad(uiDocumentObject);

            _logUIDocument = uiDocumentObject.AddComponent<UIDocument>();

            // Load PanelSettings and ThemeStyleSheet from Resources
            _logUIDocument.panelSettings = Resources.Load<PanelSettings>("ScreenLoggerPanel");

            // Create simple visual tree
            var root = new VisualElement();
            root.name = "screen-logger";
            root.style.position = Position.Absolute;

            // Set position to be centered at the bottom of the screen
            root.style.left = Length.Percent(10);
            root.style.right = Length.Percent(10);
            root.style.bottom = Length.Percent(5);
            root.style.height = Length.Percent(30);
            root.style.width = Length.Percent(80);
            
            // Console-like styling
            root.style.backgroundColor = new Color(0.05f, 0.05f, 0.05f, 0.50f);
            root.style.borderTopColor = new Color(0, 1, 0, 0.8f); // Green border like terminal
            root.style.borderTopWidth = 3;

            // Rounded corners
            root.style.borderBottomLeftRadius = 10;
            root.style.borderBottomRightRadius = 10;
            root.style.borderTopLeftRadius = 10;
            root.style.borderTopRightRadius = 10;

            // Add subtle shadow effect
            root.style.borderLeftColor = new Color(0, 0, 0, 0.3f);
            root.style.borderRightColor = new Color(0, 0, 0, 0.3f);
            root.style.borderLeftWidth = 1;
            root.style.borderRightWidth = 1;
            root.style.borderTopWidth = 2;

            // Create scroll view
            _logScrollView = new ScrollView(ScrollViewMode.Vertical);
            _logScrollView.name = "log-scroll";
            _logScrollView.mode = ScrollViewMode.Vertical;
            _logScrollView.style.flexGrow = 1;
            _logScrollView.verticalScrollerVisibility = ScrollerVisibility.Auto;
            _logScrollView.horizontalScrollerVisibility = ScrollerVisibility.Hidden;

            // Create content container
            _logContent = new VisualElement();
            _logContent.name = "log-content";
            _logContent.style.paddingLeft = 10;
            _logContent.style.paddingRight = 10;
            _logContent.style.paddingTop = 5;
            _logContent.style.paddingBottom = 5;
            _logContent.style.flexGrow = 1;

            // Build hierarchy
            _logScrollView.Add(_logContent);
            root.Add(_logScrollView);
            _logUIDocument.rootVisualElement.Add(root);

            _screenLogInitialized = true;

            #if ENABLE_CONSOLE_LOGGING
            Debug.Log("UI Toolkit screen logging initialized");
            #endif
        }
        catch (Exception ex)
        {
            #if ENABLE_CONSOLE_LOGGING
            Debug.LogError($"Failed to initialize UI Toolkit screen logging: {ex.Message}");
            #endif
        }
    }
    private static string GetColoredLogMessage(string message, LogType logType)
    {
        switch (logType)
        {
            case LogType.Warning:
                return message.Color(Color.yellow);
            case LogType.Error:
                return message.Color(Color.red);
            case LogType.Assert:
                return message.Color(Color.magenta);
            case LogType.Exception:
                return message.Color(Color.cyan).Bold().Italic();
            case LogType.Log:
            default:
                return message.Color(Color.green);
                
        }
    }
    #endregion

    #region Internal Logging Functions
    [System.Diagnostics.Conditional("ENABLE_CONSOLE_LOGGING")]
    private static void LogToConsole(string log, LogType logType, Exception e = null)
    {
        switch (logType)
        {
            case LogType.Log:
                Debug.Log(log.Color(Color.green));
                break;
            case LogType.Warning: 
                Debug.LogWarning(log.Color(Color.yellow));
                break;
            case LogType.Error: 
                Debug.LogError(log.Color(Color.red));
                break;
            case LogType.Exception:
                if (e != null) Debug.LogException(e);
                break;
            case LogType.Assert:
                Debug.LogAssertion(log.Color(Color.magenta));
                break;
        }
    }
    [System.Diagnostics.Conditional("ENABLE_FILE_LOGGING")]
    private static void LogToFile(string log, LogType logType, Exception e = null)
    {
        if (!EnableFileLogging) return;

        try
        {
            InitializeFileLogging();
            
            if (_logFileWriter != null)
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string logEntry = $"[{timestamp}] [{logType}] {log}";
                
                if (e != null)
                {
                    logEntry += $"\nException: {e}";
                }
                
                _logFileWriter.WriteLine(logEntry);
                _logFileWriter.Flush();
            }
        }
        catch (Exception ex)
        {
            #if ENABLE_CONSOLE_LOGGING
            Debug.LogError($"Failed to write to log file: {ex.Message}");
            #endif
        }
    }
    [System.Diagnostics.Conditional("ENABLE_SCREEN_LOGGING")]
    private static void LogToScreen(string log, LogType logType, Exception e = null)
    {
        if (!EnableScreenLogging) return;

        try
        {
            InitializeScreenLogging();

            if (_logContent == null) return;

            // Create log entry
            var logEntry = new LogEntry
            {
                message = log,
                logType = logType,
                timestamp = DateTime.Now,
                displayTime = Time.time + ScreenLogDisplayTime
            };

            // If an exception is provided, include its details
            if (e != null)
            {
                logEntry.message = $"Exception: {e.Message}\n{e.StackTrace}";
            }

            _screenLogs.Enqueue(logEntry);

            // Limit number of entries
            while (_screenLogs.Count > MaxScreenLogLines)
            {
                _screenLogs.Dequeue();
            }

            UpdateScreenLogDisplay();
        }
        catch (Exception ex)
        {
            #if ENABLE_CONSOLE_LOGGING
            Debug.LogError($"Error logging to screen: {ex.Message}");
            #endif
        }
    }
    #endregion

    #region Helper Methods
    private static void UpdateScreenLogDisplay()
    {
        if (_logContent == null) return;

        try
        {
            // Clear existing content
            _logContent.Clear();

            var currentTime = Time.time;

            // Add each log entry as a separate element
            foreach (var entry in _screenLogs)
            {
                // Skip expired entries
                if (currentTime > entry.displayTime) continue;

                var logElement = new Label();
                logElement.AddToClassList("log-entry");
                logElement.AddToClassList($"log-{entry.logType.ToString().ToLower()}");

                string timestamp = entry.timestamp.ToString("HH:mm:ss");
                string coloredMessage = GetColoredLogMessage(entry.message, entry.logType);
                logElement.text = $"<color=#888888>[{timestamp}]</color> {coloredMessage}";

                _logContent.Add(logElement);
            }

            // Auto-scroll to bottom
            if (_logScrollView != null)
            {
                _logScrollView.schedule.Execute(() =>
                {
                    _logScrollView.scrollOffset = new Vector2(0, float.MaxValue);
                });
            }
        }
        catch (Exception ex)
        {
            #if ENABLE_CONSOLE_LOGGING
            Debug.LogError($"Error updating screen log display: {ex.Message}");
            #endif
        }
    }
    private static string GetEnabledLoggingTypes()
    {
        var types = new System.Collections.Generic.List<string>();
        #if ENABLE_CONSOLE_LOGGING
        types.Add("Console");
        #endif
        #if ENABLE_FILE_LOGGING
        types.Add("File");
        #endif
        #if ENABLE_SCREEN_LOGGING
        types.Add("Screen");
        #endif
        return types.Count > 0 ? string.Join(", ", types.ToArray()) : "None";
    }
    private static bool IsScreenLogVisible()
    {
        if (_logUIDocument?.rootVisualElement == null) return false;
        var root = _logUIDocument.rootVisualElement.Q("screen-logger");
        return root != null && root.style.display != DisplayStyle.None;
    }
    #endregion

    #region Cleanup
    // Cleanup method to be called on application quit or play mode exit
    public static void Cleanup()
    {
        try
        {
            if (_logFileWriter != null)
            {
                _logFileWriter.WriteLine($"=== Log Session Ended: {DateTime.Now} ===");
                _logFileWriter.Close();
                _logFileWriter.Dispose();
                _logFileWriter = null;
                _fileLogInitialized = false;
            }

            CleanupScreenLogger();

            //_screenLogs.Clear();

            #if ENABLE_CONSOLE_LOGGING
            Debug.Log("Logging cleanup completed");
            #endif
        }
        catch (Exception ex)
        {
            #if ENABLE_CONSOLE_LOGGING
            Debug.LogError($"Error during logging cleanup: {ex.Message}");
            #endif
        }
    }
    private static void CleanupScreenLogger()
    {
        if (_logUIDocument != null)
        {
            if (_logUIDocument.gameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(_logUIDocument.gameObject);
            }
            _logUIDocument = null;
        }

        _logScrollView = null;
        _logContent = null;
        _screenLogInitialized = false;
    }
    #endregion
}

// Helper struct to output logging capabilities
public struct LoggingCapabilities
{
    public bool ConsoleLogging;
    public bool FileLogging;
    public bool ScreenLogging;

    public override string ToString()
    {
        var enabled = new System.Collections.Generic.List<string>();
        if (ConsoleLogging) enabled.Add("Console");
        if (FileLogging) enabled.Add("File");
        if (ScreenLogging) enabled.Add("Screen");
        return enabled.Count > 0 ? string.Join(", ", enabled.ToArray()) : "None";
    }
}
