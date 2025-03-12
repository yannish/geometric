using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RadioButtonExample))]
public class RadioButtonExampleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Reference to the enum property
        SerializedProperty selectedOption = serializedObject.FindProperty("selectedOption");

        EditorGUILayout.LabelField("Select an Option:", EditorStyles.boldLabel);

        // Draw radio buttons
        for (int i = 0; i < System.Enum.GetValues(typeof(RadioButtonExample.SelectionMode)).Length; i++)
        {
            RadioButtonExample.SelectionMode option = (RadioButtonExample.SelectionMode)i;
            bool isSelected = (selectedOption.enumValueIndex == i);

            if (EditorGUILayout.ToggleLeft(option.ToString(), isSelected))
            {
                selectedOption.enumValueIndex = i;
            }
        }

        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            GUILayout.Button("CORNER");
            GUILayout.Button("EDGE");
            GUILayout.Button("FACE");
        }

        serializedObject.ApplyModifiedProperties();
    }
}