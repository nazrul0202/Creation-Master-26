@echo off
REM Build the full CM26 solution (native bridge + managed app) and the native engine test.
setlocal
call "%~dp0vsenv.cmd" || exit /b 1

echo === Building native engine bridge (Release ^| x64) ===
REM The managed app references CM26.EngineBridge.dll by HintPath, so the bridge
REM must exist before the solution build runs. Building it explicitly first makes
REM a fresh clone work without relying on MSBuild project scheduling.
call "%~dp0build-bridge.cmd"
if errorlevel 1 ( echo BRIDGE BUILD FAILED & exit /b 1 )

echo === Building full solution (Release ^| x64) ===
msbuild "%~dp0CM26.slnx" /p:Configuration=Release /p:Platform=x64 /v:m /nologo
if errorlevel 1 ( echo SOLUTION BUILD FAILED & exit /b 1 )

echo === Building native engine + smoke test ===
call "%~dp0build.cmd"
if errorlevel 1 ( echo NATIVE BUILD FAILED & exit /b 1 )

echo === Running native engine smoke test ===
pushd "%~dp0"
EngineSmokeTest.exe
set SMOKE=%ERRORLEVEL%
popd
echo EngineSmokeTest exit=%SMOKE%
if not "%SMOKE%"=="0" ( echo ENGINE TEST FAILED & exit /b %SMOKE% )

echo === ALL BUILDS + ENGINE TEST PASSED ===
endlocal
