#define MyAppName "GameSaveEditor"
#define MyAppVersion "2.0.0"
#define MyAppPublisher "Ogunrinde Enoch"
#define MyAppExeName "GameSaveEditor.exe"

#define PublishDir "C:\Users\Enoch\source\repos\GameSaveEditor\GameSaveEditor\bin\Release\net10.0-windows10.0.19041.0\win-x64\publish"

[Setup]
AppId={{7DCEB3D8-6C4A-4E6E-B9B0-1D3A7A2D5D2B}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}

AppPublisher={#MyAppPublisher}

DefaultDirName={autopf}\Game Save Editor
DefaultGroupName={#MyAppName}

DisableProgramGroupPage=yes

OutputDir=..\Build\Installer
OutputBaseFilename=GameSaveEditor-{#MyAppVersion}-Setup

Compression=lzma2
SolidCompression=yes

WizardStyle=modern

SetupIconFile=GameSaveEditor.ico
UninstallDisplayIcon={app}\{#MyAppExeName}

ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

PrivilegesRequired=admin

Uninstallable=yes
SetupLogging=yes

VersionInfoVersion={#MyAppVersion}
VersionInfoDescription={#MyAppName} Windows Installer
VersionInfoProductName={#MyAppName}
VersionInfoProductVersion={#MyAppVersion}

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent