@echo off
:: Unity Build Script for Polygon Board Game (Windows)
:: This script builds the game for Windows

echo 🎮 Building Polygon Board Game for Windows...

:: Set variables
set PROJECT_PATH=%cd%
set UNITY_PATH="C:\Program Files\Unity\Hub\Editor\2021.3.0f1\Editor\Unity.exe"
set BUILD_PATH=%PROJECT_PATH%\Builds
set GAME_NAME=PolygonBoardGame

:: Create build directory
if not exist "%BUILD_PATH%" mkdir "%BUILD_PATH%"

:: Check if Unity is installed
if not exist %UNITY_PATH% (
    echo ❌ Unity not found at %UNITY_PATH%
    echo    Trying alternative paths...
    
    :: Try common Unity installation paths
    set UNITY_PATHS[0]="C:\Program Files\Unity\Hub\Editor\2021.3.0f1\Editor\Unity.exe"
    set UNITY_PATHS[1]="C:\Program Files\Unity\Hub\Editor\2022.3.0f1\Editor\Unity.exe"
    set UNITY_PATHS[2]="C:\Program Files\Unity\2021.3.0f1\Editor\Unity.exe"
    set UNITY_PATHS[3]="C:\Program Files (x86)\Unity\Editor\Unity.exe"
    
    for %%i in (0 1 2 3) do (
        if exist !UNITY_PATHS[%%i]! (
            set UNITY_PATH=!UNITY_PATHS[%%i]!
            echo ✅ Found Unity at: !UNITY_PATH!
            goto :unity_found
        )
    )
    
    echo ❌ Unity Editor not found! Please install Unity 2021.3 LTS or newer.
    echo    Download from: https://unity.com/download
    pause
    exit /b 1
)

:unity_found
echo 🔧 Unity Path: %UNITY_PATH%
echo 📁 Project Path: %PROJECT_PATH%
echo 📦 Build Path: %BUILD_PATH%

:: Build for Windows
echo 🏗️  Building for Windows...
%UNITY_PATH% -batchmode -quit -projectPath "%PROJECT_PATH%" ^
    -buildTarget Win64 ^
    -buildPath "%BUILD_PATH%\Windows\%GAME_NAME%.exe" ^
    -executeMethod BuildScript.BuildFromCommandLine ^
    -logFile "%BUILD_PATH%\build.log"

if %errorlevel% equ 0 (
    echo ✅ Build completed successfully!
    echo 📍 Game built to: %BUILD_PATH%\Windows\%GAME_NAME%.exe
    echo.
    echo 🎯 To run the game:
    echo    cd %BUILD_PATH%\Windows
    echo    %GAME_NAME%.exe
    echo.
    echo 🚀 Game is ready to play!
) else (
    echo ❌ Build failed! Check build.log for details:
    echo    %BUILD_PATH%\build.log
    
    if exist "%BUILD_PATH%\build.log" (
        echo.
        echo 📋 Last 20 lines of build log:
        more +20 "%BUILD_PATH%\build.log"
    )
    
    pause
    exit /b 1
)

pause