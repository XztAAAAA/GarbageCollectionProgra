using TMPro.EditorUtilities;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LocalizedTextMeshProUGUI), true), CanEditMultipleObjects]
public class LocalizedTextMeshProUGUIEditor : TMP_EditorPanelUI
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("多语言", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(
            serializedObject.FindProperty("languageKey"),
            new GUIContent("Language Key")
        );
        serializedObject.ApplyModifiedProperties();

        if (targets.Length != 1)
        {
            return;
        }

        LocalizedTextMeshProUGUI localizedText =
            (LocalizedTextMeshProUGUI)target;
        if (localizedText.TryGetLocalizedText(
            out string previewText,
            out string error))
        {
            EditorGUILayout.HelpBox(
                $"预览（{Application.systemLanguage}）：\n{previewText}",
                MessageType.Info
            );
        }
        else
        {
            EditorGUILayout.HelpBox(error, MessageType.Warning);
        }
    }
}
