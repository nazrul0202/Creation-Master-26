@echo off
REM ---------------------------------------------------------------------------
REM Shared Visual Studio environment resolver.
REM
REM Locates any edition of Visual Studio 2022 or newer (Community, Professional,
REM Enterprise, Build Tools, Preview) using the vswhere shipped with every VS
REM installer, instead of hardcoding one edition's install path.
REM
REM Usage from another script:   call "%~dp0vsenv.cmd" || exit /b 1
REM On success the C++ x64 toolchain (cl, msbuild) is on PATH.
REM ---------------------------------------------------------------------------

REM Already initialised in this shell? Nothing to do.
if defined CM26_VSENV_READY exit /b 0

set "VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
if not exist "%VSWHERE%" set "VSWHERE=%ProgramFiles%\Microsoft Visual Studio\Installer\vswhere.exe"
if not exist "%VSWHERE%" (
    echo [vsenv] ERROR: vswhere.exe not found.
    echo [vsenv] Install Visual Studio 2022+ or the Build Tools with the
    echo [vsenv] "Desktop development with C++" workload, then retry.
    exit /b 1
)

REM Require the C++ compiler toolset so we never select a C#-only installation.
set "VSPATH="
for /f "usebackq tokens=*" %%i in (`"%VSWHERE%" -latest -prerelease -products * ^
 -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 ^
 -property installationPath`) do set "VSPATH=%%i"

if not defined VSPATH (
    echo [vsenv] ERROR: no Visual Studio installation with the C++ x64 toolset was found.
    echo [vsenv] Required workload: "Desktop development with C++".
    exit /b 1
)

if not exist "%VSPATH%\Common7\Tools\VsDevCmd.bat" (
    echo [vsenv] ERROR: VsDevCmd.bat missing under "%VSPATH%".
    exit /b 1
)

echo [vsenv] Using Visual Studio at: %VSPATH%
call "%VSPATH%\Common7\Tools\VsDevCmd.bat" -arch=x64 -host_arch=x64 >nul
if errorlevel 1 (
    echo [vsenv] ERROR: VsDevCmd.bat failed to initialise the x64 environment.
    exit /b 1
)

REM Verify the compiler is actually reachable before any caller depends on it.
where cl.exe >nul 2>&1
if errorlevel 1 (
    echo [vsenv] ERROR: cl.exe is not on PATH after initialising the VS environment.
    exit /b 1
)

set "CM26_VSENV_READY=1"
exit /b 0
