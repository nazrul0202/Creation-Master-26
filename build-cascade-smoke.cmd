@echo off
REM Builds the cascade-delete regression probe (native, x64).
setlocal
call "%~dp0vsenv.cmd" || exit /b 1
if not exist "obj\native-cascade-smoke" mkdir "obj\native-cascade-smoke"
cl /nologo /std:c++20 /W4 /EHsc /DUNICODE /D_UNICODE /DWIN32_LEAN_AND_MEAN /I src /Fo:obj\native-cascade-smoke\ /Fe:CascadeSmokeTest.exe tests\cascade_smoke.cpp src\database_engine.cpp /link bcrypt.lib
if errorlevel 1 exit /b 1
endlocal
