#if UNITY_EDITOR
#pragma warning disable CS0162 // Unreachable code detected

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

// This script is used to generate a script file that contains constants for all tags in the project.
// This script is not included in the build and only used in the editor.
public sealed class TaggedSourceGenerator
{
    // -------------------- These constants can be modified for customization --------------------

    private const string TAG_CONSTANTS_PREFIX = "T";  // Prefix used for constants in generated script
    private const double UPDATE_INTERVAL_MIN_SECONDS = 2;  // Interval in seconds to check for updates
    private const string TAG_CONSTANTS_REGEX_SPECIAL_CHAR_REPLACEMENT = "_";  // Replacement for special characters in tag names

    private const int EXTRA_LOGS_LEVEL = 2;
    // 0 -> No extra logs, only errors and warnings
    // 1 -> 0 + Initialization, deinitialization, file generation
    // 2 -> 1 + Tag changes (Recommended for debugging)
    // 3 -> 2 + Every update check (Dont)

    // -------------------- These values should not be altered with --------------------

    private const string TAG_SANITIZATION_REGEX = "[^a-zA-Z0-9_]";  // Regex for sanitizing tag names of special characters, removes all characters except a-z, A-Z, 0-9, and _

    // Set on initialization
    private static string _assetsFolderPath;
    private static string _generatedFileFullPath; // .../TaggedSourceGenerated.cs
    private static string _generatorFileFullPath; // .../TaggedSourceGenerator.cs, this file

    // Set on editor update
    private static double _previousDelayCallTime;
    private static double _currentDelayCallTime;
    private static string[] _previousTags;
    private static string[] _currentTags;

    // I really want to name this _eject
    private static bool _detach;

    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        if(EXTRA_LOGS_LEVEL >= 1) Debug.Log($"[{nameof(TaggedSourceGenerator)}] Initializing...");

        _assetsFolderPath = Application.dataPath;
        _generatorFileFullPath = Path.Combine(_assetsFolderPath, "Plugins", "SametHope", "Tagged", "Editor", "TaggedSourceGenerator.cs");
        _generatedFileFullPath = Path.Combine(_assetsFolderPath, "Plugins", "SametHope", "Tagged", "Runtime", "TaggedSourceGenerated.cs");

        if(EXTRA_LOGS_LEVEL >= 1) Debug.Log($"[{nameof(TaggedSourceGenerator)}] Listing determined paths.\nGenerator:\n{_generatorFileFullPath}\nGenerated:\n{_generatedFileFullPath}\n");

        try { _generatorFileFullPath = Path.GetFullPath(_generatorFileFullPath); }
        catch
        {
            Debug.LogWarning($"[{nameof(TaggedSourceGenerator)}] Could not confirm its path to be a full valid path: \n{_generatorFileFullPath}\n");
            return;
        }

        try { _generatedFileFullPath = Path.GetFullPath(_generatedFileFullPath); }
        catch
        {
            Debug.LogWarning($"[{nameof(TaggedSourceGenerator)}] Could not confirm its path to be a full valid path: \n{_generatedFileFullPath}\n");
            return;
        }


        _previousDelayCallTime = EditorApplication.timeSinceStartup;

        EditorApplication.delayCall += OnEditorUpdate;
        EditorApplication.quitting += () => _detach = true;
    }

    private static void OnEditorUpdate()
    {
        EditorApplication.delayCall -= OnEditorUpdate; // Remove the previous delay call that won't be called again
        _currentDelayCallTime = EditorApplication.timeSinceStartup; // Update the time regardless of any action done or not

        if(_detach)
        {
            if(EXTRA_LOGS_LEVEL >= 1) Debug.Log($"[{nameof(TaggedSourceGenerator)}] Detaching from editor update.");
            return;
        }

        EditorApplication.delayCall += OnEditorUpdate; // Reattach to editor update

        // Not now
        if(Application.isPlaying || (_currentDelayCallTime - _previousDelayCallTime) < UPDATE_INTERVAL_MIN_SECONDS)
        {
            if(EXTRA_LOGS_LEVEL >= 3) Debug.Log($"[{nameof(TaggedSourceGenerator)}] Not time yet for an update.");
            return;
        }

        // No tags changed
        _currentTags = UnityEditorInternal.InternalEditorUtility.tags;
        if(_previousTags != null && _currentTags.SequenceEqual(_previousTags))
        {
            if(EXTRA_LOGS_LEVEL >= 3) Debug.Log($"[{nameof(TaggedSourceGenerator)}] No tags changed.");
            return;
        }
        if(EXTRA_LOGS_LEVEL >= 2) Debug.Log($"[{nameof(TaggedSourceGenerator)}] Tags changed. There are {_currentTags.Length} tags found. Generated script will be updated.");
        _previousTags = _currentTags;

        // if generator file is not found it means we have been removed from the project, detach
        if(!File.Exists(_generatorFileFullPath))
        {
            Debug.LogWarning($"[{nameof(TaggedSourceGenerator)}] TaggedSourceGenerator.cs file could not be found at '{_generatorFileFullPath}'. Detaching from editor update. Try reimporting the package with the generator file placed in the correct path.");
            _detach = true;
            return;
        }

        // strange but lets be ok with it
        if(!File.Exists(_generatedFileFullPath))
        {
            Debug.LogWarning($"[{nameof(TaggedSourceGenerator)}] TaggedSourceGenerated.cs file could not be found at '{_generatedFileFullPath}'. Will generate a new one.");
        }

        WriteGeneratedScriptSafe(GenerateScriptContent(_currentTags), _generatedFileFullPath);
    }

    private static void WriteGeneratedScriptSafe(string content, string fullPath)
    {
        if(EXTRA_LOGS_LEVEL >= 1) Debug.Log($"[{nameof(TaggedSourceGenerator)}] Writing tag constants script:\n{fullPath}.");
        try
        {
            // Preserve the original last write time to not freak out Unity about timestamps with 100 warning logs per second
            DateTime originalLastWriteTime = File.GetLastWriteTime(fullPath);
            File.WriteAllText(fullPath, content);
            File.SetLastWriteTime(fullPath, originalLastWriteTime);
        }
        catch(Exception e)
        {
            Debug.LogWarning($"[{nameof(TaggedSourceGenerator)}] Failed writing tag constants script.\nError: {e.Message}");
        }
    }

    private static string GenerateScriptContent(string[] tags)
    {
        var tagConstants = new StringBuilder();
        foreach(string tag in tags)
        {
            string sanitizedTagName = Regex.Replace(tag, TAG_SANITIZATION_REGEX, TAG_CONSTANTS_REGEX_SPECIAL_CHAR_REPLACEMENT);
            tagConstants.AppendLine($"    public const string {TAG_CONSTANTS_PREFIX}{sanitizedTagName} = @\"{tag}\";");
        }

        return $@"
// This file is auto-generated. Do not modify it manually.
// Whenever a tag is added, removed or modifier, this file will be regenerated.
// This file must remain at Assets/Plugins/SametHope/Tagged/Runtime/TaggedSourceGenerated.cs
// Last update at local time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
public partial class Tagged
{{
{tagConstants}}}";
    }
}

#pragma warning restore CS0162 // Unreachable code detected
#endif
