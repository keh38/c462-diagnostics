; -- Surface Setup Installer.iss --
; Sets up the nongame components on Surface

#include "file-locations.iss"

[Setup]
AppName=Digits Test
AppVerName=Digits Test
DefaultDirName={commonpf}\EPL\HTS
OutputDir=Output
OutputBaseFilename=Digits_Test_Installer
UsePreviousAppDir=no
UsePreviousGroup=no
DisableDirPage = yes
DisableReadyPage=yes
DisableProgramGroupPage=yes
PrivilegesRequired=admin
AlwaysRestart=no


[Dirs]
Name: "{#RootMusicFolder}"; Permissions: users-full;
Name: "{#RootMusicFolder}{#BasicResourcesFolder}"; Permissions: users-full;


[Files]
Source: "Z:\keh\Tablet Audio Material\Output\.Wav Files\Basic\Digits\*.*"; DestDir: "{#RootMusicFolder}{#BasicResourcesFolder}Digits"; Flags: replacesameversion recursesubdirs;
