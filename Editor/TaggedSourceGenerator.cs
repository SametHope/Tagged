#if UNITY_EDITOR

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public sealed class TaggedSourceGenerator
{
    // -------------------- These constants can be modified for customization --------------------

    private const string TAG_SANITIZATION_REGEX_REPLACEMENT = "_";
    private const string TAG_CONSTANTS_PREFIX = "T";
    private const double UPDATE_INTERVAL_MIN_SECONDS = 2;
    private const LogLevel LOG_LEVEL = LogLevel.Warning | LogLevel.Error | LogLevel.Info;
    private const bool LOG_PASSIVE_REPETITIVE = false;

    // -------------------- These constants should not be modified --------------------

    private const string GENERATED_FILE_NAME = "TaggedSourceGenerated";
    private const string GENERATED_FILE_EXTENSION = ".cs";
    private const string GENERATED_FILE_NAME_WITH_EXTENSION = GENERATED_FILE_NAME + GENERATED_FILE_EXTENSION;
    private const string THIS_SCRIPT_NAME_WITH_EXTENSION = "TaggedSourceGenerator.cs";
    private const string TAG_SANITIZATION_REGEX = "[^a-zA-Z0-9_]";

    private static string _assetsFolderPath;
    private static string _packagesFolderPath;
    private static bool _isEditorQuitting;
    private static double _lastUpdateTime;
    private static string[] _previousTags;
    private static string _cachedGeneratedFilePath;

    [Flags]
    public enum LogLevel
    {
        None = 0,
        Error = 1,
        Warning = 2,
        Info = 4,
        Everything = Error | Warning | Info
    }

    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        _assetsFolderPath = Application.dataPath;
        _packagesFolderPath = Path.GetFullPath(Path.Combine(_assetsFolderPath, "../Library/PackageCache"));
        _lastUpdateTime = EditorApplication.timeSinceStartup;

        EditorApplication.delayCall += OnEditorUpdate;
        EditorApplication.quitting += () => { _isEditorQuitting = true; };

        Log(LogLevel.Info, "TaggedSourceGenerator initialized. Paths set and update loop started.");
    }

    private static void OnEditorUpdate()
    {
        if(_isEditorQuitting)
        {
            Log(LogLevel.Info, $"{nameof(OnEditorUpdate)}: Editor is quitting. Stopping updates.");
            return;
        }

        EditorApplication.delayCall -= OnEditorUpdate;
        EditorApplication.delayCall += OnEditorUpdate;

        if(Application.isPlaying)
        {
            Log(LogLevel.Info, $"{nameof(OnEditorUpdate)}: Application is in Play Mode. Tag generation skipped.");
            return;
        }

        if(!ShouldUpdate()) return;

        _lastUpdateTime = EditorApplication.timeSinceStartup;

        if(_cachedGeneratedFilePath == null && !TryLocateGeneratedFile(out _cachedGeneratedFilePath))
        {
            Log(LogLevel.Warning, $"{nameof(OnEditorUpdate)}: Could not locate generated file. Ensure 'Tagged' folder with a 'Runtime' subfolder exists.");
            return;
        }

        string[] currentTags = UnityEditorInternal.InternalEditorUtility.tags;
        if(_previousTags != null && currentTags.SequenceEqual(_previousTags))
        {
            if(LOG_PASSIVE_REPETITIVE)
            {
                Log(LogLevel.Info, $"{nameof(OnEditorUpdate)}: Tags unchanged. Skipping regeneration.");
            }
            return;
        }

        string newScriptContent = GenerateScriptContent(currentTags);

        try
        {
            File.WriteAllText(_cachedGeneratedFilePath, newScriptContent);
            _previousTags = currentTags;
            Log(LogLevel.Info, $"{nameof(OnEditorUpdate)}: Tags successfully generated and saved to: {_cachedGeneratedFilePath}");
        }
        catch(Exception e)
        {
            Log(LogLevel.Error, $"{nameof(OnEditorUpdate)}: Failed to write tag constants file. Error: {e.Message}");
        }
    }

    private static bool ShouldUpdate()
    {
        double currentTime = EditorApplication.timeSinceStartup;
        double timeSinceLastUpdate = currentTime - _lastUpdateTime;
        bool shouldUpdate = timeSinceLastUpdate >= UPDATE_INTERVAL_MIN_SECONDS;

        if(LOG_PASSIVE_REPETITIVE)
        {
            if(shouldUpdate)
            {
                Log(LogLevel.Info, $"{nameof(ShouldUpdate)}: Update interval reached. Time since last update: {timeSinceLastUpdate:F2} seconds.");
            }
            else
            {
                double timeLeft = UPDATE_INTERVAL_MIN_SECONDS - timeSinceLastUpdate;
                Log(LogLevel.Info, $"{nameof(ShouldUpdate)}: Time interval not reached yet. {timeLeft:F2} seconds left.");
            }
        }

        return shouldUpdate;
    }

    private static bool TryLocateGeneratedFile(out string generatedFilePath)
    {
        if(!string.IsNullOrEmpty(_cachedGeneratedFilePath))
        {
            generatedFilePath = _cachedGeneratedFilePath;
            Log(LogLevel.Info, $"{nameof(TryLocateGeneratedFile)}: Using cached generated file path: {generatedFilePath}");
            return true;
        }

        Log(LogLevel.Info, $"{nameof(TryLocateGeneratedFile)}: Locating generated file...");

        string[] packagePaths = Directory.GetDirectories(_packagesFolderPath, "com.samethope.tagged", SearchOption.TopDirectoryOnly);
        Log(LogLevel.Info, $"{nameof(TryLocateGeneratedFile)}: Searched package paths: {_packagesFolderPath}");

        if(packagePaths.Length > 0)
        {
            generatedFilePath = Path.Combine(packagePaths[0], GENERATED_FILE_NAME_WITH_EXTENSION);
            _cachedGeneratedFilePath = generatedFilePath;
            Log(LogLevel.Info, $"{nameof(TryLocateGeneratedFile)}: Located generated file in packages at {generatedFilePath}");
            return true;
        }

        string[] taggedFolders = Directory.GetDirectories(_assetsFolderPath, "Tagged", SearchOption.AllDirectories);
        Log(LogLevel.Info, $"{nameof(TryLocateGeneratedFile)}: Searched asset paths: {_assetsFolderPath}");

        foreach(string taggedDir in taggedFolders)
        {
            string runtimeDir = Path.Combine(taggedDir, "Runtime");
            Log(LogLevel.Info, $"{nameof(TryLocateGeneratedFile)}: Searched directory: {taggedDir}");

            if(Directory.Exists(runtimeDir))
            {
                generatedFilePath = Path.Combine(runtimeDir, GENERATED_FILE_NAME_WITH_EXTENSION);
                _cachedGeneratedFilePath = generatedFilePath;
                Log(LogLevel.Info, $"{nameof(TryLocateGeneratedFile)}: Located generated file in assets at {generatedFilePath}");
                return true;
            }
        }

        generatedFilePath = null;
        Log(LogLevel.Info, $"{nameof(TryLocateGeneratedFile)}: Generated file not found in package cache or assets.");
        return false;
    }

    private static string GenerateScriptContent(string[] tags)
    {
        var tagConstants = new StringBuilder();
        foreach(string tag in tags)
        {
            string sanitizedTagName = Regex.Replace(tag, TAG_SANITIZATION_REGEX, TAG_SANITIZATION_REGEX_REPLACEMENT);
            tagConstants.AppendLine($"    public const string {TAG_CONSTANTS_PREFIX}{sanitizedTagName} = \"{tag}\";");
        }

        Log(LogLevel.Info, $"{nameof(GenerateScriptContent)}: Generated content for {tags.Length} tags.");
        return $@"
// This file is auto-generated. Do not modify it manually.
// Whenever a tag is added or removed, this file will be regenerated.
public partial class Tagged
{{
{tagConstants}
}}";
    }

    private static void Log(LogLevel level, string message)
    {
        if((LOG_LEVEL & level) != 0 || LOG_LEVEL == LogLevel.Everything)
        {
            Debug.Log($"[{level}] {message}");
        }
    }
}

#endif
