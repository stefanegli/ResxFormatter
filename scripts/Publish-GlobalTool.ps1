[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^\d+\.\d+\.\d+(?:-[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*)?$')]
    [string]$Version,

    [string]$CredentialTarget = "NuGet.ApiKey.ResxFormatter.Prod",
    [string]$ProjectPath = "ResxFormatter.Cli/ResxFormatter.Cli.csproj",
    [string]$PackagesDirectory = "artifacts/packages",
    [string]$Configuration = "Release",
    [string]$NuGetSource = "https://api.nuget.org/v3/index.json",
    [switch]$SkipPack,
    [switch]$SkipTests,
    [switch]$SkipPackageValidation,
    [switch]$PackOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if (-not ("CredentialManagerNative" -as [Type]))
{
    Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;

public static class CredentialManagerNative
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct CREDENTIAL
    {
        public int Flags;
        public int Type;
        public string TargetName;
        public string Comment;
        public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
        public int CredentialBlobSize;
        public IntPtr CredentialBlob;
        public int Persist;
        public int AttributeCount;
        public IntPtr Attributes;
        public string TargetAlias;
        public string UserName;
    }

    [DllImport("Advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool CredRead(string target, int type, int reservedFlag, out IntPtr credentialPtr);

    [DllImport("Advapi32.dll", SetLastError = true)]
    public static extern void CredFree(IntPtr cred);
}
"@
}

function Get-CredentialManagerSecret
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$Target
    )

    if ([Environment]::OSVersion.Platform -ne [PlatformID]::Win32NT)
    {
        throw "Windows Credential Manager is unavailable on this operating system. Set NUGET_API_KEY instead."
    }

    $credentialPtr = [IntPtr]::Zero
    $credTypeGeneric = 1
    $result = [CredentialManagerNative]::CredRead($Target, $credTypeGeneric, 0, [ref]$credentialPtr)

    if (-not $result)
    {
        $win32Error = [Runtime.InteropServices.Marshal]::GetLastWin32Error()
        throw "Credential target '$Target' was not found or could not be read (Win32 error: $win32Error)."
    }

    try
    {
        $credential = [Runtime.InteropServices.Marshal]::PtrToStructure(
            $credentialPtr,
            [Type][CredentialManagerNative+CREDENTIAL])

        if ($credential.CredentialBlobSize -le 0 -or $credential.CredentialBlob -eq [IntPtr]::Zero)
        {
            throw "Credential target '$Target' does not contain a secret."
        }

        $bytes = New-Object byte[] $credential.CredentialBlobSize
        [Runtime.InteropServices.Marshal]::Copy($credential.CredentialBlob, $bytes, 0, $credential.CredentialBlobSize)

        $secret = [Text.Encoding]::Unicode.GetString($bytes).TrimEnd([char]0)
        if ([string]::IsNullOrWhiteSpace($secret))
        {
            $secret = [Text.Encoding]::UTF8.GetString($bytes).TrimEnd([char]0)
        }

        if ([string]::IsNullOrWhiteSpace($secret))
        {
            throw "Credential target '$Target' was read but secret content is empty."
        }

        return $secret
    }
    finally
    {
        if ($credentialPtr -ne [IntPtr]::Zero)
        {
            [CredentialManagerNative]::CredFree($credentialPtr)
        }
    }
}

function Invoke-NativeCommand
{
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath,

        [Parameter(Mandatory = $true)]
        [string[]]$ArgumentList
    )

    & $FilePath @ArgumentList
    if ($LASTEXITCODE -ne 0)
    {
        throw "'$FilePath' exited with code $LASTEXITCODE."
    }
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
Push-Location $repoRoot

try
{
    $resolvedProjectPath = (Resolve-Path (Join-Path $repoRoot $ProjectPath)).Path
    $resolvedPackagesDirectory = Join-Path $repoRoot $PackagesDirectory
    New-Item -ItemType Directory -Force -Path $resolvedPackagesDirectory | Out-Null

    $packageId = (& dotnet msbuild $resolvedProjectPath -getProperty:PackageId -nologo | Out-String).Trim()
    if ($LASTEXITCODE -ne 0)
    {
        throw "Could not read PackageId from '$resolvedProjectPath'."
    }

    if ([string]::IsNullOrWhiteSpace($packageId))
    {
        throw "PackageId is empty in '$resolvedProjectPath'."
    }

    $packagePath = Join-Path $resolvedPackagesDirectory "$packageId.$Version.nupkg"

    $dotnetHome = Join-Path $repoRoot ".dotnet"
    New-Item -ItemType Directory -Force -Path $dotnetHome | Out-Null
    $env:DOTNET_CLI_HOME = $dotnetHome
    $env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = "1"

    if (-not $SkipTests)
    {
        Write-Host "Running formatter tests..."
        Invoke-NativeCommand "dotnet" @(
            "test",
            (Join-Path $repoRoot "ResxFormatterTests\ResxFormatterTests.csproj"),
            "--configuration", $Configuration,
            "--nologo"
        )
        Invoke-NativeCommand "dotnet" @(
            "test",
            (Join-Path $repoRoot "ResxFormatter.Cli.Tests\ResxFormatter.Cli.Tests.csproj"),
            "--configuration", $Configuration,
            "--nologo"
        )
    }

    if (-not $SkipPack)
    {
        Write-Host "Packing tool package from '$resolvedProjectPath'..."
        Invoke-NativeCommand "dotnet" @(
            "pack",
            $resolvedProjectPath,
            "--configuration", $Configuration,
            "--output", $resolvedPackagesDirectory,
            "--nologo",
            "-p:Version=$Version"
        )
    }
    else
    {
        Write-Host "Skipping pack step as requested."
    }

    if (-not (Test-Path -LiteralPath $packagePath -PathType Leaf))
    {
        throw "Expected package was not found: '$packagePath'."
    }

    Write-Host "Selected package: $packagePath"

    if (-not $SkipPackageValidation)
    {
        $validationRoot = Join-Path ([IO.Path]::GetTempPath()) (
            "rft-" + [Guid]::NewGuid().ToString("N").Substring(0, 8))
        $toolPath = Join-Path $validationRoot "tools"

        try
        {
            New-Item -ItemType Directory -Force -Path $toolPath | Out-Null

            Write-Host "Installing the package into an isolated tool path..."
            Invoke-NativeCommand "dotnet" @(
                "tool",
                "install",
                $packageId,
                "--tool-path", $toolPath,
                "--add-source", $resolvedPackagesDirectory,
                "--version", $Version,
                "--ignore-failed-sources"
            )

            $toolExecutableName = if ([Environment]::OSVersion.Platform -eq [PlatformID]::Win32NT)
            {
                "resxfmt.exe"
            }
            else
            {
                "resxfmt"
            }
            $toolExecutable = Join-Path $toolPath $toolExecutableName
            if (-not (Test-Path -LiteralPath $toolExecutable -PathType Leaf))
            {
                throw "Installed tool command was not found: '$toolExecutable'."
            }

            Invoke-NativeCommand $toolExecutable @("--version")
        }
        finally
        {
            if (Test-Path -LiteralPath $validationRoot)
            {
                Remove-Item -LiteralPath $validationRoot -Recurse -Force
            }
        }
    }

    if ($PackOnly)
    {
        Write-Host "Pack-only mode enabled. Skipping push."
        Write-Output $packagePath
        return
    }

    $restoreApiKey = $false
    if ([string]::IsNullOrWhiteSpace($env:NUGET_API_KEY))
    {
        $env:NUGET_API_KEY = Get-CredentialManagerSecret -Target $CredentialTarget
        $restoreApiKey = $true
    }

    try
    {
        Write-Host "Pushing package to '$NuGetSource'..."
        Invoke-NativeCommand "dotnet" @(
            "nuget",
            "push",
            $packagePath,
            "--source", $NuGetSource
        )
    }
    finally
    {
        if ($restoreApiKey)
        {
            Remove-Item Env:NUGET_API_KEY -ErrorAction SilentlyContinue
        }
    }

    Write-Host "Package push completed."
    Write-Output $packagePath
}
finally
{
    Pop-Location
}
