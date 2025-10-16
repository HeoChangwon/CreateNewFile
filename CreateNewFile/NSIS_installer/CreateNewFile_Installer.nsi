; CreateNewFile NSIS Installer Script
; Framework-dependent build - NSIS_installer folder version
; Developer: HeoChangwon

!define PRODUCT_NAME "CreateNewFile"
!define PRODUCT_VERSION "1.0.001"
!define PRODUCT_PUBLISHER "HeoChangwon"
!define PRODUCT_WEB_SITE "https://github.com/HeoChangwon/CreateNewFile"
!define PRODUCT_UNINST_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}"
!define DISPLAY_BUILD_DATE "2025-10-16 17:50"  ; Build date (YYYY-MM-DD HH:MM format)

; Modern UI
!include "MUI2.nsh"

; General settings
Name "${PRODUCT_NAME} ${PRODUCT_VERSION}"
OutFile "${PRODUCT_NAME}_Setup_v${PRODUCT_VERSION}.exe"
InstallDir "$PROGRAMFILES64\CreateNewFile"
ShowInstDetails show
ShowUnInstDetails show

; Request admin privileges
RequestExecutionLevel admin

; Target x64 architecture
!define MULTIUSER_EXECUTIONLEVEL Admin
!define MULTIUSER_MUI
!define MULTIUSER_INSTALLMODE_COMMANDLINE
!define MULTIUSER_INSTALLMODE_DEFAULT_ALLUSERS
Target x86-unicode

; Compression
SetCompressor /SOLID lzma

; Branding text (bottom of installer window)
BrandingText "${PRODUCT_NAME} v${PRODUCT_VERSION} (Build: ${DISPLAY_BUILD_DATE})"

; Icon settings (using icon from published files)
!define MUI_ICON "publish\framework-dependent\Resources\CreateNewFile.ico"
!define MUI_UNICON "publish\framework-dependent\Resources\CreateNewFile.ico"

; Install pages
!define MUI_WELCOMEPAGE_TITLE "CreateNewFile Setup"
!define MUI_WELCOMEPAGE_TEXT "This program creates new files based on templates and presets.$\r$\n$\r$\nRequires .NET 8.0 Runtime.$\r$\n$\r$\nClick Next to continue."

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_LICENSE "LICENSE.txt"
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES

!define MUI_FINISHPAGE_RUN "$INSTDIR\CreateNewFile.exe"
!define MUI_FINISHPAGE_RUN_TEXT "Run CreateNewFile"
!insertmacro MUI_PAGE_FINISH

; Uninstall pages
!insertmacro MUI_UNPAGE_WELCOME
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_UNPAGE_FINISH

; Language
!insertmacro MUI_LANGUAGE "English"

; Version info
VIProductVersion "${PRODUCT_VERSION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductName" "${PRODUCT_NAME}"
VIAddVersionKey /LANG=${LANG_ENGLISH} "Comments" "Template-based File Creation Tool"
VIAddVersionKey /LANG=${LANG_ENGLISH} "CompanyName" "${PRODUCT_PUBLISHER}"
VIAddVersionKey /LANG=${LANG_ENGLISH} "LegalCopyright" "Copyright (c) ${PRODUCT_PUBLISHER}, 2025"
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileDescription" "${PRODUCT_NAME} Setup"
VIAddVersionKey /LANG=${LANG_ENGLISH} "FileVersion" "${PRODUCT_VERSION}"
VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductVersion" "${PRODUCT_VERSION}"

; Main program section
Section "Main Program (Required)" SEC01
  SectionIn RO
  SetOutPath "$INSTDIR"
  
  ; Copy program files from publish folder
  File /r "publish\framework-dependent\*.*"
  
  ; Create shortcuts
  CreateDirectory "$SMPROGRAMS\${PRODUCT_NAME}"
  CreateShortCut "$SMPROGRAMS\${PRODUCT_NAME}\${PRODUCT_NAME}.lnk" "$INSTDIR\CreateNewFile.exe" "" "$INSTDIR\Resources\CreateNewFile.ico"
  CreateShortCut "$SMPROGRAMS\${PRODUCT_NAME}\Uninstall ${PRODUCT_NAME}.lnk" "$INSTDIR\uninst.exe"
  
  ; Registry entries
  WriteRegStr HKLM "${PRODUCT_UNINST_KEY}" "DisplayName" "${PRODUCT_NAME}"
  WriteRegStr HKLM "${PRODUCT_UNINST_KEY}" "UninstallString" "$INSTDIR\uninst.exe"
  WriteRegStr HKLM "${PRODUCT_UNINST_KEY}" "DisplayIcon" "$INSTDIR\CreateNewFile.exe"
  WriteRegStr HKLM "${PRODUCT_UNINST_KEY}" "DisplayVersion" "${PRODUCT_VERSION}"
  WriteRegStr HKLM "${PRODUCT_UNINST_KEY}" "Publisher" "${PRODUCT_PUBLISHER}"
  WriteRegStr HKLM "${PRODUCT_UNINST_KEY}" "URLInfoAbout" "${PRODUCT_WEB_SITE}"
  WriteRegStr HKLM "${PRODUCT_UNINST_KEY}" "InstallLocation" "$INSTDIR"
  WriteRegDWORD HKLM "${PRODUCT_UNINST_KEY}" "NoModify" 1
  WriteRegDWORD HKLM "${PRODUCT_UNINST_KEY}" "NoRepair" 1

  ; Register .cnfjson file association
  WriteRegStr HKCR ".cnfjson" "" "CreateNewFile.Project"
  WriteRegStr HKCR ".cnfjson" "Content Type" "application/json"
  WriteRegStr HKCR "CreateNewFile.Project" "" "CreateNewFile Project File"
  WriteRegStr HKCR "CreateNewFile.Project\DefaultIcon" "" "$INSTDIR\Resources\CreateNewFile.ico,0"
  WriteRegStr HKCR "CreateNewFile.Project\shell" "" "open"
  WriteRegStr HKCR "CreateNewFile.Project\shell\open" "" "Open with CreateNewFile"
  WriteRegStr HKCR "CreateNewFile.Project\shell\open\command" "" '"$INSTDIR\CreateNewFile.exe" "%1"'

  ; Notify shell of file association change
  System::Call 'shell32.dll::SHChangeNotify(i, i, i, i) v (0x08000000, 0, 0, 0)'

  ; Create uninstaller
  WriteUninstaller "$INSTDIR\uninst.exe"
SectionEnd

Section "Desktop Shortcut" SEC02
  CreateShortCut "$DESKTOP\${PRODUCT_NAME}.lnk" "$INSTDIR\CreateNewFile.exe" "" "$INSTDIR\Resources\CreateNewFile.ico"
SectionEnd

Section /o "Auto Start" SEC03
  WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Run" "${PRODUCT_NAME}" "$INSTDIR\CreateNewFile.exe"
SectionEnd

; Section descriptions
LangString DESC_SEC01 ${LANG_ENGLISH} "CreateNewFile main program files."
LangString DESC_SEC02 ${LANG_ENGLISH} "Creates a shortcut on the desktop."
LangString DESC_SEC03 ${LANG_ENGLISH} "Automatically starts the program when Windows starts."

!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${SEC01} $(DESC_SEC01)
  !insertmacro MUI_DESCRIPTION_TEXT ${SEC02} $(DESC_SEC02)
  !insertmacro MUI_DESCRIPTION_TEXT ${SEC03} $(DESC_SEC03)
!insertmacro MUI_FUNCTION_DESCRIPTION_END

; Uninstaller section
Section Uninstall
  ; Terminate running process if needed
  ExecWait 'taskkill /F /IM CreateNewFile.exe' $0
  
  ; Remove files and folders
  RMDir /r "$INSTDIR"
  
  ; Remove shortcuts
  Delete "$DESKTOP\${PRODUCT_NAME}.lnk"
  Delete "$SMPROGRAMS\${PRODUCT_NAME}\${PRODUCT_NAME}.lnk"
  Delete "$SMPROGRAMS\${PRODUCT_NAME}\Uninstall ${PRODUCT_NAME}.lnk"
  RMDir "$SMPROGRAMS\${PRODUCT_NAME}"
  
  ; Remove auto start entry
  DeleteRegValue HKCU "Software\Microsoft\Windows\CurrentVersion\Run" "${PRODUCT_NAME}"

  ; Remove .cnfjson file association
  DeleteRegKey HKCR ".cnfjson"
  DeleteRegKey HKCR "CreateNewFile.Project"

  ; Notify shell of file association change
  System::Call 'shell32.dll::SHChangeNotify(i, i, i, i) v (0x08000000, 0, 0, 0)'

  ; Remove registry entries
  DeleteRegKey HKLM "${PRODUCT_UNINST_KEY}"

  ; Ask about user data removal
  MessageBox MB_YESNO "Remove user settings and data files?" IDNO +2
  RMDir /r "$APPDATA\CreateNewFile"

  SetAutoClose true
SectionEnd