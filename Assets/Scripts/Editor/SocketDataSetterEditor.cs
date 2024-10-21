using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SocketDataSetter))]
[CanEditMultipleObjects]
public class SocketDataSetterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.LabelField("Solution Articles가 비어있다면 검사하지 않음");
    }
}
