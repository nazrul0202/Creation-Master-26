@echo off
REM Builds the C++/CLI engine bridge that the managed app references.
setlocal
call "%~dp0vsenv.cmd" || exit /b 1

REM Standalone Build Tools can include C++ without Visual Studio's copy of
REM Microsoft.NET.Sdk. Resolve the newest installed .NET 8 SDK only for this
REM C++/CLI build; setlocal prevents it leaking into the managed solution build.
if not exist "%VSPATH%\MSBuild\Sdks\Microsoft.NET.Sdk\Sdk" (
    set "CM26_DOTNET8_SDK="
    for /f "tokens=1" %%i in ('dotnet --list-sdks ^| findstr /b "8\."') do set "CM26_DOTNET8_SDK=%%i"
    if not defined CM26_DOTNET8_SDK (
        echo [bridge] ERROR: the .NET 8 SDK is required for the C++/CLI engine bridge.
        exit /b 1
    )
    call set "MSBuildSDKsPath=%%ProgramFiles%%\dotnet\sdk\%%CM26_DOTNET8_SDK%%\Sdks"
    call echo [bridge] Using .NET SDK %%CM26_DOTNET8_SDK%% for SDK resolution.
)

msbuild "%~dp0src-native\CM26.EngineBridge\CM26.EngineBridge.vcxproj" /t:Build /p:Configuration=Release /p:Platform=x64 /v:m /nologo
if errorlevel 1 ( echo BRIDGE BUILD FAILED & exit /b 1 )

endlocal
