# GameSaveEditor

A powerful, cross-platform modding software designed for editing game save file data. Built with modern .NET technologies, GameSaveEditor provides a user-friendly interface for managing, analyzing, and modifying game save files across multiple platforms.

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

- **Cross-Platform Support**: Runs on Windows, iOS, macOS, and Android via .NET MAUI
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
- **[.NET MAUI](https://github.com/dotnet/maui)** (Multi-platform App UI) - Cross-platform application framework
- **[.NET 10.0](https://dotnet.microsoft.com/)** - Latest .NET runtime
- **[Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)** - Web UI framework with Razor components

### Data & Security
- **[SQLite](https://www.sqlite.org/)** - Lightweight database engine
- **[SQLCipher](https://www.zetetic.net/sqlcipher/)** - Encrypted SQLite extension
- **[System.Security.Cryptography](https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography)** - Cryptographic operations
- **[Microsoft.Data.Sqlite.Core](https://github.com/dotnet/efcore)** - .NET SQLite provider

### Platform-Specific
- **Windows App SDK** - Windows 10/11 specific features
- **Native iOS/macOS APIs** - Platform-specific functionality
- **Android APIs** - Mobile-specific features

### Language Composition
- **Java** (61.2%) - Platform-specific and backend logic
- **C#** (12.9%) - Primary .NET application code
- **HTML** (12.8%) - Blazor components and web interface
- **CSS** (10.0%) - Styling and UI presentation
- **Roff** (2.9%) - Documentation
- **AIDL** (0.2%) - Android Interface Definition Language

---

## System Requirements

### Minimum Requirements

#### Windows
- Windows 10 (version 17763.0 or later)
- .NET 10.0 SDK
- 100 MB disk space

#### macOS
- macOS 15.0 or later
- .NET 10.0 SDK
- 100 MB disk space

#### iOS
- iOS 15.0 or later
- Xcode (for development)

#### Android
- Android 7.0 (API 24) or later
- Android SDK

### Recommended Requirements
- 4 GB RAM
- SSD storage (for better performance)
- Latest version of .NET 10.0
- Visual Studio 2022 or later (for development)

---

## Installation

### Prerequisites
Before installing, ensure you have:
- [.NET 10.0 SDK or Runtime](https://dotnet.microsoft.com/download)
- Platform-specific requirements (see System Requirements)

### From Release
1. Download the latest release from the [Releases](https://github.com/vEnoch24/GameSaveEditor/releases) page
2. Extract the archive to your desired location
3. Run the executable:
   - **Windows**: `GameSaveEditor.exe`
   - **macOS**: Open `GameSaveEditor.app`
   - **iOS/Android**: Install via respective app stores

### From Source
See [Building from Source](#building-from-source) section below.

---

## Getting Started

### First Launch
1. Launch the GameSaveEditor application
2. The application will initialize required services and databases
3. Create your first profile or import existing game save files
4. Begin editing your game save data

### Basic Workflow
1. **Open Save File**: Load a game save file using the file browser
2. **Analyze Data**: View and explore save file structure and contents
3. **Edit Values**: Modify specific game data (stats, inventory, etc.)
4. **Backup**: Create a backup before making changes
5. **Save Changes**: Apply modifications and export the updated save file

### Configuration
- Application settings are stored in the `AppSettings` service
- Profiles can be managed through the UI
- Database encryption is handled automatically via SQLCipher

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
├── Platforms/               # Platform-specific code (iOS, Android, Windows, macOS)
├── Properties/              # Assembly properties and metadata
├── Resources/               # App icons, splash screens, fonts, images
├── wwwroot/                 # Static web assets for Blazor
├── App.xaml                 # Application root XAML
├── App.xaml.cs              # Application code-behind
├── MainPage.xaml            # Main application page
├── MainPage.xaml.cs         # Main page code-behind
├── MauiProgram.cs           # MAUI application configuration
├── DeploymentManagerAutoInitializer.cs # Auto-initialization logic
├── WindowsAppSDK-VersionInfo.cs # Windows-specific version info
└── GameSaveEditor.csproj    # Project configuration
```

---

## Architecture Overview

### Layered Architecture
The application follows a clean, layered architecture:

```
┌─────────────────────────────────────┐
│   UI Layer (Blazor Web View)        │
│   - HTML Components                 │
│   - CSS Styling                     │
│   - User Interactions               │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│   Component Layer                    │
│   - Razor Components                │
│   - Navigation                      │
│   - Forms & Input                   │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│   Service Layer                      │
│   - Business Logic                  │
│   - Data Processing                 │
│   - Cross-cutting Concerns          │
└──────────────┬──────────────────────┘
               │
┌──────────────▼──────────────────────┐
│   Data Layer                         │
│   - Database Access (SQLite)        │
│   - Encryption (SQLCipher)          │
│   - File I/O                        │
└─────────────────────────────────────┘
```

---

## Services

### MemoryService
Manages in-memory data operations and caching:
- Efficient data structure management
- Memory optimization
- Fast data access patterns

### DatabaseService
Handles all database operations:
- SQLite connection management
- Database initialization
- CRUD operations on save files
- Schema management

### BackupService
Provides backup and recovery functionality:
- Automatic backup creation
- Backup versioning
- Recovery operations
- Backup management and cleanup

### AppSettingsService
Manages application preferences and configuration:
- User preferences persistence
- Theme and display settings
- Application state management
- Configuration persistence

### KeyVaultService
Handles secure credential storage:
- Encryption key management
- Secure credential storage
- Windows security integration
- Data protection API usage

### ProfileService
Manages user profiles:
- Profile creation and deletion
- Profile-specific settings
- Profile switching
- Profile data association

### DatabaseProbeService
Analyzes and explores database structure:
- Schema introspection
- Data type analysis
- Relationship mapping
- Statistical analysis

---

## Development

### Prerequisites for Development
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or [Visual Studio Code](https://code.visualstudio.com/)
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- Git
- Platform-specific development tools:
  - **Windows**: Windows App SDK
  - **iOS/macOS**: Xcode
  - **Android**: Android Studio

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

3. **Open in IDE**
   - **Visual Studio**: Open `GameSaveEditor.csproj`
   - **VS Code**: Open the directory and use the .NET extension

4. **Build the project**
   ```bash
   dotnet build
   ```

### Code Organization Guidelines

- **Services**: Place business logic in the `Services/` directory
- **Components**: Create Blazor components in `Components/`
- **Models**: Define data structures in `Models/`
- **Platform-Specific**: Use `Platforms/` for OS-specific code

### Debugging

#### Visual Studio
1. Set breakpoints in the code
2. Press `F5` to start debugging
3. Use Debug menu for breakpoint management
4. Monitor variables in the watch windows

#### VS Code
1. Install C# extension
2. Set breakpoints
3. Press `F5` to start debugging
4. Use the debug console for variable inspection

---

## Building from Source

### Build for All Platforms

```bash
# Restore dependencies
dotnet restore

# Build for specific platform
dotnet build -f net10.0-windows10.0.19041.0  # Windows
dotnet build -f net10.0-ios                   # iOS
dotnet build -f net10.0-maccatalyst           # macOS
dotnet build -f net10.0-android               # Android
```

### Debug Build
```bash
dotnet build --configuration Debug
```

### Release Build
```bash
dotnet build --configuration Release
```

### Run the Application

#### Windows
```bash
dotnet run --framework net10.0-windows10.0.19041.0
```

#### macOS
```bash
dotnet run --framework net10.0-maccatalyst
```

#### Android
```bash
dotnet build -t Install -f net10.0-android --configuration Debug
```

#### iOS
```bash
dotnet build -t Install -f net10.0-ios --configuration Debug
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
- Write unit tests for new features

### Testing
Before submitting a PR:
- Ensure all existing tests pass
- Add tests for new functionality
- Test across multiple platforms if possible
- Test edge cases and error conditions

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
- Check that all required dependencies are available
- Review application logs in the debug console

#### Database Errors
- Verify database file isn't corrupted
- Try restoring from a backup
- Clear application cache and restart

#### Save File Not Loading
- Confirm the save file format is supported
- Check file permissions
- Ensure sufficient disk space

---

## Acknowledgments

- Built with [.NET MAUI](https://github.com/dotnet/maui)
- Uses [SQLite](https://www.sqlite.org/) and [SQLCipher](https://www.zetetic.net/sqlcipher/) for data storage
- UI framework powered by [Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)

---

## Project Status

This project is actively under development. Features and APIs may change without notice during early development phases.

---

**Last Updated**: September 2026  
**Maintainer**: [@vEnoch24](https://github.com/vEnoch24)
