@echo off

set sql=sqlcmd -S . -E

echo Enabling mixed authentication mode..
%sql% -Q "EXEC xp_instance_regwrite N'HKEY_LOCAL_MACHINE', N'Software\Microsoft\MSSQLServer\MSSQLServer', N'LoginMode', REG_DWORD, 2"

set "serverName="

echo Enter server name (Press Enter key to use default instance [MSSQLServer]): 
set /P "serverName="

if  "%serverName%" == "" (
    set "serverName=MSSQLServer"
)

echo Microsoft SQL Server Instance: %serverName%

net stop %serverName%
net start %serverName%

echo Mixed authentication mode is now enabled.