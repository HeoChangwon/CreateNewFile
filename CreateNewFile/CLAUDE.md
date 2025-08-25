# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Development Commands

**Build the main application:**
```bash
cd "D:\Work_Claude\CreateNewFile\CreateNewFile\src\CreateNewFile"
dotnet build
```

**Run the application:**
```bash
cd "D:\Work_Claude\CreateNewFile\CreateNewFile\src\CreateNewFile"
dotnet run
```

**Build and run tests:**
```bash
cd "D:\Work_Claude\CreateNewFile\CreateNewFile\src\CreateNewFile.Tests"
dotnet test
```

**Run a specific test class:**
```bash
cd "D:\Work_Claude\CreateNewFile\CreateNewFile\src\CreateNewFile.Tests"
dotnet test --filter "ClassName=PresetItemTests"
```

**Run a specific test method:**
```bash
cd "D:\Work_Claude\CreateNewFile\CreateNewFile\src\CreateNewFile.Tests"
dotnet test --filter "FullyQualifiedName~PresetItemTests.Constructor_DefaultValues_SetsCorrectDefaults"
```

## Project Architecture

**Technology Stack:**
- .NET 8.0 WPF application with Windows Forms integration
- MVVM architecture pattern with dependency injection
- xUnit testing framework with Moq for mocking
- JSON-based configuration management

**Core Architecture:**
- **MVVM Pattern**: Clear separation between UI (Views), business logic (ViewModels), and data (Models)
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection container configured in App.xaml.cs
- **Async/Await**: All service operations use async patterns for non-blocking UI
- **Service Layer**: Interface-based services for file generation and settings management

**Key Components:**

**Models:**
- `AppSettings`: Central configuration model containing user preferences and preset items
- `PresetItem`: Simplified model with only `Id`, `Value`, and `IsEnabled` properties
- `FileCreationRequest`: Request model for file generation operations

**Services:**
- `ISettingsService/SettingsService`: Manages JSON-based configuration persistence
- `IFileGeneratorService/FileGeneratorService`: Handles file creation with template support
- Settings are stored in `config/appsettings.json` with fallback to `appsettings.default.json`

**ViewModels:**
- `MainViewModel`: Controls main window with checkbox-based filename component selection
- `SettingsViewModel`: Manages preset items with CRUD operations and multi-select deletion
- `BaseViewModel`: Provides INotifyPropertyChanged implementation

**Configuration Management:**
- `appsettings.default.json`: Template with default preset items, copied during build
- `appsettings.json`: User-specific settings created at runtime
- Checkbox states for filename components (DateTime, Abbreviation, Title, Suffix) are persisted
- Last selected values are restored on application restart

**File Generation Logic:**
- `FileNameBuilder`: Generates filename patterns based on enabled components
- `ValidationHelper`: Validates file paths and names
- Supports both template-based file creation and blank file generation
- Drag-and-drop support for setting output paths

**UI Architecture:**
- Main window uses GroupBox organization with checkbox-controlled opacity
- Settings window supports multi-select deletion and inline editing
- Real-time filename preview updates based on checkbox selections
- Status bar with progress indicators and version information

**Testing Structure:**
- Comprehensive unit tests for all core components
- Integration tests for service interactions
- User acceptance tests for end-to-end scenarios
- Tests cover the simplified PresetItem model without deprecated properties

**Important Notes:**
- Application uses DispatcherTimer for initialization to avoid UI thread deadlocks
- Version information is dynamically displayed in window title and status bar
- All preset item management uses simplified model without Description, CreatedAt, IsFavorite, or DisplayText properties
- Configuration files use minimal JSON structure with only essential properties

## Documentation Standards

When creating documentation files using Claude Code, use the following author attribution format:

### English Documents
```
**Author**: Changwon Heo (Green Power Co., Ltd.) with Claude Code Assistant
```

### Korean Documents
```
**문서 작성자**: 허창원 ((주)그린파워) with Claude Code Assistant
```