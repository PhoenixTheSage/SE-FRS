@echo off
setlocal EnableDelayedExpansion
cd /d "%~dp0"

set "CMAKE="
where cmake >nul 2>&1
if not errorlevel 1 (
    for /f "delims=" %%I in ('where cmake') do (
        if not defined CMAKE set "CMAKE=%%I"
    )
)

set "PF86=%ProgramFiles(x86)%"
set "PF=%ProgramFiles%"
if not defined CMAKE if exist "%PF86%\Microsoft Visual Studio\2022\BuildTools\Common7\IDE\CommonExtensions\Microsoft\CMake\CMake\bin\cmake.exe" (
    set "CMAKE=%PF86%\Microsoft Visual Studio\2022\BuildTools\Common7\IDE\CommonExtensions\Microsoft\CMake\CMake\bin\cmake.exe"
)
if not defined CMAKE if exist "%PF%\Microsoft Visual Studio\2022\Community\Common7\IDE\CommonExtensions\Microsoft\CMake\CMake\bin\cmake.exe" (
    set "CMAKE=%PF%\Microsoft Visual Studio\2022\Community\Common7\IDE\CommonExtensions\Microsoft\CMake\CMake\bin\cmake.exe"
)
if not defined CMAKE (
    echo error : cmake is not on PATH. Install CMake or Visual Studio Desktop C++.
    exit /b 1
)

if not exist "third_party\FidelityFX-FSR2\tools\sc\FidelityFX_SC.exe" (
    echo error : missing FidelityFX_SC.exe under third_party\FidelityFX-FSR2\tools\sc
    exit /b 1
)

echo Using cmake: %CMAKE%
"%CMAKE%" -S . -B build -G "Visual Studio 17 2022" -A x64
if errorlevel 1 exit /b 1
"%CMAKE%" --build build --config Release
if errorlevel 1 exit /b 1

if exist "..\Assets\amd_frs.dll" (
    echo Built ..\Assets\amd_frs.dll
) else (
    echo warning : amd_frs.dll was not copied to Assets. Check the CMake runtime output.
)
exit /b 0
