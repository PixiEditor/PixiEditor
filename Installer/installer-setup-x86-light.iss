// contribute: https://github.com/domgho/InnoDependencyInstaller
// official article: https://www.codeproject.com/Articles/20868/Inno-Setup-Dependency-Installer

#define MyAppName "PixiEditor"
#define MyAppVersion GetFileVersion("..\Builds\PixiEditor-x64-light\PixiEditor\PixiEditor.exe")     ;Not perfect solution, it's enviroment dependend
#define MyAppPublisher "PixiEditor"
#define MyAppURL "https://github.com/PixiEditor/PixiEditor"
#define MyAppExeName "PixiEditor.exe"
#define TargetPlatform "x86-light"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{83DE4F2A-1F75-43AE-9546-3184F1C44517}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
VersionInfoVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
DisableProgramGroupPage=yes
; The [Icons] "quicklaunchicon" entry uses {userappdata} but its [Tasks] entry has a proper IsAdminInstallMode Check.
UsedUserAreasWarning=no
LicenseFile=..\LICENSE
; Uncomment the following line to run in non administrative install mode (install for current user only.)
;PrivilegesRequired=lowest
OutputDir=Assets\PixiEditor-{#TargetPlatform}
OutputBaseFilename=PixiEditor-{#MyAppVersion}-setup-x86
SetupIconFile=..\icon.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ChangesAssociations = yes

PrivilegesRequired=admin
ArchitecturesAllowed=x86
ArchitecturesInstallIn64BitMode=x64 ia64

// downloading and installing dependencies will only work if the memo/ready page is enabled (default and current behaviour)
DisableReadyPage=no
DisableReadyMemo=no

// requires netcorecheck.exe and netcorecheck_x64.exe in src dir
#define use_netcorecheck
#define use_netcore31
#define use_netcore31desktop
// supported languages
#include "scripts\lang\english.iss"
#include "scripts\lang\german.iss"
#include "scripts\lang\french.iss"
#include "scripts\lang\italian.iss"
#include "scripts\lang\dutch.iss"

#ifdef UNICODE
#include "scripts\lang\chinese.iss"
#include "scripts\lang\polish.iss"
#include "scripts\lang\russian.iss"
#include "scripts\lang\japanese.iss"
#endif

// shared code for installing the products
#include "scripts\products.iss"

// helper functions
#include "scripts\products\stringversion.iss"
#include "scripts\products\winversion.iss"
#include "scripts\products\dotnetfxversion.iss"

#ifdef use_netcorecheck
#include "scripts\products\netcorecheck.iss"
#endif
#ifdef use_netcore31
#include "scripts\products\netcore31.iss"
#endif
#ifdef use_netcore31desktop
#include "scripts\products\netcore31desktop.iss"
#endif

// content
[Tasks]
[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode

[Files]
Source: "..\Builds\PixiEditor-{#TargetPlatform}\PixiEditor\PixiEditor.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Builds\PixiEditor-{#TargetPlatform}\PixiEditor\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Registry]

Root: HKCR; Subkey: ".pixi";                             ValueData: "{#MyAppName}";          Flags: uninsdeletevalue; ValueType: string;  ValueName: ""
Root: HKCR; Subkey: "{#MyAppName}";                     ValueData: "Program {#MyAppName}";  Flags: uninsdeletekey;   ValueType: string;  ValueName: ""
Root: HKCR; Subkey: "{#MyAppName}\DefaultIcon";             ValueData: "{app}\{#MyAppExeName},0";               ValueType: string;  ValueName: ""
Root: HKCR; Subkey: "{#MyAppName}\shell\open\command";  ValueData: """{app}\{#MyAppExeName}"" ""%1""";  ValueType: string;  ValueName: ""

[CustomMessages]
DependenciesDir=MyProgramDependencies
WindowsServicePack=Windows %1 Service Pack %2

[Code]
function InitializeSetup(): Boolean;
begin
	// initialize windows version
	initwinversion();

#ifdef use_netcore31
	netcore31();
#endif
#ifdef use_netcore31desktop
	netcore31desktop();
#endif
	Result := true;
end;
