@echo off
call "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\Tools\VsDevCmd.bat" -arch=x64
msbuild src-native\CM26.EngineBridge\CM26.EngineBridge.vcxproj /t:Build /p:Configuration=Release /p:Platform=x64
