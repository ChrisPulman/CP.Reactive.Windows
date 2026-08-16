param(
    [Parameter(Mandatory = $true)]
    [string] $RepositoryRoot
)

$ErrorActionPreference = 'Stop'

$resolvedRoot = [System.IO.Path]::GetFullPath((Resolve-Path -LiteralPath $RepositoryRoot).Path)
$excludedSegments = [System.StringComparison]::OrdinalIgnoreCase
$checkedExtensions = @('.cs', '.csproj', '.props', '.targets', '.sln', '.slnx')
$invalidFiles = [System.Collections.Generic.List[string]]::new()
$conflictFiles = [System.Collections.Generic.List[string]]::new()

function Get-RepositoryRelativePath {
    param(
        [Parameter(Mandatory = $true)]
        [string] $RootPath,

        [Parameter(Mandatory = $true)]
        [string] $FilePath
    )

    $root = [System.IO.Path]::GetFullPath($RootPath).TrimEnd([System.IO.Path]::DirectorySeparatorChar, [System.IO.Path]::AltDirectorySeparatorChar)
    $fullFilePath = [System.IO.Path]::GetFullPath($FilePath)
    $prefix = $root + [System.IO.Path]::DirectorySeparatorChar

    if ($fullFilePath.StartsWith($prefix, [System.StringComparison]::OrdinalIgnoreCase)) {
        return $fullFilePath.Substring($prefix.Length)
    }

    return $fullFilePath
}

Get-ChildItem -LiteralPath $resolvedRoot -Recurse -File |
    Where-Object {
        $relativePath = Get-RepositoryRelativePath -RootPath $resolvedRoot -FilePath $_.FullName
        $binSegment = [System.IO.Path]::DirectorySeparatorChar + 'bin' + [System.IO.Path]::DirectorySeparatorChar
        $objSegment = [System.IO.Path]::DirectorySeparatorChar + 'obj' + [System.IO.Path]::DirectorySeparatorChar

        $checkedExtensions -contains $_.Extension -and
        -not $relativePath.StartsWith('.git' + [System.IO.Path]::DirectorySeparatorChar, $excludedSegments) -and
        -not $relativePath.StartsWith('.codex-diagnostics' + [System.IO.Path]::DirectorySeparatorChar, $excludedSegments) -and
        -not $relativePath.StartsWith('.codex-recovery' + [System.IO.Path]::DirectorySeparatorChar, $excludedSegments) -and
        -not $relativePath.StartsWith('.codex-tools' + [System.IO.Path]::DirectorySeparatorChar, $excludedSegments) -and
        -not $relativePath.StartsWith('artifacts' + [System.IO.Path]::DirectorySeparatorChar, $excludedSegments) -and
        -not $relativePath.StartsWith('TestResults' + [System.IO.Path]::DirectorySeparatorChar, $excludedSegments) -and
        $relativePath.IndexOf($binSegment, $excludedSegments) -lt 0 -and
        $relativePath.IndexOf($objSegment, $excludedSegments) -lt 0
    } |
    ForEach-Object {
        $relativePath = Get-RepositoryRelativePath -RootPath $resolvedRoot -FilePath $_.FullName

        if ($_.Length -eq 0) {
            $invalidFiles.Add($relativePath)
            return
        }

        $lineNumber = 0
        foreach ($line in [System.IO.File]::ReadLines($_.FullName)) {
            $lineNumber++
            if ($line.StartsWith('<<<<<<< ', [System.StringComparison]::Ordinal) -or
                $line.StartsWith('=======', [System.StringComparison]::Ordinal) -or
                $line.StartsWith('>>>>>>> ', [System.StringComparison]::Ordinal)) {
                $conflictFiles.Add("${relativePath}:$lineNumber")
                break
            }
        }
    }

if ($invalidFiles.Count -gt 0 -or $conflictFiles.Count -gt 0) {
    if ($invalidFiles.Count -gt 0) {
        Write-Error ("Zero-byte source/configuration files detected:`n" + ($invalidFiles -join "`n"))
    }

    if ($conflictFiles.Count -gt 0) {
        Write-Error ("Merge-conflict markers detected:`n" + ($conflictFiles -join "`n"))
    }

    exit 1
}
