; -- Diagnostics project installer --
; Sets up the nongame components on Surface

#define Project "HypDiagnostics"

[Setup]
AppName={#Project} Project Installer
AppVerName=Diagnostics
DefaultDirName={pf}
OutputDir=Output
OutputBaseFilename=ProjectInstaller_{#Project}
UsePreviousAppDir=no
UsePreviousGroup=no
DisableDirPage = yes
DisableReadyPage=yes
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
AlwaysRestart=no

[Dirs]
;Name: "{userdocs}\..\Music\EPL\Diagnostics"; Permissions: users-full;
Name: "{userappdata}\..\LocalLow\Eaton-Peabody Labs\Hearing Test Suite\Projects\"; Permissions: users-full;
Name: "{userappdata}\..\LocalLow\Eaton-Peabody Labs\Hearing Test Suite\Projects\{#Project}"; Permissions: users-full;
Name: "{userappdata}\..\LocalLow\Eaton-Peabody Labs\Hearing Test Suite\Projects\{#Project}\Subjects"; Permissions: users-full;
;Name: "{userdocs}\..\Music\EPL\Diagnostics\{#Project}\Upload"; Permissions: users-full;

[Files]
Source: "Z:\keh\Diagnostics\{#Project}\Resources\*.*"; DestDir: "{userappdata}\..\LocalLow\Eaton-Peabody Labs\Hearing Test Suite\Projects\{#Project}\Resources"; Flags: replacesameversion recursesubdirs;
;Source: "C:\Users\hancock\Music\EPL\Diagnostics\{#Project}\Subjects\*.*"; DestDir: "{userdocs}\..\Music\EPL\Diagnostics\{#Project}\Subjects"; Flags: replacesameversion recursesubdirs;
;Source: "C:\Users\hancock\Music\EPL\Diagnostics\state.xml"; DestDir: "{userdocs}\..\Music\EPL\Diagnostics"; Flags: replacesameversion
