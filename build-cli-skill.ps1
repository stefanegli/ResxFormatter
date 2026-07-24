[CmdletBinding()]
param(
    [string] $Version,

    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',

    [string] $OutputDirectory,

    [switch] $SkipTests
)

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

function Get-VsixVersion {
    param(
        [Parameter(Mandatory = $true)]
        [string] $ManifestPath
    )

    $document = [System.Xml.XmlDocument]::new()
    $document.Load($ManifestPath)

    $namespace = [System.Xml.XmlNamespaceManager]::new($document.NameTable)
    $namespace.AddNamespace('vsix', $document.DocumentElement.NamespaceURI)
    $identity = $document.SelectSingleNode('//vsix:Identity', $namespace)
    if (-not $identity) {
        throw "The VSIX identity was not found in '$ManifestPath'."
    }

    return [Version]$identity.GetAttribute('Version')
}

function Get-PackageVersion {
    param(
        [string] $RequestedVersion,

        [Parameter(Mandatory = $true)]
        [string] $ManifestPath
    )

    $resolvedVersion = if ([string]::IsNullOrWhiteSpace($RequestedVersion)) {
        Get-VsixVersion -ManifestPath $ManifestPath
    } else {
        [Version]$RequestedVersion
    }

    if ($resolvedVersion.Build -lt 0) {
        return [Version]::new($resolvedVersion.Major, $resolvedVersion.Minor, 0)
    }

    return $resolvedVersion
}

function Assert-PathWithinDirectory {
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path,

        [Parameter(Mandatory = $true)]
        [string] $Directory
    )

    $directoryPath = [IO.Path]::GetFullPath($Directory).TrimEnd(
        [IO.Path]::DirectorySeparatorChar,
        [IO.Path]::AltDirectorySeparatorChar)
    $candidatePath = [IO.Path]::GetFullPath($Path)
    $directoryPrefix = $directoryPath + [IO.Path]::DirectorySeparatorChar

    if (-not $candidatePath.StartsWith($directoryPrefix, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Path '$candidatePath' is outside package directory '$directoryPath'."
    }
}

$repoRoot = $PSScriptRoot
$manifestPath = Join-Path $repoRoot 'ResxFormatter\source.extension.vsixmanifest'
$projectPath = Join-Path $repoRoot 'ResxFormatter.Cli\ResxFormatter.Cli.csproj'
$testProjectPaths = @(
    (Join-Path $repoRoot 'ResxFormatterTests\ResxFormatterTests.csproj'),
    (Join-Path $repoRoot 'ResxFormatter.Cli.Tests\ResxFormatter.Cli.Tests.csproj')
)
$runtimeIdentifiers = @('win-x64', 'linux-x64')
$skillSourcePath = Join-Path $repoRoot 'skills\resxfmt'

if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $repoRoot 'artifacts\packages'
} elseif (-not [IO.Path]::IsPathRooted($OutputDirectory)) {
    $OutputDirectory = Join-Path $repoRoot $OutputDirectory
}

$OutputDirectory = [IO.Path]::GetFullPath($OutputDirectory)
$packageVersion = Get-PackageVersion -RequestedVersion $Version -ManifestPath $manifestPath
$packageVersionText = $packageVersion.ToString()
$assemblyVersion = if ($packageVersion.Revision -lt 0) {
    [Version]::new($packageVersion.Major, $packageVersion.Minor, $packageVersion.Build, 0)
} else {
    $packageVersion
}

$commit = $null
if (Get-Command 'git' -ErrorAction SilentlyContinue) {
    $commit = (& git -C $repoRoot rev-parse --short=8 HEAD 2>$null)
    if ($LASTEXITCODE -ne 0) {
        $commit = $null
    }
}

$informationalVersion = if ([string]::IsNullOrWhiteSpace($commit)) {
    $packageVersionText
} else {
    "$packageVersionText+$commit"
}

if (-not (Test-Path -LiteralPath $skillSourcePath -PathType Container)) {
    throw "Skill source directory was not found: '$skillSourcePath'."
}

$requiredSkillFiles = @(
    'SKILL.md',
    'agents\openai.yaml',
    'references\cli-reference.md'
)
foreach ($relativePath in $requiredSkillFiles) {
    $requiredPath = Join-Path $skillSourcePath $relativePath
    if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
        throw "Required skill file was not found: '$requiredPath'."
    }
}

$skillContent = Get-Content -Raw -LiteralPath (Join-Path $skillSourcePath 'SKILL.md')
if ($skillContent -notmatch '(?s)\A---\r?\nname:\s*resxfmt\r?\ndescription:\s*.+?\r?\n---') {
    throw "The resxfmt skill has invalid or unexpected frontmatter."
}

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

$packageFileName = "resxfmt-$packageVersionText.zip"
$packagePath = Join-Path $OutputDirectory $packageFileName
if (Test-Path -LiteralPath $packagePath) {
    throw "Package already exists: '$packagePath'."
}

$temporaryId = [Guid]::NewGuid().ToString('N')
$stagingRoot = Join-Path $OutputDirectory ".resxfmt-$packageVersionText-$temporaryId.tmp"
$temporaryPackagePath = Join-Path $OutputDirectory ".$packageFileName-$temporaryId.tmp.zip"
Assert-PathWithinDirectory -Path $stagingRoot -Directory $OutputDirectory
Assert-PathWithinDirectory -Path $temporaryPackagePath -Directory $OutputDirectory

try {
    if (-not $SkipTests) {
        foreach ($testProjectPath in $testProjectPaths) {
            Invoke-NativeCommand 'dotnet' @(
                'test',
                $testProjectPath,
                '--configuration', $Configuration,
                '--nologo'
            )
        }
    }

    $stagedSkillPath = Join-Path $stagingRoot 'resxfmt'
    $cliPayloadPath = Join-Path $stagedSkillPath 'assets\cli'
    New-Item -ItemType Directory -Path $stagedSkillPath -Force | Out-Null
    Get-ChildItem -Force -LiteralPath $skillSourcePath |
        Copy-Item -Destination $stagedSkillPath -Recurse -Force
    New-Item -ItemType Directory -Path $cliPayloadPath -Force | Out-Null

    foreach ($runtimeIdentifier in $runtimeIdentifiers) {
        $executableName = if ($runtimeIdentifier.StartsWith('win-', [StringComparison]::Ordinal)) {
            'resxfmt.exe'
        } else {
            'resxfmt'
        }

        $publishPath = Join-Path $stagingRoot "publish\$runtimeIdentifier"
        $runtimePayloadPath = Join-Path $cliPayloadPath $runtimeIdentifier
        New-Item -ItemType Directory -Path $publishPath -Force | Out-Null
        New-Item -ItemType Directory -Path $runtimePayloadPath -Force | Out-Null

        Invoke-NativeCommand 'dotnet' @(
            'publish',
            $projectPath,
            '--configuration', $Configuration,
            '--nologo',
            '--runtime', $runtimeIdentifier,
            '--self-contained', 'false',
            '--output', $publishPath,
            '-p:PublishSingleFile=true',
            '-p:PublishReadyToRun=false',
            '-p:DebugType=None',
            '-p:DebugSymbols=false',
            "-p:Version=$packageVersionText",
            "-p:AssemblyVersion=$assemblyVersion",
            "-p:FileVersion=$assemblyVersion",
            "-p:InformationalVersion=$informationalVersion",
            '-p:IncludeSourceRevisionInInformationalVersion=false'
        )

        $publishedExecutablePath = Join-Path $publishPath $executableName
        if (-not (Test-Path -LiteralPath $publishedExecutablePath -PathType Leaf)) {
            throw "Published CLI executable was not found: '$publishedExecutablePath'."
        }

        Copy-Item `
            -LiteralPath $publishedExecutablePath `
            -Destination (Join-Path $runtimePayloadPath $executableName)
    }

    Copy-Item `
        -LiteralPath (Join-Path $repoRoot 'LICENSE') `
        -Destination (Join-Path $cliPayloadPath 'ResxFormatter-LICENSE.txt')

    Compress-Archive `
        -LiteralPath $stagedSkillPath `
        -DestinationPath $temporaryPackagePath `
        -CompressionLevel Optimal
    Move-Item -LiteralPath $temporaryPackagePath -Destination $packagePath

    Write-Host "ResxFormatter skill package: $packagePath"
    Write-Output $packagePath
} finally {
    if (Test-Path -LiteralPath $stagingRoot) {
        Assert-PathWithinDirectory -Path $stagingRoot -Directory $OutputDirectory
        Remove-Item -LiteralPath $stagingRoot -Recurse -Force
    }

    if (Test-Path -LiteralPath $temporaryPackagePath) {
        Assert-PathWithinDirectory -Path $temporaryPackagePath -Directory $OutputDirectory
        Remove-Item -LiteralPath $temporaryPackagePath -Force
    }
}
