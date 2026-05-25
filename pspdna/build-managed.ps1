# build-managed.ps1
# ---------------------------------------------------------------------------
# Compile all PSPDNA-based C# managed assemblies on Windows.
#
# Usage (from the pspdna/ directory):
#   .\build-managed.ps1                  # Strategy A: dotnet build (default)
#   .\build-managed.ps1 -DnaCompat       # Strategy B: csc -nostdlib
#   .\build-managed.ps1 -Project corlib  # build only corlib
#   .\build-managed.ps1 -Clean           # remove build\managed\ first
#   .\build-managed.ps1 -Verbose         # show full compiler command lines
#
# Why not 'dotnet build csc ...'?
#   dotnet build is an MSBuild wrapper.  Flags like -nostdlib, -target:library,
#   and -r:foo.dll are Roslyn (csc) flags, not MSBuild properties.  Mixing them
#   in one command causes MSB1001 "Unknown switch" errors.
#   These are completely separate tools:
#     dotnet build corlib.csproj    <- MSBuild, reads .csproj
#     csc -nostdlib -unsafe *.cs   <- Roslyn, reads source files directly
#
# Two compilation strategies
# --------------------------
#
#   Strategy A -- dotnet build (default, works immediately, no extra tools)
#     Builds via the .csproj files (net40 + NoStdLib).  MSBuild resolves the
#     net40 reference assemblies from NuGet so System.Object etc. are available.
#     LIMITATION: produced IL references the real BCL -- cannot be loaded by
#     the DNA micro-runtime on PSP.  Use this for IDE / compile-check / tests.
#
#   Strategy B -- csc -nostdlib  (use with -DnaCompat switch)
#     Invokes the raw Roslyn compiler.  Our pspdna\corlib already contains the
#     full PSPDNA System.* replacement sources (System\Object.cs, Int32.cs, ...).
#     All of these are compiled together with our Psp.* files in a single csc
#     -nostdlib invocation, so every type the compiler needs is defined in-house
#     exactly as the DNA interpreter expects.
#     Prerequisite: dotnet tool install --global Microsoft.Net.Sdk.Compilers
# ---------------------------------------------------------------------------

param(
    [ValidateSet("all", "corlib", "Cube3D", "AudioTest")]
    [string]$Project = "all",

    # Use Strategy B: csc -nostdlib, producing DNA-interpreter-compatible IL.
    [switch]$DnaCompat,

    [switch]$Clean,
    [switch]$Verbose
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# -- Paths -------------------------------------------------------------------
$ScriptDir     = $PSScriptRoot
$CorlibDir     = Join-Path $ScriptDir "corlib"
$BuildDir      = Join-Path $ScriptDir "build\managed"
$CorlibCsproj  = Join-Path $CorlibDir "corlib.csproj"
$Cube3DCsproj  = Join-Path $ScriptDir "samples\Cube3D\Cube3D.csproj"
$AudioCsproj   = Join-Path $ScriptDir "samples\AudioTest\AudioTest.csproj"
$CorlibOut     = Join-Path $BuildDir "corlib.dll"
$Cube3DOut     = Join-Path $BuildDir "Cube3D.exe"
$AudioOut      = Join-Path $BuildDir "AudioTest.exe"

# -- Clean -------------------------------------------------------------------
if ($Clean -and (Test-Path $BuildDir)) {
    Write-Host "[clean] Removing $BuildDir" -ForegroundColor Yellow
    Remove-Item -Recurse -Force $BuildDir
}
if (-not (Test-Path $BuildDir)) {
    New-Item -ItemType Directory -Force $BuildDir | Out-Null
}

# ---------------------------------------------------------------------------
if ($DnaCompat) {
    # -- Strategy B: csc -nostdlib -------------------------------------------
    Write-Host ""
    Write-Host "Strategy B: csc -nostdlib  (DNA-interpreter-compatible IL)" -ForegroundColor Cyan
    Write-Host ""

    # Verify csc is on PATH
    $CscCmd  = Get-Command csc -ErrorAction SilentlyContinue
    $CscPath = if ($CscCmd) { $CscCmd.Source } else { $null }
    if (-not $CscPath) {
        Write-Host "ERROR: 'csc' not found on PATH." -ForegroundColor Red
        Write-Host ""
        Write-Host "  Install the standalone Roslyn compiler tool once:"
        Write-Host "    dotnet tool install --global Microsoft.Net.Sdk.Compilers"
        Write-Host "  Then open a new terminal (PATH is updated on install)."
        exit 1
    }
    Write-Host "  csc   : $CscPath"
    Write-Host "  corlib: $CorlibDir"
    Write-Host ""

    # Collect all .cs sources from our corlib directory, excluding:
    #   obj\  -- MSBuild-generated intermediate files (AssemblyInfo.cs etc.)
    #   bin\  -- output binaries
    #
    # Our corlib already contains both halves:
    #   System\*, System.Collections\*, ...  -- PSPDNA primitive types
    #   Psp\*                                -- our PSP wrappers
    #
    # Compiling everything together in one -nostdlib shot means System.Object
    # is defined in the same compilation unit that references it -- required
    # by Roslyn when -nostdlib removes all predefined types.
    $ObjDir = Join-Path $CorlibDir "obj"
    $BinDir = Join-Path $CorlibDir "bin"

    $AllCorlibSrcs = @(
        Get-ChildItem $CorlibDir -Filter "*.cs" -Recurse -ErrorAction SilentlyContinue |
        Where-Object {
            -not $_.FullName.StartsWith($ObjDir, [System.StringComparison]::OrdinalIgnoreCase) -and
            -not $_.FullName.StartsWith($BinDir, [System.StringComparison]::OrdinalIgnoreCase)
        } |
        Select-Object -ExpandProperty FullName
    )

    if ($AllCorlibSrcs.Count -eq 0) {
        Write-Host "ERROR: No .cs files found under '$CorlibDir' (excluding obj\ and bin\)." -ForegroundColor Red
        exit 1
    }

    Write-Host "  Compiling $($AllCorlibSrcs.Count) source files from corlib\" -ForegroundColor DarkGray
    Write-Host ""

    $CscCommon = @("-nostdlib", "-unsafe", "-optimize+", "-langversion:7.3",
                   "-warn:3", "-noconfig", "-nologo")

    function Invoke-Csc {
        param([string]$Label, [string[]]$ExtraFlags, [string[]]$Sources)
        if ($Verbose) {
            $fileCount = $Sources.Count
            Write-Host "  CMD: csc $($CscCommon -join ' ') $($ExtraFlags -join ' ') [$fileCount files]" `
                       -ForegroundColor DarkGray
        }
        Write-Host "[csc] $Label" -ForegroundColor Green
        & csc @($CscCommon + $ExtraFlags + $Sources)
        if ($LASTEXITCODE -ne 0) { throw "csc failed for $Label (exit $LASTEXITCODE)" }
    }

    if ($Project -in "all","corlib") {
        Invoke-Csc "corlib.dll" @("-target:library", "-out:$CorlibOut") $AllCorlibSrcs
    }
    if ($Project -in "all","Cube3D") {
        $src = Join-Path $ScriptDir "samples\Cube3D\Program.cs"
        Invoke-Csc "Cube3D.exe" @("-target:exe", "-r:$CorlibOut", "-out:$Cube3DOut") @($src)
    }
    if ($Project -in "all","AudioTest") {
        $src = Join-Path $ScriptDir "samples\AudioTest\Program.cs"
        Invoke-Csc "AudioTest.exe" @("-target:exe", "-r:$CorlibOut", "-out:$AudioOut") @($src)
    }

} else {
    # -- Strategy A: dotnet build --------------------------------------------
    Write-Host ""
    Write-Host "Strategy A: dotnet build  (IDE / compile-check, desktop CLR)" -ForegroundColor Cyan
    Write-Host "  NOTE: Output IL references the real BCL -- not loadable by DNA on PSP."
    Write-Host "  For DNA-compatible output run:  .\build-managed.ps1 -DnaCompat"
    Write-Host ""

    function Invoke-DotnetBuild([string]$CsprojPath) {
        $label = [System.IO.Path]::GetFileNameWithoutExtension($CsprojPath)
        if ($Verbose) { Write-Host "  CMD: dotnet build `"$CsprojPath`"" -ForegroundColor DarkGray }
        Write-Host "[dotnet build] $label" -ForegroundColor Green
        dotnet build "$CsprojPath" --nologo -v minimal
        if ($LASTEXITCODE -ne 0) { throw "dotnet build failed for $label" }
    }

    # corlib must build before samples (samples have a ProjectReference to it)
    if ($Project -in "all","corlib")    { Invoke-DotnetBuild $CorlibCsproj  }
    if ($Project -in "all","Cube3D")    { Invoke-DotnetBuild $Cube3DCsproj  }
    if ($Project -in "all","AudioTest") { Invoke-DotnetBuild $AudioCsproj   }
}

Write-Host ""
Write-Host "Done." -ForegroundColor Green
