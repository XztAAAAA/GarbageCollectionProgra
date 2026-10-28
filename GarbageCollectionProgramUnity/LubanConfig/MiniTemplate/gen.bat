@echo off
setlocal

cd /d "%~dp0"

set "LUBAN_DLL=%~dp0Luban\Luban\Luban.dll"
set "CONF_ROOT=%~dp0"
set "OUTPUT_CODE_DIR=%~dp0..\..\Assets\_Project\Scripts\Luban"
set "OUTPUT_DATA_DIR=%~dp0..\..\Assets\Resources\Luban"

if not exist "%LUBAN_DLL%" (
    echo [ERROR] Luban.dll was not found:
    echo "%LUBAN_DLL%"
    pause
    exit /b 1
)

where dotnet >nul 2>nul
if errorlevel 1 (
    echo [ERROR] dotnet was not found in PATH.
    pause
    exit /b 1
)

if not exist "%OUTPUT_CODE_DIR%\" mkdir "%OUTPUT_CODE_DIR%"
if not exist "%OUTPUT_DATA_DIR%\" mkdir "%OUTPUT_DATA_DIR%"

dotnet "%LUBAN_DLL%" ^
    -t all ^
    -c cs-simple-json ^
    -d json ^
    --conf %CONF_ROOT%\luban.conf ^
    -x "outputCodeDir=%OUTPUT_CODE_DIR%" ^
    -x "outputDataDir=%OUTPUT_DATA_DIR%"

set "LUBAN_EXIT_CODE=%ERRORLEVEL%"

if not "%LUBAN_EXIT_CODE%"=="0" (
    echo.
    echo [ERROR] Luban generation failed with exit code %LUBAN_EXIT_CODE%.
)

pause
exit /b %LUBAN_EXIT_CODE%
