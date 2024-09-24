#if DISABLE_TAGGED_EXTENSIONS
using UnityEngine;

/// <summary>
/// Provides extension methods for interacting with the <see cref="Tagged"/> system on various types, such as <see cref="ITagged"/>, 
/// <see cref="GameObject"/>, <see cref="Component"/>, and <see cref="object"/>. 
/// <para/>
/// This functionality is currently disabled. You can re-enable it by removing DISABLE_TAGGED_EXTENSIONS definition in the project settings.
/// </summary>
public static class TaggedExtensions { }
#else
using UnityEngine;

/// <summary>
/// Provides extension methods for interacting with the <see cref="Tagged"/> system on various types, such as <see cref="ITagged"/>, 
/// <see cref="GameObject"/>, <see cref="Component"/>, and <see cref="object"/>. 
/// <para/>
/// You can disable this functionality by defining DISABLE_TAGGED_EXTENSIONS in the project settings.
/// </summary>
public static class TaggedExtensions
{
    /// <summary>
    /// Checks if the <see cref="ITagged"/> object contains all the specified tags. 
    /// <para/>
    /// You may disable extension methods by defining DISABLE_TAGGED_EXTENSIONS in the project settings.
    /// </summary>
    /// <param name="target">The target <see cref="ITagged"/> object.</param>
    /// <param name="tags">The tags to check for.</param>
    /// <returns>True if the target contains all the specified tags, false otherwise.</returns>
    public static bool IsTagged(this ITagged target, params string[] tags) => Tagged.IsTagged(target, tags);

    /// <summary>
    /// Checks if the <see cref="GameObject"/> contains all the specified tags.
    /// <para/>
    /// You may disable extension methods by defining DISABLE_TAGGED_EXTENSIONS in the project settings.
    /// </summary>
    /// <param name="target">The target <see cref="GameObject"/>.</param>
    /// <param name="tags">The tags to check for.</param>
    /// <returns>True if the GameObject contains all the specified tags, false otherwise.</returns>
    public static bool IsTagged(this GameObject target, params string[] tags) => Tagged.IsTagged(target, tags);

    /// <summary>
    /// Checks if the <see cref="Component"/>'s GameObject contains all the specified tags.
    /// <para/>
    /// You may disable extension methods by defining DISABLE_TAGGED_EXTENSIONS in the project settings.
    /// </summary>
    /// <param name="target">The target <see cref="Component"/>.</param>
    /// <param name="tags">The tags to check for.</param>
    /// <returns>True if the Component's GameObject contains all the specified tags, false otherwise.</returns>
    public static bool IsTagged(this Component target, params string[] tags) => Tagged.IsTagged(target, tags);

    /// <summary>
    /// Checks if the object contains all the specified tags.
    /// <para/>
    /// You may disable extension methods by defining DISABLE_TAGGED_EXTENSIONS in the project settings.
    /// </summary>
    /// <param name="target">The target object.</param>
    /// <param name="tags">The tags to check for.</param>
    /// <returns>True if the object contains all the specified tags, false otherwise.</returns>
    public static bool IsTagged(this object target, params string[] tags) => Tagged.IsTagged(target, tags);

    /// <summary>
    /// Adds the specified tags to the <see cref="ITagged"/> object.
    /// <para/>
    /// You may disable extension methods by defining DISABLE_TAGGED_EXTENSIONS in the project settings.
    /// </summary>
    /// <param name="target">The target <see cref="ITagged"/> object.</param>
    /// <param name="tags">The tags to add.</param>
    /// <returns>True if at least one tag was added, false otherwise.</returns>
    public static bool AddTags(this ITagged target, params string[] tags) => target != null && target.AddTags(tags);

    /// <summary>
    /// Adds the specified tags to the <see cref="GameObject"/>.
    /// <para/>
    /// You may disable extension methods by defining DISABLE_TAGGED_EXTENSIONS in the project settings.
    /// </summary>
    /// <param name="target">The target <see cref="GameObject"/>.</param>
    /// <param name="tags">The tags to add.</param>
    /// <returns>True if at least one tag was added, false otherwise.</returns>
    public static bool AddTags(this GameObject target, params string[] tags) => Tagged.AddTags(target, tags);

    /// <summary>
    /// Adds the specified tags to the <see cref="Component"/>'s GameObject.
    /// <para/>
    /// You may disable extension methods by defining DISABLE_TAGGED_EXTENSIONS in the project settings.
    /// </summary>
    /// <param name="target">The target <see cref="Component"/>.</param>
    /// <param name="tags">The tags to add.</param>
    /// <returns>True if at least one tag was added, false otherwise.</returns>
    public static bool AddTags(this Component target, params string[] tags) => Tagged.AddTags(target, tags);

    /// <summary>
    /// Adds the specified tags to the object.
    /// <para/>
    /// You may disable extension methods by defining DISABLE_TAGGED_EXTENSIONS in the project settings.
    /// </summary>
    /// <param name="target">The target object.</param>
    /// <param name="tags">The tags to add.</param>
    /// <returns>True if at least one tag was added, false otherwise.</returns>
    public static bool AddTags(this object target, params string[] tags) => Tagged.AddTags(target, tags);

    /// <summary>
    /// Removes the specified tags from the <see cref="ITagged"/> object.
    /// <para/>
    /// You may disable extension methods by defining DISABLE_TAGGED_EXTENSIONS in the project settings.
    /// </summary>
    /// <param name="target">The target <see cref="ITagged"/> object.</param>
    /// <param name="tags">The tags to remove.</param>
    /// <returns>True if at least one tag was removed, false otherwise.</returns>
    public static bool RemoveTags(this ITagged target, params string[] tags) => target != null && target.RemoveTags(tags);

    /// <summary>
    /// Removes the specified tags from the <see cref="GameObject"/>.
    /// <para/>
    /// You may disable extension methods by defining DISABLE_TAGGED_EXTENSIONS in the project settings.
    /// </summary>
    /// <param name="target">The target <see cref="GameObject"/>.</param>
    /// <param name="tags">The tags to remove.</param>
    /// <returns>True if at least one tag was removed, false otherwise.</returns>
    public static bool RemoveTags(this GameObject target, params string[] tags) => Tagged.RemoveTags(target, tags);

    /// <summary>
    /// Removes the specified tags from the <see cref="Component"/>'s GameObject.
    /// <para/>
    /// You may disable extension methods by defining DISABLE_TAGGED_EXTENSIONS in the project settings.
    /// </summary>
    /// <param name="target">The target <see cref="Component"/>.</param>
    /// <param name="tags">The tags to remove.</param>
    /// <returns>True if at least one tag was removed, false otherwise.</returns>
    public static bool RemoveTags(this Component target, params string[] tags) => Tagged.RemoveTags(target, tags);

    /// <summary>
    /// Removes the specified tags from the object.
    /// <para/>
    /// You may disable extension methods by defining DISABLE_TAGGED_EXTENSIONS in the project settings.
    /// </summary>
    /// <param name="target">The target object.</param>
    /// <param name="tags">The tags to remove.</param>
    /// <returns>True if at least one tag was removed, false otherwise.</returns>
    public static bool RemoveTags(this object target, params string[] tags) => Tagged.RemoveTags(target, tags);
}
#endif
