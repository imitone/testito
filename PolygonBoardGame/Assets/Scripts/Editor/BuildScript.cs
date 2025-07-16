using UnityEngine;
using UnityEditor;
using System.IO;

public class BuildScript
{
    [MenuItem("Build/Build All Platforms")]
    public static void BuildAllPlatforms()
    {
        BuildWindows();
        BuildMac();
        BuildLinux();
    }
    
    [MenuItem("Build/Build Windows")]
    public static void BuildWindows()
    {
        BuildGame(BuildTarget.StandaloneWindows64, "Windows/PolygonBoardGame.exe");
    }
    
    [MenuItem("Build/Build Mac")]
    public static void BuildMac()
    {
        BuildGame(BuildTarget.StandaloneOSX, "Mac/PolygonBoardGame.app");
    }
    
    [MenuItem("Build/Build Linux")]
    public static void BuildLinux()
    {
        BuildGame(BuildTarget.StandaloneLinux64, "Linux/PolygonBoardGame");
    }
    
    public static void BuildGame(BuildTarget target, string fileName)
    {
        string buildPath = Path.Combine(Application.dataPath, "../Builds", fileName);
        
        // Create directory if it doesn't exist
        string directory = Path.GetDirectoryName(buildPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        // Get all scenes in build settings
        string[] scenes = new string[EditorBuildSettings.scenes.Length];
        for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
        {
            scenes[i] = EditorBuildSettings.scenes[i].path;
        }
        
        // If no scenes in build settings, add the main scene
        if (scenes.Length == 0)
        {
            scenes = new string[] { "Assets/Scenes/MainGame.unity" };
        }
        
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = scenes;
        buildPlayerOptions.locationPathName = buildPath;
        buildPlayerOptions.target = target;
        buildPlayerOptions.options = BuildOptions.None;
        
        Debug.Log($"Building for {target} at {buildPath}");
        
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"Build succeeded for {target}: {report.summary.outputPath}");
        }
        else
        {
            Debug.LogError($"Build failed for {target}: {report.summary.result}");
        }
    }
    
    // Command line build method
    public static void BuildFromCommandLine()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        BuildTarget target = BuildTarget.StandaloneWindows64;
        string fileName = "PolygonBoardGame";
        
        // Parse command line arguments
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-buildTarget" && i + 1 < args.Length)
            {
                string targetString = args[i + 1];
                switch (targetString.ToLower())
                {
                    case "windows":
                        target = BuildTarget.StandaloneWindows64;
                        fileName = "Windows/PolygonBoardGame.exe";
                        break;
                    case "mac":
                        target = BuildTarget.StandaloneOSX;
                        fileName = "Mac/PolygonBoardGame.app";
                        break;
                    case "linux":
                        target = BuildTarget.StandaloneLinux64;
                        fileName = "Linux/PolygonBoardGame";
                        break;
                }
            }
        }
        
        BuildGame(target, fileName);
    }
}