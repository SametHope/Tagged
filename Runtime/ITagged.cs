
/// <summary>
/// Interface for objects that support tagging with string tags.
/// </summary>
public interface ITagged
{
    /// <summary>
    /// Checks if the object has all the specified tags.
    /// </summary>
    /// <param name="tags">Array of tags to check.</param>
    /// <returns>True if all tags are present, false otherwise.</returns>
    bool IsTagged(params string[] tags);

    /// <summary>
    /// Adds the specified tags to the object.
    /// </summary>
    /// <param name="tags">Array of tags to add.</param>
    /// <returns>True if at least one tag was added, false otherwise.</returns>
    bool AddTags(params string[] tags);

    /// <summary>
    /// Removes the specified tags from the object.
    /// </summary>
    /// <param name="tags">Array of tags to remove.</param>
    /// <returns>True if at least one tag was removed, false otherwise.</returns>
    bool RemoveTags(params string[] tags);
}
