@echo Off
set config=%1
if "%config%" == "" (
   set config=Debug
)
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild "%~dp0build.proj" /p:Configuration="%config%" /m /v:M /fl /flp:LogFile="%~dp0build.log";Verbosity=Normal /nr:false
pause