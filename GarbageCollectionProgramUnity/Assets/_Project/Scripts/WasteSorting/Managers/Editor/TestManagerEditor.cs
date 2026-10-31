using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TestManager))]
public class TestManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();

        if (GUILayout.Button("清除关卡存档"))
        {
            bool confirmed = EditorUtility.DisplayDialog(
                "清除关卡存档",
                "确定要清除所有关卡进度存档吗？此操作无法撤销。",
                "清除",
                "取消"
            );

            if (confirmed)
            {
                WasteLevelProgress.ClearProgress();
            }
        }
    }
}