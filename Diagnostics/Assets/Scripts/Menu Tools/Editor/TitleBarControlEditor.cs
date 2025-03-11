using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(TitleBarControl))]
public class TitleBarControlEditor : Editor
{
    private bool _showObjects = false;

    private void OnEnable()
    {
        TitleBarControl myTarget = (TitleBarControl)target;
        if (myTarget.style == null)
        {
            myTarget.style = new TitleBarStyle();
        }

        Undo.undoRedoPerformed += UndoRedoCallback;
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= UndoRedoCallback;
    }

    public override void OnInspectorGUI()
    {
        TitleBarControl myTarget = (TitleBarControl)target;
        Undo.RecordObject(myTarget, "Title style change");

        EditorStyles.label.fontStyle = FontStyle.Normal;
        myTarget.useDefault = EditorGUILayout.ToggleLeft("Use default style", myTarget.useDefault);
        if (!myTarget.useDefault)
        {
            myTarget.style.color = EditorGUILayout.ColorField("Background", myTarget.style.color);
            myTarget.style.fontColor = EditorGUILayout.ColorField("Font color", myTarget.style.fontColor);
            myTarget.style.fontSize = EditorGUILayout.IntField("Font size", myTarget.style.fontSize);
            EditorGUILayout.Separator();
            EditorGUILayout.Space();
        }

        EditorStyles.foldout.fontStyle = FontStyle.Bold;
        _showObjects = EditorGUILayout.Foldout(_showObjects, "TITLEBAR COMPONENTS");
        if (_showObjects)
        {
            myTarget.image = (Image)EditorGUILayout.ObjectField("Image", myTarget.image, typeof(Image), true);
            myTarget.label = (TMPro.TMP_Text)EditorGUILayout.ObjectField("Label", myTarget.label, typeof(TMPro.TMP_Text), true);
            EditorGUILayout.Separator();
            EditorGUILayout.Space();
        }

        if (GUI.changed)
        {
            if (myTarget.useDefault)
            {
                myTarget.style = new TitleBarStyle();
            }

            myTarget.ApplyStyle();

            EditorUtility.SetDirty(myTarget);
            if (!Application.isPlaying)
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }

    private void UndoRedoCallback()
    {
        TitleBarControl myTarget = (TitleBarControl)target;
        myTarget.ApplyStyle();
    }
}
