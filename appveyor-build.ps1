$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Get-MSBuildPath {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    if (-not (Test-Path $vswhere)) {
        throw "vswhere.exe was not found at '$vswhere'."
    }

    $msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
    if (-not $msbuild) {
        throw 'MSBuild.exe could not be located.'
    }

    return $msbuild
}

function Install-DotNetSdk {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Channel,

        [Parameter(Mandatory = $true)]
        [string] $InstallDir
    )

    $scriptPath = Join-Path $env:TEMP 'dotnet-install.ps1'
    Invoke-WebRequest 'https://dot.net/v1/dotnet-install.ps1' -OutFile $scriptPath
    & $scriptPath -Channel $Channel -InstallDir $InstallDir
    $env:PATH = "$InstallDir;$env:PATH"
}

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $repoRoot

Install-DotNetSdk -Channel '10.0' -InstallDir (Join-Path $env:ProgramFiles 'dotnet')
dotnet --info

$msbuild = Get-MSBuildPath
& $msbuild 'ResxFormatter\ResxFormatter.csproj' '/restore' '/t:Build' '/p:Configuration=Release' '/p:DeployExtension=false' '/p:ZipPackageCompressionLevel=normal' '/v:m'

dotnet build 'ResxFormatter.Cli\ResxFormatter.Cli.csproj' -c Release
dotnet build 'ResxFormatterTests\ResxFormatterTests.csproj' -c Release
dotnet test 'ResxFormatterTests\ResxFormatterTests.csproj' -c Release --no-build
