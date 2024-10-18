using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomPropertyDrawer(typeof(ShowIfAttribute))]
public class ShowIfDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ShowIfAttribute showIf = (ShowIfAttribute)attribute;

        string conditionPropertyPath;
        if (property.propertyPath.Contains('.'))
            conditionPropertyPath = Path.ChangeExtension(property.propertyPath, showIf.ConditionField);
        else
            conditionPropertyPath = showIf.ConditionField;
        SerializedProperty conditionProperty = property.serializedObject.FindProperty(conditionPropertyPath);

        if (conditionProperty != null && conditionProperty.boolValue)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ShowIfAttribute showIf = (ShowIfAttribute)attribute;

        string conditionPropertyPath;
        if (property.propertyPath.Contains('.'))
            conditionPropertyPath = Path.ChangeExtension(property.propertyPath, showIf.ConditionField);
        else
            conditionPropertyPath = showIf.ConditionField;
        SerializedProperty conditionProperty = property.serializedObject.FindProperty(conditionPropertyPath);

        if (conditionProperty != null && conditionProperty.boolValue)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }

        return 0f;
    }
}
#endif

public class ShowIfAttribute : PropertyAttribute
{
    public string ConditionField;

    public ShowIfAttribute(string conditionField)
    {
        ConditionField = conditionField;
    }
}
