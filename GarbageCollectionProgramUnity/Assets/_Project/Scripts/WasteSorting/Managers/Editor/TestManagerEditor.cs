using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TestManager))]
public class TestManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            if (GUILayout.Button("一键过关"))
            {
                var method = typeof(TestManager).GetMethod(
                    "CompleteCurrentLevel",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                );

                if (method != null)
                {
                    method.Invoke((TestManager)target, null);
                }
                else
                {
                    Debug.LogWarning("TestManager 中不存在 CompleteCurrentLevel 方法，无法执行一键过关。");
                }
            }
        }

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