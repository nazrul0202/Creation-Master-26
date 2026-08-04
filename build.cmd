@echo off
REM Builds the native CM26 engine app and the engine smoke test (Release, x64).
setlocal
call "%~dp0vsenv.cmd" || exit /b 1

if not exist "obj\native-app" mkdir "obj\native-app"
if not exist "obj\native-engine-smoke" mkdir "obj\native-engine-smoke"

cl /nologo /std:c++20 /W4 /EHsc /DUNICODE /D_UNICODE /DWIN32_LEAN_AND_MEAN /Fo:obj\native-app\ /Fe:CreationMaster26.exe src\main.cpp src\database_engine.cpp /link /SUBSYSTEM:WINDOWS bcrypt.lib comctl32.lib comdlg32.lib shell32.lib ole32.lib user32.lib gdi32.lib
if errorlevel 1 exit /b 1

cl /nologo /std:c++20 /W4 /EHsc /DUNICODE /D_UNICODE /DWIN32_LEAN_AND_MEAN /I src /Fo:obj\native-engine-smoke\ /Fe:EngineSmokeTest.exe tests\engine_smoke.cpp src\database_engine.cpp /link bcrypt.lib
if errorlevel 1 exit /b 1

endlocal
