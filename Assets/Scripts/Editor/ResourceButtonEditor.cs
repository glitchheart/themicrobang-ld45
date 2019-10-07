using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ResourceButton))]
public class ResourceButtonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // var button = (MainResourceButton)target;
        // button.Menu = EditorGUILayout.ObjectField(button.Menu, typeof(GameObject), true) as GameObject;
        base.DrawDefaultInspector();
    }
}
