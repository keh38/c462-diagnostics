using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(StyleControl))]
public class StyleControlEditor : Editor
{
    private List<string> _savedStyleNames;
    private int _selectedStyleIndex;
    private bool _locked = false;

    private void OnEnable()
    {
        _savedStyleNames = new List<string>();
        var files = Directory.EnumerateFiles(Path.Combine(Application.dataPath, "Styles"), "*.json");
        foreach (var f in files)
        {
            _savedStyleNames.Add(Path.GetFileNameWithoutExtension(f));
        }

        StyleControl myTarget = (StyleControl)target;
        _selectedStyleIndex = _savedStyleNames.IndexOf(myTarget.style.name);

        Undo.undoRedoPerformed += UndoRedoCallback;
    }

    private void OnDisable()
    {
        Undo.undoRedoPerformed -= UndoRedoCallback;
    }

    public override void OnInspectorGUI()
    {
        StyleControl myTarget = (StyleControl)target;
        Undo.RecordObject(myTarget, "Style change");

        EditorGUILayout.BeginHorizontal();
        _selectedStyleIndex = EditorGUILayout.Popup(_selectedStyleIndex, _savedStyleNames.ToArray());
        if (GUILayout.Button("Load", GUILayout.Width(80)))
        {
            var fn = Path.Combine(Application.dataPath, "Styles", _savedStyleNames[_selectedStyleIndex] + ".json");
            var data = File.ReadAllText(fn);
            myTarget.style = JsonUtility.FromJson<StyleDefinition>(data);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Separator();
        EditorGUILayout.Space();

        myTarget.style.name = EditorGUILayout.TextField("Name", myTarget.style.name);

        if (GUILayout.Button("Save", GUILayout.Width(80)))
        {
            var data = JsonUtility.ToJson(myTarget.style, true);
            File.WriteAllText(Path.Combine(Application.dataPath, "Styles", myTarget.style.name + ".json"), data);
        }

        EditorGUILayout.Separator();
        EditorGUILayout.Space();

        myTarget.style.mainColor = EditorGUILayout.ColorField("Main color", myTarget.style.mainColor);

        EditorGUILayout.Separator();
        EditorGUILayout.Space();

        GUILayout.Label("TITLE", EditorStyles.boldLabel);
        myTarget.style.title.color = EditorGUILayout.ColorField("Color", myTarget.style.title.color);
        myTarget.style.title.fontColor = EditorGUILayout.ColorField("Font color", myTarget.style.title.fontColor);
        myTarget.style.title.fontSize = EditorGUILayout.IntField("Font size", myTarget.style.title.fontSize);
        EditorGUILayout.Separator();
        EditorGUILayout.Space();

        EditorGUILayout.Space(15);

        GUILayout.Label("MENU", EditorStyles.boldLabel);
        myTarget.style.menu.color = EditorGUILayout.ColorField("Color", myTarget.style.menu.color);
        myTarget.style.menu.fontColor = EditorGUILayout.ColorField("Font color", myTarget.style.menu.fontColor);
        myTarget.style.menu.fontSize = EditorGUILayout.IntField("Font size", myTarget.style.menu.fontSize);
        EditorGUILayout.Separator();
        EditorGUILayout.Space();

        GUILayout.Label("BUTTON", EditorStyles.boldLabel);
        myTarget.style.button.color = EditorGUILayout.ColorField("Color", myTarget.style.button.color);
        myTarget.style.button.foreColor = EditorGUILayout.ColorField("Font color", myTarget.style.button.foreColor);
        myTarget.style.button.fontSize = EditorGUILayout.IntField("Font size", myTarget.style.button.fontSize);
        EditorGUILayout.Separator();
        EditorGUILayout.Space();

        _locked = EditorGUILayout.ToggleLeft("Lock", _locked);

        if (GUI.changed)
        {
            if (!_locked)
            {
                myTarget.ApplyStyle();
            }

            EditorUtility.SetDirty(myTarget);
            if (!Application.isPlaying)
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }

    private void UndoRedoCallback()
    {
        StyleControl myTarget = (StyleControl)target;
        myTarget.ApplyStyle();
    }
}
