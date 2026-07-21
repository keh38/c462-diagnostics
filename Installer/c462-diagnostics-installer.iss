; -- sync.iss --

; SEE THE DOCUMENTATION FOR DETAILS ON CREATING .ISS SCRIPT FILES!
#define SemanticVersion() \
   GetVersionComponents("..\Launcher\Launcher\bin\Release\net8.0-windows\HTSLauncher.exe", Local[0], Local[1], Local[2], Local[3]), \
   Str(Local[0]) + "." + Str(Local[1]) + ((Local[2]>0) ? "." + Str(Local[2]) : "")
    
#define verStr_ StringChange(SemanticVersion(), '.', '-')
#define DevRoot GetEnv("DEVROOT")
#if DevRoot == ""
    #error "DEVROOT environment variable is not set"
#endif

[Setup]
AppName=Hearing Test Suite
AppVerName=Hearing Test Suite V{#semanticVersion}
DefaultDirName={commonpf}\EPL\Hearing Test Suite\V{#SemanticVersion}
OutputDir=Output
DefaultGroupName=EPL
AllowNoIcons=yes
OutputBaseFilename=Hearing_Test_Suite_{#verStr_}
UsePreviousAppDir=no
UsePreviousGroup=no
UsePreviousSetupType=no
DisableProgramGroupPage=yes
PrivilegesRequired=admin
CloseApplications=yes
RestartApplications=yes

[Files]
Source: "{#DevRoot}\C462\c462-shared\Installer\Output\C462SharedSubjectSetup.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall
Source: "..\Diagnostics\Build\*.*"; DestDir: "{app}"; Flags: replacesameversion recursesubdirs;
Source: ".\Dependencies\Mono\*.*"; DestDir: "{app}"; Flags: replacesameversion;
Source: "..\Launcher\Launcher\bin\Release\net8.0-windows\*.*"; DestDir: "{app}\Launcher"; Flags: replacesameversion recursesubdirs;
Source: "..\Launcher\Restarter\bin\Release\net8.0-windows\*.*"; DestDir: "{app}\Restarter"; Flags: replacesameversion recursesubdirs;
Source: "..\CHANGELOG.md"; DestDir: "{app}"; Flags: replacesameversion;

[Run]
[Icons]
Name: "{commondesktop}\Hearing Test Suite"; Filename: "{app}\Launcher\HTSLauncher.exe"; IconFilename: "{app}\Launcher\Diagnostics.ico"; IconIndex: 0;

[Registry]
Root: HKLM64; Subkey: "Software\EPL"; Flags: uninsdeletekeyifempty
Root: HKLM64; Subkey: "Software\EPL\C462"; Flags: uninsdeletekey
Root: HKLM64; Subkey: "Software\EPL\C462\HTS"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"

[Run]
Filename: "{tmp}\C462SharedSubjectSetup.exe"; Parameters: "/SILENT"; Description: "Installing shared components"; Flags: waituntilterminated
Filename: "{app}\Launcher\HTSLauncher.exe"; Parameters: "-nodelay"; 


