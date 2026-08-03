@echo off
call "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\Tools\VsDevCmd.bat" -arch=x64
if not exist "obj\native-cascade-smoke" mkdir "obj\native-cascade-smoke"
cl /nologo /std:c++20 /W4 /EHsc /DUNICODE /D_UNICODE /DWIN32_LEAN_AND_MEAN /I src /Fo:obj\native-cascade-smoke\ /Fe:CascadeSmokeTest.exe tests\cascade_smoke.cpp src\database_engine.cpp /link bcrypt.lib
