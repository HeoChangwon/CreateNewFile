CreateNewFile WiX Installer Batch Files
=========================================

These batch files help you build the CreateNewFile installer easily.

ENCODING NOTICE:
- Files are saved with UTF-8 encoding to support Korean text
- chcp 65001 command is used to set UTF-8 code page
- If you see garbled characters, your system may need UTF-8 support

FILES:
------
01_UpdateFromProject.bat
  - Builds CreateNewFile project in Release mode
  - Publishes as Framework-dependent deployment
  - Verifies published files
  - Offers to automatically run step 2

02_BuildInstaller.bat
  - Builds WiX MSI installer package
  - Verifies all required files exist
  - Creates CreateNewFileSetup.msi
  - Provides test installation option

USAGE:
------
Method 1: Sequential (Recommended)
  1. Double-click 01_UpdateFromProject.bat
  2. Choose 'y' to automatically run installer build
  
Method 2: Individual
  1. Run 01_UpdateFromProject.bat (after code changes)
  2. Run 02_BuildInstaller.bat (to rebuild installer only)

REQUIREMENTS:
-------------
- .NET 8 SDK installed
- WiX Toolset v6 installed: dotnet tool install --global wix
- Windows with UTF-8 support (Windows 10 1903+ recommended)

TROUBLESHOOTING:
----------------
If you see Korean text display issues:
1. Right-click batch file > Edit > Save as UTF-8
2. Run: chcp 65001 in command prompt before running batch
3. Use Windows Terminal instead of Command Prompt
4. Enable UTF-8 support in Windows Settings > Region > Administrative

OUTPUT:
-------
- CreateNewFileSetup.msi (Windows Installer package)
- Approximately 2-3 MB in size
- Installs to: C:\GreenPower\CreateNewFile
- Requires .NET 8 Desktop Runtime

Created: 2025-08-25
Author: HeoChangwon (GreenPower Co., Ltd.) with Claude Code Assistant