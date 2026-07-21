$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

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

dotnet build 'ResxFormatter\ResxFormatter.csproj' -c Release -p:DeployExtension=false -p:ZipPackageCompressionLevel=normal

dotnet build 'ResxFormatter.Cli\ResxFormatter.Cli.csproj' -c Release
dotnet build 'ResxFormatterTests\ResxFormatterTests.csproj' -c Release
dotnet test 'ResxFormatterTests\ResxFormatterTests.csproj' -c Release --no-build
