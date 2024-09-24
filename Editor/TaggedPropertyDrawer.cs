#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TaggedAttribute))]
public class TagSelectorPropertyDrawer : PropertyDrawer
{
    private string[] _tags;
    private string _stringValue;
    private int _index;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if(property.propertyType == SerializedPropertyType.String)
        {
            EditorGUI.BeginProperty(position, label, property);

            _tags = UnityEditorInternal.InternalEditorUtility.tags;

            _stringValue = property.stringValue;
            _index = -1;

            if(string.IsNullOrEmpty(_stringValue) || _stringValue == "Untagged")
            {
                _index = 0; // first index is the special "Untagged" entry
            }
            else
            {
                // Check if there is an entry that matches the entry and get the index
                // we skip index 0 and start from 1 as that is a special case
                for(int i = 1; i < _tags.Length; i++)
                {
                    if(_tags[i] == _stringValue)
                    {
                        _index = i;
                        break;
                    }
                }
            }

            // Draw the popup box with the current selected index
            _index = EditorGUI.Popup(position, label.text, _index, _tags);

            // Adjust the actual string value of the property based on the selection
            if(_index >= 1) property.stringValue = _tags[_index];
            else property.stringValue = "Untagged";
        }

        EditorGUI.EndProperty();
    }
}
#endif