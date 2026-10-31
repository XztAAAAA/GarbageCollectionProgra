#if UNITY_EDITOR
using UnityEngine;

public class TestManager : MonoBehaviour
{
    public void CompleteCurrentLevel()
    {
        if (!Application.isPlaying)
        {
            CommonTool.LogWarning(
                "[TestManager] 一键过关只能在运行时使用。",
                this
            );
            return;
        }

        WasteGameManager gameManager =
            FindObjectOfType<WasteGameManager>();
        WasteLevelFlowManager flowManager =
            FindObjectOfType<WasteLevelFlowManager>();

        if (gameManager == null || flowManager == null)
        {
            CommonTool.LogError(
                "[TestManager] 找不到关卡或游戏管理器，无法完成关卡。",
                this
            );
            return;
        }

        if (gameManager.TotalItems <= 0)
        {
            CommonTool.LogWarning(
                "[TestManager] 当前关卡尚未生成垃圾。",
                this
            );
            return;
        }

        GameEventBus.Publish(
            new WasteLevelCompletedEvent(
                gameManager.TotalItems,
                gameManager.TotalItems
            )
        );

        CommonTool.Log(
            $"[TestManager] 已触发第 {flowManager.CurrentLevelId} 关完成。",
            this
        );
    }
}
#endif