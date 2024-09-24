using System;
using UnityEngine;

/// <summary>
/// Allows easy selection of currently defined tags through inspector for serialized properties and fields. Works with strings and arrays/lists of strings.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class TaggedAttribute : PropertyAttribute { }
