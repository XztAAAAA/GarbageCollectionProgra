using System;
using UnityEngine;

public class WasteTableDebug : MonoBehaviour
{
    private void Start()
    {
        try
        {
            // 使用现有的 Luban 表加载系统
            var tables = TableSystem.Tables;

            // 读取 ID 为 1001 的垃圾数据
            var item = tables.TbWasteItem.Get(1001);

            if (item == null)
            {
                Debug.LogError(
                    "[WasteTableDebug] 没有找到 ID = 1001 的垃圾数据。"
                );
                return;
            }

            // 输出中韩双语名称
            Debug.Log($"[WasteTableDebug] ID: {item.Id}");
            Debug.Log($"[WasteTableDebug] 韩语: {item.KoreanName}");
            Debug.Log($"[WasteTableDebug] 中文: {item.ChineseName}");
            Debug.Log($"[WasteTableDebug] 分类: {item.Category}");

            // 输出预处理步骤数量
            int stepCount = item.PreprocessingSteps == null
                ? 0
                : item.PreprocessingSteps.Count;

            Debug.Log(
                $"[WasteTableDebug] 预处理步骤数量: {stepCount}"
            );
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"[WasteTableDebug] 读取失败：\n{exception}"
            );
        }
    }
}