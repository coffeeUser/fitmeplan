@echo on
@echo ===============================================================================
@echo env.cmd
@echo ===============================================================================

SET SolutionPath=%cd%
IF NOT DEFINED msbuild SET msbuild=%ProgramFiles(x86)%\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\MSBuild.exe

powershell -Command ".\startServices.ps1"
@if errorlevel 1 goto error

@echo Press Any Key to stop services
pause

stopServices.bat