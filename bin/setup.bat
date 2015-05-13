@echo off
rem ----сохраняем папку с dll в переменную
set dllName="%~dp0%API.dll"
if exist %dllName% (
echo API.dll exist
rem echo %dllName%
) else (
echo API.dll not exist
goto exit
)
rem ----узнаем путь к в реестре к значению пути gacutils
set regkey = ""
rem на разных версиях ОС по разному
set query=HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSBuild\ToolsVersions\4.0\11.0
rem set query=HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSBuild\ToolsVersions\4.0
For /F "UseBackQ Tokens=2*" %%a In (`Reg Query "%query%"^|Find /i "SDK40ToolsPath"`) Do (
set regkey=%%b
)
if "%regkey%" == "" (
echo regkey not exist
goto exit
) else (
echo regkey exist
rem echo %regkey%
)
set regkey=%regkey:~11,-20%
set folderkey = ""
For /F "UseBackQ Tokens=2*" %%I In (`Reg Query "%regkey%"^|Find /i "InstallationFolder"`) Do (
set folderkey=%%J
rem echo %%J
)
if "%folderkey%" == "" (
echo folderkey not exist
goto exit
) else (
echo folderkey exist
rem echo %folderkey%
)


cd %folderkey%
gacutil /i %dllname%

:exit
pause


:concat
set registrekey=%registrekey% %%J
goto :eof