; -- Surface Setup Installer.iss --
; Sets up the nongame components on Surface

#include "CommonInfo.iss"

#define WhichTest "QuickSIN"
;#define WhichTest "BKBSIN"
;#define WhichTest "WIN"
;#define WhichTest "IEEE"
;#define WhichTest "HINT"

[Setup]
AppName={#WhichTest}
AppVerName={#WhichTest}
DefaultDirName={commonpf}\{#ServerAppFolder}
OutputDir=Output
OutputBaseFilename={#WhichTest}
UsePreviousAppDir=no
UsePreviousGroup=no
DisableDirPage = yes
DisableReadyPage=yes
DisableProgramGroupPage=yes
PrivilegesRequired=admin
AlwaysRestart=no

[Files]
Source: "Z:\keh\Tablet Audio Material\Output\.Wav Files\{#WhichTest}\*.*"; DestDir: "{#RootMusicFolder}{#SpeechWavFolder}\{#WhichTest}"; Flags: replacesameversion;

