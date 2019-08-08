@echo off

echo.
echo.
echo Start installing Node..
echo.

echo Important Note. 
echo This may take some time installing Node.js in your system.

msiexec /qn /l* node-log.txt /i node-v10.16.2-x64.msi

echo Done installing Node.
echo.
echo.