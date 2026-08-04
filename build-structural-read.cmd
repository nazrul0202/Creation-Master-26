@echo off
REM Builds the structural-read regression probe (native, x64).
setlocal
call "%~dp0vsenv.cmd" || exit /b 1
if not exist "obj\native-structural-read" mkdir "obj\native-structural-read"
cl /nologo /std:c++20 /W4 /EHsc /DUNICODE /D_UNICODE /DWIN32_LEAN_AND_MEAN /I src /Fo:obj\native-structural-read\ /Fe:StructuralReadTest.exe tests\structural_read.cpp src\database_engine.cpp /link bcrypt.lib
if errorlevel 1 exit /b 1
endlocal
