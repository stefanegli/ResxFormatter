[CmdletBinding(SupportsShouldProcess = $true)]
param(
    [string] $AppVeyorProjectUri = 'https://ci.appveyor.com/api/projects/stefanegli/resxformatter',

    [string] $GitHubRemote = 'github',

    [string] $RepositoryRoot
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Invoke-Git {
    param(
        [Parameter(Mandatory = $true)]
        [string] $RepositoryRoot,

        [Parameter(Mandatory = $true)]
        [string[]] $ArgumentList
    )

    $output = @(& git -C $RepositoryRoot @ArgumentList 2>&1)
    if ($LASTEXITCODE -ne 0) {
        $details = ($output | Out-String).Trim()
        throw "git $($ArgumentList -join ' ') failed with exit code $LASTEXITCODE. $details"
    }

    return $output
}

function Test-LocalTag {
    param(
        [Parameter(Mandatory = $true)]
        [string] $RepositoryRoot,

        [Parameter(Mandatory = $true)]
        [string] $TagName
    )

    & git -C $RepositoryRoot show-ref --verify --quiet "refs/tags/$TagName"
    if ($LASTEXITCODE -eq 0) {
        return $true
    }

    if ($LASTEXITCODE -eq 1) {
        return $false
    }

    throw "Could not inspect local tag '$TagName'; git exited with code $LASTEXITCODE."
}

function Test-RemoteTag {
    param(
        [Parameter(Mandatory = $true)]
        [string] $RepositoryRoot,

        [Parameter(Mandatory = $true)]
        [string] $Remote,

        [Parameter(Mandatory = $true)]
        [string] $TagName
    )

    $output = @(& git -C $RepositoryRoot ls-remote --exit-code --tags $Remote "refs/tags/$TagName" 2>&1)
    if ($LASTEXITCODE -eq 0) {
        return $true
    }

    if ($LASTEXITCODE -eq 2) {
        return $false
    }

    $details = ($output | Out-String).Trim()
    throw "Could not inspect tag '$TagName' on '$Remote'; git exited with code $LASTEXITCODE. $details"
}

$skillRoot = Split-Path -Parent $PSScriptRoot
$skillsRoot = Split-Path -Parent $skillRoot
if ([string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    $RepositoryRoot = Split-Path -Parent $skillsRoot
}

if (-not (Get-Command 'git' -ErrorAction SilentlyContinue)) {
    throw 'Git was not found on PATH.'
}

$RepositoryRoot = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$gitDirectory = (Invoke-Git -RepositoryRoot $RepositoryRoot -ArgumentList @('rev-parse', '--git-dir') |
        Select-Object -First 1).ToString().Trim()
if ([string]::IsNullOrWhiteSpace($gitDirectory)) {
    throw "'$RepositoryRoot' is not a Git repository."
}

$worktreeStatus = (Invoke-Git -RepositoryRoot $RepositoryRoot -ArgumentList @('status', '--porcelain') |
        Out-String).Trim()
if (-not [string]::IsNullOrWhiteSpace($worktreeStatus)) {
    throw 'The worktree is not clean. Commit or remove pending changes before publishing a VSIX release.'
}

$remoteUrl = (Invoke-Git -RepositoryRoot $RepositoryRoot -ArgumentList @(
        'remote', 'get-url', '--push', $GitHubRemote) | Select-Object -First 1).ToString().Trim()
if ($remoteUrl -notmatch '(?i)(?:github\.com[:/])stefanegli/ResxFormatter(?:\.git)?$') {
    throw "Remote '$GitHubRemote' is not the expected GitHub repository: '$remoteUrl'."
}

Write-Host "Reading the latest AppVeyor build from $AppVeyorProjectUri..."
$response = Invoke-RestMethod -Uri $AppVeyorProjectUri -Method Get
$build = $response.build
if (-not $build) {
    throw 'AppVeyor did not return a latest build.'
}

if ($build.status -ne 'success') {
    throw "The latest AppVeyor build is '$($build.status)', not 'success'."
}

if ($build.isTag) {
    $existingTag = if ($build.tag) { $build.tag } else { "v$($build.version)" }
    throw "The latest AppVeyor build is already the tagged release '$existingTag'."
}

$version = [string]$build.version
if ($version -notmatch '^\d+\.\d+\.\d+(?:\.\d+)?$') {
    throw "AppVeyor returned version '$version', which is not a numeric VSIX version."
}

$headCommit = (Invoke-Git -RepositoryRoot $RepositoryRoot -ArgumentList @('rev-parse', 'HEAD') |
        Select-Object -First 1).ToString().Trim()
$buildCommit = [string]$build.commitId
if ([string]::IsNullOrWhiteSpace($buildCommit)) {
    throw 'AppVeyor did not return the commit for the latest build.'
}

if (-not [string]::Equals($headCommit, $buildCommit, [StringComparison]::OrdinalIgnoreCase)) {
    throw "The latest AppVeyor build commit '$buildCommit' does not match HEAD '$headCommit'."
}

$tagName = "v$version"
if (Test-RemoteTag -RepositoryRoot $RepositoryRoot -Remote $GitHubRemote -TagName $tagName) {
    throw "Tag '$tagName' already exists on '$GitHubRemote'."
}

$localTagExists = Test-LocalTag -RepositoryRoot $RepositoryRoot -TagName $tagName
if ($localTagExists) {
    $tagCommit = (Invoke-Git -RepositoryRoot $RepositoryRoot -ArgumentList @(
            'rev-list', '-n', '1', $tagName) | Select-Object -First 1).ToString().Trim()
    if (-not [string]::Equals($tagCommit, $headCommit, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Local tag '$tagName' points to '$tagCommit', not HEAD '$headCommit'."
    }
}

Write-Host "AppVeyor build: $($build.buildId) ($($build.status))"
Write-Host "Version: $version"
Write-Host "Commit: $headCommit"
Write-Host "Tag: $tagName"
Write-Host "GitHub remote: $remoteUrl"

if (-not $localTagExists -and $PSCmdlet.ShouldProcess($headCommit, "Create annotated tag '$tagName'")) {
    Invoke-Git -RepositoryRoot $RepositoryRoot -ArgumentList @(
        'tag', '-a', $tagName, '-m', "Release $version") | Out-Null
}

if ($PSCmdlet.ShouldProcess($GitHubRemote, "Push tag '$tagName'")) {
    Invoke-Git -RepositoryRoot $RepositoryRoot -ArgumentList @(
        'push', $GitHubRemote, "refs/tags/$tagName") | Out-Null
    Write-Host "Published tag '$tagName' to '$GitHubRemote'."
}
