@echo off
call "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\Tools\VsDevCmd.bat" -arch=x64
if not exist "obj\native-structural-smoke" mkdir "obj\native-structural-smoke"
cl /nologo /std:c++20 /W4 /EHsc /DUNICODE /D_UNICODE /DWIN32_LEAN_AND_MEAN /I src /Fo:obj\native-structural-smoke\ /Fe:StructuralSmokeTest.exe tests\structural_smoke.cpp src\database_engine.cpp /link bcrypt.lib
