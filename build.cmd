@echo off
call "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\Tools\VsDevCmd.bat" -arch=x64
cl /nologo /std:c++20 /W4 /EHsc /DUNICODE /D_UNICODE /DWIN32_LEAN_AND_MEAN /Fe:CreationMaster26.exe src\main.cpp src\database_engine.cpp /link /SUBSYSTEM:WINDOWS bcrypt.lib comctl32.lib comdlg32.lib shell32.lib ole32.lib user32.lib gdi32.lib
if errorlevel 1 exit /b 1
cl /nologo /std:c++20 /W4 /EHsc /DUNICODE /D_UNICODE /DWIN32_LEAN_AND_MEAN /I src /Fe:EngineSmokeTest.exe tests\engine_smoke.cpp src\database_engine.cpp /link bcrypt.lib
