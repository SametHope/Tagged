#if UNITY_EDITOR

using System.Linq;
using System.Reflection;
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
            // Create a tooltip-enabled label if a TooltipAttribute is present
            var tooltipAttribute = fieldInfo.GetCustomAttribute<TooltipAttribute>();
            var tippedLabel = new GUIContent(label.text, tooltipAttribute?.tooltip);

            // Convert tags to GUIContent array for tooltip support
            var contents = _tags.Select(tag => new GUIContent(tag)).ToArray();

            // Draw the popup box with the tooltip-enabled label
            int newIndex = EditorGUI.Popup(position, tippedLabel, _selectedIndex, contents);
            
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
            property.stringValue = EditorGUI.TextField(position, label.text, $"{property.stringValue}", EditorStyles.textField);
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
