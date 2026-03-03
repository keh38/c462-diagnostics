# release.ps1
# Usage: .\release.ps1
# Automates the full release process for c462-turandot-editor

$ErrorActionPreference = "Stop"

# Ensure system PATH is loaded (fixes tools like gh that are visible in cmd but not PowerShell)
$env:Path = [System.Environment]::GetEnvironmentVariable("Path", "Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path", "User")

# --- Configuration -----------------------------------------------------------
$Version        = "1.12"
$RepoRoot       = "D:\Development\C462\c462-diagnostics"
$CsprojFile     = "$RepoRoot\Launcher\Launcher\Launcher.csproj"
$Changelog      = "$RepoRoot\CHANGELOG.md"
$SolutionFile   = "$RepoRoot\Launcher\Launcher.sln"
$InstallerPath  = "$RepoRoot\Installer\Output\Hearing_Test_Suite_1-12.exe"
$ReleaseDate    = (Get-Date -Format "yyyy-MM-dd")
$CommitMessage  = "Built $Version"
$TagName        = $Version
$ReleaseTitle   = "v$Version"
# -----------------------------------------------------------------------------

function Step($msg) {
    Write-Host "`n==> $msg" -ForegroundColor Cyan
}

# --- Step 1: Update .csproj version ------------------------------------------
Step "Updating Launcher.csproj to version $Version"

$csproj = Get-Content $CsprojFile -Raw
$csproj = $csproj -replace '<Version>[^<]*</Version>',         "<Version>$Version</Version>"
$csproj = $csproj -replace '<AssemblyVersion>[^<]*</AssemblyVersion>', "<AssemblyVersion>$Version.0.0</AssemblyVersion>"
$csproj = $csproj -replace '<FileVersion>[^<]*</FileVersion>', "<FileVersion>$Version.0.0</FileVersion>"
Set-Content $CsprojFile $csproj -NoNewline
Write-Host "Done."

# --- Step 2: Update CHANGELOG.md (replace '(unreleased)' with today's date) -
Step "Updating CHANGELOG.md - replacing '(unreleased)' with today's date"

$cl = Get-Content $Changelog -Raw
if ($cl -notmatch "\(unreleased\)") {
    Write-Warning "No '(unreleased)' marker found in CHANGELOG.md - skipping date update."
} else {
    $cl = $cl -replace "\(unreleased\)", "($ReleaseDate)"
    Set-Content $Changelog $cl -NoNewline
    Write-Host "Done."
}

# --- Step 3: Extract release notes from CHANGELOG ----------------------------
Step "Extracting release notes for v$Version from CHANGELOG.md"

$cl = Get-Content $Changelog -Raw
$match = [regex]::Match(
    $cl,
    "(?ms)(### v$([regex]::Escape($Version))\s.*?)(?=\r?\n### v|\z)"
)
if (-not $match.Success) {
    Write-Error "Could not find changelog entry for v$Version. Aborting."
    exit 1
}
$ReleaseNotes = ($match.Value.Trim() -split "\r?\n", 2)[1].Trim()
$ReleaseNotes = ($ReleaseNotes -split "\r?\n" | Where-Object { $_ -notmatch "^---$" }) -join "`n"
Write-Host "Release notes:`n$ReleaseNotes"

# --- Step 4: Build the solution ----------------------------------------------
Step "Building solution in Release mode"

& "${env:ProgramFiles}\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" `
    $SolutionFile `
    /p:Configuration=Release `
    /p:Platform=x64 `
    /v:quiet

if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed. Aborting."
    exit 1
}
Write-Host "Build succeeded."

# --- Step 5: Verify installer exists -----------------------------------------
Step "Checking for installer at: $InstallerPath"

if (-not (Test-Path $InstallerPath)) {
    Write-Error "Installer not found at '$InstallerPath'. Did Inno Setup run? Aborting."
    exit 1
}
Write-Host "Installer found."

# --- Step 6: Git commit and push ---------------------------------------------
Step "Committing and pushing changes"

Push-Location $RepoRoot
git add "$CsprojFile" "$Changelog"
git commit -m $CommitMessage
git push
Pop-Location
Write-Host "Pushed to GitHub."

# --- Step 7: Create GitHub release -------------------------------------------
Step "Creating GitHub release v$Version"

Push-Location $RepoRoot
gh release create $TagName $InstallerPath `
    --title $ReleaseTitle `
    --notes $ReleaseNotes
Pop-Location

Write-Host "`n[OK] Release $Version complete!" -ForegroundColor Green
