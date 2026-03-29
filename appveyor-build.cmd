@echo off
setlocal
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0appveyor-build.ps1"
exit /b %ERRORLEVEL%
