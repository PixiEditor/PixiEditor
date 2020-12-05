// contribute: https://github.com/DomGries/InnoDependencyInstaller
// official article: https://codeproject.com/Articles/20868/Inno-Setup-Dependency-Installer

// requires netcorecheck.exe and netcorecheck_x64.exe (see download link below)
#define UseNetCoreCheck
#ifdef UseNetCoreCheck
  ;#define UseDotNet50
  #define UseDotNet50Desktop
#endif

// custom setup info
#define MyAppName "PixiEditor"
#define MyAppVersion GetFileVersion("..\Builds\PixiEditor-x86-light\PixiEditor\PixiEditor.exe")     ;Not perfect solution, it's enviroment dependend
#define MyAppPublisher "PixiEditor"
#define MyAppURL "https://github.com/PixiEditor/PixiEditor"
#define MyAppExeName "PixiEditor.exe"
#define TargetPlatform "x86-light"

[Setup]
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

MinVersion=6.0
PrivilegesRequired=admin

// dependency installation requires ready page and ready memo to be enabled (default behaviour)
DisableReadyPage=no
DisableReadyMemo=no


// shared code for installing the dependencies
[Code]
// types and variables
type
  TDependency = record
    Filename: String;
    Parameters: String;
    Title: String;
    URL: String;
    Checksum: String;
    ForceSuccess: Boolean;
    InstallClean: Boolean;
    RebootAfter: Boolean;
  end;

  InstallResult = (InstallSuccessful, InstallRebootRequired, InstallError);

var
  MemoInstallInfo: String;
  Dependencies: array of TDependency;
  DelayedReboot, ForceX86: Boolean;
  DownloadPage: TDownloadWizardPage;

procedure AddDependency(const Filename, Parameters, Title, URL, Checksum: String; const ForceSuccess, InstallClean, RebootAfter: Boolean);
var
  Dependency: TDependency;
  I: Integer;
begin
  MemoInstallInfo := MemoInstallInfo + #13#10 + '%1' + Title;

  Dependency.Filename := Filename;
  Dependency.Parameters := Parameters;
  Dependency.Title := Title;

  if FileExists(ExpandConstant('{tmp}{\}') + Filename) then begin
    Dependency.URL := '';
  end else begin
    Dependency.URL := URL;
  end;

  Dependency.Checksum := Checksum;
  Dependency.ForceSuccess := ForceSuccess;
  Dependency.InstallClean := InstallClean;
  Dependency.RebootAfter := RebootAfter;

  I := GetArrayLength(Dependencies);
  SetArrayLength(Dependencies, I + 1);
  Dependencies[I] := Dependency;
end;

function IsPendingReboot: Boolean;
var
  Value: String;
begin
  Result := RegQueryMultiStringValue(HKEY_LOCAL_MACHINE, 'SYSTEM\CurrentControlSet\Control\Session Manager', 'PendingFileRenameOperations', Value) or
    (RegQueryMultiStringValue(HKEY_LOCAL_MACHINE, 'SYSTEM\CurrentControlSet\Control\Session Manager', 'SetupExecute', Value) and (Value <> ''));
end;

function InstallProducts: InstallResult;
var
  ResultCode, I, ProductCount: Integer;
begin
  Result := InstallSuccessful;
  ProductCount := GetArrayLength(Dependencies);
  MemoInstallInfo := SetupMessage(msgReadyMemoTasks);

  if ProductCount > 0 then begin
    DownloadPage.Show;

    for I := 0 to ProductCount - 1 do begin
      if Dependencies[I].InstallClean and (DelayedReboot or IsPendingReboot) then begin
        Result := InstallRebootRequired;
        break;
      end;

      DownloadPage.SetText(Dependencies[I].Title, '');
      DownloadPage.SetProgress(I + 1, ProductCount);

      while True do begin
        ResultCode := 0;
        if ShellExec('', ExpandConstant('{tmp}{\}') + Dependencies[I].Filename, Dependencies[I].Parameters, '', SW_SHOWNORMAL, ewWaitUntilTerminated, ResultCode) then begin
          if Dependencies[I].RebootAfter then begin
            // delay reboot after install if we installed the last dependency anyways
            if I = ProductCount - 1 then begin
              DelayedReboot := True;
            end else begin
              Result := InstallRebootRequired;
              MemoInstallInfo := Dependencies[I].Title;
            end;
            break;
          end else if (ResultCode = 0) or Dependencies[I].ForceSuccess then begin
            break;
          end else if ResultCode = 3010 then begin
            // Windows Installer ResultCode 3010: ERROR_SUCCESS_REBOOT_REQUIRED
            DelayedReboot := True;
            break;
          end;
        end;

        case SuppressibleMsgBox(FmtMessage(SetupMessage(msgErrorFunctionFailed), [Dependencies[I].Title, IntToStr(ResultCode)]), mbError, MB_ABORTRETRYIGNORE, IDIGNORE) of
          IDABORT: begin
            Result := InstallError;
            MemoInstallInfo := MemoInstallInfo + #13#10 + '      ' + Dependencies[I].Title;
            break;
          end;
          IDIGNORE: begin
            MemoInstallInfo := MemoInstallInfo + #13#10 + '      ' + Dependencies[I].Title;
            break;
          end;
        end;
      end;

      if Result <> InstallSuccessful then begin
        break;
      end;
    end;

    DownloadPage.Hide;
  end;
end;

// Inno Setup event functions
procedure InitializeWizard;
begin
  DownloadPage := CreateDownloadPage(SetupMessage(msgWizardPreparing), SetupMessage(msgPreparingDesc), nil);
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  I: Integer;
begin
  DelayedReboot := False;

  case InstallProducts of
    InstallError: begin
      Result := MemoInstallInfo;
    end;
    InstallRebootRequired: begin
      Result := MemoInstallInfo;
      NeedsRestart := True;

      // write into the registry that the installer needs to be executed again after restart
      RegWriteStringValue(HKEY_CURRENT_USER, 'SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce', 'InstallBootstrap', ExpandConstant('{srcexe}'));
    end;
  end;
end;

function NeedRestart: Boolean;
begin
  Result := DelayedReboot;
end;

function UpdateReadyMemo(const Space, NewLine, MemoUserInfoInfo, MemoDirInfo, MemoTypeInfo, MemoComponentsInfo, MemoGroupInfo, MemoTasksInfo: String): String;
begin
  Result := '';
  if MemoUserInfoInfo <> '' then begin
    Result := Result + MemoUserInfoInfo + Newline + NewLine;
  end;
  if MemoDirInfo <> '' then begin
    Result := Result + MemoDirInfo + Newline + NewLine;
  end;
  if MemoTypeInfo <> '' then begin
    Result := Result + MemoTypeInfo + Newline + NewLine;
  end;
  if MemoComponentsInfo <> '' then begin
    Result := Result + MemoComponentsInfo + Newline + NewLine;
  end;
  if MemoGroupInfo <> '' then begin
    Result := Result + MemoGroupInfo + Newline + NewLine;
  end;
  if MemoTasksInfo <> '' then begin
    Result := Result + MemoTasksInfo;
  end;

  if MemoInstallInfo <> '' then begin
    if MemoTasksInfo = '' then begin
      Result := Result + SetupMessage(msgReadyMemoTasks);
    end;
    Result := Result + FmtMessage(MemoInstallInfo, [Space]);
  end;
end;

function NextButtonClick(const CurPageID: Integer): Boolean;
var
  I, ProductCount: Integer;
  Retry: Boolean;
begin
  Result := True;

  if (CurPageID = wpReady) and (MemoInstallInfo <> '') then begin
    DownloadPage.Show;

    ProductCount := GetArrayLength(Dependencies);
    for I := 0 to ProductCount - 1 do begin
      if Dependencies[I].URL <> '' then begin
        DownloadPage.Clear;
        DownloadPage.Add(Dependencies[I].URL, Dependencies[I].Filename, Dependencies[I].Checksum);

        Retry := True;
        while Retry do begin
          Retry := False;

          try
            DownloadPage.Download;
          except
            if GetExceptionMessage = SetupMessage(msgErrorDownloadAborted) then begin
              Result := False;
              I := ProductCount;
            end else begin
              case SuppressibleMsgBox(AddPeriod(GetExceptionMessage), mbError, MB_ABORTRETRYIGNORE, IDIGNORE) of
                IDABORT: begin
                  Result := False;
                  I := ProductCount;
                end;
                IDRETRY: begin
                  Retry := True;
                end;
              end;
            end;
          end;
        end;
      end;
    end;

    DownloadPage.Hide;
  end;
end;

// architecture helper functions
function IsX64: Boolean;
begin
  Result := not ForceX86 and Is64BitInstallMode;
end;

function GetString(const x86, x64: String): String;
begin
  if IsX64 then begin
    Result := x64;
  end else begin
    Result := x86;
  end;
end;

function GetArchitectureSuffix: String;
begin
  Result := GetString('', '_x64');
end;

function GetArchitectureTitle: String;
begin
  Result := GetString(' (x86)', ' (x64)');
end;

function CompareVersion(const Version1, Version2: String): Integer;
var
  Position, Number1, Number2: Integer;
begin
  Result := 0;
  while (Version1 <> '') or (Version2 <> '') do begin
    Position := Pos('.', Version1);
    if Position > 0 then begin
      Number1 := StrToIntDef(Copy(Version1, 1, Position - 1), 0);
      Delete(Version1, 1, Position);
    end else if Version1 <> '' then begin
      Number1 := StrToIntDef(Version1, 0);
      Version1 := '';
    end else begin
      Number1 := 0;
    end;

    Position := Pos('.', Version2);
    if Position > 0 then begin
      Number2 := StrToIntDef(Copy(Version2, 1, Position - 1), 0);
      Delete(Version2, 1, Position);
    end else if Version2 <> '' then begin
      Number2 := StrToIntDef(Version2, 0);
      Version2 := '';
    end else begin
      Number2 := 0;
    end;

    if Number1 < Number2 then begin
      Result := -1;
      break;
    end else if Number1 > Number2 then begin
      Result := 1;
      break;
    end;
  end;
end;

#ifdef UseNetCoreCheck
// https://github.com/dotnet/deployment-tools/tree/master/src/clickonce/native/projects/NetCoreCheck
function IsNetCoreInstalled(const Version: String): Boolean;
var
  ResultCode: Integer;
begin
  if not FileExists(ExpandConstant('{tmp}{\}') + 'netcorecheck' + GetArchitectureSuffix + '.exe') then begin
    ExtractTemporaryFile('netcorecheck' + GetArchitectureSuffix + '.exe');
  end;
  Result := ShellExec('', ExpandConstant('{tmp}{\}') + 'netcorecheck' + GetArchitectureSuffix + '.exe', Version, '', SW_HIDE, ewWaitUntilTerminated, ResultCode) and (ResultCode = 0);
end;
#endif

// custom setup content
[Languages]
Name: en; MessagesFile: "compiler:Default.isl"
Name: nl; MessagesFile: "compiler:Languages\Dutch.isl"
Name: de; MessagesFile: "compiler:Languages\German.isl"

[Files]
#ifdef UseNetCoreCheck
// download netcorecheck.exe: https://go.microsoft.com/fwlink/?linkid=2135256
// download netcorecheck_x64.exe: https://go.microsoft.com/fwlink/?linkid=2135504
Source: "netcorecheck.exe"; Flags: dontcopy noencryption
Source: "netcorecheck_x64.exe"; Flags: dontcopy noencryption
#endif

Source: "..\Builds\PixiEditor-{#TargetPlatform}\PixiEditor\PixiEditor.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Builds\PixiEditor-{#TargetPlatform}\PixiEditor\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Registry]

Root: HKCR; Subkey: ".pixi";                             ValueData: "{#MyAppName}";          Flags: uninsdeletevalue; ValueType: string;  ValueName: ""
Root: HKCR; Subkey: "{#MyAppName}";                     ValueData: "Program {#MyAppName}";  Flags: uninsdeletekey;   ValueType: string;  ValueName: ""
Root: HKCR; Subkey: "{#MyAppName}\DefaultIcon";             ValueData: "{app}\{#MyAppExeName},0";               ValueType: string;  ValueName: ""
Root: HKCR; Subkey: "{#MyAppName}\shell\open\command";  ValueData: """{app}\{#MyAppExeName}"" ""%1""";  ValueType: string;  ValueName: ""

[Code]
function InitializeSetup: Boolean;
var
  Version: String;
begin

#ifdef UseDotNet50
  // https://dotnet.microsoft.com/download/dotnet/5.0
  if not IsNetCoreInstalled('Microsoft.NETCore.App 5.0.0') then begin
    AddDependency('dotnet50' + GetArchitectureSuffix + '.exe',
      '/lcid ' + IntToStr(GetUILanguage) + ' /passive /norestart',
      '.NET Runtime 5.0' + GetArchitectureTitle,
      GetString('https://download.visualstudio.microsoft.com/download/pr/a7e15da3-7a15-43c2-a481-cf50bf305214/c69b951e8b47101e90b1289c387bb01a/dotnet-runtime-5.0.0-win-x86.exe', 'https://download.visualstudio.microsoft.com/download/pr/36a9dc4e-1745-4f17-8a9c-f547a12e3764/ae25e38f20a4854d5e015a88659a22f9/dotnet-runtime-5.0.0-win-x64.exe'),
      '', False, False, False);
  end;
#endif

#ifdef UseDotNet50Desktop
  // https://dotnet.microsoft.com/download/dotnet/5.0
  if not IsNetCoreInstalled('Microsoft.WindowsDesktop.App 5.0.0') then begin
    AddDependency('dotnet50desktop' + GetArchitectureSuffix + '.exe',
      '/lcid ' + IntToStr(GetUILanguage) + ' /passive /norestart',
      '.NET Desktop Runtime 5.0' + GetArchitectureTitle,
      GetString('https://download.visualstudio.microsoft.com/download/pr/b2780d75-e54a-448a-95fc-da9721b2b4c2/62310a9e9f0ba7b18741944cbae9f592/windowsdesktop-runtime-5.0.0-win-x86.exe', 'https://download.visualstudio.microsoft.com/download/pr/1b3a8899-127a-4465-a3c2-7ce5e4feb07b/1e153ad470768baa40ed3f57e6e7a9d8/windowsdesktop-runtime-5.0.0-win-x64.exe'),
      '', False, False, False);
  end;
#endif

  Result := True;
end;
