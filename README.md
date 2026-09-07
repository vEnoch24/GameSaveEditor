# GameSaveEditor

A powerful Windows application designed for editing game save file data. Built with modern .NET technologies, GameSaveEditor provides a user-friendly interface for managing, analyzing, and modifying game save files with ease and precision.

![GameSaveEditor Screenshot](image.png)

---

## Table of Contents

- [Features](#features)
- [Technology Stack](#technology-stack)
- [System Requirements](#system-requirements)
- [Installation](#installation)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [Architecture Overview](#architecture-overview)
- [Services](#services)
- [Development](#development)
- [Building from Source](#building-from-source)
- [Contributing](#contributing)
- [License](#license)
- [Support](#support)

---

## Features

- **Windows-Optimized**: Native Windows 10/11 experience with full integration
- **Game Save Editing**: Seamlessly edit and modify game save file data
- **Database Integration**: SQLite database support with encryption capabilities via SQLCipher
- **Secure Storage**: Protected data handling using Windows App SDK and System.Security.Cryptography
- **User Profiles**: Manage multiple profiles and their associated settings
- **Data Backup**: Comprehensive backup and recovery functionality
- **Memory Management**: Advanced memory management utilities for efficient data handling
- **Web Interface**: Blazor Web View for modern, responsive UI
- **Settings Management**: Persistent application settings and preferences

---

## Technology Stack

### Core Framework
- **[.NET MAUI](https://github.com/dotnet/maui)** (Multi-platform App UI) - Application framework optimized for Windows
- **[.NET 10.0](https://dotnet.microsoft.com/)** - Latest .NET runtime
- **[Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)** - Web UI framework with Razor components

### Data & Security
- **[SQLite](https://www.sqlite.org/)** - Lightweight database engine
- **[SQLCipher](https://www.zetetic.net/sqlcipher/)** - Encrypted SQLite extension
- **[System.Security.Cryptography](https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography)** - Cryptographic operations
- **[Microsoft.Data.Sqlite.Core](https://github.com/dotnet/efcore)** - .NET SQLite provider

### Windows Integration
- **[Windows App SDK](https://docs.microsoft.com/en-us/windows/apps/windows-app-sdk/)** - Modern Windows application features
- **[Windows 10/11 APIs](https://docs.microsoft.com/en-us/windows/)** - Full Windows platform integration

### Language Composition
- **Java** (61.2%) - Platform-specific and backend logic
- **C#** (12.9%) - Primary .NET application code
- **HTML** (12.8%) - Blazor components and web interface
- **CSS** (10.0%) - Styling and UI presentation
- **Roff** (2.9%) - Documentation
- **AIDL** (0.2%) - Miscellaneous platform-specific code

---

## System Requirements

### Minimum Requirements

- **Windows 10** (version 17763.0 or later) or **Windows 11**
- **Processor**: Intel Core i3 or equivalent
- **RAM**: 2 GB minimum
- **Disk Space**: 100 MB for installation
- **.NET 10.0 Runtime** (automatically installed with the application)

### Recommended Requirements

- **Windows 11** (latest version)
- **Processor**: Intel Core i5 or newer / AMD Ryzen 5 or newer
- **RAM**: 4 GB or more
- **Storage**: SSD (for better performance)
- **Display**: 1080p or higher resolution
- **.NET 10.0 SDK** (for development only)

---

## Installation

### From Release

1. Download the latest release from the [Releases](https://github.com/vEnoch24/GameSaveEditor/releases) page
2. Extract the archive to your desired location (e.g., `C:\Program Files\GameSaveEditor`)
3. Run `GameSaveEditor.exe` to launch the application
4. (Optional) Create a shortcut on your desktop for easy access

### Prerequisites

- Windows 10 (17763.0+) or Windows 11
- .NET 10.0 Runtime (included in the release or downloadable from [Microsoft](https://dotnet.microsoft.com/download))

### From Source

See [Building from Source](#building-from-source) section below.

---

## Getting Started

### First Launch

1. Launch `GameSaveEditor.exe`
2. The application will initialize required services and databases
3. Create your first profile or import existing game save files
4. Begin editing your game save data

### Basic Workflow

1. **Open Save File**: Load a game save file using the file browser
2. **Analyze Data**: View and explore save file structure and contents
3. **Edit Values**: Modify specific game data (stats, inventory, achievements, etc.)
4. **Backup**: Create a backup before making changes
5. **Save Changes**: Apply modifications and export the updated save file

### Configuration

- Application settings are accessible through the Settings menu
- Profiles can be managed and switched through the UI
- Database encryption is handled automatically via SQLCipher
- All data is stored securely with Windows security integration

---

## Project Structure

```
GameSaveEditor/
├── Components/               # Blazor components and UI components
├── Models/                   # Data models and entity definitions
├── Services/                 # Business logic and service layer
│   ├── DatabaseService.cs    # Database operations
│   ├── MemoryService.cs      # Memory management
│   ├── BackupService.cs      # Backup and recovery
│   ├── AppSettingsService.cs # Settings management
│   ├── KeyVaultService.cs    # Secure credential storage
│   ├── ProfileService.cs     # Profile management
│   └── DatabaseProbeService.cs # Database analysis
├── Platforms/               # Windows-specific platform code
├── Properties/              # Assembly properties and metadata
├── Resources/               # App icons, splash screens, fonts, images
├── wwwroot/                 # Static web assets for Blazor
├── App.xaml                 # Application root XAML
├── App.xaml.cs              # Application code-behind
├── MainPage.xaml            # Main application page
├── MainPage.xaml.cs         # Main page code-behind
├── MauiProgram.cs           # MAUI application configuration
├── DeploymentManagerAutoInitializer.cs # Auto-initialization logic
├── WindowsAppSDK-VersionInfo.cs # Windows SDK version information
└── GameSaveEditor.csproj    # Project configuration
```

---

## Architecture Overview

### Layered Architecture

The application follows a clean, layered architecture optimized for Windows:

```
┌─────────────────────────────────────┐
│   UI Layer (Blazor Web View)        │
│   - HTML Components                 │
│   - CSS Styling                     │
│   - Windows Integration             │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│   Component Layer                    │
│   - Razor Components                │
│   - Navigation & Dialogs            │
│   - Forms & Input Controls          │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│   Service Layer                      │
│   - Business Logic                  │
│   - Data Processing                 │
│   - Windows-Specific Services       │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│   Data Layer                         │
│   - Database Access (SQLite)        │
│   - Encryption (SQLCipher)          │
│   - Windows File I/O                │
└─────────────────────────────────────┘
```

---

## Services

### MemoryService
Manages in-memory data operations and caching:
- Efficient data structure management
- Memory optimization
- Fast data access patterns
- Cache management

### DatabaseService
Handles all database operations:
- SQLite connection management
- Database initialization
- CRUD operations on save files
- Schema management
- Query optimization

### BackupService
Provides backup and recovery functionality:
- Automatic backup creation
- Backup versioning
- Recovery operations
- Backup management and cleanup
- Scheduled backups

### AppSettingsService
Manages application preferences and configuration:
- User preferences persistence
- Theme and display settings
- Application state management
- Configuration persistence
- Default settings initialization

### KeyVaultService
Handles secure credential storage:
- Encryption key management
- Secure credential storage
- Windows security integration
- Data protection API (DPAPI) usage
- Secure credential retrieval

### ProfileService
Manages user profiles:
- Profile creation and deletion
- Profile-specific settings
- Profile switching
- Profile data association
- Profile backup management

### DatabaseProbeService
Analyzes and explores database structure:
- Schema introspection
- Data type analysis
- Relationship mapping
- Statistical analysis
- Data validation

---

## Development

### Prerequisites for Development

- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) (Community, Professional, or Enterprise)
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- Git
- Windows App SDK (installed with Visual Studio)

### Setting Up Development Environment

1. **Clone the repository**
   ```bash
   git clone https://github.com/vEnoch24/GameSaveEditor.git
   cd GameSaveEditor
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Open in Visual Studio**
   - Open `GameSaveEditor.csproj` with Visual Studio 2022
   - Visual Studio will automatically restore all NuGet packages

4. **Build the project**
   ```bash
   dotnet build
   ```

### Code Organization Guidelines

- **Services**: Place business logic in the `Services/` directory
- **Components**: Create Blazor components in `Components/`
- **Models**: Define data structures in `Models/`
- **Windows-Specific**: Use `Platforms/` for Windows-specific implementation

### Debugging

#### Visual Studio Debugging
1. Set breakpoints in the code by clicking on the line number
2. Press `F5` to start debugging
3. Use **Debug** menu for breakpoint management
4. Monitor variables in the **Watch** window
5. Use **Immediate Window** for runtime evaluation
6. Step through code with **F10** (step over) or **F11** (step into)

#### Common Debugging Scenarios
- **Database Issues**: Check DatabaseService logs
- **Settings Problems**: Verify AppSettingsService state
- **Backup Failures**: Review BackupService error logs
- **Memory Issues**: Use the memory profiler in Visual Studio

---

## Building from Source

### Prerequisites

- Windows 10 (17763.0+) or Windows 11
- .NET 10.0 SDK installed
- Visual Studio 2022 or Visual Studio Code with C# extension

### Build Commands

**Restore dependencies**
```bash
dotnet restore
```

**Debug Build**
```bash
dotnet build --configuration Debug --framework net10.0-windows10.0.19041.0
```

**Release Build**
```bash
dotnet build --configuration Release --framework net10.0-windows10.0.19041.0
```

### Run the Application

**From Visual Studio**
- Press `F5` to run with debugging
- Press `Ctrl+F5` to run without debugging

**From Command Line**
```bash
dotnet run --framework net10.0-windows10.0.19041.0
```

### Create Installer (Release)

For creating a Windows installer, use MSIX or WiX toolset:

```bash
# Build for release
dotnet build --configuration Release --framework net10.0-windows10.0.19041.0

# Package as MSIX
dotnet publish --configuration Release --framework net10.0-windows10.0.19041.0
```

---

## Contributing

Contributions are welcome! Please follow these guidelines:

### Before Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature-name`
3. Ensure your code follows the project's coding standards
4. Write or update tests as needed

### Submitting Changes

1. Commit your changes: `git commit -am 'Add your commit message'`
2. Push to the branch: `git push origin feature/your-feature-name`
3. Create a Pull Request with a clear description
4. Link any related issues in your PR description

### Coding Standards

- Follow [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful variable and method names
- Add XML documentation comments to public APIs
- Keep methods focused and concise
- Use async/await for I/O operations

### Testing

Before submitting a PR:
- Ensure all existing tests pass
- Add tests for new functionality
- Test on Windows 10 and Windows 11
- Test edge cases and error conditions
- Verify UI responsiveness and performance

---

## License

This project is currently unlicensed. By contributing to this project, you agree to allow the repository owner to decide on the appropriate license in the future.

For more information about choosing a license, see [Choose a License](https://choosealicense.com/).

---

## Support

### Getting Help

- **Issues**: Report bugs or request features via [GitHub Issues](https://github.com/vEnoch24/GameSaveEditor/issues)
- **Discussions**: Join the conversation in [GitHub Discussions](https://github.com/vEnoch24/GameSaveEditor/discussions)
- **Documentation**: Check the [Wiki](https://github.com/vEnoch24/GameSaveEditor/wiki) for additional documentation

### Common Issues

#### Application Won't Start
- Ensure .NET 10.0 runtime is installed
- Check that all required Windows components are available
- Run as Administrator if permission issues occur
- Review Windows Event Viewer for error details

#### Database Errors
- Verify database file isn't corrupted
- Try restoring from a backup using BackupService
- Clear application cache and restart
- Check disk space and permissions

#### Save File Not Loading
- Confirm the save file format is supported
- Check file permissions and ensure read access
- Ensure sufficient disk space available
- Verify the save file isn't corrupted

#### Performance Issues
- Close other applications to free up system memory
- Check available disk space (minimum 100 MB required)
- Update graphics drivers and Windows updates
- Disable unnecessary background applications

---

## Acknowledgments

- Built with [.NET MAUI](https://github.com/dotnet/maui)
- Uses [SQLite](https://www.sqlite.org/) and [SQLCipher](https://www.zetetic.net/sqlcipher/) for data storage
- UI framework powered by [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)
- Windows integration via [Windows App SDK](https://docs.microsoft.com/en-us/windows/apps/windows-app-sdk/)

---

## Project Status

This project is actively under development. Features and APIs may change without notice during early development phases. Currently optimized for Windows 10 and Windows 11.

---

**Last Updated**: September 2026  
**Maintainer**: [@vEnoch24](https://github.com/vEnoch24)  
**Platform**: Windows 10/11 Only
