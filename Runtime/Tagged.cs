using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A multipurpose class that serves three primary functions:
/// <para/>
/// 1. Provides methods to interact with the <see cref="ITagged"/> interface, supporting the addition, removal, and querying of tags on <see cref="ITagged"/>, <see cref="GameObject"/>, <see cref="Component"/>, and <see langword="object"/> types.
/// <para/>
/// 2. Acts as a MonoBehaviour that implements the <see cref="ITagged"/> interface, exposing a special dropdown list of tags in the inspector with <see cref="TaggedAttribute"/> for easy configuration.
/// <para/>
/// 3. Offers automatically generated and synced constants for all tags defined within the project at all times.
/// </summary>
[DefaultExecutionOrder(-10)] // In case any other scripts need to use the tagging system in Awake
[DisallowMultipleComponent]
public partial class Tagged : MonoBehaviour, ITagged
{
#if UNITY_EDITOR
    [ContextMenuItem("Clear Tags", nameof(ClearTags), order = 100001)]
    [ContextMenuItem("Manage Tags", nameof(GoToUnityTagManager), order = 100002)]
#endif
    [Tooltip("Tags assigned to this GameObject.\n A hashset correlating to this list is initialized and used internally for fast lookup on Awake.")]
    [SerializeField, Tagged] private List<string> _tags; // Exposed in inspector for editing
    [SerializeField] private HashSet<string> _tagSet; // For fast lookup, initialized in Awake

    private void OnValidate()
    {
        if(_tags == null) _tags = new List<string>();
    }
    private void Awake()
    {
        _tagSet = new HashSet<string>(_tags);
        CheckAndRemoveDuplicates();
    }
    private void CheckAndRemoveDuplicates()
    {
        if(_tags.Count > 1)
        {
            _tags.Sort();
            for(int i = _tags.Count - 1; i > 0; i--)
            {
                if(_tags[i] == _tags[i - 1])
                {
                    Debug.LogWarning($"[Tagged] Duplicate tag \"{_tags[i]}\" found on GameObject \"{gameObject.name}\". Removing duplicate.", this);
                    _tags.RemoveAt(i);
                }
            }
        }
    }

    // -------------------- Instance Methods --------------------

    /// <inheritdoc/>
    public bool IsTagged(params string[] tags)
    {
        foreach(var tag in tags)
        {
            if(!_tagSet.Contains(tag)) return false;
        }
        return true;
    }

    /// <inheritdoc/>
    public bool AddTags(params string[] tags)
    {
        bool added = false;
        foreach(var tag in tags)
        {
            if(_tagSet.Add(tag))
            {
                _tags.Add(tag);
                added = true;
            }
        }
        return added;
    }

    /// <inheritdoc/>
    public bool RemoveTags(params string[] tags)
    {
        bool removed = false;
        foreach(var tag in tags)
        {
            if(_tagSet.Remove(tag))
            {
                _tags.Remove(tag);
                removed = true;
            }
        }
        return removed;
    }

    /// <summary>
    /// Adds a single tag. This is primarily for Unity events, differing from <see cref="AddTags(string[])"/>.
    /// </summary>
    /// <param name="tag">The tag to add.</param>
    public void AddTag(string tag)
    {
        AddTags(tag);
    }

    /// <summary>
    /// Removes a single tag. This is primarily for Unity events, differing from <see cref="RemoveTags(string[])"/>.
    /// </summary>
    /// <param name="tag">The tag to remove.</param>
    public void RemoveTag(string tag)
    {
        RemoveTags(tag);
    }

    // -------------------- Static Methods --------------------

    /// <summary>
    /// Checks if the target <see cref="ITagged"/> object has all the specified tags.
    /// </summary>
    /// <param name="target">The target object to check.</param>
    /// <param name="tags">The tags to check for.</param>
    /// <returns>True if the target has all specified tags, false otherwise.</returns>
    public static bool IsTagged(ITagged target, params string[] tags)
    {
        if(target == null) return false;
        return target.IsTagged(tags);
    }

    /// <summary>
    /// Checks if the specified GameObject has all the specified tags.
    /// </summary>
    /// <param name="target">The GameObject to check.</param>
    /// <param name="tags">The tags to check for.</param>
    /// <returns>True if the GameObject has all specified tags, false otherwise.</returns>
    public static bool IsTagged(GameObject target, params string[] tags)
    {
        if(target == null) return false;
        if(target.TryGetComponent(out ITagged tagged))
        {
            return tagged.IsTagged(tags);
        }
        return false;
    }

    /// <summary>
    /// Checks if the specified Component's GameObject has all the specified tags.
    /// </summary>
    /// <param name="target">The Component to check.</param>
    /// <param name="tags">The tags to check for.</param>
    /// <returns>True if the Component's GameObject has all specified tags, false otherwise.</returns>
    public static bool IsTagged(Component target, params string[] tags)
    {
        if(target == null) return false;
        return IsTagged(target.gameObject, tags);
    }

    /// <summary>
    /// Checks if the specified object has all the specified tags.
    /// </summary>
    /// <param name="target">The object to check.</param>
    /// <param name="tags">The tags to check for.</param>
    /// <returns>True if the object has all specified tags, false otherwise.</returns>
    public static bool IsTagged(object target, params string[] tags)
    {
        if(target is ITagged tagged) return tagged.IsTagged(tags);
        if(target is GameObject go) return IsTagged(go, tags);
        if(target is Component comp) return IsTagged(comp, tags);
        return false;
    }

    /// <summary>
    /// Adds the specified tags to the target <see cref="ITagged"/> object.
    /// </summary>
    /// <param name="target">The target object to add tags to.</param>
    /// <param name="tags">The tags to add.</param>
    /// <returns>True if at least one tag was added, false otherwise.</returns>
    public static bool AddTags(ITagged target, params string[] tags)
    {
        if(target == null) return false;
        return target.AddTags(tags);
    }

    /// <summary>
    /// Adds the specified tags to the GameObject. If no <see cref="Tagged"/> component exists, one is added.
    /// </summary>
    /// <param name="target">The GameObject to add tags to.</param>
    /// <param name="tags">The tags to add.</param>
    /// <returns>True if at least one tag was added, false otherwise.</returns>
    public static bool AddTags(GameObject target, params string[] tags)
    {
        if(target == null) return false;
        if(target.TryGetComponent(out ITagged tagged))
        {
            return tagged.AddTags(tags);
        }
        tagged = target.AddComponent<Tagged>();
        return tagged.AddTags(tags);
    }

    /// <summary>
    /// Adds the specified tags to the Component's GameObject.
    /// </summary>
    /// <param name="target">The Component whose GameObject to add tags to.</param>
    /// <param name="tags">The tags to add.</param>
    /// <returns>True if at least one tag was added, false otherwise.</returns>
    public static bool AddTags(Component target, params string[] tags)
    {
        if(target == null) return false;
        return AddTags(target.gameObject, tags);
    }

    /// <summary>
    /// Adds the specified tags to the object.
    /// </summary>
    /// <param name="target">The object to add tags to.</param>
    /// <param name="tags">The tags to add.</param>
    /// <returns>True if at least one tag was added, false otherwise.</returns>
    public static bool AddTags(object target, params string[] tags)
    {
        if(target is ITagged tagged) return tagged.AddTags(tags);
        if(target is GameObject go) return AddTags(go, tags);
        if(target is Component comp) return AddTags(comp, tags);
        return false;
    }

    /// <summary>
    /// Removes the specified tags from the target <see cref="ITagged"/> object.
    /// </summary>
    /// <param name="target">The target object to remove tags from.</param>
    /// <param name="tags">The tags to remove.</param>
    /// <returns>True if at least one tag was removed, false otherwise.</returns>
    public static bool RemoveTags(ITagged target, params string[] tags)
    {
        if(target == null) return false;
        return target.RemoveTags(tags);
    }

    /// <summary>
    /// Removes the specified tags from the GameObject.
    /// </summary>
    /// <param name="target">The GameObject to remove tags from.</param>
    /// <param name="tags">The tags to remove.</param>
    /// <returns>True if at least one tag was removed, false otherwise.</returns>
    public static bool RemoveTags(GameObject target, params string[] tags)
    {
        if(target == null) return false;
        if(target.TryGetComponent(out ITagged tagged))
        {
            return tagged.RemoveTags(tags);
        }
        return false;
    }

    /// <summary>
    /// Removes the specified tags from the Component's GameObject.
    /// </summary>
    /// <param name="target">The Component whose GameObject to remove tags from.</param>
    /// <param name="tags">The tags to remove.</param>
    /// <returns>True if at least one tag was removed, false otherwise.</returns>
    public static bool RemoveTags(Component target, params string[] tags)
    {
        if(target == null) return false;
        return RemoveTags(target.gameObject, tags);
    }

    /// <summary>
    /// Removes the specified tags from the object.
    /// </summary>
    /// <param name="target">The object to remove tags from.</param>
    /// <param name="tags">The tags to remove.</param>
    /// <returns>True if at least one tag was removed, false otherwise.</returns>
    public static bool RemoveTags(object target, params string[] tags)
    {
        if(target is ITagged tagged) return tagged.RemoveTags(tags);
        if(target is GameObject go) return RemoveTags(go, tags);
        if(target is Component comp) return RemoveTags(comp, tags);
        return false;
    }

#if UNITY_EDITOR
    [ContextMenu("Clear Tags")]
    private void ClearTags()
    {
        _tags = new List<string>();
        _tagSet = new HashSet<string>();
    }
    [ContextMenu("Manage Tags")]
    private void GoToUnityTagManager()
    {
        UnityEditor.SettingsService.OpenProjectSettings("Project/Tags and Layers");
    }
#endif
}
