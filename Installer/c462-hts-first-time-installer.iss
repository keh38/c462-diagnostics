; -- Surface Setup Installer.iss --
; Sets up the nongame components on Surface

#include "file-locations.iss"

[Setup]
AppName=Hearing Test Suite
AppVerName=Hearing Test Suite
DefaultDirName={commonpf}\EPL\Diagnostics
OutputDir=Output
OutputBaseFilename=HTS_First_Time_Installer
UsePreviousAppDir=no
UsePreviousGroup=no
DisableDirPage = yes
DisableReadyPage=yes
DisableProgramGroupPage=yes
PrivilegesRequired=admin
AlwaysRestart=no

[Types]
Name: "full"; Description: "Full installation";
Name: "custom"; Description: "Custom installation"; Flags: iscustom

[Dirs]
Name: "{#RootMusicFolder}"; Permissions: users-full;
Name: "{#RootMusicFolder}{#BasicResourcesFolder}"; Permissions: users-full;
Name: "{#RootMusicFolder}{#SpeechWavFolder}"; Permissions: users-full;
Name: "{userdocs}\..\Music\EPL\Diagnostics"; Permissions: users-full;

[Components]
Name: "support"; Description: "Support files"; Types: full custom; Flags: restart;
Name: "basic"; Description: "Basic .wav files"; Types: full custom;
Name: "basic\babble"; Description: "IEEE babble components"; Check: FileExists('Diagnostics_IEEE_Babble.exe'); Types: full custom;
;Name: "sin"; Description: "Speech in noise"; Check: FileExists('QuickSIN.exe') or FileExists('BKBSIN.exe') or FileExists('WIN.exe'); Types: full custom;
;Name: "sin\quicksin"; Description: "QuickSIN"; Check: FileExists('QuickSIN.exe'); Types: full custom;
;Name: "sin\bkbsin"; Description: "BKBSIN"; Check: FileExists('BKBSIN.exe'); Types: full custom;
;Name: "sin\win"; Description: "WIN"; Check: FileExists('WIN.exe'); Types: full custom;
;Name: "azbio"; Description: "AzBio"; Check: FileExists('AzBioInstall.exe'); Types: full custom;
;Name: "ieee"; Description: "IEEE"; Check: FileExists('IEEE.exe'); Types: full custom;
;Name: "phonemes"; Description: "Phonemes"; Check: FileExists('Phonemes.exe'); Types: full custom;
;Name: "tablet"; Description: "Install tablet software ({code:TabletInstallerVersion})"; Check: CheckTabletInstaller; Types: full custom
;Name: "project"; Description: "Install project settings ({code:ProjectName})"; Check: CheckProjectInstaller; Types: full custom

[Files]
;Source: "D:\Development\Standalone Diagnostics\Tablet Network Interface\Tablet Network Interface\bin\Release\*.*"; DestDir: "{app}\TNI"; Flags: replacesameversion; Components: support;
Source: "Z:\keh\Tablet Audio Material\Output\.Wav Files\Basic\*.*"; DestDir: "{#RootMusicFolder}{#BasicResourcesFolder}"; Flags: replacesameversion recursesubdirs; Components: basic;
Source: "Z:\keh\Tablet Audio Material\Output\.Wav Files\Maskers\*.*"; DestDir: "{#RootMusicFolder}{#SpeechWavFolder}Maskers"; Flags: replacesameversion recursesubdirs; Components: basic;
Source: "Z:\keh\Tablet Audio Material\Output\.Wav Files\Calibration\*.*"; DestDir: "{#RootMusicFolder}{#BasicResourcesFolder}Calibration"; Flags: replacesameversion recursesubdirs; Components: basic;

[Registry]
; Make the Update Server run (invisibly) on start up
;Root: HKLM64; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "Tablet Network Interface"; ValueData: """{app}\TNI\Tablet Network Interface.exe"""; Flags: uninsdeletevalue; Components: support;
;Root: HKLM64; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Run"; ValueType: none; ValueName: "UpdateServer"; Flags: deletevalue; Components: support;
;Root: HKCU; Subkey: "SOFTWARE\MEEI\Diagnostics"; ValueType: dword; ValueName: "Screenmanager Is Fullscreen mode_h3981298716"; ValueData: "1"; Flags: uninsdeletevalue;

[Run]
;Filename: "{tmp}\{code:GetTNIInstaller}"; Parameters: "/VERYSILENT"; StatusMsg: "Installing network interface"; Components: support
;Filename: "{src}\{code:GetTabletInstaller}"; Parameters: "/SILENT"; StatusMsg: "Installing tablet software"; Components: tablet;
Filename: "{src}\Diagnostics_IEEE_Babble.exe"; Parameters: "/VERYSILENT"; StatusMsg: "Installing IEEE babble"; Components: basic\babble;
;Filename: "{src}\QuickSIN.exe"; Parameters: "/VERYSILENT"; StatusMsg: "Installing QuickSIN"; Components: sin\quicksin;
;Filename: "{src}\BKBSIN.exe"; Parameters: "/VERYSILENT"; StatusMsg: "Installing BKBSIN"; Components: sin\bkbsin;
;Filename: "{src}\WIN.exe"; Parameters: "/VERYSILENT"; StatusMsg: "Installing WIN"; Components: sin\win;
;Filename: "{src}\AzBioInstall.exe"; Parameters: "/VERYSILENT"; StatusMsg: "Installing AzBio sentences"; Components: azbio;
;Filename: "{src}\IEEE.exe"; Parameters: "/VERYSILENT"; StatusMsg: "Installing IEEE sentences"; Components: ieee;
;Filename: "{src}\Phonemes.exe"; Parameters: "/VERYSILENT"; StatusMsg: "Installing phonemes"; Components: phonemes;
;Filename: "{src}\{code:GetProjectInstaller}"; Parameters: "/VERYSILENT"; StatusMsg: "Installing project"; Components: project;

[Code]
var 
  TNIInstaller: String;
  TabletInstaller: String;
  ProjectInstaller: String;

function CheckTNIInstaller(): Boolean;
  var rec: TFindRec;
begin
  Result := false;
  TNIInstaller := '';
  
  if FindFirst('TNI_Installer_*', rec) then begin
    TNIInstaller := rec.Name;
    Result := true;
  end
end;

function GetTNIInstaller(Dummy: String): String;
begin
  Result := TNIInstaller;
end;

function CheckTabletInstaller(): Boolean;
  var rec: TFindRec;
begin
  Result := false;
  TabletInstaller := '';
  
  if FindFirst('Diagnostics_Desktop_*', rec) then begin
    TabletInstaller := rec.Name;
    Result := true;
  end
end;
function TNIInstallerVersion(Dummy: String): String;
var idot: Integer;
var iext: Integer;
var vers: String;
begin
  vers := ExtractFileName(TNIInstaller);
  idot := Pos('_', vers);
  iext := Pos('.exe', vers);
  vers := Copy(vers, idot+1, iext-idot-1);
  Result := vers;
end;


function GetTabletInstaller(Dummy: String): String;
begin
  Result := TabletInstaller;
end;

function TabletInstallerVersion(Dummy: String): String;
var idot: Integer;
var iext: Integer;
var vers: String;
begin
  vers := ExtractFileName(TabletInstaller);
  idot := Pos('_', vers);
  iext := Pos('.exe', vers);
  vers := Copy(vers, idot+1, iext-idot-1);
  Result := vers;
end;

function CheckProjectInstaller(): Boolean;
  var rec: TFindRec;
begin
  Result := false;
  ProjectInstaller := '';
  
  if FindFirst('ProjectInstaller_*', rec) then begin
    ProjectInstaller := rec.Name;
    Result := true;
  end
end;

function GetProjectInstaller(Dummy: String): String;
begin
  Result := ProjectInstaller;
end;

function ProjectName(Dummy: String): String;
var idot: Integer;
var iext: Integer;
var name: String;
begin
  name := ExtractFileName(ProjectInstaller);
  idot := Pos('_', name);
  iext := Pos('.exe', name);
  name := Copy(name, idot+1, iext-idot-1);
  Result := name;
end;

