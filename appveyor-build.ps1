$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Invoke-NativeCommand {
    param(
        [Parameter(Mandatory = $true)]
        [string] $FilePath,

        [Parameter(Mandatory = $true)]
        [string[]] $ArgumentList
    )

    & $FilePath @ArgumentList
    if ($LASTEXITCODE -ne 0) {
        throw "'$FilePath' exited with code $LASTEXITCODE."
    }
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
    if (-not $?) {
        throw "The .NET SDK $Channel installation failed."
    }

    $env:PATH = "$InstallDir;$env:PATH"
}

function Set-VsixBuildVersion {
    param(
        [Parameter(Mandatory = $true)]
        [string] $ManifestPath,

        [Parameter(Mandatory = $true)]
        [int] $BuildNumber,

        [switch] $UpdateAppVeyor
    )

    $document = [System.Xml.XmlDocument]::new()
    $document.PreserveWhitespace = $true
    $document.Load($ManifestPath)

    $namespace = [System.Xml.XmlNamespaceManager]::new($document.NameTable)
    $namespace.AddNamespace('vsix', $document.DocumentElement.NamespaceURI)
    $identity = $document.SelectSingleNode('//vsix:Identity', $namespace)
    if (-not $identity) {
        throw "The VSIX identity was not found in '$ManifestPath'."
    }

    $baseVersion = [Version]$identity.GetAttribute('Version')
    $version = [Version]::new($baseVersion.Major, $baseVersion.Minor, $BuildNumber)
    $identity.SetAttribute('Version', $version.ToString())
    $document.Save($ManifestPath)

    Write-Host "VSIX version: $version"

    if ($UpdateAppVeyor) {
        if (-not (Get-Command 'appveyor' -ErrorAction SilentlyContinue)) {
            throw 'The AppVeyor build worker command was not found.'
        }

        Invoke-NativeCommand 'appveyor' @('UpdateBuild', '-Version', $version.ToString())
        $env:APPVEYOR_BUILD_VERSION = $version.ToString()
    }

    return $version
}

function Publish-VsixToGallery {
    [CmdletBinding(SupportsShouldProcess = $true)]
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    if ($env:APPVEYOR -ne 'True') {
        Write-Host 'Not running in AppVeyor; skipping VSIX publication.'
        return
    }

    if ($env:APPVEYOR_PULL_REQUEST_NUMBER) {
        Write-Host 'Pull request build; skipping VSIX Gallery publication.'
        return
    }

    if ([string]::IsNullOrWhiteSpace($env:APPVEYOR_REPO_NAME)) {
        throw "AppVeyor did not provide 'APPVEYOR_REPO_NAME'."
    }

    $vsixPath = (Resolve-Path $Path).Path
    $repositoryUrl = "https://github.com/$($env:APPVEYOR_REPO_NAME)/"
    $issueTrackerUrl = "${repositoryUrl}issues/"
    $branch = if ($env:APPVEYOR_REPO_BRANCH) { $env:APPVEYOR_REPO_BRANCH } else { 'master' }
    $readmeUrl = "$branch/README.md"
    $uploadUrl = 'https://www.vsixgallery.com/api/upload' +
        '?repo=' + [Uri]::EscapeDataString($repositoryUrl) +
        '&issuetracker=' + [Uri]::EscapeDataString($issueTrackerUrl) +
        '&readmeUrl=' + [Uri]::EscapeDataString($readmeUrl)

    if (-not $PSCmdlet.ShouldProcess($uploadUrl, "Publish '$vsixPath'")) {
        return
    }

    if ([string]::IsNullOrWhiteSpace($env:VSIX_GALLERY_MANAGE_TOKEN)) {
        throw "Set 'VSIX_GALLERY_MANAGE_TOKEN' as a secure AppVeyor environment variable before publishing."
    }

    Write-Host "Publishing '$vsixPath' to the Open VSIX Gallery..."
    $client = [System.Net.WebClient]::new()
    $client.Headers.Add('X-Manage-Token', $env:VSIX_GALLERY_MANAGE_TOKEN)

    try {
        $responseBytes = $client.UploadFile($uploadUrl, 'POST', $vsixPath)
        $responseJson = [Text.Encoding]::UTF8.GetString($responseBytes)
        $response = $responseJson | ConvertFrom-Json
    } catch [System.Net.WebException] {
        $galleryError = if ($_.Exception.Response) { $_.Exception.Response.Headers['x-error'] } else { $null }
        if ($galleryError) {
            throw "VSIX Gallery publication failed: $galleryError"
        }

        throw
    } finally {
        $client.Dispose()
    }

    $idProperty = $response.PSObject.Properties['ID']
    if (-not $idProperty) {
        $idProperty = $response.PSObject.Properties['id']
    }

    $nameProperty = $response.PSObject.Properties['Name']
    if (-not $nameProperty) {
        $nameProperty = $response.PSObject.Properties['name']
    }

    $versionProperty = $response.PSObject.Properties['Version']
    if (-not $versionProperty) {
        $versionProperty = $response.PSObject.Properties['version']
    }

    $extensionId = if ($idProperty) { $idProperty.Value } else { $null }
    $extensionName = if ($nameProperty) { $nameProperty.Value } else { $extensionId }
    $extensionVersion = if ($versionProperty) { $versionProperty.Value } else { $null }
    if (-not $extensionId) {
        throw "VSIX Gallery returned an unexpected response: $responseJson"
    }

    Write-Host "Published $extensionName $extensionVersion"
    Write-Host "Extension page: https://www.vsixgallery.com/extension/$extensionId"
}

function Invoke-Build {
    $repoRoot = $PSScriptRoot
    Set-Location $repoRoot

    Install-DotNetSdk -Channel '10.0' -InstallDir (Join-Path $env:ProgramFiles 'dotnet')
    Invoke-NativeCommand 'dotnet' @('--info')

    if ($env:APPVEYOR_BUILD_NUMBER) {
        Set-VsixBuildVersion `
            -ManifestPath (Join-Path $repoRoot 'ResxFormatter\source.extension.vsixmanifest') `
            -BuildNumber ([int]$env:APPVEYOR_BUILD_NUMBER) `
            -UpdateAppVeyor | Out-Null
    }

    Invoke-NativeCommand 'dotnet' @(
        'build',
        'ResxFormatter\ResxFormatter.csproj',
        '-c', 'Release',
        '-p:DeployExtension=false',
        '-p:ZipPackageCompressionLevel=normal'
    )

    Invoke-NativeCommand 'dotnet' @('build', 'ResxFormatter.Cli\ResxFormatter.Cli.csproj', '-c', 'Release')
    Invoke-NativeCommand 'dotnet' @('build', 'ResxFormatter.Cli.Tests\ResxFormatter.Cli.Tests.csproj', '-c', 'Release')
    Invoke-NativeCommand 'dotnet' @('build', 'ResxFormatterTests\ResxFormatterTests.csproj', '-c', 'Release')
    Invoke-NativeCommand 'dotnet' @(
        'test',
        'ResxFormatterTests\ResxFormatterTests.csproj',
        '-c', 'Release',
        '--no-build'
    )
    Invoke-NativeCommand 'dotnet' @(
        'test',
        'ResxFormatter.Cli.Tests\ResxFormatter.Cli.Tests.csproj',
        '-c', 'Release',
        '--no-build'
    )

    Publish-VsixToGallery 'ResxFormatter\bin\Release\net472\ResxFormatter.vsix'
}

if ($MyInvocation.InvocationName -ne '.') {
    Invoke-Build
}
