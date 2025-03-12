using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(QuickMono))]
public class QuickMonoInspector : Editor
{
    
    private SerializedProperty radioButtonIntProp;
    private SerializedProperty quickDataProp;
    private SerializedProperty quickDataArrayProp;
    
    private SerializedProperty nestedFloatProp;
    
    QuickMono quickMono;
    
    private void OnEnable()
    {
        quickMono = (QuickMono)target;
        radioButtonIntProp = serializedObject.FindProperty("radioButtonInt");
        quickDataProp = serializedObject.FindProperty("quickMonoData");
        quickDataArrayProp = serializedObject.FindProperty("quickMonoDataArray");
        nestedFloatProp = quickDataProp.FindPropertyRelative("quickFloat");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawQuickDataProperty(quickDataProp);
        
        GUIStyle style = new GUIStyle()
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,

            normal = new GUIStyleState()
            {
                
                background = Texture2D.whiteTexture
            },
            hover = new GUIStyleState()
            {
                background = Texture2D.grayTexture
            },
            active = new GUIStyleState()
            {
                background = Texture2D.blackTexture
            }
        };

        GUILayout.Button("Hello!", style);
        
        EditorGUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        GUIStyle firstOffStyle = EditorStyles.miniButtonLeft;
        GUIStyle firstOnStyle = EditorStyles.miniButtonLeft;
        firstOnStyle.normal = firstOnStyle.active; 
        
        GUIStyle secondOffStyle = EditorStyles.miniButtonMid;
        GUIStyle secondOnStyle = EditorStyles.miniButtonMid;
        secondOnStyle.normal = firstOnStyle.active; 
        
        GUIStyle thirdOffStyle = EditorStyles.miniButtonRight;
        GUIStyle thirdOnStyle = EditorStyles.miniButtonRight;
        thirdOnStyle.normal = firstOnStyle.active; 
        
        if (GUILayout.Button("Select", quickMono.radioButtonInt == 0 ? firstOnStyle : firstOffStyle)) 
        {
            quickMono.radioButtonInt = 0;
        }
        if (GUILayout.Button ("Revert", quickMono.radioButtonInt == 1 ? secondOnStyle : secondOffStyle)) 
        {
            quickMono.radioButtonInt = 1;
        }
        if (GUILayout.Button ("Apply", quickMono.radioButtonInt == 2 ? thirdOnStyle : thirdOffStyle))
        {
            quickMono.radioButtonInt = 2;
        }
        
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.normal = buttonStyle.active; // Make the normal state look pressed
        if (GUILayout.Button("Toggle Button", buttonStyle))
        {
            
        }
        
        EditorGUILayout.EndHorizontal ();

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            Debug.LogWarning($"changed {radioButtonIntProp.intValue}");
        }

        EditorGUILayout.Space(20f);
        EditorGUILayout.LabelField("And now the normal stuff");
        
        EditorGUILayout.LabelField(
            "Newlines. \n how do they work?", 
            GUILayout.Height(EditorGUIUtility.singleLineHeight * 2f) 
            );

        // DrawDefaultInspector();
        
        // EditorGUILayout.PropertyField(quickDataProp);
        // EditorGUILayout.PropertyField(quickDataArrayProp);
        //
        // EditorGUILayout.Space(20f);
        // EditorGUILayout.LabelField("And now the nested stuff");
        //
        // EditorGUILayout.PropertyField(nestedFloatProp);
    }

    void DrawQuickDataProperty(SerializedProperty prop)
    {
        var boolProp = prop.FindPropertyRelative("quickBool");
        EditorGUILayout.PropertyField(boolProp);
        if (boolProp.boolValue)
        {
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("quickFloat"));
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("quickInt"));
            EditorGUILayout.PropertyField(prop.FindPropertyRelative("quickString"));
        }
    }
}
