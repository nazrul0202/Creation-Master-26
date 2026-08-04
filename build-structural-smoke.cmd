@echo off
REM Builds the structural-writer regression probe (native, x64).
setlocal
call "%~dp0vsenv.cmd" || exit /b 1
if not exist "obj\native-structural-smoke" mkdir "obj\native-structural-smoke"
cl /nologo /std:c++20 /W4 /EHsc /DUNICODE /D_UNICODE /DWIN32_LEAN_AND_MEAN /I src /Fo:obj\native-structural-smoke\ /Fe:StructuralSmokeTest.exe tests\structural_smoke.cpp src\database_engine.cpp /link bcrypt.lib
if errorlevel 1 exit /b 1
endlocal
