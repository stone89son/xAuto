@echo off
D:
set "EXE_PATH=D:\Projects\xAuto\src\xAuto.Console\bin\Debug\xAuto.exe"
for /L %%i in (1,1,100) do (
    echo Run %%i
    "%EXE_PATH%"
)
echo Done.
pause