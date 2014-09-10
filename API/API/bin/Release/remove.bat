@echo off
rem ----сохраняем папку с dll в переменную
set dllName=API


rem ----узнаем путь к в реестре к значению пути gacutils
set regkey = ""
For /F "UseBackQ Tokens=2*" %%a In (`Reg Query "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSBuild\ToolsVersions\4.0"^|Find /i "SDK40ToolsPath"`) Do (
set regkey=%%b
)
echo %regkey%
set regkey=%regkey:~11,-20%
set folderkey = ""
For /F "UseBackQ Tokens=2*" %%I In (`Reg Query "%regkey%"^|Find /i "InstallationFolder"`) Do (
set folderkey=%%J
echo %%J
)


cd %folderkey%
gacutil /u %dllname%

:exit
pause


:concat
set registrekey=%registrekey% %%J
goto :eof