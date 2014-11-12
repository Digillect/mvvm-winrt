@echo off

setlocal enableextensions
set PATH=%~dp0\tools;%PATH%
set BuildTargets=%~dp0\packages\Digillect.Build.Tools.1.2.6\tools\Build.targets
set EnableNuGetPackageRestore=true

if not exist "%BuildTargets%" (
	nuget.exe install -o "%~dp0\packages" "%~dp0\.nuget\packages.config"
)

if not errorlevel 1 (
	if "%PROCESSOR_ARCHITECTURE%"=="AMD64" (
		"%ProgramFiles(x86)%\MSBuild\12.0\Bin\amd64\MSBuild.exe" build.proj %*
	) else (
		"%ProgramFiles%\MSBuild\12.0\Bin\MSBuild.exe" build.proj %*
	)
)
