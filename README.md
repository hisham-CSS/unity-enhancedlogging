# Enhanced Logging System for Unity

A comprehensive Unity logging system with automatic symbol management, file logging, screen logging, and build-integrated configuration. Designed for professional game development with zero performance overhead in production builds.

[![Unity Version](https://img.shields.io/badge/Unity-2022.2%2B-blue.svg)](https://unity3d.com/get-unity/download)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## ✨ Features

### 🎯 **Multi-Target Logging**
- **Console Logging**: Colored debug output in Unity Editor
- **File Logging**: Persistent logging to disk with timestamps
- **Screen Logging**: Real-time on-screen log display with auto-expiration

### 🔧 **Automatic Build Integration**
- **Smart Symbol Management**: Automatically configures logging based on build type
- **Zero Performance Overhead**: Complete code stripping in release builds
- **Build-Type Aware**: Different logging configurations for Editor/Development/Release

### 🚀 **Performance Optimized**
- **Conditional Compilation**: Uses `[Conditional]` attributes for automatic stripping
- **Memory Efficient**: Automatic cleanup and configurable limits
- **Thread Safe**: Proper error handling for file operations

### 🎨 **Rich Text Support**
- **Color-Coded Messages**: Different colors for log levels
- **Text Formatting**: Bold, italic, underline, strikethrough, size
- **Extensible**: Easy-to-use string extension methods

## 📦 Installation

### Via Git URL (Recommended)
1. Open Unity Package Manager (`Window > Package Manager`)
2. Click the `+` button and select `Add package from git URL`
3. Enter: `https://github.com/hisham-css/unity-enhancedlogging.git`

### Via Git Clone
```bash
git clone https://github.com/hisham-css/unity-enhancedlogging.git
```
Then add the local package via Package Manager.

## 🚀 Quick Start

### Basic Usage
```csharp
using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        // Configure logging (optional)
        Logger.EnableFileLogging = true;
        Logger.EnableScreenLogging = true;
        
        // Basic logging
        Logger.Log("Game started!", true);
        Logger.LogWarning("Low health warning");
        Logger.LogError("Connection failed");
    }
}
```

### Advanced Configuration
```csharp
void ConfigureLogging()
{
    // Enable features
    Logger.EnableFileLogging = true;
    Logger.EnableScreenLogging = true;
    
    // Customize settings
    Logger.MaxScreenLogLines = 30;
    Logger.ScreenLogDisplayTime = 8.0f;
    Logger.LogFilePath = Path.Combine(Application.persistentDataPath, "my_game_log.txt");
    
    // Use convenience method
    Logger.ConfigureLogging(fileLogging: true, screenLogging: true, customLogPath: "custom_log.txt");
}
```

## 📚 API Reference

### Core Logging Methods

#### `Logger.Log(string message, bool verbose, LogType logType = LogType.Log, Exception exception = null)`
Main logging method that routes to all enabled outputs.

```csharp
Logger.Log("Player connected", true, LogType.Log);
Logger.Log("Invalid input", true, LogType.Warning);
Logger.Log("Network error", true, LogType.Error, networkException);
```

#### Convenience Methods
```csharp
Logger.LogInfo("Information message");
Logger.LogWarning("Warning message");
Logger.LogError("Error message");
Logger.LogException(exception, "Custom message");
Logger.LogAssert("Assertion failed");
```

#### Target-Specific Logging
```csharp
Logger.LogConsoleOnly("Debug info for editor only");
Logger.LogFileOnly("Analytics data for file only");
Logger.LogScreenOnly("Player notification for screen only");
```

### Configuration Properties

```csharp
// Enable/disable logging types
Logger.EnableFileLogging = true;
Logger.EnableScreenLogging = true;

// File logging settings
Logger.LogFilePath = "path/to/logfile.txt";

// Screen logging settings
Logger.MaxScreenLogLines = 50;
Logger.ScreenLogDisplayTime = 5.0f;
```

### Utility Methods

```csharp
// Check capabilities
LoggingCapabilities caps = Logger.GetLoggingCapabilities();
bool consoleEnabled = Logger.IsConsoleLoggingEnabled();
bool fileEnabled = Logger.IsFileLoggingEnabled();
bool screenEnabled = Logger.IsScreenLoggingEnabled();

// Screen log control
Logger.ClearScreenLogs();
Logger.SetScreenLogVisibility(false);
bool isVisible = Logger.IsScreenLogVisible;

// File operations
Logger.FlushLogFile();
```

### String Extensions

```csharp
using UnityEngine;

// Color formatting
string coloredText = "Error message".Color(Color.red);
string greenText = "Success".Color(Color.green);

// Text formatting
string boldText = "Important".Bold();
string italicText = "Emphasis".Italic();
string underlinedText = "Underlined".Underline();
string strikeText = "Crossed out".Strikethrough();
string bigText = "Large text".Size(20f);

// Chaining
string formatted = "Warning".Color(Color.yellow).Bold().Size(16f);
```

## 🔧 Build Configuration

The logging system automatically configures itself based on your build settings:

### Editor Environment
- ✅ Console logging (colored Debug.Log output)
- ✅ File logging (if enabled)
- ✅ Screen logging (if enabled)

### Development Builds
- ❌ Console logging (not available in builds)
- ✅ File logging (crash reports, debugging)
- ✅ Screen logging (real-time debugging)

### Release Builds
- ❌ Console logging (not available in builds)
- ✅ File logging (analytics, crash reports)*
- ❌ Screen logging (stripped for performance)

*File logging in release builds can be disabled via `Tools > Logger > Toggle File Logging in Release Builds`

### Manual Symbol Control

Access logging configuration via Unity menu:
- `Tools > Logger > Toggle File Logging in Release Builds`
- `Tools > Logger > Check Current Configuration`
- `Tools > Logger > Refresh Editor Symbols`

## 📁 File Logging

### Default Location
```
Application.persistentDataPath/game_log.txt
```

### Log Format
```
=== Log Session Started: 2024-06-19 15:30:45 ===
Unity Version: 2022.3.0f1
Platform: WindowsPlayer
Product Name: MyGame
Version: 1.0.0
Development Build: False
Logging Capabilities: File

[2024-06-19 15:30:45.123] [Log] Game started successfully
[2024-06-19 15:30:46.456] [Warning] Low memory warning
[2024-06-19 15:30:47.789] [Error] Network connection failed
Exception: System.Net.NetworkException: Connection timeout
   at NetworkManager.Connect() in NetworkManager.cs:line 45

=== Log Session Ended: 2024-06-19 15:35:22 ===
```

## 🖥️ Screen Logging

### Features
- **Real-time Display**: Shows logs in bottom 30% of screen
- **Auto-Expiration**: Messages disappear after configurable time
- **Color-Coded**: Different colors for different log types
- **Scrollable**: Automatic scrolling to show latest messages
- **Non-Intrusive**: Semi-transparent background, high sorting order

### Customization
```csharp
// Configure display
Logger.MaxScreenLogLines = 20;           // Maximum visible lines
Logger.ScreenLogDisplayTime = 10.0f;     // Seconds before expiration

// Control visibility
Logger.SetScreenLogVisibility(false);    // Hide screen logs
Logger.ClearScreenLogs();                // Clear all screen logs
```

## 🎮 Sample Usage

Import the "Basic Usage Examples" sample via Package Manager to see:
- Complete logging setup
- Interactive testing with keyboard input
- GUI controls for testing different features
- Exception handling examples

## 🔍 Troubleshooting

### Common Issues

**Q: Logs not appearing in builds**
A: This is expected behavior. Console logging is editor-only. Enable file or screen logging for builds.

**Q: File logging not working**
A: Check that `Logger.EnableFileLogging = true` and verify write permissions to the log file path.

**Q: Screen logs not visible**
A: Ensure `Logger.EnableScreenLogging = true` and check that screen log visibility is enabled.

**Q: Performance impact in release builds**
A: There should be zero impact. The system uses conditional compilation to completely strip logging code in release builds.

### Debug Current Configuration
```csharp
// Check what's currently enabled
LoggingCapabilities caps = Logger.GetLoggingCapabilities();
Debug.Log($"Console: {caps.ConsoleLogging}, File: {caps.FileLogging}, Screen: {caps.ScreenLogging}");
```

Or use the Unity menu: `Tools > Logger > Check Current Configuration`

## 📄 License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/hisham-css/unity-enhancedlogging/issues)
- **Email**: hisham@catsplatstudios.com
- **Website**: [catsplatstudios.com](https://catsplatstudios.com)

