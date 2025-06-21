# Changelog

All notable changes to the Enhanced Logging System will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2024-06-21

### 🎉 Initial Release

The Enhanced Logging System for Unity provides a comprehensive, performance-optimized logging solution with automatic build integration and zero overhead in production builds.

### ✨ Added

#### Core Logging Features
- **Multi-Target Logging System**: Console, File, and Screen logging with independent control
- **Automatic Build Integration**: Smart symbol management based on build configuration
- **Performance Optimization**: Conditional compilation with complete code stripping in release builds
- **Thread-Safe Operations**: Robust error handling and resource management

#### Console Logging
- **Color-Coded Output**: Different colors for each log level (Log=Green, Warning=Yellow, Error=Red, Assert=Magenta)
- **Rich Text Support**: Bold, italic, underline, strikethrough, and size formatting
- **Editor-Only**: Automatically stripped from builds (console not available in builds)
- **String Extensions**: Easy-to-use extension methods for text formatting

#### File Logging
- **Persistent Storage**: Automatic file creation and management
- **Detailed Session Info**: Unity version, platform, product info, and build type
- **Timestamped Entries**: Millisecond precision timestamps for all log entries
- **Exception Logging**: Full exception details with stack traces
- **Configurable Path**: Custom log file locations
- **Automatic Cleanup**: Proper file handle management and session boundaries
- **Release Build Support**: Configurable retention in production builds

#### Screen Logging
- **Real-Time Display**: On-screen log overlay in bottom of screen
- **Auto-Expiration**: Configurable message display duration (default 5 seconds)
- **Color-Coded Messages**: Visual distinction between log levels
- **Scrollable Interface**: Automatic scrolling to show latest messages
- **Memory Management**: Configurable maximum line limits (default 50 lines)
- **Non-Intrusive Design**: Semi-transparent background, high sorting order
- **Runtime Controls**: Show/hide and clear functionality
- **Development Only**: Automatically stripped from release builds

#### Build System Integration
- **Automatic Symbol Management**: No manual configuration required
- **Build-Type Detection**: Different configurations for Editor/Development/Release
- **Symbol Preservation**: Maintains non-logging scripting defines
- **Platform Awareness**: Handles build target changes automatically
- **User Preferences**: Optional file logging control in release builds

#### API Design
- **Simple Interface**: Clean `Logger.Log()` syntax
- **Convenience Methods**: `LogInfo()`, `LogWarning()`, `LogError()`, `LogException()`, `LogAssert()`
- **Target-Specific Logging**: `LogConsoleOnly()`, `LogFileOnly()`, `LogScreenOnly()`
- **Configuration API**: Runtime configuration of all logging features
- **Capability Checking**: Query which logging types are currently enabled
- **Verbose Control**: Optional verbose parameter for conditional logging

#### Developer Tools
- **Unity Menu Integration**: `Cat Splat Tools > Logger` menu for configuration and debugging
- **Configuration Checker**: Real-time status of symbols and build settings
- **Symbol Refresh**: Manual symbol management for testing
- **Sample Project**: Complete usage examples with interactive testing

#### Assembly Organization
- **Separate Assemblies**: Runtime and Editor assemblies for optimal compilation
- **Dependency Management**: Clean assembly references and isolation
- **Package Structure**: Professional Unity package organization
- **No Namespace Pollution**: Global access without using statements

### 🔧 Technical Implementation

#### Symbol Management
- **ENABLE_CONSOLE_LOGGING**: Editor-only console output with colors
- **ENABLE_FILE_LOGGING**: Persistent file logging (configurable in release)
- **ENABLE_SCREEN_LOGGING**: Development-only screen overlay
- **Conditional Compilation**: `[System.Diagnostics.Conditional]` attributes for automatic stripping

#### Build Configurations
- **Editor**: All logging types enabled (Console + File + Screen)
- **Development Build**: File + Screen logging (no console in builds)
- **Release Build**: File logging only (or disabled based on preference)

#### Performance Features
- **Zero Overhead**: Complete code removal in release builds
- **Lazy Initialization**: Resources created only when needed
- **Memory Efficient**: Automatic cleanup and configurable limits
- **Exception Safe**: Robust error handling throughout

#### File System
- **Default Location**: `Application.persistentDataPath/game_log.txt`
- **Directory Creation**: Automatic directory structure creation
- **Append Mode**: Session logs appended to existing files
- **Session Boundaries**: Clear session start/end markers
- **Flush Control**: Manual and automatic file flushing

#### UI System
- **Programmatic Creation**: No prefabs or scene dependencies
- **Canvas Management**: Automatic canvas creation with proper settings
- **Scroll View**: Full scrolling interface with content size fitting
- **Text Rendering**: Built-in font usage for maximum compatibility
- **Lifecycle Management**: Proper creation and destruction

### 📦 Package Features
- **Unity Package Manager**: Full UPM support with git URL installation
- **Sample Integration**: Importable samples via Package Manager
- **Assembly Definitions**: Optimized compilation with proper dependencies
- **MIT License**: Open source with permissive licensing
- **Documentation**: Comprehensive README and API documentation

### 🎯 Compatibility
- **Unity Version**: 2022.2 or higher
- **Platforms**: All Unity-supported platforms
- **Build Pipeline**: Compatible with standard Unity build process
- **Package Manager**: Full UPM integration and git package support

### 📋 Known Limitations
- Screen logging requires Unity UIToolkit
- File logging requires write permissions to persistent data path
- Console logging only available in Unity Editor (by design)
- String extensions use Unity's rich text format (HTML-like tags)

---

**For the latest updates and releases, visit [GitHub Releases](https://github.com/hisham-css/unity-enhancedlogging/releases)**

