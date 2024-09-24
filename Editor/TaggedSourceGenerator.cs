#if UNITY_EDITOR

using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

// This script generates a pre-defined script with constants for all tags in the project
public sealed class TaggedSourceGenerator
{
    // -------------------- These constants can be modified for customization --------------------

    // What to replace invalid characters with in tag constant names
    // Different tags that are only differentiated by special chars such as "enemy!", "enemy?" and "enemy " will result in the same constant and not compile
    // Regex that is used to replace invalid characters replaces all characters that are not a-z, A-Z, 0-9 or _
    private const string TAG_SANITIZATION_REGEX_REPLACEMENT = "_";

    // Prefix for all tag constant names
    // Modifying this will overwrite existing constant names casuing existing code will need to be updated
    // Unless this value is a number or a special character that is not allowed in C# variable names and there are no tags starting with numbers, removing (replacing with a empty string) this prefix will not cause any issues
    private const string TAG_CONSTANTS_PREFIX = "T";

    // How often to try and update the generated script file in seconds, not taking into account how often EditorApplication.delayCall is called
    private const double UPDATE_INTERVAL_MIN_SECONDS = 2;

    // -------------------- These constants should not be modified --------------------

    private const string GENERATED_FILE_NAME = "TaggedSourceGenerated";
    private const string GENERATED_FILE_EXTENSION = ".cs";
    private const string GENERATED_FILE_NAME_WITH_EXTENSION = GENERATED_FILE_NAME + GENERATED_FILE_EXTENSION;
    private const string THIS_SCRIPT_NAME_WITH_EXTENSION = "TaggedSourceGenerator.cs";
    // See http://www.regexstorm.net/tester for testing
    private const string TAG_SANITIZATION_REGEX = "[^a-zA-Z0-9_]";

    // These are defined on class level instead of methods to avoid memory allocation and garbage collection
    // they are mostly set per-frame
    private static string __generatedScriptFilePath;
    private static string __generatedScriptFileFullPath;
    private static string[] __tags;
    private static string[] __previousTags;
    private static string[] _possiblePaths;
    private static string __formattedTagsString;
    private static string __newFullScriptContent;
    private static string __oldFullScriptContent;
    private static double __lastUpdateTime;
    private static string __timelessOldContent;
    private static string __timelessNewContent;
    private static bool __isQuittingEditorApp;
    private static string[] __thisScriptsPossiblePaths;

    private static string AssetsFolderPath => Application.dataPath; // To use when script is inside Assets
    private static string PackagesFolderPath => Path.GetFullPath(Path.Combine(AssetsFolderPath, "../Library/PackageCache")); // To use when script is inside Packages

    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        //EditorApplication.update += OnEditorUpdate;

        // This strange way of hooking into the update loop is preferred over refular update delegate to prevent the script from running every frame
        EditorApplication.delayCall += OnEditorUpdate;
        __lastUpdateTime = EditorApplication.timeSinceStartup;
        EditorApplication.quitting += () =>
        {
            __isQuittingEditorApp = true;
        };
    }

    private static void OnEditorUpdate()
    {
        // Establish a delay call loop
        EditorApplication.delayCall -= OnEditorUpdate; // Remove the previous (current) call to prevent stacking

        // If we are quitting the editor or got deleted, we must detach from the update loop
        if(__isQuittingEditorApp) return;
        if(!AreWeInProject())
        {
            Debug.LogWarning($"{THIS_SCRIPT_NAME_WITH_EXTENSION} was not found in the project. Tags will not be generated or updated.");
            return;
        }
        else EditorApplication.delayCall += OnEditorUpdate;

        if(Application.isPlaying || EditorApplication.isUpdating || !IntervalPassed()) return;
        __lastUpdateTime = EditorApplication.timeSinceStartup;

        // Update all known tags
        __previousTags = __tags;
        __tags = GetTags();

        // If the tags have not changed, we do not need further processing
        if(__tags == __previousTags) return;

        // Get the path of the generated script file
        __generatedScriptFilePath = GetGeneratedScriptFilePath();
        if(string.IsNullOrEmpty(__generatedScriptFilePath)) return;

        // Determine full path of the generated script file
        __generatedScriptFileFullPath = Path.GetFullPath(__generatedScriptFilePath);

        // Generate the full script content
        __newFullScriptContent = GetFullScriptContent(__tags);

        // Do not overwrite the file if the content is the same
        if(__oldFullScriptContent == __newFullScriptContent) return;

        try
        {
            // Try to write the new content to the file
            File.WriteAllText(__generatedScriptFileFullPath, __newFullScriptContent);
            __oldFullScriptContent = __newFullScriptContent;
        }
        catch(Exception e)
        {
            Debug.LogWarning($"Could not write to {__generatedScriptFileFullPath}. Tags will not be generated or updated. Error: {e.Message}");
        }
    }

    private static bool IntervalPassed()
    {
        return (EditorApplication.timeSinceStartup - __lastUpdateTime) > UPDATE_INTERVAL_MIN_SECONDS;
    }

    private static string[] GetTags()
    {
        return UnityEditorInternal.InternalEditorUtility.tags;
    }

    private static string GetGeneratedScriptFilePath()
    {
        if(AreWeInPackages())
        {
            _possiblePaths = Directory.GetFiles(PackagesFolderPath, GENERATED_FILE_NAME_WITH_EXTENSION, SearchOption.AllDirectories);
        }
        else if(AreWeInAssets())
        {
            _possiblePaths = Directory.GetFiles(AssetsFolderPath, GENERATED_FILE_NAME_WITH_EXTENSION, SearchOption.AllDirectories);
        }
        else
        {
            Debug.LogError($"Could not determine if we are in the assets or in the packages folder as {THIS_SCRIPT_NAME_WITH_EXTENSION} was not found. Tags will not be generated or updated.");
            return null;
        }

        if(_possiblePaths.Length == 0)
        {
            Debug.LogWarning($"Could not find {GENERATED_FILE_NAME_WITH_EXTENSION} in the project. Tags will not be generated or updated. Please create a script with this name in the project.");
            return null;
        }
        else if(_possiblePaths.Length > 1)
        {
            Debug.LogWarning($"Found multiple {GENERATED_FILE_NAME_WITH_EXTENSION} in the project. Tags will not be generated or updated. Please remove duplicates.");
            return null;
        }
        else
        {
            return _possiblePaths[0];
        }
    }

    private static bool AreWeInProject()
    {
        return AreWeInPackages() || AreWeInAssets();
    }

    private static bool AreWeInPackages()
    {
        return Directory.GetFiles(PackagesFolderPath, THIS_SCRIPT_NAME_WITH_EXTENSION, SearchOption.AllDirectories).Length > 0;
    }
    private static bool AreWeInAssets()
    {
        return Directory.GetFiles(AssetsFolderPath, THIS_SCRIPT_NAME_WITH_EXTENSION, SearchOption.AllDirectories).Length > 0;
    }

    private static string GetFullScriptContent(string[] tags)
    {
        __formattedTagsString = string.Empty;
        for(int i = 0; i < tags.Length; i++)
        {
            __formattedTagsString += $"    public const string {TAG_CONSTANTS_PREFIX}{GetSanitizedTagName(tags[i])} = \"{tags[i]}\";\n";
        }

        return PARTIAL_SCRIPT_TEMPLATE.Replace("[TAGS]", __formattedTagsString).Replace("[TIMESTAMP]", DateTime.Now.ToString());
    }

    private static string GetSanitizedTagName(string tag)
    {
        return Regex.Replace(tag, TAG_SANITIZATION_REGEX, TAG_SANITIZATION_REGEX_REPLACEMENT);
    }

    private const string PARTIAL_SCRIPT_TEMPLATE =
@"
// This file is auto-generated. Do not modify it manually.
// Whenever a tag is added or removed, this file will be regenerated.
public partial class Tagged
{
[TAGS]}";
}


#endif
