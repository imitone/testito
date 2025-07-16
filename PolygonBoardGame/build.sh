#!/bin/bash

# Unity Build Script for Polygon Board Game
# This script builds the game for Linux

echo "🎮 Building Polygon Board Game..."

# Set variables
PROJECT_PATH="$(pwd)"
UNITY_PATH="/opt/Unity/Editor/Unity"
BUILD_PATH="$PROJECT_PATH/Builds"
GAME_NAME="PolygonBoardGame"

# Create build directory
mkdir -p "$BUILD_PATH"

# Check if Unity is installed
if [ ! -f "$UNITY_PATH" ]; then
    echo "❌ Unity not found at $UNITY_PATH"
    echo "   Trying alternative paths..."
    
    # Try common Unity installation paths
    UNITY_PATHS=(
        "/Applications/Unity/Unity.app/Contents/MacOS/Unity"
        "/opt/unity/Editor/Unity"
        "/usr/bin/unity-editor"
        "unity-editor"
    )
    
    for path in "${UNITY_PATHS[@]}"; do
        if command -v "$path" >/dev/null 2>&1; then
            UNITY_PATH="$path"
            echo "✅ Found Unity at: $UNITY_PATH"
            break
        fi
    done
    
    if [ ! -f "$UNITY_PATH" ] && ! command -v "$UNITY_PATH" >/dev/null 2>&1; then
        echo "❌ Unity Editor not found! Please install Unity 2021.3 LTS or newer."
        echo "   Download from: https://unity.com/download"
        exit 1
    fi
fi

echo "🔧 Unity Path: $UNITY_PATH"
echo "📁 Project Path: $PROJECT_PATH"
echo "📦 Build Path: $BUILD_PATH"

# Build for Linux
echo "🏗️  Building for Linux..."
"$UNITY_PATH" -batchmode -quit -projectPath "$PROJECT_PATH" \
    -buildTarget Linux64 \
    -buildPath "$BUILD_PATH/Linux/$GAME_NAME" \
    -executeMethod BuildScript.BuildFromCommandLine \
    -logFile "$BUILD_PATH/build.log" \
    -buildTarget linux

BUILD_RESULT=$?

if [ $BUILD_RESULT -eq 0 ]; then
    echo "✅ Build completed successfully!"
    echo "📍 Game built to: $BUILD_PATH/Linux/$GAME_NAME"
    
    # Make the game executable
    chmod +x "$BUILD_PATH/Linux/$GAME_NAME"
    
    echo ""
    echo "🎯 To run the game:"
    echo "   cd $BUILD_PATH/Linux"
    echo "   ./$GAME_NAME"
    echo ""
    echo "🚀 Game is ready to play!"
else
    echo "❌ Build failed! Check build.log for details:"
    echo "   $BUILD_PATH/build.log"
    
    if [ -f "$BUILD_PATH/build.log" ]; then
        echo ""
        echo "📋 Last 20 lines of build log:"
        tail -n 20 "$BUILD_PATH/build.log"
    fi
    
    exit 1
fi