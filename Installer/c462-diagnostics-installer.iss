; -- sync.iss --

; SEE THE DOCUMENTATION FOR DETAILS ON CREATING .ISS SCRIPT FILES!
#define SemanticVersion() \
   GetVersionComponents("..\Launcher\Launcher\bin\Release\HTSLauncher.exe", Local[0], Local[1], Local[2], Local[3]), \
   Str(Local[0]) + "." + Str(Local[1]) + ((Local[2]>0) ? "." + Str(Local[2]) : "")
    
#define verStr_ StringChange(SemanticVersion(), '.', '-')

[Setup]
AppName=Hearing Test Suite
AppVerName=Hearing Test Suite V{#semanticVersion}
DefaultDirName={commonpf}\EPL\Hearing Test Suite
OutputDir=Output
DefaultGroupName=EPL
AllowNoIcons=yes
OutputBaseFilename=Hearing_Test_Suite_{#verStr_}
UsePreviousAppDir=no
UsePreviousGroup=no
UsePreviousSetupType=no
DisableProgramGroupPage=yes
PrivilegesRequired=admin

[Files]
Source: "..\Diagnostics\Build\*.*"; DestDir: "{app}"; Flags: replacesameversion recursesubdirs;
Source: "..\Launcher\Launcher\bin\Release\*.*"; DestDir: "{app}\Launcher"; Flags: replacesameversion recursesubdirs;
;Source: "D:\Development\C462\c462-odi\Installer\Output\ODI_Installer_1-0.exe"; DestDir: "{tmp}";

[Icons]
;Name: "{commondesktop}\Hearing Diagnostics"; Filename: "{app}\HearingDiagnostics.exe";
Name: "{commondesktop}\Hearing Test Suite"; Filename: "{app}\Launcher\HTSLauncher.exe"; IconFilename: "{app}\Launcher\Diagnostics.ico"; IconIndex: 0;

[Registry]
;Root: HKCU; Subkey: "SOFTWARE\MEEI\HearingDiagnostics"; ValueType: dword; ValueName: "Screenmanager Is Fullscreen mode_h3981298716"; ValueData: "1"; Flags: uninsdeletevalue;
;Root: HKCU; Subkey: "SOFTWARE\MEEI\HearingDiagnostics"; ValueType: none; ValueName: "Screenmanager Resolution Width_h182942802"; Flags: deleteValue
;Root: HKCU; Subkey: "SOFTWARE\MEEI\HearingDiagnostics"; ValueType: none; ValueName: "Screenmanager Resolution Height_h2627697771"; Flags: deleteValue

[Run]
;Filename: "{tmp}\{code:GetODIInstaller}"; Parameters: "/VERYSILENT"; StatusMsg: "Installing OneDrive interface";

[Code]
function GetODIInstaller(Dummy: String): String;
  var rec: TFindRec;
  var installerName: String;
begin
  installerName := '';
  
  if FindFirst(ExpandConstant('{tmp}\ODI_Installer_*'), rec) then begin
    installerName := rec.Name;
  end;

  Result := installerName;
end;

