@echo off
REM Build the full CM26 solution (native bridge + managed app) and the native engine test.
setlocal
set MSBUILD="C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"

echo === Building full solution (Release ^| x64) ===
%MSBUILD% "%~dp0CM26.slnx" /p:Configuration=Release /p:Platform=x64 /v:m /nologo
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
