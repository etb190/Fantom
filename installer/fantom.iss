; Fantom — Inno Setup installer script
; Built by GitHub Actions:
;   iscc installer\fantom.iss /DVersion=1.0.0 /DBinDir=..\publish\win-x64

#define AppName "Fantom"
#define AppPublisher "Fantom"
#define AppExeName "Fantom.exe"

#ifndef Version
#define Version "0.0.0-dev"
#endif

#ifndef BinDir
#define BinDir "..\publish\win-x64"
#endif

[Setup]
AppId={{17F689BD-4A76-4E98-88F1-FC9D17A4965C}
AppName={#AppName}
AppVersion={#Version}
AppVerName={#AppName} {#Version}
AppPublisher={#AppPublisher}
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
OutputDir=..\dist
OutputBaseFilename=Fantom-Setup-{#Version}
SetupIconFile=..\src\Fantom\Assets\app-icon.ico
UninstallDisplayIcon={app}\{#AppExeName}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
PrivilegesRequiredOverridesAllowed=dialog commandline
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=6.3

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; \
    GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#BinDir}\*"; Excludes: "*.pdb"; DestDir: "{app}"; \
    Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{group}\Uninstall {#AppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#AppName}}"; \
    Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Remove program files only. User data in %LOCALAPPDATA%\Fantom is NEVER touched.
Type: filesandordirs; Name: "{app}"
