using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

[CustomEditor(typeof(MenuButtonControl))]
public class MenuButtonControlEditor : Editor
{
    private bool _showObjects = false;

    private void OnEnable()
    {
        MenuButtonControl myTarget = (MenuButtonControl)target;
        if (myTarget.style == null)
        {
            myTarget.style = new MenuButtonStyle();
        }

        Undo.undoRedoPerformed += UndoRedoCallback;
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= UndoRedoCallback;
    }

    public override void OnInspectorGUI()
    {
        MenuButtonControl myTarget = (MenuButtonControl)target;
        Undo.RecordObject(myTarget, "Menu button style change");

        EditorStyles.label.fontStyle = FontStyle.Normal;
        myTarget.useDefault = EditorGUILayout.ToggleLeft("Use default style", myTarget.useDefault);
        if (!myTarget.useDefault)
        {
            myTarget.style.color = EditorGUILayout.ColorField("Background", myTarget.style.color);
            myTarget.style.fontSize = EditorGUILayout.IntField("Font size", myTarget.style.fontSize);
            EditorGUILayout.Separator();
            EditorGUILayout.Space();
        }

        EditorStyles.foldout.fontStyle = FontStyle.Bold;
        _showObjects = EditorGUILayout.Foldout(_showObjects, "MENU BUTTON COMPONENTS");
        if (_showObjects)
        {
            myTarget.button = (Button)EditorGUILayout.ObjectField("Button", myTarget.button, typeof(Button), true);
            myTarget.label = (TMPro.TMP_Text)EditorGUILayout.ObjectField("Label", myTarget.label, typeof(TMPro.TMP_Text), true);
            myTarget.icon = (Image)EditorGUILayout.ObjectField("Icon", myTarget.icon, typeof(Image), true);
            myTarget.checkmark = (Image)EditorGUILayout.ObjectField("Checkmark", myTarget.checkmark, typeof(Image), true);
            EditorGUILayout.Separator();
            EditorGUILayout.Space();
        }

        if (GUI.changed)
        {
            if (myTarget.useDefault)
            {
                //myTarget.style = new TitleBarStyle();
            }

            myTarget.ApplyStyle();

            EditorUtility.SetDirty(myTarget);
            if (!Application.isPlaying)
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }

    private void UndoRedoCallback()
    {
        MenuButtonControl myTarget = (MenuButtonControl)target;
        myTarget.ApplyStyle();
    }
}
