[CmdletBinding()]
param(
    [ValidateSet('Debug','Release')][string]$Configuration = 'Debug',
    [switch]$BuildInstaller
)
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
$taskSource = Join-Path $taskRoot 'src/proxifyre'
$taskMsbuild = (Get-Command msbuild.exe -ErrorAction SilentlyContinue).Source
if (-not $taskMsbuild) {
    $taskVswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
    if (Test-Path $taskVswhere) {
        $taskMsbuild = & $taskVswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild/Current/Bin/MSBuild.exe' | Select-Object -First 1
    }
}
if (-not $taskMsbuild) { throw 'VS 2022 MSBuild with v143/C++ CLI and .NET 4.7.2 targeting pack is required.' }
$taskNuget = (Get-Command nuget.exe -ErrorAction SilentlyContinue).Source
if (-not $taskNuget) { throw 'NuGet CLI is required.' }
function Invoke-BuildCommand {
    param([string]$Executable, [string[]]$Arguments)
    & $Executable @Arguments
    if ($LASTEXITCODE -ne 0) { throw "Build command failed ($LASTEXITCODE): $Executable $($Arguments -join ' ')" }
}
Push-Location $taskSource
try {
    foreach ($taskProject in @('ProxiFyre.Configuration','ProxiFyre','ProxiFyre.Tests','netlib-tests')) {
        Invoke-BuildCommand $taskNuget @('restore',"$taskProject/packages.config",'-PackagesDirectory','packages','-NonInteractive')
    }
    foreach ($taskProject in @('ProxiFyreSetupBootstrapper','ProxiFyreSetupEngineExtension')) {
        Invoke-BuildCommand $taskMsbuild @("$taskProject/$taskProject.vcxproj",'-t:Restore','-v:minimal','-p:RestoreLockedMode=true',"-p:RestoreConfigFile=$taskSource/NuGet.Installer.Config")
    }
    Invoke-BuildCommand $taskMsbuild @('socksify.sln','-t:Build','-v:minimal',"-p:Configuration=$Configuration",'-p:Platform=x64','-p:Repository=KennyChaos7/codex_proxifier')
    if ($BuildInstaller) {
        if ($Configuration -ne 'Release') { throw 'Installer requires Configuration=Release.' }
        $taskDotnet = (Get-Command dotnet.exe -ErrorAction Stop).Source
        foreach ($taskProject in @('ProxiFyre.Installer','ProxiFyre.Bundle')) {
            Invoke-BuildCommand $taskDotnet @('restore',"$taskProject/$taskProject.wixproj",'--configfile','NuGet.Installer.Config','--locked-mode')
        }
        [xml]$taskProps = Get-Content 'Directory.Build.props'
        $taskVersion = [string]$taskProps.Project.PropertyGroup.ProxiFyreReleaseVersion
        & (Join-Path $taskSource 'scripts/Build-Installer.ps1') -Platform x64 -Version $taskVersion -NoRestore -Force
        if ($LASTEXITCODE -ne 0) { throw "Installer build failed: $LASTEXITCODE" }
    }
}
finally { Pop-Location }

