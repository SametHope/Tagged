#if UNITY_EDITOR

using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TaggedAttribute))]
public class TagSelectorPropertyDrawer : PropertyDrawer
{
    private string[] _tags;
    private string _stringValue;
    private int _selectedIndex;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if(property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.LabelField(position, $"{nameof(TaggedAttribute)} can only be used on strings and lists/arrays of strings.", EditorStyles.helpBox);
            return;
        }

        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.BeginChangeCheck();

        _tags = UnityEditorInternal.InternalEditorUtility.tags;
        // add "Add Tag..." option to array
        _tags = _tags.Concat(new[] { "Add Tag..." }).ToArray();

        _stringValue = property.stringValue;
        _selectedIndex = -1;

        if(string.IsNullOrEmpty(_stringValue) || _stringValue == "Untagged")
        {
            _selectedIndex = 0; // first index is the special "Untagged" entry
        }
        else
        {
            // Check if there is an entry that matches the entry and get the index
            // we skip index 0 and start from 1 as that is a special case
            for(int i = 1; i < _tags.Length; i++)
            {
                if(_tags[i] == _stringValue)
                {
                    _selectedIndex = i;
                    break;
                }
            }
        }

        if(_selectedIndex != -1)
        {
            // Draw the popup box
            int newIndex = EditorGUI.Popup(position, label.text, _selectedIndex, _tags);

            // If "Add Tag..." is selected, open the tag manager
            if(newIndex == _tags.Length - 1)
            {
                SettingsService.OpenProjectSettings("Project/Tags and Layers");
                return;
            }

            _selectedIndex = newIndex;

            // Adjust the actual string value of the property based on the selection
            if(_selectedIndex >= 1) property.stringValue = _tags[_selectedIndex];
            else property.stringValue = "Untagged";
        }
        else
        {
            // Non tag-manager tag, just display this field without the popup
            property.stringValue = EditorGUI.TextField(position, label.text, $"{property.stringValue}", EditorStyles.miniTextField);
        }

        if(EditorGUI.EndChangeCheck())
        {
            // Save any change made
            property.serializedObject.ApplyModifiedProperties();
        }
        EditorGUI.EndProperty();

    }
}
#endif