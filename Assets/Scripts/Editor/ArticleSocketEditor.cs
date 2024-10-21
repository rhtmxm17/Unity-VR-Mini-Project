using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ArticleSocket), true)]
[CanEditMultipleObjects]
public class ArticleSocketEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.LabelField("CustomEditor Test");
    }
}
