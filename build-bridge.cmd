@echo off
REM Builds the C++/CLI engine bridge that the managed app references.
setlocal
call "%~dp0vsenv.cmd" || exit /b 1

msbuild "%~dp0src-native\CM26.EngineBridge\CM26.EngineBridge.vcxproj" /t:Build /p:Configuration=Release /p:Platform=x64 /v:m /nologo
if errorlevel 1 ( echo BRIDGE BUILD FAILED & exit /b 1 )

endlocal
