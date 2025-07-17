# CHIM AI Installer

An interactive, modern Windows installer for CHIM AI for Skyrim. Built with .NET 8 WPF and Material Design.

## 🚀 Features

- **Modern Material Design UI** with dark/light theme support
- **System Requirements Validation** (admin rights, virtualization, disk space, Windows features)
- **Automatic Prerequisites Installation** (Visual C++ Redistributable, 7-Zip, Windows features)
- **Smart Download Management** with auto-detection and progress tracking
- **Robust File Extraction** with progress indicators and error handling
- **Component-based Installation** with customizable options
- **Comprehensive Logging** and error reporting
- **Single-file EXE** deployment with embedded resources

## 🛠️ Architecture

### Project Structure
```
CHIM-installer/
├── src/
│   ├── CHIMInstaller/              # Main WPF application
│   │   ├── Views/                  # UI screens/windows
│   │   ├── ViewModels/            # MVVM logic
│   │   ├── Models/                # Data models
│   │   ├── Services/              # Core services
│   │   ├── Resources/             # Images, styles
│   │   └── Utils/                 # Helper utilities
│   ├── CHIMInstaller.Core/        # Core business logic
│   └── CHIMInstaller.Tests/       # Unit tests
├── assets/                        # Icons, images, resources
├── scripts/                       # Embedded PowerShell scripts
├── docs/                          # Documentation
├── build/                         # Build scripts and configs
└── releases/                      # Compiled installers
```

### Core Services

- **ISystemCheckService** - System requirements validation
- **IDownloadService** - File download management
- **IExtractionService** - Archive extraction
- **IPrerequisiteService** - Dependencies installation
- **IConfigurationService** - Installation settings
- **INavigationService** - Screen navigation

### Installation Flow

1. **Welcome Screen** - Branding and overview
2. **System Check** - Requirements validation
3. **Prerequisites** - Install dependencies
4. **Installation Path** - Choose destination
5. **Download** - Get mod files from Nexus
6. **Extraction** - Extract and install
7. **Completion** - Finish and launch

## 🔧 Requirements

### Development
- Visual Studio 2022 or VS Code
- .NET 8.0 SDK
- Windows 10/11

### Runtime
- Windows 10/11 (64-bit)
- Administrator privileges
- 10+ GB available disk space
- Internet connection
- Virtualization enabled (for full functionality)

## 🏗️ Building

### Debug Build
```bash
dotnet build
```

### Release Build (Single File EXE)
```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

### Running Tests
```bash
dotnet test
```

## 📦 Dependencies

### Main Application
- MaterialDesignThemes - Modern UI components
- CommunityToolkit.Mvvm - MVVM framework
- Microsoft.Extensions.* - Dependency injection & configuration
- Serilog - Structured logging
- SharpCompress - Archive extraction
- System.Management - WMI queries

### Development
- xUnit - Testing framework
- Moq - Mocking framework
- FluentAssertions - Assertion library

## 🎨 UI Design

### Color Scheme
- **Primary**: Deep Purple (#673AB7)
- **Secondary**: Cyan (#00BCD4)
- **Accent**: Deep Orange (#FF5722)
- **Success**: Green (#4CAF50)
- **Warning**: Orange (#FF9800)
- **Error**: Red (#F44336)

### Screens
- Dark theme by default
- Material Design principles
- Responsive layout
- Progress indicators
- Real-time status updates

## 📋 Current Status

### ✅ Completed
- [x] Project structure and solution setup
- [x] Core service interfaces
- [x] Data models and DTOs
- [x] Dependency injection configuration
- [x] Application manifest for admin privileges
- [x] Material Design theming setup

### 🚧 In Progress
- [ ] UI framework setup
- [ ] Service implementations
- [ ] Main window and navigation
- [ ] System requirements checking

### 📝 TODO
- [ ] Welcome screen implementation
- [ ] Download management
- [ ] File extraction
- [ ] Prerequisites installation
- [ ] Error handling and logging
- [ ] Build pipeline and release

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## 📄 License

See [LICENSE](LICENSE) file for details.

## 🆘 Support

For issues and questions:
- Check the [docs/](docs/) folder
- Create an issue on GitHub
- Check the log files in `%LocalAppData%\CHIMInstaller\Logs\`

---

**Built with ❤️ for the Skyrim modding community** 