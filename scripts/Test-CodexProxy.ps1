param([ValidateSet('Debug','Release')][string]$Configuration = 'Release')
$ErrorActionPreference = 'Stop'
$taskRoot = Split-Path $PSScriptRoot -Parent
$taskSource = Join-Path $taskRoot 'src/proxifyre'
$taskVswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio/Installer/vswhere.exe'
$taskVs = & $taskVswhere -latest -products '*' -requires Microsoft.Component.MSBuild -property installationPath
$taskVstest = Join-Path $taskVs 'Common7/IDE/CommonExtensions/Microsoft/TestWindow/vstest.console.exe'
$taskResults = Join-Path $taskRoot 'artifacts/test-results'
New-Item -ItemType Directory -Path $taskResults -Force | Out-Null
$taskTests = Join-Path $taskSource "bin/tests/x64/$Configuration/ProxiFyre.Tests.dll"
$taskAdapter = Join-Path $taskSource 'packages/NUnit3TestAdapter.4.6.0/build/net462'
& $taskVstest $taskTests '/Platform:x64' "/TestAdapterPath:$taskAdapter" "/ResultsDirectory:$taskResults" '/Logger:trx;LogFileName=CodexProxy.trx'
if ($LASTEXITCODE -ne 0) { throw "Managed tests failed: $LASTEXITCODE" }
$taskNative = Join-Path $taskSource "bin/tests/x64/$Configuration/netlib-tests.exe"
& $taskNative "--gtest_filter=OwnerRoutingPolicyTest.*" "--gtest_output=xml:$taskResults/native-tests.xml"
if ($LASTEXITCODE -ne 0) { throw "Native routing tests failed: $LASTEXITCODE" }
[xml]$taskManifest = Get-Content (Join-Path $taskSource 'ProxiFyreUI/app.manifest')
$taskUac = $taskManifest.SelectSingleNode('//*[local-name()="requestedExecutionLevel"]')
if ($taskUac.level -ne 'requireAdministrator' -or $taskUac.uiAccess -ne 'false') { throw 'GUI must request administrator at startup.' }
Write-Output 'Codex Proxy tests completed.'


