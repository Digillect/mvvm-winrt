@echo off

setlocal enableextensions
set PATH=%~dp0\tools;%PATH%
set EnableNuGetPackageRestore=true

pushd %~dp0

if exist .nuget\nuget.exe (
	set NuGetExe=.nuget\nuget.exe
) else (
	set NuGetExe=nuget.exe
)

if exist .nuget\packages.config (
	%NuGetExe% install .nuget\packages.config -OutputDirectory packages -Verbosity quiet -NonInteractive
)

if exist *.sln (
	for %%f in (*.sln) do %NuGetExe% restore "%%f" -Verbosity quiet -NonInteractive
)

if not errorlevel 1 (
	if "%PROCESSOR_ARCHITECTURE%"=="AMD64" (
		"%ProgramFiles(x86)%\MSBuild\12.0\Bin\amd64\MSBuild.exe" build.proj /v:m %*
	) else (
		"%ProgramFiles%\MSBuild\12.0\Bin\MSBuild.exe" build.proj /v:m %*
	)
)

popd
